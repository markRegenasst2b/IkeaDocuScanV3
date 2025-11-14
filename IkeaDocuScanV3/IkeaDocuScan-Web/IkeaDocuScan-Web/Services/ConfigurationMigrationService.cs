using IkeaDocuScan.Infrastructure.Data;
using IkeaDocuScan.Infrastructure.Entities;
using IkeaDocuScan.Infrastructure.Entities.Configuration;
using IkeaDocuScan.Shared.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace IkeaDocuScan_Web.Services;

/// <summary>
/// Service for migrating configuration from appsettings.json to the database
/// </summary>
public class ConfigurationMigrationService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ConfigurationMigrationService> _logger;

    public ConfigurationMigrationService(
        AppDbContext context,
        IConfiguration configuration,
        ILogger<ConfigurationMigrationService> logger)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Migrates all configuration settings from appsettings.json to database
    /// </summary>
    /// <param name="changedBy">Username performing the migration</param>
    /// <param name="overwriteExisting">If true, overwrites existing database values</param>
    public async Task<MigrationResult> MigrateAllAsync(string changedBy, bool overwriteExisting = false)
    {
        var result = new MigrationResult();

        try
        {
            _logger.LogInformation("Starting configuration migration by {User}", changedBy);

            // Migrate SMTP settings
            result.SmtpSettingsMigrated = await MigrateSmtpSettingsAsync(changedBy, overwriteExisting);

            // Migrate email recipient groups
            result.RecipientGroupsMigrated = await MigrateEmailRecipientGroupsAsync(changedBy, overwriteExisting);

            // Create default email templates
            result.EmailTemplatesCreated = await CreateDefaultEmailTemplatesAsync(changedBy, overwriteExisting);

            result.Success = true;
            result.Message = $"Migration completed successfully. SMTP: {result.SmtpSettingsMigrated}, Recipients: {result.RecipientGroupsMigrated}, Templates: {result.EmailTemplatesCreated}";

            _logger.LogInformation("Configuration migration completed successfully");
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.Message = $"Migration failed: {ex.Message}";
            _logger.LogError(ex, "Configuration migration failed");
        }

        return result;
    }

    /// <summary>
    /// Migrates SMTP settings from Email section in appsettings.json
    /// </summary>
    private async Task<int> MigrateSmtpSettingsAsync(string changedBy, bool overwriteExisting)
    {
        var count = 0;
        var emailSection = _configuration.GetSection("Email");

        if (!emailSection.Exists())
        {
            _logger.LogWarning("Email section not found in configuration");
            return count;
        }

        var settings = new Dictionary<string, string>
        {
            { "SmtpHost", emailSection["SmtpHost"] ?? "localhost" },
            { "SmtpPort", emailSection["SmtpPort"] ?? "587" },
            { "UseSsl", emailSection["UseSsl"] ?? "true" },
            { "SmtpUsername", emailSection["SmtpUsername"] ?? "" },
            { "SmtpPassword", emailSection["SmtpPassword"] ?? "" },
            { "FromAddress", emailSection["FromAddress"] ?? "noreply@company.com" },
            { "FromDisplayName", emailSection["FromDisplayName"] ?? "DocuScan System" }
        };

        foreach (var setting in settings)
        {
            if (await SetConfigurationAsync("Email", setting.Key, setting.Value, changedBy, "Migrated from appsettings.json", overwriteExisting))
            {
                count++;
            }
        }

        _logger.LogInformation("Migrated {Count} SMTP settings", count);
        return count;
    }

    /// <summary>
    /// Migrates email recipient groups from configuration
    /// </summary>
    private async Task<int> MigrateEmailRecipientGroupsAsync(string changedBy, bool overwriteExisting)
    {
        var count = 0;

        // Access Request Admins (from Email:AdminEmail and Email:AdditionalAdminEmails)
        var adminEmail = _configuration["Email:AdminEmail"];
        var additionalAdmins = _configuration.GetSection("Email:AdditionalAdminEmails").Get<string[]>() ?? Array.Empty<string>();

        var accessRequestAdmins = new List<string>();
        if (!string.IsNullOrWhiteSpace(adminEmail))
            accessRequestAdmins.Add(adminEmail);
        accessRequestAdmins.AddRange(additionalAdmins.Where(e => !string.IsNullOrWhiteSpace(e)));

        if (accessRequestAdmins.Any())
        {
            if (await SetEmailRecipientGroupAsync(
                "AccessRequestAdmins",
                "Access Request Notification Recipients",
                accessRequestAdmins.ToArray(),
                changedBy,
                "Migrated from appsettings.json",
                overwriteExisting))
            {
                count++;
            }
        }

        // Action Reminder Recipients (from ActionReminderService:RecipientEmails)
        var actionReminderEmails = _configuration.GetSection("ActionReminderService:RecipientEmails").Get<string[]>() ?? Array.Empty<string>();

        if (actionReminderEmails.Any())
        {
            if (await SetEmailRecipientGroupAsync(
                "ActionReminderRecipients",
                "Action Reminder Daily Email Recipients",
                actionReminderEmails,
                changedBy,
                "Migrated from appsettings.json",
                overwriteExisting))
            {
                count++;
            }
        }

        // Default Document Recipients (from Email:SearchResults:DefaultRecipient)
        var defaultRecipient = _configuration["Email:SearchResults:DefaultRecipient"];

        if (!string.IsNullOrWhiteSpace(defaultRecipient))
        {
            if (await SetEmailRecipientGroupAsync(
                "DocumentEmailRecipients",
                "Default Document Email Recipients",
                new[] { defaultRecipient },
                changedBy,
                "Migrated from appsettings.json",
                overwriteExisting))
            {
                count++;
            }
        }

        _logger.LogInformation("Migrated {Count} email recipient groups", count);
        return count;
    }

    /// <summary>
    /// Creates default email templates
    /// </summary>
    private async Task<int> CreateDefaultEmailTemplatesAsync(string changedBy, bool overwriteExisting)
    {
        var count = 0;

        // 1. Access Request Notification
        if (await CreateEmailTemplateAsync(
            "AccessRequestNotification",
            "Access Request Notification",
            "Notifications",
            _configuration["Email:AccessRequestSubject"] ?? "New Access Request - DocuScan",
            GetAccessRequestNotificationTemplate(),
            changedBy,
            overwriteExisting))
        {
            count++;
        }

        // 2. Access Request Confirmation
        if (await CreateEmailTemplateAsync(
            "AccessRequestConfirmation",
            "Access Request Confirmation",
            "Notifications",
            _configuration["Email:AccessRequestConfirmationSubject"] ?? "Your Access Request Has Been Received",
            GetAccessRequestConfirmationTemplate(),
            changedBy,
            overwriteExisting))
        {
            count++;
        }

        // 3. Action Reminder Daily
        if (await CreateEmailTemplateAsync(
            "ActionReminderDaily",
            "Action Reminder Daily Email",
            "Reminders",
            _configuration["ActionReminderService:EmailSubject"] ?? "Action Reminders Due Today - {{Count}} Items",
            GetActionReminderDailyTemplate(),
            changedBy,
            overwriteExisting))
        {
            count++;
        }

        // 4. Document Link Email
        var linkTemplate = _configuration["Email:SearchResults:LinkEmailTemplate"];
        if (!string.IsNullOrWhiteSpace(linkTemplate))
        {
            if (await CreateEmailTemplateAsync(
                "DocumentLink",
                "Document Link Email",
                "Documents",
                "Documents Available for Download",
                linkTemplate,
                changedBy,
                overwriteExisting))
            {
                count++;
            }
        }

        // 5. Document Attachment Email
        var attachTemplate = _configuration["Email:SearchResults:AttachEmailTemplate"];
        if (!string.IsNullOrWhiteSpace(attachTemplate))
        {
            if (await CreateEmailTemplateAsync(
                "DocumentAttachment",
                "Document Attachment Email",
                "Documents",
                "Documents Attached",
                attachTemplate,
                changedBy,
                overwriteExisting))
            {
                count++;
            }
        }

        _logger.LogInformation("Created {Count} email templates", count);
        return count;
    }

    /// <summary>
    /// Sets a configuration value in the database
    /// </summary>
    private async Task<bool> SetConfigurationAsync(
        string section,
        string configKey,
        string value,
        string changedBy,
        string? reason,
        bool overwriteExisting)
    {
        var existing = await _context.SystemConfigurations
            .FirstOrDefaultAsync(c => c.ConfigSection == section && c.ConfigKey == configKey);

        if (existing != null && !overwriteExisting)
        {
            _logger.LogDebug("Skipping existing configuration {Section}:{Key}", section, configKey);
            return false;
        }

        if (existing != null)
        {
            // Update existing
            var oldValue = existing.ConfigValue;
            existing.ConfigValue = value;
            existing.ModifiedDate = DateTime.UtcNow;
            existing.ModifiedBy = changedBy;

            // Audit trail
            _context.SystemConfigurationAudits.Add(new SystemConfigurationAudit
            {
                ConfigKey = configKey,
                OldValue = oldValue,
                NewValue = value,
                ChangedBy = changedBy,
                ChangedDate = DateTime.UtcNow,
                ChangeReason = reason ?? "Overwritten during migration"
            });
        }
        else
        {
            // Create new
            _context.SystemConfigurations.Add(new SystemConfiguration
            {
                ConfigSection = section,
                ConfigKey = configKey,
                ConfigValue = value,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = changedBy
            });

            // Audit trail
            _context.SystemConfigurationAudits.Add(new SystemConfigurationAudit
            {
                ConfigKey = configKey,
                OldValue = null,
                NewValue = value,
                ChangedBy = changedBy,
                ChangedDate = DateTime.UtcNow,
                ChangeReason = reason ?? "Created during migration"
            });
        }

        await _context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Sets email recipient group in the database
    /// </summary>
    private async Task<bool> SetEmailRecipientGroupAsync(
        string groupKey,
        string groupName,
        string[] emailAddresses,
        string changedBy,
        string? reason,
        bool overwriteExisting)
    {
        var existingGroup = await _context.EmailRecipientGroups
            .Include(g => g.Recipients)
            .FirstOrDefaultAsync(g => g.GroupKey == groupKey);

        if (existingGroup != null && !overwriteExisting)
        {
            _logger.LogDebug("Skipping existing recipient group {GroupKey}", groupKey);
            return false;
        }

        if (existingGroup != null)
        {
            // Remove existing recipients
            _context.EmailRecipients.RemoveRange(existingGroup.Recipients);
        }
        else
        {
            // Create new group
            existingGroup = new EmailRecipientGroup
            {
                GroupKey = groupKey,
                GroupName = groupName,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = changedBy,
                Recipients = new List<EmailRecipient>()
            };
            _context.EmailRecipientGroups.Add(existingGroup);
        }

        // Add new recipients
        var sortOrder = 1;
        foreach (var email in emailAddresses.Where(e => !string.IsNullOrWhiteSpace(e)))
        {
            existingGroup.Recipients.Add(new EmailRecipient
            {
                EmailAddress = email.Trim(),
                CreatedDate = DateTime.UtcNow,
                CreatedBy = changedBy,
                SortOrder = sortOrder++
            });
        }

        await _context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Creates an email template in the database
    /// </summary>
    private async Task<bool> CreateEmailTemplateAsync(
        string templateKey,
        string templateName,
        string category,
        string subject,
        string htmlBody,
        string changedBy,
        bool overwriteExisting)
    {
        var existing = await _context.EmailTemplates
            .FirstOrDefaultAsync(t => t.TemplateKey == templateKey);

        if (existing != null && !overwriteExisting)
        {
            _logger.LogDebug("Skipping existing template {TemplateKey}", templateKey);
            return false;
        }

        if (existing != null)
        {
            // Update existing
            existing.TemplateName = templateName;
            existing.Category = category;
            existing.Subject = subject;
            existing.HtmlBody = htmlBody;
            existing.ModifiedDate = DateTime.UtcNow;
            existing.ModifiedBy = changedBy;
        }
        else
        {
            // Create new
            _context.EmailTemplates.Add(new EmailTemplate
            {
                TemplateKey = templateKey,
                TemplateName = templateName,
                Category = category,
                Subject = subject,
                HtmlBody = htmlBody,
                IsActive = true,
                IsDefault = true,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = changedBy,
                ModifiedDate = DateTime.UtcNow,
                ModifiedBy = changedBy
            });
        }

        await _context.SaveChangesAsync();
        return true;
    }

    #region Email Template HTML Definitions

    private string GetAccessRequestNotificationTemplate()
    {
        return @"<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; background-color: #f5f5f5; margin: 0; padding: 0; }
        .container { max-width: 600px; margin: 20px auto; background-color: #fff; border: 1px solid #ddd; }
        .header { background-color: #0051ba; color: #fff; padding: 20px; text-align: center; }
        .header h1 { margin: 0; font-size: 24px; }
        .content { padding: 30px; }
        .request-details { background-color: #f9f9f9; border-left: 4px solid #0051ba; padding: 15px; margin: 20px 0; }
        .detail-row { padding: 8px 0; border-bottom: 1px solid #e0e0e0; }
        .detail-label { font-weight: bold; color: #0051ba; }
        .action-button { display: inline-block; margin-top: 20px; padding: 12px 24px; background-color: #0051ba; color: #fff; text-decoration: none; border-radius: 4px; }
        .footer { background-color: #f5f5f5; padding: 15px; text-align: center; font-size: 12px; color: #666; }
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>New Access Request</h1>
        </div>
        <div class='content'>
            <p>A new access request has been submitted in the DocuScan System:</p>
            <div class='request-details'>
                <div class='detail-row'>
                    <span class='detail-label'>Requested By:</span> {{Username}}
                </div>
                <div class='detail-row'>
                    <span class='detail-label'>Date:</span> {{Date}}
                </div>
                <div class='detail-row'>
                    <span class='detail-label'>Reason:</span> {{Reason}}
                </div>
            </div>
            <p>Please review this request and grant access if appropriate.</p>
            <a href='{{ApplicationUrl}}' class='action-button'>Open DocuScan System</a>
        </div>
        <div class='footer'>
            <p>This is an automated message from the DocuScan System.</p>
        </div>
    </div>
</body>
</html>";
    }

    private string GetAccessRequestConfirmationTemplate()
    {
        return @"<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; background-color: #f5f5f5; margin: 0; padding: 0; }
        .container { max-width: 600px; margin: 20px auto; background-color: #fff; border: 1px solid #ddd; }
        .header { background-color: #0051ba; color: #fff; padding: 20px; text-align: center; }
        .header h1 { margin: 0; font-size: 24px; }
        .content { padding: 30px; }
        .confirmation-box { background-color: #d4edda; border-left: 4px solid #28a745; padding: 15px; margin: 20px 0; }
        .footer { background-color: #f5f5f5; padding: 15px; text-align: center; font-size: 12px; color: #666; }
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Access Request Received</h1>
        </div>
        <div class='content'>
            <p>Dear {{Username}},</p>
            <div class='confirmation-box'>
                <p><strong>Your access request has been received and is being processed.</strong></p>
            </div>
            <p>An administrator will review your request and contact you shortly.</p>
            <p>Thank you for your patience.</p>
        </div>
        <div class='footer'>
            <p>This is an automated message from the DocuScan System.</p>
        </div>
    </div>
</body>
</html>";
    }

    private string GetActionReminderDailyTemplate()
    {
        return @"<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; background-color: #f5f5f5; margin: 0; padding: 0; }
        .container { max-width: 800px; margin: 20px auto; background-color: #fff; border: 1px solid #ddd; }
        .header { background-color: #0051ba; color: #fff; padding: 20px; text-align: center; }
        .header h1 { margin: 0; font-size: 24px; }
        .content { padding: 30px; }
        .reminder-count { background-color: #fff3cd; border-left: 4px solid #ffc107; padding: 15px; margin: 20px 0; font-size: 18px; font-weight: bold; }
        .reminder-table { width: 100%; border-collapse: collapse; margin: 20px 0; }
        .reminder-table th { background-color: #0051ba; color: #fff; padding: 12px; text-align: left; }
        .reminder-table td { padding: 10px; border-bottom: 1px solid #e0e0e0; }
        .reminder-table tr:hover { background-color: #f9f9f9; }
        .footer { background-color: #f5f5f5; padding: 15px; text-align: center; font-size: 12px; color: #666; }
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Action Reminders Due Today</h1>
        </div>
        <div class='content'>
            <div class='reminder-count'>
                {{TestEnvironmentIndicator}}You have {{Count}} action reminder(s) due today ({{Date}}).
            </div>
            <table class='reminder-table'>
                <thead>
                    <tr>
                        <th>Barcode</th>
                        <th>Document Type</th>
                        <th>Action Date</th>
                        <th>Description</th>
                    </tr>
                </thead>
                <tbody>
                    {{#ActionRows}}
                    <!-- Supported Placeholders: ""BarCode,DocumentType,DocumentName,DocumentNo,CounterParty,CounterPartyNo,ActionDate,ReceivingDate,ActionDescription,Comment,IsOverdue""  -->
                    <tr>
                        <td><strong><a href='https://localhost:44101/documents/edit/{{BarCode}}'>{{BarCode}}</a></strong></td>
                        <td>{{DocumentType}}</td>
                        <td>{{ActionDate}}</td>
                        <td>{{ActionDescription}}</td>
                    </tr>
                    {{/ActionRows}}
                </tbody>
            </table>
            <p>Please review these documents and take the necessary actions.</p>
        </div>
        <div class='footer'>
            <p>This is an automated reminder from the DocuScan System.</p>
        </div>
    </div>
</body>
</html>
";
    }

    #endregion
}

/// <summary>
/// Result of configuration migration
/// </summary>
public class MigrationResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int SmtpSettingsMigrated { get; set; }
    public int RecipientGroupsMigrated { get; set; }
    public int EmailTemplatesCreated { get; set; }
}
