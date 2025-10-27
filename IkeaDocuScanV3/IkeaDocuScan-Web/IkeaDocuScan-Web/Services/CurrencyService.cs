using IkeaDocuScan.Infrastructure.Data;
using IkeaDocuScan.Infrastructure.Entities;
using IkeaDocuScan.Shared.DTOs.Currencies;
using IkeaDocuScan.Shared.Exceptions;
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
    /// Create a new currency
    /// </summary>
    public async Task<CurrencyDto> CreateAsync(CreateCurrencyDto dto)
    {
        _logger.LogInformation("Creating new currency with code {CurrencyCode}", dto.CurrencyCode);

        await using var context = await _contextFactory.CreateDbContextAsync();

        // Check if currency code already exists
        var exists = await context.Currencies.AnyAsync(c => c.CurrencyCode == dto.CurrencyCode);
        if (exists)
        {
            throw new ValidationException($"Currency with code '{dto.CurrencyCode}' already exists");
        }

        var currency = new Currency
        {
            CurrencyCode = dto.CurrencyCode.ToUpperInvariant(),
            Name = dto.Name,
            DecimalPlaces = dto.DecimalPlaces
        };

        context.Currencies.Add(currency);
        await context.SaveChangesAsync();

        ClearCache();

        _logger.LogInformation("Created currency {CurrencyCode}", currency.CurrencyCode);

        return new CurrencyDto
        {
            CurrencyCode = currency.CurrencyCode,
            Name = currency.Name ?? string.Empty,
            DecimalPlaces = currency.DecimalPlaces
        };
    }

    /// <summary>
    /// Update an existing currency
    /// </summary>
    public async Task<CurrencyDto> UpdateAsync(string currencyCode, UpdateCurrencyDto dto)
    {
        _logger.LogInformation("Updating currency with code {CurrencyCode}", currencyCode);

        await using var context = await _contextFactory.CreateDbContextAsync();

        var currency = await context.Currencies.FindAsync(currencyCode);
        if (currency == null)
        {
            throw new ValidationException($"Currency with code '{currencyCode}' not found");
        }

        currency.Name = dto.Name;
        currency.DecimalPlaces = dto.DecimalPlaces;

        await context.SaveChangesAsync();

        ClearCache();

        _logger.LogInformation("Updated currency {CurrencyCode}", currencyCode);

        return new CurrencyDto
        {
            CurrencyCode = currency.CurrencyCode,
            Name = currency.Name ?? string.Empty,
            DecimalPlaces = currency.DecimalPlaces
        };
    }

    /// <summary>
    /// Delete a currency by code
    /// </summary>
    public async Task DeleteAsync(string currencyCode)
    {
        _logger.LogInformation("Deleting currency with code {CurrencyCode}", currencyCode);

        await using var context = await _contextFactory.CreateDbContextAsync();

        var currency = await context.Currencies.FindAsync(currencyCode);
        if (currency == null)
        {
            throw new ValidationException($"Currency with code '{currencyCode}' not found");
        }

        // Check if currency is in use
        var usageCount = await context.Documents
            .Where(d => d.CurrencyCode == currencyCode)
            .CountAsync();

        if (usageCount > 0)
        {
            throw new ValidationException(
                $"Cannot delete currency '{currencyCode}'. It is currently used by {usageCount} document(s). " +
                "Please remove or update all documents using this currency before deleting it.");
        }

        context.Currencies.Remove(currency);
        await context.SaveChangesAsync();

        ClearCache();

        _logger.LogInformation("Deleted currency {CurrencyCode}", currencyCode);
    }

    /// <summary>
    /// Check if a currency is in use by any documents
    /// </summary>
    public async Task<bool> IsInUseAsync(string currencyCode)
    {
        _logger.LogInformation("Checking if currency {CurrencyCode} is in use", currencyCode);

        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Documents.AnyAsync(d => d.CurrencyCode == currencyCode);
    }

    /// <summary>
    /// Get count of documents using this currency
    /// </summary>
    public async Task<int> GetUsageCountAsync(string currencyCode)
    {
        _logger.LogInformation("Getting usage count for currency {CurrencyCode}", currencyCode);

        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Documents
            .Where(d => d.CurrencyCode == currencyCode)
            .CountAsync();
    }

    /// <summary>
    /// Clear the currencies cache. Call this when currencies are added, updated, or deleted.
    /// </summary>
    private void ClearCache()
    {
        _cache.Remove(CacheKey);
        _logger.LogInformation("Currencies cache cleared");
    }
}
