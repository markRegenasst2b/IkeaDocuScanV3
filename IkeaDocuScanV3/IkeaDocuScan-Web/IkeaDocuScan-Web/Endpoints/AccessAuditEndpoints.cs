using IkeaDocuScan.Shared.DTOs.AccessAudit;
using IkeaDocuScan.Shared.Interfaces;

namespace IkeaDocuScan_Web.Endpoints;

/// <summary>
/// API endpoints for access audit functionality
/// All endpoints are restricted to SuperUser role only
/// </summary>
public static class AccessAuditEndpoints
{
    public static void MapAccessAuditEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/access-audit")
            .RequireAuthorization("SuperUser")  // All endpoints require SuperUser
            .WithTags("AccessAudit");

        // ========================================
        // DOCUMENT TYPE ACCESS AUDIT
        // ========================================

        /// <summary>
        /// Get all users who have access to a specific document type
        /// Includes both global access (DocumentTypeId = null) and direct access users
        /// </summary>
        group.MapGet("/document-type/{documentTypeId}", async (
            int documentTypeId,
            bool? showOnlyActiveUsers,
            bool? showOnlySuperUsers,
            string? accountNameFilter,
            int? activeDaysThreshold,
            IAccessAuditService service) =>
        {
            var result = await service.GetDocumentTypeAccessAsync(
                documentTypeId,
                showOnlyActiveUsers,
                showOnlySuperUsers,
                accountNameFilter,
                activeDaysThreshold ?? 90);

            return Results.Ok(result);
        })
        .WithName("GetDocumentTypeAccessAudit")
        .RequireAuthorization("Endpoint:GET:/api/access-audit/document-type/{documentTypeId}")
        .Produces<DocumentTypeAccessAuditDto>(200)
        .Produces(403);

        /// <summary>
        /// Export document type access audit to Excel
        /// </summary>
        group.MapGet("/document-type/{documentTypeId}/export", async (
            int documentTypeId,
            bool? showOnlyActiveUsers,
            bool? showOnlySuperUsers,
            string? accountNameFilter,
            int? activeDaysThreshold,
            IAccessAuditService service) =>
        {
            var excelBytes = await service.ExportDocumentTypeAccessToExcelAsync(
                documentTypeId,
                showOnlyActiveUsers,
                showOnlySuperUsers,
                accountNameFilter,
                activeDaysThreshold ?? 90);

            if (excelBytes.Length == 0)
            {
                return Results.NotFound(new { error = "Document type not found or no data to export" });
            }

            var fileName = $"DocumentTypeAccess_{documentTypeId}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            return Results.File(
                excelBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        })
        .WithName("ExportDocumentTypeAccessAudit")
        .RequireAuthorization("Endpoint:GET:/api/access-audit/document-type/{documentTypeId}/export")
        .Produces(200)
        .Produces(403)
        .Produces(404);

        // ========================================
        // USER ACCESS AUDIT
        // ========================================

        /// <summary>
        /// Get all document types a specific user has access to
        /// Shows whether user has global access or specific document type permissions
        /// </summary>
        group.MapGet("/user/{userId}", async (int userId, IAccessAuditService service) =>
        {
            var result = await service.GetUserAccessAsync(userId);

            if (result == null)
            {
                return Results.NotFound(new { error = $"User with ID {userId} not found" });
            }

            return Results.Ok(result);
        })
        .WithName("GetUserAccessAudit")
        .RequireAuthorization("Endpoint:GET:/api/access-audit/user/{userId}")
        .Produces<UserAccessAuditDto>(200)
        .Produces(403)
        .Produces(404);

        /// <summary>
        /// Export user access audit to Excel
        /// </summary>
        group.MapGet("/user/{userId}/export", async (int userId, IAccessAuditService service) =>
        {
            var excelBytes = await service.ExportUserAccessToExcelAsync(userId);

            if (excelBytes.Length == 0)
            {
                return Results.NotFound(new { error = $"User with ID {userId} not found or no data to export" });
            }

            var fileName = $"UserAccess_{userId}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            return Results.File(
                excelBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        })
        .WithName("ExportUserAccessAudit")
        .RequireAuthorization("Endpoint:GET:/api/access-audit/user/{userId}/export")
        .Produces(200)
        .Produces(403)
        .Produces(404);

        // ========================================
        // USER SEARCH
        // ========================================

        /// <summary>
        /// Search users with optional filters for access audit
        /// </summary>
        group.MapGet("/users", async (
            string? accountNameFilter,
            bool? showOnlyActiveUsers,
            bool? showOnlySuperUsers,
            bool? showOnlyGlobalAccess,
            int? activeDaysThreshold,
            IAccessAuditService service) =>
        {
            var request = new AccessAuditUserSearchRequest
            {
                AccountNameFilter = accountNameFilter,
                ShowOnlyActiveUsers = showOnlyActiveUsers,
                ShowOnlySuperUsers = showOnlySuperUsers,
                ShowOnlyGlobalAccess = showOnlyGlobalAccess,
                ActiveDaysThreshold = activeDaysThreshold ?? 90
            };

            var result = await service.SearchUsersAsync(request);
            return Results.Ok(result);
        })
        .WithName("SearchUsersForAccessAudit")
        .RequireAuthorization("Endpoint:GET:/api/access-audit/users")
        .Produces<List<AccessAuditUserSearchDto>>(200)
        .Produces(403);
    }
}
