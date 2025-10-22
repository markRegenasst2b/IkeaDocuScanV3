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
}
