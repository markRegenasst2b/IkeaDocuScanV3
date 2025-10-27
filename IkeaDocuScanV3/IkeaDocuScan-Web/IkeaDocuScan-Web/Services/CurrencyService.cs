using IkeaDocuScan.Infrastructure.Data;
using IkeaDocuScan.Shared.DTOs.Currencies;
using IkeaDocuScan.Shared.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace IkeaDocuScan_Web.Services;

/// <summary>
/// Service for managing currencies with caching
/// </summary>
public class CurrencyService : ICurrencyService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    private readonly ILogger<CurrencyService> _logger;
    private readonly IMemoryCache _cache;

    private const string CacheKey = "Currencies_All";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(1);

    public CurrencyService(
        IDbContextFactory<AppDbContext> contextFactory,
        ILogger<CurrencyService> logger,
        IMemoryCache cache)
    {
        _contextFactory = contextFactory;
        _logger = logger;
        _cache = cache;
    }

    /// <summary>
    /// Get all currencies
    /// </summary>
    public async Task<List<CurrencyDto>> GetAllAsync()
    {
        return await _cache.GetOrCreateAsync(CacheKey, async entry =>
        {
            _logger.LogInformation("Retrieving all currencies from database (cache miss)");

            entry.AbsoluteExpirationRelativeToNow = CacheDuration;
            entry.SetPriority(CacheItemPriority.High);

            await using var context = await _contextFactory.CreateDbContextAsync();
            var currencies = await context.Currencies
                .AsNoTracking()
                .OrderBy(c => c.CurrencyCode)
                .Select(c => new CurrencyDto
                {
                    CurrencyCode = c.CurrencyCode,
                    Name = c.Name ?? string.Empty,
                    DecimalPlaces = c.DecimalPlaces
                })
                .ToListAsync();

            _logger.LogInformation("Cached {Count} currencies for {Duration}", currencies.Count, CacheDuration);
            return currencies;
        }) ?? new List<CurrencyDto>();
    }

    /// <summary>
    /// Get a specific currency by code
    /// </summary>
    public async Task<CurrencyDto?> GetByCodeAsync(string currencyCode)
    {
        _logger.LogInformation("Retrieving currency with code {CurrencyCode}", currencyCode);

        await using var context = await _contextFactory.CreateDbContextAsync();
        var currency = await context.Currencies
            .AsNoTracking()
            .Where(c => c.CurrencyCode == currencyCode)
            .Select(c => new CurrencyDto
            {
                CurrencyCode = c.CurrencyCode,
                Name = c.Name ?? string.Empty,
                DecimalPlaces = c.DecimalPlaces
            })
            .FirstOrDefaultAsync();

        if (currency == null)
        {
            _logger.LogWarning("Currency with code {CurrencyCode} not found", currencyCode);
        }

        return currency;
    }

    /// <summary>
    /// Clear the currencies cache. Call this when currencies are added, updated, or deleted.
    /// </summary>
    public void ClearCache()
    {
        _cache.Remove(CacheKey);
        _logger.LogInformation("Currencies cache cleared");
    }
}
