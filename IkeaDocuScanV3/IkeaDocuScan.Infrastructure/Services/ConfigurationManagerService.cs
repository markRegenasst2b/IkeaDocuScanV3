using System.Collections.Concurrent;
using System.Text.Json;
using IkeaDocuScan.Infrastructure.Data;
using IkeaDocuScan.Infrastructure.Entities.Configuration;
using IkeaDocuScan.Shared.Configuration;
using IkeaDocuScan.Shared.DTOs.Configuration;
using IkeaDocuScan.Shared.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace IkeaDocuScan.Infrastructure.Services;

/// <summary>
/// Hybrid configuration manager with database-first, file-fallback pattern
/// Includes automatic rollback on errors and 5-minute cache TTL
/// </summary>
public class ConfigurationManagerService : ISystemConfigurationManager
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    private readonly IConfiguration _fileConfiguration;
    private readonly ILogger<ConfigurationManagerService> _logger;
    private readonly EmailOptions _emailOptions;

    // Cache for performance (with 5-minute TTL)
    private readonly ConcurrentDictionary<string, CacheEntry> _cache = new();
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    public ConfigurationManagerService(
        IDbContextFactory<AppDbContext> contextFactory,
        IConfiguration fileConfiguration,
        ILogger<ConfigurationManagerService> logger,
        IOptions<EmailOptions> emailOptions)
    {
        _contextFactory = contextFactory;
        _fileConfiguration = fileConfiguration;
        _logger = logger;
        _emailOptions = emailOptions.Value;
    }

    #region Configuration CRUD

    public async Task<T?> GetConfigurationAsync<T>(string configKey, string section, T? defaultValue = default)
        where T : class
    {
        var cacheKey = $"{section}:{configKey}";

        // Check cache first
        if (_cache.TryGetValue(cacheKey, out var cachedEntry) && !cachedEntry.IsExpired)
        {
            if (cachedEntry.Value is T cachedValue)
            {
                _logger.LogDebug("Configuration cache hit: {CacheKey}", cacheKey);
                return cachedValue;
            }
        }

        try
        {
            // Try database first
            await using var context = await _contextFactory.CreateDbContextAsync();

            var dbConfig = await context.SystemConfigurations
                .AsNoTracking()
                .Where(c => c.ConfigKey == configKey &&
                           c.ConfigSection == section &&
                           c.IsActive &&
                           c.IsOverride)
                .FirstOrDefaultAsync();

            if (dbConfig != null)
            {
                _logger.LogInformation("Loading configuration from database: {ConfigKey}", configKey);
                var value = DeserializeValue<T>(dbConfig.ConfigValue, dbConfig.ValueType);

                // Cache the value
                _cache[cacheKey] = new CacheEntry(value, DateTime.UtcNow.Add(CacheDuration));

                return value;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load configuration from database: {ConfigKey}. Falling back to appsettings.", configKey);
        }

        // Fallback to file configuration
        _logger.LogInformation("Loading configuration from appsettings: {ConfigKey}", configKey);
        var fileValue = _fileConfiguration.GetSection($"{section}:{configKey}").Get<T>();

        if (fileValue != null)
        {
            // Cache the file value too
            _cache[cacheKey] = new CacheEntry(fileValue, DateTime.UtcNow.Add(CacheDuration));
            return fileValue;
        }

        _logger.LogWarning("Configuration not found in database or appsettings: {ConfigKey}. Using default.", configKey);
        return defaultValue;
    }

    public T? GetConfiguration<T>(string configKey, string section, T? defaultValue = default)
        where T : class
    {
        // Synchronous version - uses cache only, does not hit database
        var cacheKey = $"{section}:{configKey}";

        if (_cache.TryGetValue(cacheKey, out var cachedEntry) && !cachedEntry.IsExpired)
        {
            if (cachedEntry.Value is T cachedValue)
            {
                return cachedValue;
            }
        }

        // Fallback to file configuration (synchronous)
        var fileValue = _fileConfiguration.GetSection($"{section}:{configKey}").Get<T>();
        if (fileValue != null)
        {
            _cache[cacheKey] = new CacheEntry(fileValue, DateTime.UtcNow.Add(CacheDuration));
            return fileValue;
        }

        return defaultValue;
    }

    public async Task SetConfigurationAsync<T>(string configKey, string section, T value, string changedBy, string? reason = null)
        where T : class
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        // Use execution strategy to handle transactions with retry logic
        var strategy = context.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await context.Database.BeginTransactionAsync();

            try
            {
                var existing = await context.SystemConfigurations
                    .Where(c => c.ConfigKey == configKey && c.ConfigSection == section)
                    .FirstOrDefaultAsync();

                var (serializedValue, valueType) = SerializeValue(value);
                string? oldValue = existing?.ConfigValue;

                if (existing != null)
                {
                    // Update existing configuration
                    existing.ConfigValue = serializedValue;
                    existing.ValueType = valueType;
                    existing.ModifiedBy = changedBy;
                    existing.ModifiedDate = DateTime.UtcNow;
                    existing.IsActive = true;
                    existing.IsOverride = true;
                }
                else
                {
                    // Create new configuration
                    var newConfig = new SystemConfiguration
                    {
                        ConfigKey = configKey,
                        ConfigSection = section,
                        ConfigValue = serializedValue,
                        ValueType = valueType,
                        IsActive = true,
                        IsOverride = true,
                        CreatedBy = changedBy,
                        CreatedDate = DateTime.UtcNow
                    };
                    context.SystemConfigurations.Add(newConfig);
                    await context.SaveChangesAsync(); // Save to get ConfigurationId
                    existing = newConfig;
                }

                // Create audit trail
                var audit = new SystemConfigurationAudit
                {
                    ConfigurationId = existing.ConfigurationId,
                    ConfigKey = configKey,
                    OldValue = oldValue,
                    NewValue = serializedValue,
                    ChangedBy = changedBy,
                    ChangedDate = DateTime.UtcNow,
                    ChangeReason = reason
                };
                context.SystemConfigurationAudits.Add(audit);

                await context.SaveChangesAsync();

                // Commit transaction
                await transaction.CommitAsync();

                // Invalidate cache
                var cacheKey = $"{section}:{configKey}";
                _cache.TryRemove(cacheKey, out _);

                _logger.LogInformation("Configuration updated successfully: {ConfigKey} by {User}", configKey, changedBy);
            }
            catch (Exception ex)
            {
                // Rollback transaction
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error updating configuration {ConfigKey}. Changes rolled back.", configKey);
                throw;
            }
        });
    }

    #endregion

    #region Email Recipients

    public async Task<string[]> GetEmailRecipientsAsync(string groupKey)
    {
        var cacheKey = $"EmailRecipients:{groupKey}";

        // Check cache
        if (_cache.TryGetValue(cacheKey, out var cachedEntry) && !cachedEntry.IsExpired)
        {
            if (cachedEntry.Value is string[] cachedValue)
            {
                _logger.LogDebug("Email recipients cache hit: {GroupKey}", groupKey);
                return cachedValue;
            }
        }

        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var group = await context.EmailRecipientGroups
                .AsNoTracking()
                .Include(g => g.Recipients)
                .Where(g => g.GroupKey == groupKey && g.IsActive)
                .FirstOrDefaultAsync();

            if (group != null)
            {
                var recipients = group.Recipients
                    .Where(r => r.IsActive)
                    .OrderBy(r => r.SortOrder)
                    .Select(r => r.EmailAddress)
                    .ToArray();

                // Cache the result
                _cache[cacheKey] = new CacheEntry(recipients, DateTime.UtcNow.Add(CacheDuration));

                _logger.LogInformation("Loaded {Count} email recipients from database for group: {GroupKey}", recipients.Length, groupKey);
                return recipients;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load email recipients from database for group: {GroupKey}", groupKey);
        }

        // Fallback to configuration file
        _logger.LogInformation("Loading email recipients from appsettings for group: {GroupKey}", groupKey);

        // Try different config paths based on group key
        string[] fileValue = groupKey switch
        {
            "AdminEmails" => GetAdminEmailsFromConfig(),
            "ActionReminderRecipients" => _fileConfiguration.GetSection("ActionReminderService:RecipientEmails").Get<string[]>() ?? Array.Empty<string>(),
            _ => _fileConfiguration.GetSection($"EmailRecipients:{groupKey}").Get<string[]>() ?? Array.Empty<string>()
        };

        _cache[cacheKey] = new CacheEntry(fileValue, DateTime.UtcNow.Add(CacheDuration));
        return fileValue;
    }

    public async Task SetEmailRecipientsAsync(string groupKey, string[] emailAddresses, string changedBy, string? reason = null)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        // Use execution strategy - required because DbContext is configured with EnableRetryOnFailure
        var strategy = context.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await context.Database.BeginTransactionAsync();

            try
            {
                var group = await context.EmailRecipientGroups
                    .Include(g => g.Recipients)
                    .Where(g => g.GroupKey == groupKey)
                    .FirstOrDefaultAsync();

                if (group == null)
                {
                    // Create new group
                    group = new EmailRecipientGroup
                    {
                        GroupName = FormatGroupName(groupKey),
                        GroupKey = groupKey,
                        Description = $"Email recipient group for {groupKey}",
                        IsActive = true,
                        CreatedBy = changedBy,
                        CreatedDate = DateTime.UtcNow
                    };
                    context.EmailRecipientGroups.Add(group);
                    await context.SaveChangesAsync(); // Save to get GroupId
                }

                // Remove all existing recipients
                context.EmailRecipients.RemoveRange(group.Recipients);

                // Add new recipients
                for (int i = 0; i < emailAddresses.Length; i++)
                {
                    var recipient = new EmailRecipient
                    {
                        GroupId = group.GroupId,
                        EmailAddress = emailAddresses[i].Trim(),
                        IsActive = true,
                        SortOrder = i,
                        CreatedBy = changedBy,
                        CreatedDate = DateTime.UtcNow
                    };
                    context.EmailRecipients.Add(recipient);
                }

                await context.SaveChangesAsync();

                // Test email configuration if this is an admin group
                if (groupKey.Contains("Admin", StringComparison.OrdinalIgnoreCase))
                {
                    // Optional: Could test sending an email here
                    _logger.LogInformation("Updated admin email recipients. Consider testing email delivery.");
                }

                // Commit transaction
                await transaction.CommitAsync();

                // Invalidate cache
                var cacheKey = $"EmailRecipients:{groupKey}";
                _cache.TryRemove(cacheKey, out _);

                _logger.LogInformation("Updated {Count} email recipients for group: {GroupKey} by {User}", emailAddresses.Length, groupKey, changedBy);
            }
            catch (Exception ex)
            {
                // Rollback transaction
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error updating email recipients for group {GroupKey}. Changes rolled back.", groupKey);
                throw;
            }
        });
    }

    public async Task<List<EmailRecipientGroupDto>> GetAllEmailRecipientGroupsAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var groups = await context.EmailRecipientGroups
            .AsNoTracking()
            .Include(g => g.Recipients)
            .Where(g => g.IsActive)
            .OrderBy(g => g.GroupName)
            .ToListAsync();

        return groups.Select(g => new EmailRecipientGroupDto
        {
            GroupId = g.GroupId,
            GroupName = g.GroupName,
            GroupKey = g.GroupKey,
            Description = g.Description,
            IsActive = g.IsActive,
            Recipients = g.Recipients
                .Where(r => r.IsActive)
                .OrderBy(r => r.SortOrder)
                .Select(r => new EmailRecipientDto
                {
                    RecipientId = r.RecipientId,
                    EmailAddress = r.EmailAddress,
                    DisplayName = r.DisplayName,
                    IsActive = r.IsActive,
                    SortOrder = r.SortOrder
                }).ToList()
        }).ToList();
    }

    #endregion

    #region Email Templates

    public async Task<EmailTemplateDto?> GetEmailTemplateAsync(string templateKey)
    {
        var cacheKey = $"EmailTemplate:{templateKey}";

        // Check cache
        if (_cache.TryGetValue(cacheKey, out var cachedEntry) && !cachedEntry.IsExpired)
        {
            if (cachedEntry.Value is EmailTemplateDto cachedValue)
            {
                _logger.LogDebug("Email template cache hit: {TemplateKey}", templateKey);
                return cachedValue;
            }
        }

        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var template = await context.EmailTemplates
                .AsNoTracking()
                .Where(t => t.TemplateKey == templateKey && t.IsActive)
                .OrderByDescending(t => t.IsDefault)
                .ThenByDescending(t => t.ModifiedDate ?? t.CreatedDate)
                .FirstOrDefaultAsync();

            if (template != null)
            {
                var dto = new EmailTemplateDto
                {
                    TemplateId = template.TemplateId,
                    TemplateName = template.TemplateName,
                    TemplateKey = template.TemplateKey,
                    Subject = template.Subject,
                    HtmlBody = template.HtmlBody,
                    PlainTextBody = template.PlainTextBody,
                    PlaceholderDefinitions = template.PlaceholderDefinitions,
                    Category = template.Category,
                    IsActive = template.IsActive,
                    IsDefault = template.IsDefault
                };

                // Cache the result
                _cache[cacheKey] = new CacheEntry(dto, DateTime.UtcNow.Add(CacheDuration));

                _logger.LogInformation("Loaded email template from database: {TemplateKey}", templateKey);
                return dto;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load email template from database: {TemplateKey}", templateKey);
        }

        _logger.LogInformation("Email template not found in database: {TemplateKey}", templateKey);
        return null;
    }

    public async Task<EmailTemplateDto> SaveEmailTemplateAsync(EmailTemplateDto template, string changedBy)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        // Use execution strategy - required because DbContext is configured with EnableRetryOnFailure
        var strategy = context.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await context.Database.BeginTransactionAsync();

            try
            {
                EmailTemplate entity;

                if (template.TemplateId.HasValue)
                {
                    // Update existing template
                    entity = await context.EmailTemplates
                        .FirstOrDefaultAsync(t => t.TemplateId == template.TemplateId.Value);

                    if (entity == null)
                    {
                        throw new InvalidOperationException($"Email template with ID {template.TemplateId} not found");
                    }

                    entity.TemplateName = template.TemplateName;
                    entity.Subject = template.Subject;
                    entity.HtmlBody = template.HtmlBody;
                    entity.PlainTextBody = template.PlainTextBody;
                    entity.PlaceholderDefinitions = template.PlaceholderDefinitions;
                    entity.Category = template.Category;
                    entity.IsActive = template.IsActive;
                    entity.IsDefault = template.IsDefault;
                    entity.ModifiedBy = changedBy;
                    entity.ModifiedDate = DateTime.UtcNow;
                }
                else
                {
                    // Create new template
                    entity = new EmailTemplate
                    {
                        TemplateName = template.TemplateName,
                        TemplateKey = template.TemplateKey,
                        Subject = template.Subject,
                        HtmlBody = template.HtmlBody,
                        PlainTextBody = template.PlainTextBody,
                        PlaceholderDefinitions = template.PlaceholderDefinitions,
                        Category = template.Category,
                        IsActive = template.IsActive,
                        IsDefault = template.IsDefault,
                        CreatedBy = changedBy,
                        CreatedDate = DateTime.UtcNow
                    };
                    context.EmailTemplates.Add(entity);
                }

                await context.SaveChangesAsync();

                // Validate template by attempting to render it
                ValidateTemplate(entity.HtmlBody, entity.Subject);

                // Commit transaction
                await transaction.CommitAsync();

                // Invalidate cache
                var cacheKey = $"EmailTemplate:{template.TemplateKey}";
                _cache.TryRemove(cacheKey, out _);

                _logger.LogInformation("Email template saved successfully: {TemplateKey} by {User}", template.TemplateKey, changedBy);

                // Return updated DTO
                template.TemplateId = entity.TemplateId;
                return template;
            }
            catch (Exception ex)
            {
                // Rollback transaction
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error saving email template {TemplateKey}. Changes rolled back.", template.TemplateKey);
                throw;
            }
        });
    }

    public async Task<List<EmailTemplateDto>> GetAllEmailTemplatesAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var templates = await context.EmailTemplates
            .AsNoTracking()
            .Where(t => t.IsActive)
            .OrderBy(t => t.Category)
            .ThenBy(t => t.TemplateName)
            .ToListAsync();

        return templates.Select(t => new EmailTemplateDto
        {
            TemplateId = t.TemplateId,
            TemplateName = t.TemplateName,
            TemplateKey = t.TemplateKey,
            Subject = t.Subject,
            HtmlBody = t.HtmlBody,
            PlainTextBody = t.PlainTextBody,
            PlaceholderDefinitions = t.PlaceholderDefinitions,
            Category = t.Category,
            IsActive = t.IsActive,
            IsDefault = t.IsDefault
        }).ToList();
    }

    #endregion

    #region Cache Management

    public async Task ReloadAsync()
    {
        _cache.Clear();
        _logger.LogInformation("Configuration cache cleared. Total entries removed: {Count}", _cache.Count);
        await Task.CompletedTask;
    }

    #endregion

    #region SMTP Testing

    public async Task<bool> TestSmtpConnectionAsync()
    {
        // Use CancellationToken to ensure test fails fast
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));

        try
        {
            _logger.LogInformation("Testing SMTP connection...");

            // Get SMTP configuration (from database or fallback to file)
            var smtpHost = await GetConfigurationAsync<string>("SmtpHost", "Email") ?? _emailOptions.SmtpHost;
            var smtpPort = await GetConfigurationAsync<string>("SmtpPort", "Email");
            int port = smtpPort != null ? int.Parse(smtpPort) : _emailOptions.SmtpPort;

            var useSsl = await GetConfigurationAsync<string>("UseSsl", "Email");
            bool ssl = useSsl != null ? bool.Parse(useSsl) : _emailOptions.UseSsl;

            var username = await GetConfigurationAsync<string>("SmtpUsername", "Email") ?? _emailOptions.SmtpUsername;
            var password = await GetConfigurationAsync<string>("SmtpPassword", "Email") ?? _emailOptions.SmtpPassword;

            // Validate configuration
            if (string.IsNullOrWhiteSpace(smtpHost))
            {
                _logger.LogWarning("SMTP test skipped: SmtpHost is not configured");
                return false;
            }

            using var client = new SmtpClient();
            client.Timeout = 10000; // 10 second timeout for operations

            // Connect with cancellation token to ensure timeout is respected
            await client.ConnectAsync(smtpHost, port, ssl ? SecureSocketOptions.StartTls : SecureSocketOptions.None, cts.Token);

            if (!string.IsNullOrWhiteSpace(username))
            {
                await client.AuthenticateAsync(username, password, cts.Token);
            }

            await client.DisconnectAsync(true, cts.Token);

            _logger.LogInformation("SMTP connection test successful: {Host}:{Port}", smtpHost, port);
            return true;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("SMTP connection test timed out after 15 seconds");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SMTP connection test failed: {Message}", ex.Message);
            return false;
        }
    }

    /// <summary>
    /// Updates all SMTP settings atomically and tests the configuration
    /// Only commits if SMTP test passes, otherwise rolls back all changes
    /// </summary>
    /// <param name="config">SMTP configuration settings</param>
    /// <param name="changedBy">User making the change</param>
    /// <param name="reason">Optional reason for the change</param>
    /// <param name="skipTest">If true, skips SMTP testing and saves settings immediately (use with caution)</param>
    public async Task SetSmtpConfigurationAsync(SmtpConfigurationDto config, string changedBy, string? reason = null, bool skipTest = false)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        // Use execution strategy - required because DbContext is configured with EnableRetryOnFailure
        // SMTP test failures throw InvalidOperationException (not transient), so won't trigger retries
        var strategy = context.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await context.Database.BeginTransactionAsync();

            try
            {
                _logger.LogInformation("Updating SMTP configuration atomically (skipTest: {SkipTest})...", skipTest);

                // Dictionary to store old values for audit trail
                var oldValues = new Dictionary<string, string?>();
                var settings = new Dictionary<string, string>
            {
                { "SmtpHost", config.SmtpHost },
                { "SmtpPort", config.SmtpPort.ToString() },
                { "UseSsl", config.UseSsl.ToString() },
                { "SmtpUsername", config.SmtpUsername ?? string.Empty },
                { "SmtpPassword", config.SmtpPassword ?? string.Empty },
                { "FromAddress", config.FromAddress },
                { "FromName", config.FromName ?? string.Empty }
            };

            // Step 1: Update/create all configuration entries
            var configEntities = new Dictionary<string, SystemConfiguration>();

            foreach (var kvp in settings)
            {
                var configKey = kvp.Key;
                var configValue = kvp.Value;

                var existing = await context.SystemConfigurations
                    .Where(c => c.ConfigKey == configKey && c.ConfigSection == "Email")
                    .FirstOrDefaultAsync();

                oldValues[configKey] = existing?.ConfigValue;

                if (existing != null)
                {
                    // Update existing
                    existing.ConfigValue = configValue;
                    existing.ValueType = "String";
                    existing.ModifiedBy = changedBy;
                    existing.ModifiedDate = DateTime.UtcNow;
                    existing.IsActive = true;
                    existing.IsOverride = true;
                    configEntities[configKey] = existing;
                }
                else
                {
                    // Create new
                    var newConfig = new SystemConfiguration
                    {
                        ConfigKey = configKey,
                        ConfigSection = "Email",
                        ConfigValue = configValue,
                        ValueType = "String",
                        IsActive = true,
                        IsOverride = true,
                        CreatedBy = changedBy,
                        CreatedDate = DateTime.UtcNow
                    };
                    context.SystemConfigurations.Add(newConfig);
                    configEntities[configKey] = newConfig;
                }
            }

            // Step 2: Save configurations to get ConfigurationId for new entries
            await context.SaveChangesAsync();

            // Step 3: Create audit trail entries with correct ConfigurationId
            foreach (var kvp in settings)
            {
                var configKey = kvp.Key;
                var configValue = kvp.Value;
                var configEntity = configEntities[configKey];

                var audit = new SystemConfigurationAudit
                {
                    ConfigurationId = configEntity.ConfigurationId,
                    ConfigKey = configKey,
                    OldValue = oldValues[configKey],
                    NewValue = configValue,
                    ChangedBy = changedBy,
                    ChangedDate = DateTime.UtcNow,
                    ChangeReason = reason ?? "Bulk SMTP configuration update"
                };
                context.SystemConfigurationAudits.Add(audit);
            }

            // Step 4: Save audit trail entries
            await context.SaveChangesAsync();

            // Step 5: Test SMTP if requested
            if (!skipTest)
            {
                _logger.LogInformation("Testing SMTP configuration with all new settings...");

                // Temporarily clear cache to force reading from database
                var keysToInvalidate = new[] { "SmtpHost", "SmtpPort", "UseSsl", "SmtpUsername", "SmtpPassword", "FromAddress", "FromName" };
                foreach (var key in keysToInvalidate)
                {
                    var cacheKey = $"Email:{key}";
                    _cache.TryRemove(cacheKey, out _);
                }

                var testResult = await TestSmtpConnectionAsync();

                if (!testResult)
                {
                    _logger.LogWarning("SMTP test failed with new configuration. Rolling back all changes.");
                    throw new InvalidOperationException("SMTP configuration test failed. All changes rolled back. Please verify your SMTP settings.");
                }

                _logger.LogInformation("SMTP connection test passed");
            }
            else
            {
                _logger.LogWarning("SMTP testing SKIPPED - settings saved without validation");

                // Still clear cache
                var keysToInvalidate = new[] { "SmtpHost", "SmtpPort", "UseSsl", "SmtpUsername", "SmtpPassword", "FromAddress", "FromName" };
                foreach (var key in keysToInvalidate)
                {
                    var cacheKey = $"Email:{key}";
                    _cache.TryRemove(cacheKey, out _);
                }
            }

                // Commit transaction
                await transaction.CommitAsync();

                _logger.LogInformation("SMTP configuration updated successfully");
            }
            catch (Exception ex)
            {
                // Rollback transaction
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error updating SMTP configuration. All changes rolled back.");
                throw;
            }
        });
    }

    #endregion

    #region Helper Methods

    private (string SerializedValue, string ValueType) SerializeValue<T>(T value) where T : class
    {
        if (value is string strValue)
        {
            return (strValue, "String");
        }
        else if (value is string[] arrValue)
        {
            return (JsonSerializer.Serialize(arrValue), "StringArray");
        }
        else if (value is int intValue)
        {
            return (intValue.ToString(), "Int");
        }
        else if (value is bool boolValue)
        {
            return (boolValue.ToString(), "Bool");
        }
        else
        {
            return (JsonSerializer.Serialize(value), "Json");
        }
    }

    private T? DeserializeValue<T>(string serializedValue, string valueType) where T : class
    {
        try
        {
            if (valueType == "String" && typeof(T) == typeof(string))
            {
                return (serializedValue as T)!;
            }
            else if (valueType == "StringArray" && typeof(T) == typeof(string[]))
            {
                return (JsonSerializer.Deserialize<string[]>(serializedValue) as T)!;
            }
            else if (valueType == "Int")
            {
                return (int.Parse(serializedValue) as T);
            }
            else if (valueType == "Bool")
            {
                return (bool.Parse(serializedValue) as T);
            }
            else
            {
                return JsonSerializer.Deserialize<T>(serializedValue);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deserializing value of type {ValueType}", valueType);
            return null;
        }
    }

    private string[] GetAdminEmailsFromConfig()
    {
        var emails = new List<string>();

        var adminEmail = _fileConfiguration["Email:AdminEmail"];
        if (!string.IsNullOrEmpty(adminEmail))
        {
            emails.Add(adminEmail);
        }

        var additionalEmails = _fileConfiguration.GetSection("Email:AdditionalAdminEmails").Get<string[]>();
        if (additionalEmails != null)
        {
            emails.AddRange(additionalEmails);
        }

        return emails.ToArray();
    }

    private string FormatGroupName(string groupKey)
    {
        // Convert "ActionReminderRecipients" to "Action Reminder Recipients"
        return string.Concat(groupKey.Select((x, i) => i > 0 && char.IsUpper(x) ? " " + x : x.ToString()));
    }

    private void ValidateTemplate(string htmlBody, string subject)
    {
        // Basic validation - check for obvious issues
        if (string.IsNullOrWhiteSpace(htmlBody))
        {
            throw new InvalidOperationException("Email template HTML body cannot be empty");
        }

        if (string.IsNullOrWhiteSpace(subject))
        {
            throw new InvalidOperationException("Email template subject cannot be empty");
        }

        // Check for unclosed placeholders
        var openCount = htmlBody.Count(c => c == '{');
        var closeCount = htmlBody.Count(c => c == '}');
        if (openCount != closeCount)
        {
            _logger.LogWarning("Template may have mismatched braces. Open: {Open}, Close: {Close}", openCount, closeCount);
        }
    }

    private class CacheEntry
    {
        public object Value { get; }
        public DateTime ExpiresAt { get; }

        public CacheEntry(object value, DateTime expiresAt)
        {
            Value = value;
            ExpiresAt = expiresAt;
        }

        public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    }

    #endregion
}
