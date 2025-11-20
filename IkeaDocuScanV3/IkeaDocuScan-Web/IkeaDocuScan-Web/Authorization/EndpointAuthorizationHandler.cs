using IkeaDocuScan.Shared.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace IkeaDocuScan_Web.Authorization;

/// <summary>
/// Authorization handler that checks endpoint permissions against database
/// Uses endpoint metadata to get the route template with parameters like {id}
/// </summary>
public class EndpointAuthorizationHandler : AuthorizationHandler<EndpointAuthorizationRequirement>
{
    private readonly IEndpointAuthorizationService _authService;
    private readonly ILogger<EndpointAuthorizationHandler> _logger;

    public EndpointAuthorizationHandler(
        IEndpointAuthorizationService authService,
        ILogger<EndpointAuthorizationHandler> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        EndpointAuthorizationRequirement requirement)
    {
        // Get the HTTP context
        if (context.Resource is not HttpContext httpContext)
        {
            _logger.LogWarning("Authorization context resource is not HttpContext");
            return;
        }

        // Get the endpoint metadata (this contains the route template)
        var endpoint = httpContext.GetEndpoint();
        if (endpoint == null)
        {
            _logger.LogWarning("No endpoint found in HTTP context");
            return;
        }

        // Get the route pattern from endpoint metadata
        var routeEndpoint = endpoint as RouteEndpoint;
        if (routeEndpoint == null)
        {
            _logger.LogWarning("Endpoint is not a RouteEndpoint");
            return;
        }

        var method = httpContext.Request.Method;
        var routePattern = routeEndpoint.RoutePattern.RawText ?? "";

        _logger.LogDebug("Checking authorization for {Method} {RoutePattern}", method, routePattern);

        // Get user roles from claims
        var userRoles = context.User.Claims
            .Where(c => c.Type == System.Security.Claims.ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList();

        if (!userRoles.Any())
        {
            _logger.LogWarning("User {User} has no roles", context.User.Identity?.Name);
            return;
        }

        // Check if user has access
        var hasAccess = await _authService.CheckAccessAsync(method, routePattern, userRoles);

        if (hasAccess)
        {
            _logger.LogDebug("User {User} authorized for {Method} {RoutePattern}",
                context.User.Identity?.Name, method, routePattern);
            context.Succeed(requirement);
        }
        else
        {
            _logger.LogWarning("User {User} with roles [{Roles}] denied access to {Method} {RoutePattern}",
                context.User.Identity?.Name, string.Join(", ", userRoles), method, routePattern);
        }
    }
}

/// <summary>
/// Authorization requirement for endpoint-based authorization
/// </summary>
public class EndpointAuthorizationRequirement : IAuthorizationRequirement
{
    public string Method { get; }
    public string Route { get; }

    public EndpointAuthorizationRequirement(string method, string route)
    {
        Method = method;
        Route = route;
    }
}
