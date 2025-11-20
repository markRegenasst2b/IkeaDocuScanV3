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

                    _logger.LogDebug("Creating dynamic authorization policy for {Method} {Route}", method, route);

                    // Create a policy that uses the EndpointAuthorizationHandler
                    // The handler will use endpoint metadata to get the actual route template
                    // and check it against the database at authorization time
                    return new AuthorizationPolicyBuilder()
                        .RequireAuthenticatedUser()
                        .AddRequirements(new EndpointAuthorizationRequirement(method, route))
                        .Build();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating dynamic policy for {PolicyName}", policyName);
                // Fall through to default provider
            }
        }

        // For non-endpoint policies or if dynamic resolution fails, use the fallback provider
        return await _fallbackProvider.GetPolicyAsync(policyName);
    }
}
