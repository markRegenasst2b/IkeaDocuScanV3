using IkeaDocuScan.Shared.DTOs.CounterParties;

namespace IkeaDocuScan.Shared.Interfaces;

/// <summary>
/// Service interface for CounterParty operations
/// </summary>
public interface ICounterPartyService
{
    /// <summary>
    /// Search for counter parties by name, number, city, or country
    /// </summary>
    Task<List<CounterPartyDto>> SearchAsync(string searchTerm);

    /// <summary>
    /// Get all counter parties
    /// </summary>
    Task<List<CounterPartyDto>> GetAllAsync();

    /// <summary>
    /// Get a counter party by ID
    /// </summary>
    Task<CounterPartyDto?> GetByIdAsync(int id);

    /// <summary>
    /// Create a new counter party
    /// </summary>
    Task<CounterPartyDto> CreateAsync(CreateCounterPartyDto dto);

    /// <summary>
    /// Update an existing counter party
    /// </summary>
    Task<CounterPartyDto> UpdateAsync(int id, UpdateCounterPartyDto dto);

    /// <summary>
    /// Delete a counter party by ID
    /// </summary>
    Task DeleteAsync(int id);

    /// <summary>
    /// Check if a counter party is in use
    /// </summary>
    Task<bool> IsInUseAsync(int id);

    /// <summary>
    /// Get usage count for a counter party (documents, user permissions)
    /// </summary>
    Task<(int documentCount, int userPermissionCount)> GetUsageCountAsync(int id);
}
