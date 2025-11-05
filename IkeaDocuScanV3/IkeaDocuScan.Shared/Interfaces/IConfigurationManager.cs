using IkeaDocuScan.Shared.DTOs.Configuration;

namespace IkeaDocuScan.Shared.Interfaces;

/// <summary>
/// Hybrid configuration manager that reads from database first, falls back to appsettings
/// Provides caching and automatic rollback on errors
/// </summary>
public interface ISystemConfigurationManager
{
    /// <summary>
    /// Get configuration value with database override support
    /// </summary>
    Task<T?> GetConfigurationAsync<T>(string configKey, string section, T? defaultValue = default)
        where T : class;

    /// <summary>
    /// Get configuration value synchronously (uses cached values)
    /// </summary>
    T? GetConfiguration<T>(string configKey, string section, T? defaultValue = default)
        where T : class;

    /// <summary>
    /// Set configuration value in database with automatic rollback on errors
    /// </summary>
    Task SetConfigurationAsync<T>(string configKey, string section, T value, string changedBy, string? reason = null)
        where T : class;

    /// <summary>
    /// Get email recipient group
    /// </summary>
    Task<string[]> GetEmailRecipientsAsync(string groupKey);

    /// <summary>
    /// Set email recipient group with automatic rollback on errors
    /// </summary>
    Task SetEmailRecipientsAsync(string groupKey, string[] emailAddresses, string changedBy, string? reason = null);

    /// <summary>
    /// Get email template by key
    /// </summary>
    Task<EmailTemplateDto?> GetEmailTemplateAsync(string templateKey);

    /// <summary>
    /// Create or update email template with automatic rollback on errors
    /// </summary>
    Task<EmailTemplateDto> SaveEmailTemplateAsync(EmailTemplateDto template, string changedBy);

    /// <summary>
    /// Get all email templates
    /// </summary>
    Task<List<EmailTemplateDto>> GetAllEmailTemplatesAsync();

    /// <summary>
    /// Get all email recipient groups
    /// </summary>
    Task<List<EmailRecipientGroupDto>> GetAllEmailRecipientGroupsAsync();

    /// <summary>
    /// Reload configuration cache
    /// </summary>
    Task ReloadAsync();

    /// <summary>
    /// Test SMTP configuration
    /// </summary>
    Task<bool> TestSmtpConnectionAsync();

    /// <summary>
    /// Update all SMTP settings atomically with automatic testing and rollback
    /// All settings are saved in a transaction and tested together before commit
    /// </summary>
    /// <param name="config">SMTP configuration settings</param>
    /// <param name="changedBy">User making the change</param>
    /// <param name="reason">Optional reason for the change</param>
    /// <param name="skipTest">If true, skips SMTP testing and saves immediately (use with caution)</param>
    Task SetSmtpConfigurationAsync(SmtpConfigurationDto config, string changedBy, string? reason = null, bool skipTest = false);
}
