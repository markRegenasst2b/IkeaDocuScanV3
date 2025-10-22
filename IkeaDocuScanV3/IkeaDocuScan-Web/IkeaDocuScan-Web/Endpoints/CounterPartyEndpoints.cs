using IkeaDocuScan.Shared.Interfaces;
using IkeaDocuScan.Shared.DTOs.CounterParties;

namespace IkeaDocuScan_Web.Endpoints;

public static class CounterPartyEndpoints
{
    public static void MapCounterPartyEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/counterparties")
            .RequireAuthorization()
            .WithTags("CounterParties");

        group.MapGet("/", async (ICounterPartyService service) =>
        {
            var counterParties = await service.GetAllAsync();
            return Results.Ok(counterParties);
        })
        .WithName("GetAllCounterParties")
        .Produces<List<CounterPartyDto>>(200);

        group.MapGet("/search", async (string? searchTerm, ICounterPartyService service) =>
        {
            var counterParties = await service.SearchAsync(searchTerm ?? string.Empty);
            return Results.Ok(counterParties);
        })
        .WithName("SearchCounterParties")
        .Produces<List<CounterPartyDto>>(200);

        group.MapGet("/{id}", async (int id, ICounterPartyService service) =>
        {
            var counterParty = await service.GetByIdAsync(id);
            if (counterParty == null)
                return Results.NotFound(new { error = $"CounterParty with ID {id} not found" });

            return Results.Ok(counterParty);
        })
        .WithName("GetCounterPartyById")
        .Produces<CounterPartyDto>(200)
        .Produces(404);
    }
}
