using IkeaDocuScan.Shared.DTOs.ActionReminders;
using IkeaDocuScan.Shared.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace IkeaDocuScan_Web.Endpoints;

/// <summary>
/// API endpoints for action reminder management
/// Uses dynamic database-driven authorization
/// </summary>
public static class ActionReminderEndpoints
{
    public static void MapActionReminderEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/action-reminders")
            .RequireAuthorization()  // Base authentication required
            .WithTags("Action Reminders");

        // GET /api/action-reminders
        group.MapGet("/", async (
            IActionReminderService service,
            ILogger<IActionReminderService> logger,
            [FromQuery] DateTime? dateFrom,
            [FromQuery] DateTime? dateTo,
            [FromQuery] int[]? documentTypeIds,
            [FromQuery] int[]? counterPartyIds,
            [FromQuery] string? counterPartySearch,
            [FromQuery] string? searchString,
            [FromQuery] bool? includeFutureActions,
            [FromQuery] bool? includeOverdueOnly) =>
        {
            try
            {
                var request = new ActionReminderSearchRequestDto
                {
                    DateFrom = dateFrom,
                    DateTo = dateTo,
                    DocumentTypeIds = documentTypeIds?.ToList(),
                    CounterPartyIds = counterPartyIds?.ToList(),
                    CounterPartySearch = counterPartySearch,
                    SearchString = searchString,
                    IncludeFutureActions = includeFutureActions ?? true,
                    IncludeOverdueOnly = includeOverdueOnly ?? false
                };

                logger.LogInformation("API: Getting due actions");
                var results = await service.GetDueActionsAsync(request);
                logger.LogInformation("API: Returning {Count} due actions", results.Count);
                return Results.Ok(results);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "API: Error getting due actions");
                return Results.Problem(
                    title: "Error retrieving action reminders",
                    detail: ex.Message,
                    statusCode: 500);
            }
        })
        .WithName("GetDueActions")
        .RequireAuthorization("Endpoint:GET:/api/action-reminders/")
        .Produces<List<ActionReminderDto>>(200)
        .Produces(500);

        // GET /api/action-reminders/count
        group.MapGet("/count", async (IActionReminderService service, ILogger<IActionReminderService> logger) =>
        {
            try
            {
                logger.LogInformation("API: Getting due actions count");
                var count = await service.GetDueActionsCountAsync();
                logger.LogInformation("API: Returning count: {Count}", count);
                return Results.Ok(count);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "API: Error getting due actions count");
                return Results.Problem(
                    title: "Error retrieving action reminders count",
                    detail: ex.Message,
                    statusCode: 500);
            }
        })
        .WithName("GetDueActionsCount")
        .RequireAuthorization("Endpoint:GET:/api/action-reminders/count")
        .Produces<int>(200)
        .Produces(500);

        // GET /api/action-reminders/date/{date}
        group.MapGet("/date/{date}", async (DateTime date, IActionReminderService service, ILogger<IActionReminderService> logger) =>
        {
            try
            {
                logger.LogInformation("API: Getting actions due on {Date}", date);
                var results = await service.GetActionsDueOnDateAsync(date);
                logger.LogInformation("API: Returning {Count} actions for {Date}", results.Count, date);
                return Results.Ok(results);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "API: Error getting actions due on {Date}", date);
                return Results.Problem(
                    title: "Error retrieving action reminders for date",
                    detail: ex.Message,
                    statusCode: 500);
            }
        })
        .WithName("GetActionsDueOnDate")
        .RequireAuthorization("Endpoint:GET:/api/action-reminders/date/{date}")
        .Produces<List<ActionReminderDto>>(200)
        .Produces(500);
    }
}
