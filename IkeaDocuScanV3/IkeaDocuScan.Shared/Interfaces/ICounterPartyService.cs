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
}
