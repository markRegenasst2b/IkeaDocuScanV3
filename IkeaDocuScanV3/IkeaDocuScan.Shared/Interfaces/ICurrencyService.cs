using IkeaDocuScan.Shared.DTOs.Currencies;

namespace IkeaDocuScan.Shared.Interfaces;

/// <summary>
/// Service for managing currencies
/// </summary>
public interface ICurrencyService
{
    /// <summary>
    /// Get all currencies
    /// </summary>
    Task<List<CurrencyDto>> GetAllAsync();

    /// <summary>
    /// Get a specific currency by code
    /// </summary>
    Task<CurrencyDto?> GetByCodeAsync(string currencyCode);

    /// <summary>
    /// Create a new currency
    /// </summary>
    Task<CurrencyDto> CreateAsync(CreateCurrencyDto dto);

    /// <summary>
    /// Update an existing currency
    /// </summary>
    Task<CurrencyDto> UpdateAsync(string currencyCode, UpdateCurrencyDto dto);

    /// <summary>
    /// Delete a currency by code
    /// </summary>
    Task DeleteAsync(string currencyCode);

    /// <summary>
    /// Check if a currency is in use by any documents
    /// </summary>
    Task<bool> IsInUseAsync(string currencyCode);

    /// <summary>
    /// Get count of documents using this currency
    /// </summary>
    Task<int> GetUsageCountAsync(string currencyCode);
}
