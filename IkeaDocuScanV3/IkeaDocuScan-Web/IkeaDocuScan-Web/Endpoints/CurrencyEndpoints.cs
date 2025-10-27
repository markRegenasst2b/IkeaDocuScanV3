using IkeaDocuScan.Shared.Interfaces;
using IkeaDocuScan.Shared.DTOs.Currencies;

namespace IkeaDocuScan_Web.Endpoints;

/// <summary>
/// API endpoints for currencies
/// </summary>
public static class CurrencyEndpoints
{
    /// <summary>
    /// Map currency endpoints to the route builder
    /// </summary>
    public static void MapCurrencyEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/currencies")
            .RequireAuthorization("HasAccess")
            .WithTags("Currencies");

        // GET /api/currencies
        group.MapGet("/", async (ICurrencyService service) =>
        {
            var currencies = await service.GetAllAsync();
            return Results.Ok(currencies);
        })
        .WithName("GetAllCurrencies")
        .Produces<List<CurrencyDto>>(200);

        // GET /api/currencies/{code}
        group.MapGet("/{code}", async (string code, ICurrencyService service) =>
        {
            var currency = await service.GetByCodeAsync(code);
            if (currency == null)
            {
                return Results.NotFound(new { error = $"Currency with code '{code}' not found" });
            }
            return Results.Ok(currency);
        })
        .WithName("GetCurrencyByCode")
        .Produces<CurrencyDto>(200)
        .Produces(404);
    }
}
