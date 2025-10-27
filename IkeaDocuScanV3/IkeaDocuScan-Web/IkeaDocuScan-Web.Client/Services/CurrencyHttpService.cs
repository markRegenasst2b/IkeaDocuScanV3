using System.Net.Http.Json;
using IkeaDocuScan.Shared.DTOs.Currencies;
using IkeaDocuScan.Shared.Interfaces;

namespace IkeaDocuScan_Web.Client.Services;

/// <summary>
/// Client-side HTTP service for currencies
/// </summary>
public class CurrencyHttpService : ICurrencyService
{
    private readonly HttpClient _http;
    private readonly ILogger<CurrencyHttpService> _logger;

    public CurrencyHttpService(HttpClient http, ILogger<CurrencyHttpService> logger)
    {
        _http = http;
        _logger = logger;
    }

    /// <summary>
    /// Get all currencies
    /// </summary>
    public async Task<List<CurrencyDto>> GetAllAsync()
    {
        try
        {
            _logger.LogInformation("Fetching all currencies from API");
            var currencies = await _http.GetFromJsonAsync<List<CurrencyDto>>("/api/currencies");
            return currencies ?? new List<CurrencyDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching currencies");
            throw;
        }
    }

    /// <summary>
    /// Get a specific currency by code
    /// </summary>
    public async Task<CurrencyDto?> GetByCodeAsync(string currencyCode)
    {
        try
        {
            _logger.LogInformation("Fetching currency with code {CurrencyCode}", currencyCode);
            return await _http.GetFromJsonAsync<CurrencyDto>($"/api/currencies/{currencyCode}");
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Currency with code {CurrencyCode} not found", currencyCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching currency with code {CurrencyCode}", currencyCode);
            throw;
        }
    }
}
