using IkeaDocuScan.Infrastructure.Data;
using IkeaDocuScan.Infrastructure.Entities;
using IkeaDocuScan.Shared.DTOs.CounterParties;
using IkeaDocuScan.Shared.Exceptions;
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

    public async Task<CounterPartyDto> CreateAsync(CreateCounterPartyDto dto)
    {
        _logger.LogInformation("Creating counter party: {Name}", dto.Name);

        await using var context = await _contextFactory.CreateDbContextAsync();

        // Check if counter party with same name already exists
        var exists = await context.CounterParties
            .AnyAsync(cp => cp.Name == dto.Name);

        if (exists)
        {
            throw new ValidationException($"Counter party with name '{dto.Name}' already exists");
        }

        var entity = new CounterParty
        {
            Name = dto.Name,
            CounterPartyNoAlpha = dto.CounterPartyNoAlpha,
            Address = dto.Address,
            City = dto.City,
            Country = dto.Country,
            AffiliatedTo = dto.AffiliatedTo,
            DisplayAtCheckIn = dto.DisplayAtCheckIn,
            Since = dto.Since,
            Comments = dto.Comments
        };

        context.CounterParties.Add(entity);
        await context.SaveChangesAsync();

        ClearCache();
        _logger.LogInformation("Counter party created with ID: {Id}", entity.CounterPartyId);

        return MapToDto(entity);
    }

    public async Task<CounterPartyDto> UpdateAsync(int id, UpdateCounterPartyDto dto)
    {
        _logger.LogInformation("Updating counter party ID: {Id}", id);

        await using var context = await _contextFactory.CreateDbContextAsync();

        var entity = await context.CounterParties
            .FirstOrDefaultAsync(cp => cp.CounterPartyId == id);

        if (entity == null)
        {
            throw new ValidationException($"Counter party with ID {id} not found");
        }

        // Check if another counter party has the same name
        var duplicateName = await context.CounterParties
            .AnyAsync(cp => cp.Name == dto.Name && cp.CounterPartyId != id);

        if (duplicateName)
        {
            throw new ValidationException($"Counter party with name '{dto.Name}' already exists");
        }

        entity.Name = dto.Name;
        entity.CounterPartyNoAlpha = dto.CounterPartyNoAlpha;
        entity.Address = dto.Address;
        entity.City = dto.City;
        entity.Country = dto.Country;
        entity.AffiliatedTo = dto.AffiliatedTo;
        entity.DisplayAtCheckIn = dto.DisplayAtCheckIn;
        entity.Since = dto.Since;
        entity.Comments = dto.Comments;

        await context.SaveChangesAsync();

        ClearCache();
        _logger.LogInformation("Counter party ID {Id} updated successfully", id);

        return MapToDto(entity);
    }

    public async Task DeleteAsync(int id)
    {
        _logger.LogInformation("Deleting counter party ID: {Id}", id);

        await using var context = await _contextFactory.CreateDbContextAsync();

        // Check if counter party is in use
        var documentCount = await context.Documents
            .Where(d => d.CounterPartyId == id)
            .CountAsync();

        var userPermissionCount = await context.UserPermissions
            .Where(up => up.CounterPartyId == id)
            .CountAsync();

        var totalUsage = documentCount + userPermissionCount;

        if (totalUsage > 0)
        {
            var usageDetails = new List<string>();
            if (documentCount > 0)
                usageDetails.Add($"{documentCount} document{(documentCount != 1 ? "s" : "")}");
            if (userPermissionCount > 0)
                usageDetails.Add($"{userPermissionCount} user permission{(userPermissionCount != 1 ? "s" : "")}");

            throw new ValidationException(
                $"Cannot delete counter party. It is currently used by {string.Join(" and ", usageDetails)}.");
        }

        var entity = await context.CounterParties
            .FirstOrDefaultAsync(cp => cp.CounterPartyId == id);

        if (entity == null)
        {
            throw new ValidationException($"Counter party with ID {id} not found");
        }

        context.CounterParties.Remove(entity);
        await context.SaveChangesAsync();

        ClearCache();
        _logger.LogInformation("Counter party ID {Id} deleted successfully", id);
    }

    public async Task<bool> IsInUseAsync(int id)
    {
        _logger.LogInformation("Checking if counter party ID {Id} is in use", id);

        await using var context = await _contextFactory.CreateDbContextAsync();

        var isInUse = await context.Documents.AnyAsync(d => d.CounterPartyId == id) ||
                      await context.UserPermissions.AnyAsync(up => up.CounterPartyId == id);

        return isInUse;
    }

    public async Task<(int documentCount, int userPermissionCount)> GetUsageCountAsync(int id)
    {
        _logger.LogInformation("Getting usage count for counter party ID {Id}", id);

        await using var context = await _contextFactory.CreateDbContextAsync();

        var documentCount = await context.Documents
            .Where(d => d.CounterPartyId == id)
            .CountAsync();

        var userPermissionCount = await context.UserPermissions
            .Where(up => up.CounterPartyId == id)
            .CountAsync();

        return (documentCount, userPermissionCount);
    }

    /// <summary>
    /// Clear the counter parties cache. Call this when counter parties are added, updated, or deleted.
    /// </summary>
    public void ClearCache()
    {
        _cache.Remove(CacheKey);
        _logger.LogInformation("Counter parties cache cleared");
    }

    private static CounterPartyDto MapToDto(CounterParty entity)
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
