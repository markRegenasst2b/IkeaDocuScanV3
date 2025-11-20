using IkeaDocuScan.Shared.Interfaces;
using IkeaDocuScan.Shared.Enums;

namespace IkeaDocuScan_Web.Endpoints;

/// <summary>
/// API endpoints for audit trail management
/// Uses dynamic database-driven authorization
/// </summary>
public static class AuditTrailEndpoints
{
    public static void MapAuditTrailEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/audittrail")
            .RequireAuthorization()  // Base authentication required
            .WithTags("AuditTrail");

        // Log audit entry by barcode
        group.MapPost("/", async (LogAuditRequest request, IAuditTrailService service) =>
        {
            var action = Enum.Parse<AuditAction>(request.Action);
            await service.LogAsync(action, request.BarCode, request.Details, request.Username);
            return Results.Ok(new { message = "Audit entry logged successfully" });
        })
        .WithName("LogAuditTrail")
        .RequireAuthorization("Endpoint:POST:/api/audittrail/")
        .Produces(200)
        .Produces(400);

        // Log audit entry by document ID
        group.MapPost("/document/{documentId}", async (int documentId, LogAuditByDocumentRequest request, IAuditTrailService service) =>
        {
            var action = Enum.Parse<AuditAction>(request.Action);
            await service.LogByDocumentIdAsync(action, documentId, request.Details, request.Username);
            return Results.Ok(new { message = "Audit entry logged successfully" });
        })
        .WithName("LogAuditTrailByDocument")
        .RequireAuthorization("Endpoint:POST:/api/audittrail/document/{documentId}")
        .Produces(200)
        .Produces(400);

        // Log batch audit entries
        group.MapPost("/batch", async (LogAuditBatchRequest request, IAuditTrailService service) =>
        {
            var action = Enum.Parse<AuditAction>(request.Action);
            await service.LogBatchAsync(action, request.BarCodes, request.Details, request.Username);
            return Results.Ok(new { message = "Batch audit entries logged successfully" });
        })
        .WithName("LogAuditTrailBatch")
        .RequireAuthorization("Endpoint:POST:/api/audittrail/batch")
        .Produces(200)
        .Produces(400);

        // Get audit entries by barcode
        group.MapGet("/barcode/{barCode}", async (string barCode, int limit, IAuditTrailService service) =>
        {
            var entries = await service.GetByBarCodeAsync(barCode, limit);
            return Results.Ok(entries);
        })
        .WithName("GetAuditTrailByBarCode")
        .RequireAuthorization("Endpoint:GET:/api/audittrail/barcode/{barCode}")
        .Produces<List<AuditTrailDto>>(200);

        // Get audit entries by user
        group.MapGet("/user/{username}", async (string username, int limit, IAuditTrailService service) =>
        {
            var entries = await service.GetByUserAsync(username, limit);
            return Results.Ok(entries);
        })
        .WithName("GetAuditTrailByUser")
        .RequireAuthorization("Endpoint:GET:/api/audittrail/user/{username}")
        .Produces<List<AuditTrailDto>>(200);

        // Get recent audit entries
        group.MapGet("/recent", async (int limit, IAuditTrailService service) =>
        {
            var entries = await service.GetRecentAsync(limit);
            return Results.Ok(entries);
        })
        .WithName("GetRecentAuditTrail")
        .RequireAuthorization("Endpoint:GET:/api/audittrail/recent")
        .Produces<List<AuditTrailDto>>(200);

        // Get audit entries by date range
        group.MapGet("/daterange", async (DateTime startDate, DateTime endDate, AuditAction? action, IAuditTrailService service) =>
        {
            var entries = await service.GetByDateRangeAsync(startDate, endDate, action);
            return Results.Ok(entries);
        })
        .WithName("GetAuditTrailByDateRange")
        .RequireAuthorization("Endpoint:GET:/api/audittrail/daterange")
        .Produces<List<AuditTrailDto>>(200);
    }

    // Request DTOs
    private record LogAuditRequest(string Action, string BarCode, string? Details, string? Username);
    private record LogAuditByDocumentRequest(string Action, string? Details, string? Username);
    private record LogAuditBatchRequest(string Action, IEnumerable<string> BarCodes, string? Details, string? Username);
}
