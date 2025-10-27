using IkeaDocuScan.Infrastructure.Data;
using IkeaDocuScan.Shared.DTOs.CounterParties;
using IkeaDocuScan.Shared.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace IkeaDocuScan_Web.Services;

/// <summary>
/// Service for CounterParty operations with caching
/// </summary>
public class CounterPartyService : ICounterPartyService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    private readonly ILogger<CounterPartyService> _logger;
    private readonly IMemoryCache _cache;

    private const string CacheKey = "CounterParties_All";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(1);

    public CounterPartyService(
        IDbContextFactory<AppDbContext> contextFactory,
        ILogger<CounterPartyService> logger,
        IMemoryCache cache)
    {
        _contextFactory = contextFactory;
        _logger = logger;
        _cache = cache;
    }

    public async Task<List<CounterPartyDto>> SearchAsync(string searchTerm)
    {
        _logger.LogInformation("Searching counter parties with term: {SearchTerm}", searchTerm);

        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return await GetAllAsync();
        }

        var term = searchTerm.ToLower().Trim();

        await using var context = await _contextFactory.CreateDbContextAsync();
        var counterParties = await context.CounterParties
            .AsNoTracking()
            .Where(cp =>
                (cp.Name != null && cp.Name.ToLower().Contains(term)) ||
                (cp.CounterPartyNoAlpha != null && cp.CounterPartyNoAlpha.ToLower().Contains(term)) ||
                cp.City.ToLower().Contains(term) ||
                cp.Country.ToLower().Contains(term))
            .OrderBy(cp => cp.Name)
            .Take(100) // Limit results to prevent huge result sets
            .ToListAsync();

        _logger.LogInformation("Found {Count} counter parties matching search term", counterParties.Count);

        return counterParties.Select(MapToDto).ToList();
    }

    public async Task<List<CounterPartyDto>> GetAllAsync()
    {
        return await _cache.GetOrCreateAsync(CacheKey, async entry =>
        {
            _logger.LogInformation("Fetching all counter parties from database (cache miss)");

            entry.AbsoluteExpirationRelativeToNow = CacheDuration;
            entry.SetPriority(CacheItemPriority.High);

            await using var context = await _contextFactory.CreateDbContextAsync();
            var counterParties = await context.CounterParties
                .AsNoTracking()
                .OrderBy(cp => cp.Name)
                .ToListAsync();

            var result = counterParties.Select(MapToDto).ToList();
            _logger.LogInformation("Cached {Count} counter parties for {Duration}", result.Count, CacheDuration);

            return result;
        }) ?? new List<CounterPartyDto>();
    }

    public async Task<CounterPartyDto?> GetByIdAsync(int id)
    {
        _logger.LogInformation("Fetching counter party with ID: {Id}", id);

        await using var context = await _contextFactory.CreateDbContextAsync();
        var counterParty = await context.CounterParties
            .AsNoTracking()
            .FirstOrDefaultAsync(cp => cp.CounterPartyId == id);

        if (counterParty == null)
        {
            _logger.LogWarning("Counter party with ID {Id} not found", id);
            return null;
        }

        return MapToDto(counterParty);
    }

    /// <summary>
    /// Clear the counter parties cache. Call this when counter parties are added, updated, or deleted.
    /// </summary>
    public void ClearCache()
    {
        _cache.Remove(CacheKey);
        _logger.LogInformation("Counter parties cache cleared");
    }

    private static CounterPartyDto MapToDto(IkeaDocuScan.Infrastructure.Entities.CounterParty entity)
    {
        return new CounterPartyDto
        {
            CounterPartyId = entity.CounterPartyId,
            Name = entity.Name,
            CounterPartyNoAlpha = entity.CounterPartyNoAlpha,
            Address = entity.Address,
            City = entity.City,
            Country = entity.Country,
            AffiliatedTo = entity.AffiliatedTo,
            DisplayAtCheckIn = entity.DisplayAtCheckIn,
            Since = entity.Since,
            Comments = entity.Comments
        };
    }
}
