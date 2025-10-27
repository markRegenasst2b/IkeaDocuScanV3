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

    /// <summary>
    /// Create a new country
    /// </summary>
    Task<CountryDto> CreateAsync(CreateCountryDto dto);

    /// <summary>
    /// Update an existing country
    /// </summary>
    Task<CountryDto> UpdateAsync(string countryCode, UpdateCountryDto dto);

    /// <summary>
    /// Delete a country by code
    /// </summary>
    Task DeleteAsync(string countryCode);

    /// <summary>
    /// Check if a country is in use by counter parties or user permissions
    /// </summary>
    Task<bool> IsInUseAsync(string countryCode);

    /// <summary>
    /// Get usage count (counter parties + user permissions)
    /// </summary>
    Task<(int counterPartyCount, int userPermissionCount)> GetUsageCountAsync(string countryCode);
}
