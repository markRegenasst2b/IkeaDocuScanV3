using IkeaDocuScan.Shared.DTOs;
using IkeaDocuScan.Shared.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace IkeaDocuScan_Web.Endpoints;

/// <summary>
/// API endpoints for managing endpoint authorization permissions
/// All endpoints require SuperUser role except /check which is accessible to all roles
/// </summary>
public static class EndpointAuthorizationEndpoints
{
    public static void MapEndpointAuthorizationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/endpoint-authorization")
            .WithTags("Endpoint Authorization");

        // ===================================================================
        // ENDPOINT REGISTRY MANAGEMENT
        // ===================================================================

        // GET /api/endpoint-authorization/endpoints
        // Get all endpoints with their role permissions
        group.MapGet("/endpoints", async ([FromServices] IEndpointAuthorizationManagementService service) =>
        {
            var endpoints = await service.GetAllEndpointsAsync();
            return Results.Ok(endpoints);
        })
        .WithName("GetAllEndpoints")
        .RequireAuthorization("SuperUser")
        .Produces<List<EndpointRegistryDto>>(200)
        .Produces(403);

        // GET /api/endpoint-authorization/endpoints/{id}
        // Get specific endpoint by ID
        group.MapGet("/endpoints/{id:int}", async (
            int id,
            [FromServices] IEndpointAuthorizationManagementService service) =>
        {
            var endpoint = await service.GetEndpointByIdAsync(id);
            return endpoint == null ? Results.NotFound() : Results.Ok(endpoint);
        })
        .WithName("GetEndpointById")
        .RequireAuthorization("SuperUser")
        .Produces<EndpointRegistryDto>(200)
        .Produces(404)
        .Produces(403);

        // GET /api/endpoint-authorization/endpoints/{id}/roles
        // Get roles that have access to a specific endpoint
        group.MapGet("/endpoints/{id:int}/roles", async (
            int id,
            [FromServices] IEndpointAuthorizationManagementService service) =>
        {
            var roles = await service.GetEndpointRolesAsync(id);
            return Results.Ok(roles);
        })
        .WithName("GetEndpointRoles")
        .RequireAuthorization("SuperUser")
        .Produces<List<string>>(200)
        .Produces(403);

        // POST /api/endpoint-authorization/endpoints/{id}/roles
        // Update roles for a specific endpoint
        group.MapPost("/endpoints/{id:int}/roles", async (
            int id,
            [FromBody] UpdateEndpointRolesDto dto,
            [FromServices] IEndpointAuthorizationManagementService service,
            [FromServices] ICurrentUserService currentUserService) =>
        {
            try
            {
                var currentUser = await currentUserService.GetCurrentUserAsync();
                await service.UpdateEndpointRolesAsync(id, dto.RoleNames, currentUser.AccountName, dto.ChangeReason);
                return Results.Ok(new { message = "Roles updated successfully" });
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return Results.NotFound(new { error = ex.Message });
            }
        })
        .WithName("UpdateEndpointRoles")
        .RequireAuthorization("SuperUser")
        .DisableAntiforgery()
        .Produces(200)
        .Produces(400)
        .Produces(404)
        .Produces(403);

        // ===================================================================
        // ROLES & AUDIT
        // ===================================================================

        // GET /api/endpoint-authorization/roles
        // Get all available role names
        group.MapGet("/roles", async ([FromServices] IEndpointAuthorizationManagementService service) =>
        {
            var roles = await service.GetAvailableRolesAsync();
            return Results.Ok(roles);
        })
        .WithName("GetAvailableRoles")
        .RequireAuthorization("SuperUser")
        .Produces<List<string>>(200)
        .Produces(403);

        // GET /api/endpoint-authorization/audit
        // Get audit log for permission changes
        group.MapGet("/audit", async (
            [FromQuery] int? endpointId,
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate,
            [FromServices] IEndpointAuthorizationManagementService service) =>
        {
            var logs = await service.GetAuditLogAsync(endpointId, fromDate, toDate);
            return Results.Ok(logs);
        })
        .WithName("GetPermissionAuditLog")
        .RequireAuthorization("SuperUser")
        .Produces<List<PermissionChangeAuditLogDto>>(200)
        .Produces(403);

