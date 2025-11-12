using IkeaDocuScan.Shared.DTOs.Testing;
using System.Net.Http.Json;

namespace IkeaDocuScan_Web.Client.Services;

/// <summary>
/// HTTP client service for test identity operations (DEVELOPMENT ONLY)
/// ⚠️ WARNING: THIS SERVICE ONLY EXISTS IN DEBUG MODE ⚠️
/// </summary>
public class TestIdentityHttpService
{
#if DEBUG
    private readonly HttpClient _http;
    private readonly ILogger<TestIdentityHttpService> _logger;

    public TestIdentityHttpService(HttpClient http, ILogger<TestIdentityHttpService> logger)
    {
        _http = http;
        _logger = logger;
    }

    public async Task<List<TestIdentityProfile>> GetProfilesAsync()
    {
        try
        {
            _logger.LogInformation("Fetching test identity profiles from API");
            var result = await _http.GetFromJsonAsync<List<TestIdentityProfile>>("/api/test-identity/profiles");
            return result ?? new List<TestIdentityProfile>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching test identity profiles");
            throw;
        }
    }

    public async Task<TestIdentityStatus> GetStatusAsync()
    {
        try
        {
            _logger.LogInformation("Fetching test identity status from API");
            var result = await _http.GetFromJsonAsync<TestIdentityStatus>("/api/test-identity/status");
            return result ?? new TestIdentityStatus { IsActive = false };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching test identity status");
            throw;
        }
    }

    public async Task<bool> ActivateProfileAsync(string profileId)
    {
        try
        {
            _logger.LogWarning("⚠️ Activating test identity profile: {ProfileId}", profileId);
            var response = await _http.PostAsync($"/api/test-identity/activate/{profileId}", null);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating test identity profile: {ProfileId}", profileId);
            throw;
        }
    }

    public async Task<bool> ResetToRealIdentityAsync()
    {
        try
        {
            _logger.LogWarning("⚠️ Resetting to real Windows identity");
            var response = await _http.PostAsync("/api/test-identity/reset", null);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting test identity");
            throw;
        }
    }
#endif
}
