using IkeaDocuScan.Shared.DTOs.Countries;
using IkeaDocuScan.Shared.Interfaces;
using System.Net.Http.Json;

namespace IkeaDocuScan_Web.Client.Services;

/// <summary>
/// HTTP client service for Country operations
/// Implements ICountryService interface to call server APIs
/// </summary>
public class CountryHttpService : ICountryService
{
    private readonly HttpClient _http;
    private readonly ILogger<CountryHttpService> _logger;

    public CountryHttpService(HttpClient http, ILogger<CountryHttpService> logger)
    {
        _http = http;
        _logger = logger;
    }

    public async Task<List<CountryDto>> GetAllAsync()
    {
        try
        {
            _logger.LogInformation("Fetching all countries from API");
            var result = await _http.GetFromJsonAsync<List<CountryDto>>("/api/countries");
            return result ?? new List<CountryDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching countries");
            throw;
        }
    }

    public async Task<CountryDto?> GetByCodeAsync(string countryCode)
    {
        try
        {
            _logger.LogInformation("Fetching country {CountryCode} from API", countryCode);
            return await _http.GetFromJsonAsync<CountryDto>($"/api/countries/{countryCode}");
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Country {CountryCode} not found", countryCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching country {CountryCode}", countryCode);
            throw;
        }
    }

    public async Task<CountryDto> CreateAsync(CreateCountryDto dto)
    {
        try
        {
            _logger.LogInformation("Creating country with code {CountryCode}", dto.CountryCode);
            var response = await _http.PostAsJsonAsync("/api/countries", dto);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                var errorMessage = TryExtractErrorMessage(errorContent);
                throw new HttpRequestException(errorMessage);
            }

            var country = await response.Content.ReadFromJsonAsync<CountryDto>();
            return country ?? throw new InvalidOperationException("Failed to deserialize created country");
        }
        catch (HttpRequestException)
        {
            throw; // Re-throw HttpRequestException with our custom message
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating country");
            throw;
        }
    }

    public async Task<CountryDto> UpdateAsync(string countryCode, UpdateCountryDto dto)
    {
        try
        {
            _logger.LogInformation("Updating country with code {CountryCode}", countryCode);
            var response = await _http.PutAsJsonAsync($"/api/countries/{countryCode}", dto);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                var errorMessage = TryExtractErrorMessage(errorContent);
                throw new HttpRequestException(errorMessage);
            }

            var country = await response.Content.ReadFromJsonAsync<CountryDto>();
            return country ?? throw new InvalidOperationException("Failed to deserialize updated country");
        }
        catch (HttpRequestException)
        {
            throw; // Re-throw HttpRequestException with our custom message
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating country with code {CountryCode}", countryCode);
            throw;
        }
    }

    public async Task DeleteAsync(string countryCode)
    {
        try
        {
            _logger.LogInformation("Deleting country with code {CountryCode}", countryCode);
            var response = await _http.DeleteAsync($"/api/countries/{countryCode}");

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                var errorMessage = TryExtractErrorMessage(errorContent);
                throw new HttpRequestException(errorMessage);
            }
        }
        catch (HttpRequestException)
        {
            throw; // Re-throw HttpRequestException with our custom message
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting country with code {CountryCode}", countryCode);
            throw;
        }
    }

    public async Task<bool> IsInUseAsync(string countryCode)
    {
        try
        {
            _logger.LogInformation("Checking if country {CountryCode} is in use", countryCode);
            var response = await _http.GetFromJsonAsync<UsageResponse>($"/api/countries/{countryCode}/usage");
            return response?.IsInUse ?? false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking usage for country {CountryCode}", countryCode);
            throw;
        }
    }

    public async Task<(int counterPartyCount, int userPermissionCount)> GetUsageCountAsync(string countryCode)
    {
        try
        {
            _logger.LogInformation("Getting usage count for country {CountryCode}", countryCode);
            var response = await _http.GetFromJsonAsync<UsageResponse>($"/api/countries/{countryCode}/usage");
            return (response?.CounterPartyCount ?? 0, response?.UserPermissionCount ?? 0);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting usage count for country {CountryCode}", countryCode);
            throw;
        }
    }

    private string TryExtractErrorMessage(string errorContent)
    {
        try
        {
            var options = new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            var errorResponse = System.Text.Json.JsonSerializer.Deserialize<ErrorResponse>(errorContent, options);
            if (errorResponse?.Error != null)
            {
                return errorResponse.Error;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to deserialize error response: {Content}", errorContent);
            // If deserialization fails, return raw content or a default message
        }

        return !string.IsNullOrEmpty(errorContent) ? errorContent : "An error occurred";
    }

    private class UsageResponse
    {
        public string CountryCode { get; set; } = string.Empty;
        public bool IsInUse { get; set; }
        public int CounterPartyCount { get; set; }
        public int UserPermissionCount { get; set; }
        public int TotalUsage { get; set; }
    }

    private class ErrorResponse
    {
        public string? Error { get; set; }
    }
}
