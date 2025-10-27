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
}
