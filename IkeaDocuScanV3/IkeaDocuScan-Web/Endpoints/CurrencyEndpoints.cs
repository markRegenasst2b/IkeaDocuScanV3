using IkeaDocuScan.Shared.Interfaces;

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
            .WithTags("Currencies")
            .WithOpenApi();

        // GET /api/currencies
        group.MapGet("/", async (ICurrencyService service) =>
        {
            var currencies = await service.GetAllAsync();
            return Results.Ok(currencies);
        })
        .WithName("GetAllCurrencies")
        .WithSummary("Get all currencies")
        .Produces(StatusCodes.Status200OK);

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
        .WithSummary("Get a specific currency by code")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);
    }
}
