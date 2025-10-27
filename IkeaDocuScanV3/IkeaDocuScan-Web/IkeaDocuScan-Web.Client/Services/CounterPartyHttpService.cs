using IkeaDocuScan.Shared.DTOs.CounterParties;
using IkeaDocuScan.Shared.Interfaces;
using System.Net.Http.Json;

namespace IkeaDocuScan_Web.Client.Services;

/// <summary>
/// HTTP client service for CounterParty operations
/// Implements ICounterPartyService interface to call server APIs
/// </summary>
public class CounterPartyHttpService : ICounterPartyService
{
    private readonly HttpClient _http;
    private readonly ILogger<CounterPartyHttpService> _logger;

    public CounterPartyHttpService(HttpClient http, ILogger<CounterPartyHttpService> logger)
    {
        _http = http;
        _logger = logger;
    }

    public async Task<List<CounterPartyDto>> SearchAsync(string searchTerm)
    {
        try
        {
            _logger.LogInformation("Searching counter parties with term: {SearchTerm}", searchTerm);
            var result = await _http.GetFromJsonAsync<List<CounterPartyDto>>(
                $"/api/counterparties/search?searchTerm={Uri.EscapeDataString(searchTerm ?? string.Empty)}");
            return result ?? new List<CounterPartyDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching counter parties");
            throw;
        }
    }

    public async Task<List<CounterPartyDto>> GetAllAsync()
    {
        try
        {
            _logger.LogInformation("Fetching all counter parties from API");
            var result = await _http.GetFromJsonAsync<List<CounterPartyDto>>("/api/counterparties");
            return result ?? new List<CounterPartyDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching counter parties");
            throw;
        }
    }

    public async Task<CounterPartyDto?> GetByIdAsync(int id)
    {
        try
        {
            _logger.LogInformation("Fetching counter party {CounterPartyId} from API", id);
            return await _http.GetFromJsonAsync<CounterPartyDto>($"/api/counterparties/{id}");
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Counter party {CounterPartyId} not found", id);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching counter party {CounterPartyId}", id);
            throw;
        }
    }

    public async Task<CounterPartyDto> CreateAsync(CreateCounterPartyDto dto)
    {
        try
        {
            _logger.LogInformation("Creating counter party: {Name}", dto.Name);
            var response = await _http.PostAsJsonAsync("/api/counterparties", dto);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                var errorMessage = TryExtractErrorMessage(errorContent);
                throw new HttpRequestException(errorMessage);
            }

            var counterParty = await response.Content.ReadFromJsonAsync<CounterPartyDto>();
            return counterParty ?? throw new InvalidOperationException("Failed to deserialize created counter party");
        }
        catch (HttpRequestException)
        {
            throw; // Re-throw HttpRequestException with our custom message
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating counter party");
            throw;
        }
    }

    public async Task<CounterPartyDto> UpdateAsync(int id, UpdateCounterPartyDto dto)
    {
        try
        {
            _logger.LogInformation("Updating counter party ID: {Id}", id);
            var response = await _http.PutAsJsonAsync($"/api/counterparties/{id}", dto);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                var errorMessage = TryExtractErrorMessage(errorContent);
                throw new HttpRequestException(errorMessage);
            }

            var counterParty = await response.Content.ReadFromJsonAsync<CounterPartyDto>();
            return counterParty ?? throw new InvalidOperationException("Failed to deserialize updated counter party");
        }
        catch (HttpRequestException)
        {
            throw; // Re-throw HttpRequestException with our custom message
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating counter party ID: {Id}", id);
            throw;
        }
    }

    public async Task DeleteAsync(int id)
    {
        try
        {
            _logger.LogInformation("Deleting counter party ID: {Id}", id);
            var response = await _http.DeleteAsync($"/api/counterparties/{id}");

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
            _logger.LogError(ex, "Error deleting counter party ID: {Id}", id);
            throw;
        }
    }

    public async Task<bool> IsInUseAsync(int id)
    {
        try
        {
            _logger.LogInformation("Checking if counter party ID {Id} is in use", id);
            var response = await _http.GetFromJsonAsync<UsageResponse>($"/api/counterparties/{id}/usage");
            return response?.IsInUse ?? false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking usage for counter party ID {Id}", id);
            throw;
        }
    }

    public async Task<(int documentCount, int userPermissionCount)> GetUsageCountAsync(int id)
    {
        try
        {
            _logger.LogInformation("Getting usage count for counter party ID {Id}", id);
            var response = await _http.GetFromJsonAsync<UsageResponse>($"/api/counterparties/{id}/usage");
            return (response?.DocumentCount ?? 0, response?.UserPermissionCount ?? 0);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting usage count for counter party ID {Id}", id);
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
        }

        return !string.IsNullOrEmpty(errorContent) ? errorContent : "An error occurred";
    }

    private class UsageResponse
    {
        public int CounterPartyId { get; set; }
        public bool IsInUse { get; set; }
        public int DocumentCount { get; set; }
        public int UserPermissionCount { get; set; }
        public int TotalUsage { get; set; }
    }

    private class ErrorResponse
    {
        public string? Error { get; set; }
    }
}
