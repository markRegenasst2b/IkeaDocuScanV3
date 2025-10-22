using IkeaDocuScan.Shared.Interfaces;
using IkeaDocuScan.Shared.DTOs.Countries;

namespace IkeaDocuScan_Web.Endpoints;

public static class CountryEndpoints
{
    public static void MapCountryEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/countries")
            .RequireAuthorization()
            .WithTags("Countries");

        group.MapGet("/", async (ICountryService service) =>
        {
            var countries = await service.GetAllAsync();
            return Results.Ok(countries);
        })
        .WithName("GetAllCountries")
        .Produces<List<CountryDto>>(200);

        group.MapGet("/{code}", async (string code, ICountryService service) =>
        {
            var country = await service.GetByCodeAsync(code);
            if (country == null)
                return Results.NotFound(new { error = $"Country with code {code} not found" });

            return Results.Ok(country);
        })
        .WithName("GetCountryByCode")
        .Produces<CountryDto>(200)
        .Produces(404);
    }
}
