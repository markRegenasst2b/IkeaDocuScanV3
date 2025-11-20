using IkeaDocuScan.Shared.Interfaces;
using IkeaDocuScan.Shared.DTOs.Currencies;
using IkeaDocuScan.Shared.Exceptions;

namespace IkeaDocuScan_Web.Endpoints;

/// <summary>
/// API endpoints for currency management
/// Uses dynamic database-driven authorization
/// </summary>
public static class CurrencyEndpoints
{
    public static void MapCurrencyEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/currencies")
            .RequireAuthorization()  // Base authentication required
            .WithTags("Currencies");

        // GET /api/currencies
        group.MapGet("/", async (ICurrencyService service) =>
        {
            var currencies = await service.GetAllAsync();
            return Results.Ok(currencies);
        })
        .WithName("GetAllCurrencies")
        .RequireAuthorization("Endpoint:GET:/api/currencies/")
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
        .RequireAuthorization("Endpoint:GET:/api/currencies/{code}")
        .Produces<CurrencyDto>(200)
        .Produces(404);

        // POST /api/currencies
        group.MapPost("/", async (CreateCurrencyDto dto, ICurrencyService service) =>
        {
            try
            {
                var currency = await service.CreateAsync(dto);
                return Results.Created($"/api/currencies/{currency.CurrencyCode}", currency);
            }
            catch (ValidationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        })
        .WithName("CreateCurrency")
        .RequireAuthorization("Endpoint:POST:/api/currencies/")
        .Produces<CurrencyDto>(201)
        .Produces(400);

        // PUT /api/currencies/{code}
        group.MapPut("/{code}", async (string code, UpdateCurrencyDto dto, ICurrencyService service) =>
        {
            try
            {
                var currency = await service.UpdateAsync(code, dto);
                return Results.Ok(currency);
            }
            catch (ValidationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        })
        .WithName("UpdateCurrency")
        .RequireAuthorization("Endpoint:PUT:/api/currencies/{code}")
        .Produces<CurrencyDto>(200)
        .Produces(400);

        // DELETE /api/currencies/{code}
        group.MapDelete("/{code}", async (string code, ICurrencyService service) =>
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
        .WithName("DeleteCurrency")
        .RequireAuthorization("Endpoint:DELETE:/api/currencies/{code}")
        .Produces(204)
        .Produces(400);

        // GET /api/currencies/{code}/usage
        group.MapGet("/{code}/usage", async (string code, ICurrencyService service) =>
        {
            var isInUse = await service.IsInUseAsync(code);
            var count = await service.GetUsageCountAsync(code);
            return Results.Ok(new { currencyCode = code, isInUse, usageCount = count });
        })
        .WithName("GetCurrencyUsage")
        .RequireAuthorization("Endpoint:GET:/api/currencies/{code}/usage")
        .Produces<object>(200);
    }
}
