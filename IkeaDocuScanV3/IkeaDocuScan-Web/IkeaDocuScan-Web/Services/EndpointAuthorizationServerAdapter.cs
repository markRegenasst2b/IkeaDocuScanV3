using IkeaDocuScan.Shared.Interfaces;
using IkeaDocuScan_Web.Client.Services;
using System.Security.Claims;

namespace IkeaDocuScan_Web.Services;

/// <summary>
/// Server-side adapter for EndpointAuthorizationHttpService.
/// Directly uses IEndpointAuthorizationService instead of making HTTP calls.
/// This enables server-side rendered Blazor components to check endpoint access.
/// </summary>
public class EndpointAuthorizationServerAdapter : EndpointAuthorizationHttpService
{
    private readonly IEndpointAuthorizationService _authService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<EndpointAuthorizationServerAdapter> _logger;

    public EndpointAuthorizationServerAdapter(
        HttpClient httpClient,
        ILogger<EndpointAuthorizationHttpService> baseLogger,
        IEndpointAuthorizationService authService,
        IHttpContextAccessor httpContextAccessor,
        ILogger<EndpointAuthorizationServerAdapter> logger)
        : base(httpClient, baseLogger)
    {
        _authService = authService;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    /// <summary>
    /// Check if the current user has access to a specific endpoint.
    /// Uses the server-side service directly instead of HTTP calls.
    /// </summary>
    public override async Task<bool> CheckAccessAsync(string method, string route)
    {
        try
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null || !user.Identity?.IsAuthenticated == true)
            {
                _logger.LogWarning("No authenticated user found when checking endpoint access");
                return false;
            }

            var userRoles = user.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();

            return await _authService.CheckAccessAsync(method, route, userRoles);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to check endpoint access for {Method} {Route}", method, route);
            return false;
        }
    }
}
