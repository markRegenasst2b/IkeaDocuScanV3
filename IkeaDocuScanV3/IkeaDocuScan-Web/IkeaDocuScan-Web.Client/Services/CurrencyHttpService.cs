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

    /// <summary>
    /// Create a new currency
    /// </summary>
    public async Task<CurrencyDto> CreateAsync(CreateCurrencyDto dto)
    {
        try
        {
            _logger.LogInformation("Creating currency with code {CurrencyCode}", dto.CurrencyCode);
            var response = await _http.PostAsJsonAsync("/api/currencies", dto);
            response.EnsureSuccessStatusCode();
            var currency = await response.Content.ReadFromJsonAsync<CurrencyDto>();
            return currency ?? throw new InvalidOperationException("Failed to deserialize created currency");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating currency");
            throw;
        }
    }

    /// <summary>
    /// Update an existing currency
    /// </summary>
    public async Task<CurrencyDto> UpdateAsync(string currencyCode, UpdateCurrencyDto dto)
    {
        try
        {
            _logger.LogInformation("Updating currency with code {CurrencyCode}", currencyCode);
            var response = await _http.PutAsJsonAsync($"/api/currencies/{currencyCode}", dto);
            response.EnsureSuccessStatusCode();
            var currency = await response.Content.ReadFromJsonAsync<CurrencyDto>();
            return currency ?? throw new InvalidOperationException("Failed to deserialize updated currency");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating currency with code {CurrencyCode}", currencyCode);
            throw;
        }
    }

    /// <summary>
    /// Delete a currency by code
    /// </summary>
    public async Task DeleteAsync(string currencyCode)
    {
        try
        {
            _logger.LogInformation("Deleting currency with code {CurrencyCode}", currencyCode);
            var response = await _http.DeleteAsync($"/api/currencies/{currencyCode}");
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting currency with code {CurrencyCode}", currencyCode);
            throw;
        }
    }

    /// <summary>
    /// Check if a currency is in use by any documents
    /// </summary>
    public async Task<bool> IsInUseAsync(string currencyCode)
    {
        try
        {
            _logger.LogInformation("Checking if currency {CurrencyCode} is in use", currencyCode);
            var response = await _http.GetFromJsonAsync<UsageResponse>($"/api/currencies/{currencyCode}/usage");
            return response?.IsInUse ?? false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking usage for currency {CurrencyCode}", currencyCode);
            throw;
        }
    }

    /// <summary>
    /// Get count of documents using this currency
    /// </summary>
    public async Task<int> GetUsageCountAsync(string currencyCode)
    {
        try
        {
            _logger.LogInformation("Getting usage count for currency {CurrencyCode}", currencyCode);
            var response = await _http.GetFromJsonAsync<UsageResponse>($"/api/currencies/{currencyCode}/usage");
            return response?.UsageCount ?? 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting usage count for currency {CurrencyCode}", currencyCode);
            throw;
        }
    }

    private class UsageResponse
    {
        public string CurrencyCode { get; set; } = string.Empty;
        public bool IsInUse { get; set; }
        public int UsageCount { get; set; }
    }
}
