using IkeaDocuScan.Shared.DTOs.Configuration;

namespace IkeaDocuScan.Shared.Interfaces;

/// <summary>
/// HTTP service interface for configuration management
/// Used by Blazor WebAssembly client to call configuration APIs
/// </summary>
public interface IConfigurationHttpService
{
    // Email Recipients
    Task<List<EmailRecipientGroupDto>> GetAllEmailRecipientGroupsAsync();
    Task<string[]> GetEmailRecipientsAsync(string groupKey);
    Task SetEmailRecipientsAsync(string groupKey, string[] emailAddresses, string? reason = null);

    // Email Templates
    Task<List<EmailTemplateDto>> GetAllEmailTemplatesAsync();
    Task<EmailTemplateDto?> GetEmailTemplateAsync(string templateKey);
    Task<EmailTemplateDto> CreateEmailTemplateAsync(EmailTemplateDto template);
    Task<EmailTemplateDto> UpdateEmailTemplateAsync(int id, EmailTemplateDto template);
    Task DeleteEmailTemplateAsync(int id);

    // Configuration
    Task<string?> GetConfigurationAsync(string section, string key);
    Task SetConfigurationAsync(string section, string key, string value, string? reason = null);

    // Testing & Management
    Task<bool> TestSmtpConnectionAsync();
    Task ReloadCacheAsync();
}
