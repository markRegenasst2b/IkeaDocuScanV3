using IkeaDocuScan.Shared.DTOs.UserPermissions;
using System.Net.Http.Json;

namespace IkeaDocuScan_Web.Client.Services;

/// <summary>
/// HTTP client service for user identity operations
/// </summary>
public class UserIdentityHttpService
{
    private readonly HttpClient _http;
    private readonly ILogger<UserIdentityHttpService> _logger;

    public UserIdentityHttpService(HttpClient http, ILogger<UserIdentityHttpService> logger)
    {
        _http = http;
        _logger = logger;
    }

    public async Task<UserIdentityDto?> GetUserIdentityAsync()
    {
        try
        {
            _logger.LogInformation("Fetching user identity from API");
            return await _http.GetFromJsonAsync<UserIdentityDto>("/api/user/identity");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching user identity");
            throw;
        }
    }
}
