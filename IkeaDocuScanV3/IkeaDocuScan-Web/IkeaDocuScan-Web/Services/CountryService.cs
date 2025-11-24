using IkeaDocuScan.Infrastructure.Data;
using IkeaDocuScan.Infrastructure.Entities;
using IkeaDocuScan.Shared.DTOs.Countries;
using IkeaDocuScan.Shared.Exceptions;
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

    public async Task<CountryDto> CreateAsync(CreateCountryDto dto)
    {
        _logger.LogInformation("Creating new country with code {CountryCode}", dto.CountryCode);

        await using var context = await _contextFactory.CreateDbContextAsync();

        // Check if country code already exists
        var exists = await context.Countries.AnyAsync(c => c.CountryCode == dto.CountryCode);
        if (exists)
        {
            throw new ValidationException($"Country with code '{dto.CountryCode}' already exists");
        }

        var country = new Country
        {
            CountryCode = dto.CountryCode.ToUpperInvariant(),
            Name = dto.Name
        };

        context.Countries.Add(country);
        await context.SaveChangesAsync();

        _logger.LogInformation("Created country {CountryCode}", country.CountryCode);

        return MapToDto(country);
    }

    public async Task<CountryDto> UpdateAsync(string countryCode, UpdateCountryDto dto)
    {
        _logger.LogInformation("Updating country with code {CountryCode}", countryCode);

        await using var context = await _contextFactory.CreateDbContextAsync();

        var country = await context.Countries.FindAsync(countryCode);
        if (country == null)
        {
            throw new ValidationException($"Country with code '{countryCode}' not found");
        }

        country.Name = dto.Name;

        await context.SaveChangesAsync();

        _logger.LogInformation("Updated country {CountryCode}", countryCode);

        return MapToDto(country);
    }

    public async Task DeleteAsync(string countryCode)
    {
        _logger.LogInformation("Deleting country with code {CountryCode}", countryCode);

        await using var context = await _contextFactory.CreateDbContextAsync();

        var country = await context.Countries.FindAsync(countryCode);
        if (country == null)
        {
            throw new ValidationException($"Country with code '{countryCode}' not found");
        }

        // Check if country is in use
        var counterPartyCount = await context.CounterParties
            .Where(cp => cp.Country == countryCode)
            .CountAsync();

        if (counterPartyCount > 0)
        {
            throw new ValidationException(
                $"Cannot delete country '{countryCode}'. It is currently used by {counterPartyCount} counter part{(counterPartyCount != 1 ? "ies" : "y")}. " +
                "Please remove or update all references before deleting it.");
        }

        context.Countries.Remove(country);
        await context.SaveChangesAsync();

        _logger.LogInformation("Deleted country {CountryCode}", countryCode);
    }

    public async Task<bool> IsInUseAsync(string countryCode)
    {
        _logger.LogInformation("Checking if country {CountryCode} is in use", countryCode);

        await using var context = await _contextFactory.CreateDbContextAsync();

        var counterPartyUsage = await context.CounterParties.AnyAsync(cp => cp.Country == countryCode);
        return counterPartyUsage;
    }

    public async Task<(int counterPartyCount, int userPermissionCount)> GetUsageCountAsync(string countryCode)
    {
        _logger.LogInformation("Getting usage count for country {CountryCode}", countryCode);

        await using var context = await _contextFactory.CreateDbContextAsync();

        var counterPartyCount = await context.CounterParties
            .Where(cp => cp.Country == countryCode)
            .CountAsync();

        return (counterPartyCount, 0);
    }

    private static CountryDto MapToDto(Country entity)
    {
        return new CountryDto
        {
            CountryCode = entity.CountryCode,
            Name = entity.Name
        };
    }
}
