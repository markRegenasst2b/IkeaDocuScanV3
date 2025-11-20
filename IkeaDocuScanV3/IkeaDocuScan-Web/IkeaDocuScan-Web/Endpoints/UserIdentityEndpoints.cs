using IkeaDocuScan.Shared.DTOs.UserPermissions;

namespace IkeaDocuScan_Web.Endpoints;

/// <summary>
/// API endpoints for user identity information
/// Uses dynamic database-driven authorization
/// </summary>
public static class UserIdentityEndpoints
{
    public static void MapUserIdentityEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/user")
            .RequireAuthorization()  // Base authentication required
            .WithTags("UserIdentity");

        group.MapGet("/identity", (HttpContext httpContext) =>
        {
            var user = httpContext.User;

            var identityDto = new UserIdentityDto
            {
                UserName = user.Identity?.Name,
                AuthenticationType = user.Identity?.AuthenticationType,
                IsAuthenticated = user.Identity?.IsAuthenticated ?? false,
                Claims = user.Claims
                    .Select(c => new UserClaimDto
                    {
                        Type = c.Type,
                        Value = c.Value
                    })
                    .OrderBy(c => c.Type)
                    .ToList()
            };

            return Results.Ok(identityDto);
        })
        .WithName("GetUserIdentity")
        .RequireAuthorization("Endpoint:GET:/api/user/identity")
        .Produces<UserIdentityDto>(200);
    }
}
