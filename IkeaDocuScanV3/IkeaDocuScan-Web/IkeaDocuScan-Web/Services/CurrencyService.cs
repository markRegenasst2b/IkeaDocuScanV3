using IkeaDocuScan.Infrastructure.Data;
using IkeaDocuScan.Shared.DTOs.Currencies;
using IkeaDocuScan.Shared.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IkeaDocuScan_Web.Services;

/// <summary>
/// Service for managing currencies
/// </summary>
public class CurrencyService : ICurrencyService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    private readonly ILogger<CurrencyService> _logger;

    public CurrencyService(
        IDbContextFactory<AppDbContext> contextFactory,
        ILogger<CurrencyService> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }

    /// <summary>
    /// Get all currencies
    /// </summary>
    public async Task<List<CurrencyDto>> GetAllAsync()
    {
        _logger.LogInformation("Retrieving all currencies");

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

        _logger.LogInformation("Retrieved {Count} currencies", currencies.Count);
        return currencies;
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
}
