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
}
