using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace IkeaDocuScan_Web.Authorization;

/// <summary>
/// Handler for UserAccessRequirement - checks if user has access to the system
/// </summary>
public class UserAccessHandler : AuthorizationHandler<UserAccessRequirement>
{
    private readonly ILogger<UserAccessHandler> _logger;

    public UserAccessHandler(ILogger<UserAccessHandler> logger)
    {
        _logger = logger;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        UserAccessRequirement requirement)
    {
        var hasAccessClaim = context.User.FindFirst("HasAccess");

        if (hasAccessClaim != null && bool.TryParse(hasAccessClaim.Value, out bool hasAccess) && hasAccess)
        {
            _logger.LogDebug("User {Username} has access", context.User.Identity?.Name);
            context.Succeed(requirement);
        }
        else
        {
            _logger.LogWarning("User {Username} does not have access", context.User.Identity?.Name);
        }

        return Task.CompletedTask;
    }
}

/// <summary>
/// Handler for SuperUserRequirement - checks if user is a super user
/// </summary>
public class SuperUserHandler : AuthorizationHandler<SuperUserRequirement>
{
    private readonly ILogger<SuperUserHandler> _logger;

    public SuperUserHandler(ILogger<SuperUserHandler> logger)
    {
        _logger = logger;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        SuperUserRequirement requirement)
    {
        var isSuperUserClaim = context.User.FindFirst("IsSuperUser");

        if (isSuperUserClaim != null && bool.TryParse(isSuperUserClaim.Value, out bool isSuperUser) && isSuperUser)
        {
            _logger.LogDebug("User {Username} is SuperUser", context.User.Identity?.Name);
            context.Succeed(requirement);
        }
        else
        {
            _logger.LogDebug("User {Username} is not SuperUser", context.User.Identity?.Name);
        }

        return Task.CompletedTask;
    }
}
