using IkeaDocuScan.Shared.Interfaces;
using IkeaDocuScan.Shared.DTOs.Countries;
using IkeaDocuScan.Shared.Exceptions;

namespace IkeaDocuScan_Web.Endpoints;

/// <summary>
/// API endpoints for country management
/// Uses dynamic database-driven authorization
/// </summary>
public static class CountryEndpoints
{
    public static void MapCountryEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/countries")
            .RequireAuthorization()  // Base authentication required
            .WithTags("Countries");

        // GET /api/countries
        group.MapGet("/", async (ICountryService service) =>
        {
            var countries = await service.GetAllAsync();
            return Results.Ok(countries);
        })
        .WithName("GetAllCountries")
        .RequireAuthorization("Endpoint:GET:/api/countries/")
        .Produces<List<CountryDto>>(200);

        // GET /api/countries/{code}
        group.MapGet("/{code}", async (string code, ICountryService service) =>
        {
            var country = await service.GetByCodeAsync(code);
            if (country == null)
                return Results.NotFound(new { error = $"Country with code {code} not found" });

            return Results.Ok(country);
        })
        .WithName("GetCountryByCode")
        .RequireAuthorization("Endpoint:GET:/api/countries/{code}")
        .Produces<CountryDto>(200)
        .Produces(404);

        // POST /api/countries
        group.MapPost("/", async (CreateCountryDto dto, ICountryService service) =>
        {
            try
            {
                var country = await service.CreateAsync(dto);
                return Results.Created($"/api/countries/{country.CountryCode}", country);
            }
            catch (ValidationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        })
        .WithName("CreateCountry")
        .RequireAuthorization("Endpoint:POST:/api/countries/")
        .Produces<CountryDto>(201)
        .Produces(400);

        // PUT /api/countries/{code}
        group.MapPut("/{code}", async (string code, UpdateCountryDto dto, ICountryService service) =>
        {
            try
            {
                var country = await service.UpdateAsync(code, dto);
                return Results.Ok(country);
            }
            catch (ValidationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        })
        .WithName("UpdateCountry")
        .RequireAuthorization("Endpoint:PUT:/api/countries/{code}")
        .Produces<CountryDto>(200)
        .Produces(400);

        // DELETE /api/countries/{code}
        group.MapDelete("/{code}", async (string code, ICountryService service) =>
        {
            try
            {
                await service.DeleteAsync(code);
                return Results.NoContent();
            }
            catch (ValidationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        })
        .WithName("DeleteCountry")
        .RequireAuthorization("Endpoint:DELETE:/api/countries/{code}")
        .Produces(204)
        .Produces(400);

        // GET /api/countries/{code}/usage
        group.MapGet("/{code}/usage", async (string code, ICountryService service) =>
        {
            var isInUse = await service.IsInUseAsync(code);
            var (counterPartyCount, userPermissionCount) = await service.GetUsageCountAsync(code);
            return Results.Ok(new
            {
                countryCode = code,
                isInUse,
                counterPartyCount,
                userPermissionCount,
                totalUsage = counterPartyCount + userPermissionCount
            });
        })
        .WithName("GetCountryUsage")
        .RequireAuthorization("Endpoint:GET:/api/countries/{code}/usage")
        .Produces<object>(200);
    }
}
