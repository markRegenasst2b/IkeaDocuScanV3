using IkeaDocuScan.Infrastructure.Data;
using IkeaDocuScan.Shared.DTOs.Countries;
using IkeaDocuScan.Shared.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IkeaDocuScan_Web.Services;

/// <summary>
/// Service for Country operations
/// </summary>
public class CountryService : ICountryService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    private readonly ILogger<CountryService> _logger;

    public CountryService(
        IDbContextFactory<AppDbContext> contextFactory,
        ILogger<CountryService> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }

    public async Task<List<CountryDto>> GetAllAsync()
    {
        _logger.LogInformation("Fetching all countries");

        await using var context = await _contextFactory.CreateDbContextAsync();
        var countries = await context.Countries
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ToListAsync();

        return countries.Select(MapToDto).ToList();
    }

    public async Task<CountryDto?> GetByCodeAsync(string countryCode)
    {
        _logger.LogInformation("Fetching country with code: {CountryCode}", countryCode);

        await using var context = await _contextFactory.CreateDbContextAsync();
        var country = await context.Countries
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.CountryCode == countryCode);

        if (country == null)
        {
            _logger.LogWarning("Country with code {CountryCode} not found", countryCode);
            return null;
        }

        return MapToDto(country);
    }

    private static CountryDto MapToDto(IkeaDocuScan.Infrastructure.Entities.Country entity)
    {
        return new CountryDto
        {
            CountryCode = entity.CountryCode,
            Name = entity.Name
        };
    }
}
