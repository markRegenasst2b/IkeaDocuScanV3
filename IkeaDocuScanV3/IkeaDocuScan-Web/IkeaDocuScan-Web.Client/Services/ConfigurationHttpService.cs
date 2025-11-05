using IkeaDocuScan.Shared.DTOs.Configuration;
using IkeaDocuScan.Shared.Interfaces;
using System.Net.Http.Json;

namespace IkeaDocuScan_Web.Client.Services;

/// <summary>
/// HTTP service for configuration management API calls
/// </summary>
public class ConfigurationHttpService : IConfigurationHttpService
{
    private readonly HttpClient _httpClient;

    public ConfigurationHttpService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    #region Email Recipients

    public async Task<List<EmailRecipientGroupDto>> GetAllEmailRecipientGroupsAsync()
    {
        var response = await _httpClient.GetAsync("/api/configuration/email-recipients");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<EmailRecipientGroupDto>>() ?? new();
    }

    public async Task<string[]> GetEmailRecipientsAsync(string groupKey)
    {
        var response = await _httpClient.GetAsync($"/api/configuration/email-recipients/{groupKey}");

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return Array.Empty<string>();

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<GetEmailRecipientsResponse>();
        return result?.Recipients ?? Array.Empty<string>();
    }

    public async Task SetEmailRecipientsAsync(string groupKey, string[] emailAddresses, string? reason = null)
    {
        var request = new { EmailAddresses = emailAddresses, Reason = reason };
        var response = await _httpClient.PostAsJsonAsync($"/api/configuration/email-recipients/{groupKey}", request);
        response.EnsureSuccessStatusCode();
    }

    #endregion

    #region Email Templates

    public async Task<List<EmailTemplateDto>> GetAllEmailTemplatesAsync()
    {
        var response = await _httpClient.GetAsync("/api/configuration/email-templates");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<EmailTemplateDto>>() ?? new();
    }

    public async Task<EmailTemplateDto?> GetEmailTemplateAsync(string templateKey)
    {
        var response = await _httpClient.GetAsync($"/api/configuration/email-templates/{templateKey}");

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<EmailTemplateDto>();
    }

    public async Task<EmailTemplateDto> CreateEmailTemplateAsync(EmailTemplateDto template)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/configuration/email-templates", template);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<EmailTemplateDto>() ?? template;
    }

    public async Task<EmailTemplateDto> UpdateEmailTemplateAsync(int id, EmailTemplateDto template)
    {
        var response = await _httpClient.PutAsJsonAsync($"/api/configuration/email-templates/{id}", template);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<EmailTemplateDto>() ?? template;
    }

    public async Task DeleteEmailTemplateAsync(int id)
    {
        var response = await _httpClient.DeleteAsync($"/api/configuration/email-templates/{id}");
        response.EnsureSuccessStatusCode();
    }

    public async Task<string> PreviewEmailTemplateAsync(string template, Dictionary<string, object> data, Dictionary<string, List<Dictionary<string, object>>>? loops = null)
    {
        var request = new { Template = template, Data = data, Loops = loops };
        var response = await _httpClient.PostAsJsonAsync("/api/configuration/email-templates/preview", request);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<PreviewTemplateResponse>();
        return result?.Preview ?? string.Empty;
    }

    public async Task<PlaceholderDocumentation> GetPlaceholderDocumentationAsync()
    {
        var response = await _httpClient.GetAsync("/api/configuration/email-templates/placeholders");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<PlaceholderDocumentation>() ?? new();
    }

    #endregion

    #region Configuration

    public async Task<ConfigurationSection[]> GetConfigurationSectionsAsync()
    {
        var response = await _httpClient.GetAsync("/api/configuration/sections");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ConfigurationSection[]>() ?? Array.Empty<ConfigurationSection>();
    }

    public async Task<string?> GetConfigurationAsync(string section, string key)
    {
        var response = await _httpClient.GetAsync($"/api/configuration/{section}/{key}");

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<GetConfigurationResponse>();
        return result?.Value;
    }

    public async Task SetConfigurationAsync(string section, string key, string value, string? reason = null)
    {
        var request = new { Value = value, Reason = reason };
        var response = await _httpClient.PostAsJsonAsync($"/api/configuration/{section}/{key}", request);
        response.EnsureSuccessStatusCode();
    }

    public async Task<bool> TestSmtpConnectionAsync()
    {
        var response = await _httpClient.PostAsync("/api/configuration/test-smtp", null);

        if (!response.IsSuccessStatusCode)
            return false;

        var result = await response.Content.ReadFromJsonAsync<TestSmtpResponse>();
        return result?.Success ?? false;
    }

    public async Task ReloadCacheAsync()
    {
        var response = await _httpClient.PostAsync("/api/configuration/reload", null);
        response.EnsureSuccessStatusCode();
    }

    #endregion

    #region Response Models

    private record GetEmailRecipientsResponse(string GroupKey, string[] Recipients);
    private record GetConfigurationResponse(string Section, string Key, string Value);
    private record TestSmtpResponse(bool Success, string? Message);
    private record PreviewTemplateResponse(string Preview);

    #endregion
}

#region Public Models for API Responses

public record ConfigurationSection(string Section, string Description);

public class PlaceholderDocumentation
{
    public PlaceholderInfo[] Placeholders { get; set; } = Array.Empty<PlaceholderInfo>();
    public LoopInfo[] Loops { get; set; } = Array.Empty<LoopInfo>();
}

public record PlaceholderInfo(string Name, string Description, string Example, string[] Templates);
public record LoopInfo(string Name, string Description, string[] Fields, string[] Templates);

#endregion
