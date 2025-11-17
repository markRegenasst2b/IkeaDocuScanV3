using IkeaDocuScan.Shared.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace IkeaDocuScan_Web.Authorization;

/// <summary>
/// Dynamic authorization policy provider that reads endpoint permissions from database
/// Enables policy names like "Endpoint:GET:/api/documents/" to be resolved dynamically
/// </summary>
public class DynamicAuthorizationPolicyProvider : IAuthorizationPolicyProvider
{
    private readonly DefaultAuthorizationPolicyProvider _fallbackProvider;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DynamicAuthorizationPolicyProvider> _logger;

    public DynamicAuthorizationPolicyProvider(
        IOptions<AuthorizationOptions> options,
        IServiceProvider serviceProvider,
        ILogger<DynamicAuthorizationPolicyProvider> logger)
    {
        _fallbackProvider = new DefaultAuthorizationPolicyProvider(options);
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync()
    {
        return _fallbackProvider.GetDefaultPolicyAsync();
    }

    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync()
    {
        return _fallbackProvider.GetFallbackPolicyAsync();
    }

    public async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        // Check if this is a dynamic endpoint authorization policy
        if (policyName.StartsWith("Endpoint:", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                var parts = policyName.Split(':', 3);
                if (parts.Length == 3)
                {
                    var method = parts[1];
                    var route = parts[2];

                    _logger.LogDebug("Resolving dynamic policy for {PolicyName}", policyName);

                    // Create a scoped service provider to get the authorization service
                    using var scope = _serviceProvider.CreateScope();
                    var authService = scope.ServiceProvider.GetRequiredService<IEndpointAuthorizationService>();

                    // Get allowed roles from database
                    var allowedRoles = await authService.GetAllowedRolesAsync(method, route);

                    if (allowedRoles.Any())
                    {
                        _logger.LogInformation("Dynamic policy {PolicyName} resolved to roles: {Roles}",
                            policyName, string.Join(", ", allowedRoles));

                        // Build a policy that requires any of the allowed roles
                        return new AuthorizationPolicyBuilder()
                            .RequireAuthenticatedUser()
                            .RequireRole(allowedRoles.ToArray())
                            .Build();
                    }
                    else
                    {
                        _logger.LogWarning("No roles configured for endpoint {Method} {Route} - denying access",
                            method, route);

                        // No roles configured - deny access by creating an impossible-to-satisfy policy
                        return new AuthorizationPolicyBuilder()
                            .RequireAssertion(context => false)
                            .Build();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resolving dynamic policy {PolicyName}", policyName);
                // Fall through to default provider
            }
        }

        // For non-endpoint policies or if dynamic resolution fails, use the fallback provider
        return await _fallbackProvider.GetPolicyAsync(policyName);
    }
}
