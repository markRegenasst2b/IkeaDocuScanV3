using System.Net.Http.Json;
using IkeaDocuScan.Shared.DTOs.Currencies;
using IkeaDocuScan.Shared.Interfaces;

namespace IkeaDocuScan_Web.Client.Services;

/// <summary>
/// Client-side HTTP service for currencies
/// Includes client-side in-memory caching for performance
/// </summary>
public class CurrencyHttpService : ICurrencyService
{
    private readonly HttpClient _http;
    private readonly ILogger<CurrencyHttpService> _logger;

    // Client-side cache
    private static List<CurrencyDto>? _cachedCurrencies;
    private static DateTime? _cacheExpiration;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);
    private static readonly SemaphoreSlim _cacheLock = new(1, 1);

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
        // Check cache first
        if (_cachedCurrencies != null && _cacheExpiration.HasValue && DateTime.Now < _cacheExpiration.Value)
        {
            _logger.LogInformation("⚡ Returning {Count} currencies from client-side cache", _cachedCurrencies.Count);
            return _cachedCurrencies;
        }

        // Cache miss or expired - fetch from server
        await _cacheLock.WaitAsync();
        try
        {
            // Double-check after acquiring lock (another thread might have updated cache)
            if (_cachedCurrencies != null && _cacheExpiration.HasValue && DateTime.Now < _cacheExpiration.Value)
            {
                _logger.LogInformation("⚡ Returning {Count} currencies from client-side cache (after lock)", _cachedCurrencies.Count);
                return _cachedCurrencies;
            }

            _logger.LogInformation("🌐 Fetching all currencies from API (client cache miss)");
            var currencies = await _http.GetFromJsonAsync<List<CurrencyDto>>("/api/currencies");

            if (currencies != null)
            {
                _cachedCurrencies = currencies;
                _cacheExpiration = DateTime.Now.Add(CacheDuration);
                _logger.LogInformation("💾 Cached {Count} currencies on client for {Duration}", currencies.Count, CacheDuration);
            }

            return currencies ?? new List<CurrencyDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching currencies");
            throw;
        }
        finally
        {
            _cacheLock.Release();
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

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                var errorMessage = TryExtractErrorMessage(errorContent);
                throw new HttpRequestException(errorMessage);
            }

            var currency = await response.Content.ReadFromJsonAsync<CurrencyDto>();

            // Invalidate client cache after create
            ClearCache();

            return currency ?? throw new InvalidOperationException("Failed to deserialize created currency");
        }
        catch (HttpRequestException)
        {
            throw; // Re-throw HttpRequestException with our custom message
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

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                var errorMessage = TryExtractErrorMessage(errorContent);
                throw new HttpRequestException(errorMessage);
            }

            var currency = await response.Content.ReadFromJsonAsync<CurrencyDto>();

            // Invalidate client cache after update
            ClearCache();

            return currency ?? throw new InvalidOperationException("Failed to deserialize updated currency");
        }
        catch (HttpRequestException)
        {
            throw; // Re-throw HttpRequestException with our custom message
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

            // Invalidate client cache after delete
            ClearCache();

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

    /// <summary>
    /// Clears the client-side cache. Call this when currencies are modified.
    /// </summary>
    public void ClearCache()
    {
        _cachedCurrencies = null;
        _cacheExpiration = null;
        _logger.LogInformation("Client-side currencies cache cleared");
    }

    private class UsageResponse
    {
        public string CurrencyCode { get; set; } = string.Empty;
        public bool IsInUse { get; set; }
        public int UsageCount { get; set; }
    }

    private class ErrorResponse
    {
        public string? Error { get; set; }
    }
}
