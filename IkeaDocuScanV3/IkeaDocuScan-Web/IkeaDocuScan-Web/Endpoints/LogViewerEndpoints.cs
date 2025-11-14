using IkeaDocuScan.Shared.DTOs;
using IkeaDocuScan.Shared.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace IkeaDocuScan_Web.Endpoints;

/// <summary>
/// API endpoints for log viewer (SuperUser only)
/// </summary>
public static class LogViewerEndpoints
{
    public static void MapLogViewerEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/logs")
            .RequireAuthorization("SuperUser") // Only SuperUser can access logs
            .WithTags("LogViewer");

        // Search logs
        group.MapPost("/search", async (
            [FromBody] LogSearchRequest request,
            ILogViewerService logService,
            IAuditTrailService auditService,
            ICurrentUserService currentUserService,
            CancellationToken cancellationToken) =>
        {
            try
            {
                // Audit log access
                var user = await currentUserService.GetCurrentUserAsync();
                await auditService.LogAsync(
                    IkeaDocuScan.Shared.Enums.AuditAction.ViewLogs,
                    "LOGVIEWER",
                    $"Viewed logs: Level={request.Level}, From={request.FromDate:yyyy-MM-dd}, To={request.ToDate:yyyy-MM-dd}, Search={request.SearchText} (User: {user.AccountName})"
                );

                var result = await logService.SearchLogsAsync(request, cancellationToken);
                return Results.Ok(result);
            }
            catch (Exception ex)
            {
                return Results.Problem(
                    title: "Error searching logs",
                    detail: ex.Message,
                    statusCode: 500
                );
            }
        })
        .WithName("SearchLogs")
        .Produces<LogSearchResult>(200)
        .Produces(500);

        // Export logs (GET endpoint with query parameters for easy download)
        group.MapGet("/export", async (
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate,
            [FromQuery] string? level,
            [FromQuery] string? source,
            [FromQuery] string? searchText,
            [FromQuery] string format,
            ILogViewerService logService,
            IAuditTrailService auditService,
            ICurrentUserService currentUserService,
            ILogger<ILogViewerService> logger,
            CancellationToken cancellationToken) =>
        {
            try
            {
                logger.LogInformation("Export endpoint called - Raw parameters: fromDate={FromDate}, toDate={ToDate}, level={Level}, source={Source}, searchText={SearchText}, format={Format}",
                    fromDate, toDate, level, source, searchText, format);

                // Adjust toDate to end of day if provided
                var adjustedToDate = toDate.HasValue ? toDate.Value.Date.AddDays(1).AddSeconds(-1) : (DateTime?)null;

                logger.LogInformation("Export endpoint - After adjustment: fromDate={FromDate}, adjustedToDate={AdjustedToDate}",
                    fromDate, adjustedToDate);

                var request = new LogSearchRequest
                {
                    FromDate = fromDate,
                    ToDate = adjustedToDate,
                    Level = level,
                    Source = source,
                    SearchText = searchText
                };

                logger.LogInformation("Export request object - From: {FromDate}, To: {ToDate}, Level: {Level}, Source: {Source}, Search: {SearchText}",
                    request.FromDate, request.ToDate, request.Level, request.Source, request.SearchText);

                // Audit log export
                var user = await currentUserService.GetCurrentUserAsync();
                await auditService.LogAsync(
                    IkeaDocuScan.Shared.Enums.AuditAction.ExportLogs,
                    "LOGEXPORT",
                    $"Exported logs as {format}: Level={request.Level}, From={request.FromDate:yyyy-MM-dd}, To={request.ToDate:yyyy-MM-dd} (User: {user.AccountName})"
                );

                var data = await logService.ExportLogsAsync(request, format, cancellationToken);

                logger.LogInformation("Export completed - {ByteCount} bytes exported", data.Length);

                var contentType = format.ToLower() == "csv" ? "text/csv" : "application/json";
                var fileName = $"logs-{DateTime.Now:yyyyMMddHHmmss}.{format}";

                return Results.File(data, contentType, fileName);
            }
            catch (Exception ex)
            {
                return Results.Problem(
                    title: "Error exporting logs",
                    detail: ex.Message,
                    statusCode: 500
                );
            }
        })
        .WithName("ExportLogs")
        .Produces(200)
        .Produces(500);

        // Get available log dates
        group.MapGet("/dates", async (ILogViewerService logService) =>
        {
            try
            {
                var dates = await logService.GetAvailableLogDatesAsync();
                return Results.Ok(dates);
            }
            catch (Exception ex)
            {
                return Results.Problem(
                    title: "Error getting log dates",
                    detail: ex.Message,
                    statusCode: 500
                );
            }
        })
        .WithName("GetLogDates")
        .Produces<List<string>>(200)
        .Produces(500);

        // Get log sources
        group.MapGet("/sources", async (ILogViewerService logService) =>
        {
            try
            {
                var sources = await logService.GetLogSourcesAsync();
                return Results.Ok(sources);
            }
            catch (Exception ex)
            {
                return Results.Problem(
                    title: "Error getting log sources",
                    detail: ex.Message,
                    statusCode: 500
                );
            }
        })
        .WithName("GetLogSources")
        .Produces<List<string>>(200)
        .Produces(500);

        // Get log statistics
        group.MapGet("/statistics", async (
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate,
            ILogViewerService logService,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var stats = await logService.GetLogStatisticsAsync(fromDate, toDate, cancellationToken);
                return Results.Ok(stats);
            }
            catch (Exception ex)
            {
                return Results.Problem(
                    title: "Error getting log statistics",
                    detail: ex.Message,
                    statusCode: 500
                );
            }
        })
        .WithName("GetLogStatistics")
        .Produces<LogStatisticsDto>(200)
        .Produces(500);
    }
}
