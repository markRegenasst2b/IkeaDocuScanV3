using IkeaDocuScan.Shared.Interfaces;
using IkeaDocuScan.Shared.DTOs.UserPermissions;

namespace IkeaDocuScan_Web.Endpoints;

public static class UserPermissionEndpoints
{
    public static void MapUserPermissionEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/userpermissions")
            .RequireAuthorization()
            .WithTags("UserPermissions");

        group.MapGet("/", async (string? accountNameFilter, IUserPermissionService service) =>
        {
            var permissions = await service.GetAllAsync(accountNameFilter);
            return Results.Ok(permissions);
        })
        .WithName("GetAllUserPermissions")
        .Produces<List<UserPermissionDto>>(200);

        group.MapGet("/users", async (string? accountNameFilter, IUserPermissionService service) =>
        {
            var users = await service.GetAllUsersAsync(accountNameFilter);
            return Results.Ok(users);
        })
        .WithName("GetAllDocuScanUsers")
        .Produces<List<DocuScanUserDto>>(200);

        group.MapGet("/{id}", async (int id, IUserPermissionService service) =>
        {
            var permission = await service.GetByIdAsync(id);
            if (permission == null)
                return Results.NotFound(new { error = $"UserPermission with ID {id} not found" });

            return Results.Ok(permission);
        })
        .WithName("GetUserPermissionById")
        .Produces<UserPermissionDto>(200)
        .Produces(404);

        group.MapGet("/user/{userId}", async (int userId, IUserPermissionService service) =>
        {
            var permissions = await service.GetByUserIdAsync(userId);
            return Results.Ok(permissions);
        })
        .WithName("GetUserPermissionsByUserId")
        .Produces<List<UserPermissionDto>>(200);

        group.MapPost("/", async (CreateUserPermissionDto dto, IUserPermissionService service) =>
        {
            var created = await service.CreateAsync(dto);
            return Results.Created($"/api/userpermissions/{created.Id}", created);
        })
        .WithName("CreateUserPermission")
        .Produces<UserPermissionDto>(201)
        .Produces(400);

        group.MapPut("/{id}", async (int id, UpdateUserPermissionDto dto, IUserPermissionService service) =>
        {
            if (id != dto.Id)
                return Results.BadRequest(new { error = "ID mismatch between URL and body" });

            var updated = await service.UpdateAsync(dto);
            return Results.Ok(updated);
        })
        .WithName("UpdateUserPermission")
        .Produces<UserPermissionDto>(200)
        .Produces(400)
        .Produces(404);

        group.MapDelete("/{id}", async (int id, IUserPermissionService service) =>
        {
            await service.DeleteAsync(id);
            return Results.NoContent();
        })
        .WithName("DeleteUserPermission")
        .Produces(204)
        .Produces(404);
    }
}
