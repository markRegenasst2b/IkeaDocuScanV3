using IkeaDocuScan.Shared.DTOs;
using System.Net.Http.Json;

namespace IkeaDocuScan_Web.Client.Services;

/// <summary>
/// Client-side HTTP service for checking endpoint authorization
/// Used primarily for menu visibility checks
/// </summary>
public class EndpointAuthorizationHttpService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<EndpointAuthorizationHttpService> _logger;

    public EndpointAuthorizationHttpService(
        HttpClient httpClient,
        ILogger<EndpointAuthorizationHttpService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    /// <summary>
    /// Check if the current user has access to a specific endpoint
    /// </summary>
    /// <param name="method">HTTP method (GET, POST, etc.)</param>
    /// <param name="route">Endpoint route (e.g., /api/documents/)</param>
    /// <returns>True if user has access, false otherwise</returns>
    public virtual async Task<bool> CheckAccessAsync(string method, string route)
    {
        try
        {
            var encodedRoute = Uri.EscapeDataString(route);
            var response = await _httpClient.GetFromJsonAsync<EndpointAccessCheckResult>(
                $"/api/endpoint-authorization/check?method={method}&route={encodedRoute}"
            );

            return response?.HasAccess ?? false;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to check endpoint access for {Method} {Route}", method, route);
            return false; // Default to no access on error
        }
    }

    /// <summary>
    /// Check access to multiple endpoints in parallel
    /// Returns a dictionary of route -> hasAccess
    /// </summary>
    public async Task<Dictionary<string, bool>> CheckMultipleAccessAsync(Dictionary<string, (string Method, string Route)> endpoints)
    {
        var tasks = endpoints.Select(async kvp =>
        {
            var hasAccess = await CheckAccessAsync(kvp.Value.Method, kvp.Value.Route);
            return new KeyValuePair<string, bool>(kvp.Key, hasAccess);
        });

        var results = await Task.WhenAll(tasks);
        return results.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }

    /// <summary>
    /// Get detailed access check result with roles information
    /// </summary>
    public async Task<EndpointAccessCheckResult?> GetAccessDetailsAsync(string method, string route)
    {
        try
        {
            var encodedRoute = Uri.EscapeDataString(route);
            var response = await _httpClient.GetFromJsonAsync<EndpointAccessCheckResult>(
                $"/api/endpoint-authorization/check?method={method}&route={encodedRoute}"
            );

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get access details for {Method} {Route}", method, route);
            return null;
        }
    }
}
