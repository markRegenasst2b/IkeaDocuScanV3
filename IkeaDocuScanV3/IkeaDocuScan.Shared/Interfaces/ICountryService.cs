using IkeaDocuScan.Shared.DTOs.Countries;

namespace IkeaDocuScan.Shared.Interfaces;

/// <summary>
/// Service interface for Country operations
/// </summary>
public interface ICountryService
{
    /// <summary>
    /// Get all countries
    /// </summary>
    Task<List<CountryDto>> GetAllAsync();

    /// <summary>
    /// Get a country by code
    /// </summary>
    Task<CountryDto?> GetByCodeAsync(string countryCode);
}
