using IkeaDocuScan.Shared.Interfaces;
using IkeaDocuScan.Shared.DTOs.UserPermissions;

namespace IkeaDocuScan_Web.Endpoints;

/// <summary>
/// API endpoints for user permission management
/// Uses dynamic database-driven authorization
/// </summary>
public static class UserPermissionEndpoints
{
    public static void MapUserPermissionEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/userpermissions")
            .RequireAuthorization()  // Base authentication required
            .WithTags("UserPermissions");

        // ========================================
        // READ OPERATIONS
        // ========================================

        group.MapGet("/", async (string? accountNameFilter, IUserPermissionService service) =>
        {
            var permissions = await service.GetAllAsync(accountNameFilter);
            return Results.Ok(permissions);
        })
        .WithName("GetAllUserPermissions")
        .RequireAuthorization("Endpoint:GET:/api/userpermissions/")
        .Produces<List<UserPermissionDto>>(200)
        .Produces(403);

        // STEP 5 TEST: Changed to dynamic authorization (database-driven)
        // Expected roles per database seed: ADAdmin, SuperUser
        // Test: Reader → 403, Publisher → 403, ADAdmin → 200, SuperUser → 200
        group.MapGet("/users", async (string? accountNameFilter, IUserPermissionService service) =>
        {
            var users = await service.GetAllUsersAsync(accountNameFilter);
            return Results.Ok(users);
        })
        .WithName("GetAllDocuScanUsers")
        .RequireAuthorization("Endpoint:GET:/api/userpermissions/users")  // ← CHANGED: Dynamic authorization
        .Produces<List<DocuScanUserDto>>(200)
        .Produces(403);

        group.MapGet("/{id}", async (int id, IUserPermissionService service) =>
        {
            var permission = await service.GetByIdAsync(id);
            if (permission == null)
                return Results.NotFound(new { error = $"UserPermission with ID {id} not found" });

            return Results.Ok(permission);
        })
        .WithName("GetUserPermissionById")
        .RequireAuthorization("Endpoint:GET:/api/userpermissions/{id}")
        .Produces<UserPermissionDto>(200)
        .Produces(403)
        .Produces(404);

        group.MapGet("/user/{userId}", async (int userId, IUserPermissionService service) =>
        {
            var permissions = await service.GetByUserIdAsync(userId);
            return Results.Ok(permissions);
        })
        .WithName("GetUserPermissionsByUserId")
        .RequireAuthorization("Endpoint:GET:/api/userpermissions/user/{userId}")
        .Produces<List<UserPermissionDto>>(200)
        .Produces(403);

        // Get current user's own permissions (accessible to all authenticated users)
        group.MapGet("/me", async (HttpContext httpContext, IUserPermissionService service) =>
        {
            var username = httpContext.User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
                return Results.Unauthorized();

            var allPermissions = await service.GetAllAsync(username);
            var userPermissions = allPermissions.Where(p => p.AccountName == username).ToList();
            return Results.Ok(userPermissions);
        })
        .WithName("GetMyPermissions")
        .RequireAuthorization("Endpoint:GET:/api/userpermissions/me")
        .Produces<List<UserPermissionDto>>(200)
        .Produces(401);

        // ========================================
        // WRITE OPERATIONS
        // ========================================

        group.MapPost("/", async (CreateUserPermissionDto dto, IUserPermissionService service) =>
        {
            var created = await service.CreateAsync(dto);
            return Results.Created($"/api/userpermissions/{created.Id}", created);
        })
        .WithName("CreateUserPermission")
        .RequireAuthorization("Endpoint:POST:/api/userpermissions/")
        .Produces<UserPermissionDto>(201)
        .Produces(400)
        .Produces(403);

        group.MapPut("/{id}", async (int id, UpdateUserPermissionDto dto, IUserPermissionService service) =>
        {
            if (id != dto.Id)
                return Results.BadRequest(new { error = "ID mismatch between URL and body" });

            var updated = await service.UpdateAsync(dto);
            return Results.Ok(updated);
        })
        .WithName("UpdateUserPermission")
        .RequireAuthorization("Endpoint:PUT:/api/userpermissions/{id}")
        .Produces<UserPermissionDto>(200)
        .Produces(400)
        .Produces(403)
        .Produces(404);

        group.MapDelete("/{id}", async (int id, IUserPermissionService service) =>
        {
            await service.DeleteAsync(id);
            return Results.NoContent();
        })
        .WithName("DeleteUserPermission")
        .RequireAuthorization("Endpoint:DELETE:/api/userpermissions/{id}")
        .Produces(204)
        .Produces(403)
        .Produces(404);

        group.MapDelete("/user/{userId}", async (int userId, IUserPermissionService service) =>
        {
            await service.DeleteUserAsync(userId);
            return Results.NoContent();
        })
        .WithName("DeleteDocuScanUser")
        .RequireAuthorization("Endpoint:DELETE:/api/userpermissions/user/{userId}")
        .Produces(204)
        .Produces(403)
        .Produces(404);

        group.MapPost("/user", async (CreateDocuScanUserDto dto, IUserPermissionService service) =>
        {
            var created = await service.CreateUserAsync(dto);
            return Results.Created($"/api/userpermissions/users", created);
        })
        .WithName("CreateDocuScanUser")
        .RequireAuthorization("Endpoint:POST:/api/userpermissions/user")
        .Produces<DocuScanUserDto>(201)
        .Produces(400)
        .Produces(403);

        group.MapPut("/user/{userId}", async (int userId, UpdateDocuScanUserDto dto, IUserPermissionService service) =>
        {
            if (userId != dto.UserId)
                return Results.BadRequest(new { error = "ID mismatch between URL and body" });

            var updated = await service.UpdateUserAsync(dto);
            return Results.Ok(updated);
        })
        .WithName("UpdateDocuScanUser")
        .RequireAuthorization("Endpoint:PUT:/api/userpermissions/user/{userId}")
        .Produces<DocuScanUserDto>(200)
        .Produces(400)
        .Produces(403)
        .Produces(404);

        // Batch update document type permissions for a user (SuperUser only)
        group.MapPost("/user/{userId}/batch-document-types", async (int userId, BatchUpdateDocumentTypePermissionsDto dto, IUserPermissionService service) =>
        {
            if (userId != dto.UserId)
                return Results.BadRequest(new { error = "ID mismatch between URL and body" });

            var result = await service.BatchUpdateDocumentTypePermissionsAsync(dto);
            return Results.Ok(result);
        })
        .WithName("BatchUpdateDocumentTypePermissions")
        .RequireAuthorization("Endpoint:POST:/api/userpermissions/user/{userId}/batch-document-types")
        .Produces<BatchUpdateResultDto>(200)
        .Produces(400)
        .Produces(403)
        .Produces(404);
    }
}