        // ===================================================================
        // CACHE & SYNC OPERATIONS
        // ===================================================================

        // POST /api/endpoint-authorization/cache/invalidate
        // Invalidate the authorization cache
        group.MapPost("/cache/invalidate", async (
            [FromServices] IEndpointAuthorizationManagementService service,
            [FromServices] ILogger<IEndpointAuthorizationManagementService> logger) =>
        {
            await service.InvalidateCacheAsync();
            logger.LogInformation("Authorization cache invalidated via API");
            return Results.Ok(new { message = "Cache invalidated successfully" });
        })
        .WithName("InvalidateAuthorizationCache")
        .RequireAuthorization("SuperUser")
        .DisableAntiforgery()
        .Produces(200)
        .Produces(403);

        // POST /api/endpoint-authorization/sync
        // Sync endpoints from code to database (placeholder for auto-discovery)
        group.MapPost("/sync", async ([FromServices] IEndpointAuthorizationManagementService service) =>
        {
            await service.SyncEndpointsFromCodeAsync();
            return Results.Ok(new { message = "Endpoint sync completed (manual seeding still required)" });
        })
        .WithName("SyncEndpointsFromCode")
        .RequireAuthorization("SuperUser")
        .DisableAntiforgery()
        .Produces(200)
        .Produces(403);

        // ===================================================================
        // ACCESS CHECK (ALL ROLES)
        // ===================================================================

        // GET /api/endpoint-authorization/check
        // Check if current user has access to a specific endpoint
        // This endpoint uses simple "HasAccess" policy to avoid circular dependency
        // ALL authenticated users can call this endpoint
        group.MapGet("/check", async (
            [FromQuery] string method,
            [FromQuery] string route,
            [FromServices] IEndpointAuthorizationService authService,
            [FromServices] ILogger<IEndpointAuthorizationService> logger,
            HttpContext httpContext) =>
        {
            try
            {
                // Get user's roles from claims
                var userRoles = httpContext.User.Claims
                    .Where(c => c.Type == System.Security.Claims.ClaimTypes.Role)
                    .Select(c => c.Value)
                    .ToList();

                logger.LogInformation("Checking access for {Method} {Route} for user with roles: {Roles}",
                    method, route, string.Join(", ", userRoles));

                var allowedRoles = await authService.GetAllowedRolesAsync(method, route);

                logger.LogInformation("Allowed roles for {Method} {Route}: {AllowedRoles}",
                    method, route, string.Join(", ", allowedRoles));

                // Check if user has any of the allowed roles
                var hasAccess = allowedRoles.Any(role => userRoles.Contains(role));

                return Results.Ok(new EndpointAccessCheckResult
                {
                    HasAccess = hasAccess,
                    AllowedRoles = allowedRoles,
                    UserRoles = userRoles
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error checking endpoint access for {Method} {Route}", method, route);
                return Results.Ok(new EndpointAccessCheckResult
                {
                    HasAccess = false,
                    AllowedRoles = new List<string>(),
                    UserRoles = new List<string>(),
                    Error = ex.Message
                });
            }
        })
        .WithName("CheckEndpointAccess")
        .RequireAuthorization("HasAccess")  // Simple policy - all authenticated users with HasAccess claim
        .Produces<EndpointAccessCheckResult>(200);

        // POST /api/endpoint-authorization/validate
        // Validate a permission change before applying it
        group.MapPost("/validate", async (
            [FromBody] ValidatePermissionChangeDto dto,
            [FromServices] IEndpointAuthorizationManagementService service) =>
        {
            var errors = await service.ValidatePermissionChangeAsync(dto.EndpointId, dto.RoleNames);
            return Results.Ok(new ValidatePermissionChangeResult
            {
                IsValid = !errors.Any(),
                ValidationErrors = errors
            });
        })
        .WithName("ValidatePermissionChange")
        .RequireAuthorization("SuperUser")
        .DisableAntiforgery()
        .Produces<ValidatePermissionChangeResult>(200)
        .Produces(403);
    }
}
