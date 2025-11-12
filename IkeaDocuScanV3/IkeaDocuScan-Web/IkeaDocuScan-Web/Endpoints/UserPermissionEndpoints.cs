using IkeaDocuScan.Shared.Interfaces;
using IkeaDocuScan.Shared.DTOs.UserPermissions;

namespace IkeaDocuScan_Web.Endpoints;

public static class UserPermissionEndpoints
{
    public static void MapUserPermissionEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/userpermissions")
            .RequireAuthorization("HasAccess")  // Base policy - user must have access to system
            .WithTags("UserPermissions");

        // ========================================
        // READ OPERATIONS - SuperUser only (viewing user permissions is sensitive)
        // ========================================

        group.MapGet("/", async (string? accountNameFilter, IUserPermissionService service) =>
        {
            var permissions = await service.GetAllAsync(accountNameFilter);
            return Results.Ok(permissions);
        })
        .WithName("GetAllUserPermissions")
        .RequireAuthorization(policy => policy.RequireRole("SuperUser"))
        .Produces<List<UserPermissionDto>>(200)
        .Produces(403);

        group.MapGet("/users", async (string? accountNameFilter, IUserPermissionService service) =>
        {
            var users = await service.GetAllUsersAsync(accountNameFilter);
            return Results.Ok(users);
        })
        .WithName("GetAllDocuScanUsers")
        .RequireAuthorization(policy => policy.RequireRole("SuperUser"))
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
        .RequireAuthorization(policy => policy.RequireRole("SuperUser"))
        .Produces<UserPermissionDto>(200)
        .Produces(403)
        .Produces(404);

        group.MapGet("/user/{userId}", async (int userId, IUserPermissionService service) =>
        {
            var permissions = await service.GetByUserIdAsync(userId);
            return Results.Ok(permissions);
        })
        .WithName("GetUserPermissionsByUserId")
        .RequireAuthorization(policy => policy.RequireRole("SuperUser"))
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
        .Produces<List<UserPermissionDto>>(200)
        .Produces(401);

        // ========================================
        // WRITE OPERATIONS - SuperUser only
        // ========================================

        group.MapPost("/", async (CreateUserPermissionDto dto, IUserPermissionService service) =>
        {
            var created = await service.CreateAsync(dto);
            return Results.Created($"/api/userpermissions/{created.Id}", created);
        })
        .WithName("CreateUserPermission")
        .RequireAuthorization(policy => policy.RequireRole("SuperUser"))
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
        .RequireAuthorization(policy => policy.RequireRole("SuperUser"))
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
        .RequireAuthorization(policy => policy.RequireRole("SuperUser"))
        .Produces(204)
        .Produces(403)
        .Produces(404);

        group.MapDelete("/user/{userId}", async (int userId, IUserPermissionService service) =>
        {
            await service.DeleteUserAsync(userId);
            return Results.NoContent();
        })
        .WithName("DeleteDocuScanUser")
        .RequireAuthorization(policy => policy.RequireRole("SuperUser"))
        .Produces(204)
        .Produces(403)
        .Produces(404);

        group.MapPost("/user", async (CreateDocuScanUserDto dto, IUserPermissionService service) =>
        {
            var created = await service.CreateUserAsync(dto);
            return Results.Created($"/api/userpermissions/users", created);
        })
        .WithName("CreateDocuScanUser")
        .RequireAuthorization(policy => policy.RequireRole("SuperUser"))
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
        .RequireAuthorization(policy => policy.RequireRole("SuperUser"))
        .Produces<DocuScanUserDto>(200)
        .Produces(400)
        .Produces(403)
        .Produces(404);
    }
}
