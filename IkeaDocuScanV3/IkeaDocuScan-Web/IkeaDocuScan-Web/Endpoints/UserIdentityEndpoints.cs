using IkeaDocuScan.Shared.DTOs.UserPermissions;

namespace IkeaDocuScan_Web.Endpoints;

public static class UserIdentityEndpoints
{
    public static void MapUserIdentityEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/user")
            .RequireAuthorization()
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
        .Produces<UserIdentityDto>(200);
    }
}
