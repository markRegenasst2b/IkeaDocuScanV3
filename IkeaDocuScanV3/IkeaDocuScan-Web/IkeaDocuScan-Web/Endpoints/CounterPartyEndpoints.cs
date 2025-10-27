using IkeaDocuScan.Shared.Exceptions;
using IkeaDocuScan.Shared.Interfaces;
using IkeaDocuScan.Shared.DTOs.CounterParties;

namespace IkeaDocuScan_Web.Endpoints;

public static class CounterPartyEndpoints
{
    public static void MapCounterPartyEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/counterparties")
            .RequireAuthorization("HasAccess")
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

        group.MapPost("/", async (CreateCounterPartyDto dto, ICounterPartyService service) =>
        {
            try
            {
                var counterParty = await service.CreateAsync(dto);
                return Results.Created($"/api/counterparties/{counterParty.CounterPartyId}", counterParty);
            }
            catch (ValidationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        })
        .WithName("CreateCounterParty")
        .Produces<CounterPartyDto>(201)
        .Produces(400);

        group.MapPut("/{id}", async (int id, UpdateCounterPartyDto dto, ICounterPartyService service) =>
        {
            try
            {
                var counterParty = await service.UpdateAsync(id, dto);
                return Results.Ok(counterParty);
            }
            catch (ValidationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        })
        .WithName("UpdateCounterParty")
        .Produces<CounterPartyDto>(200)
        .Produces(400);

        group.MapDelete("/{id}", async (int id, ICounterPartyService service) =>
        {
            try
            {
                await service.DeleteAsync(id);
                return Results.NoContent();
            }
            catch (ValidationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        })
        .WithName("DeleteCounterParty")
        .Produces(204)
        .Produces(400);

        group.MapGet("/{id}/usage", async (int id, ICounterPartyService service) =>
        {
            var (documentCount, userPermissionCount) = await service.GetUsageCountAsync(id);
            var isInUse = documentCount > 0 || userPermissionCount > 0;

            return Results.Ok(new
            {
                counterPartyId = id,
                isInUse,
                documentCount,
                userPermissionCount,
                totalUsage = documentCount + userPermissionCount
            });
        })
        .WithName("GetCounterPartyUsage")
        .Produces(200);
    }
}
