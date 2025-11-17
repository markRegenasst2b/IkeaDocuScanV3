namespace IkeaDocuScan.Shared.Interfaces;

/// <summary>
/// Service for checking endpoint authorization dynamically from database
/// Used by DynamicAuthorizationPolicyProvider
/// </summary>
public interface IEndpointAuthorizationService
{
    /// <summary>
    /// Get the list of roles allowed to access a specific endpoint
    /// Results are cached for performance
    /// </summary>
    /// <param name="httpMethod">HTTP method (GET, POST, PUT, DELETE)</param>
    /// <param name="route">Route template (e.g., /api/documents/, /api/documents/{id})</param>
    /// <returns>List of role names (Reader, Publisher, ADAdmin, SuperUser)</returns>
    Task<List<string>> GetAllowedRolesAsync(string httpMethod, string route);

    /// <summary>
    /// Check if a user with specific roles can access an endpoint
    /// </summary>
    /// <param name="httpMethod">HTTP method</param>
    /// <param name="route">Route template</param>
    /// <param name="userRoles">User's role names</param>
    /// <returns>True if user has access, false otherwise</returns>
    Task<bool> CheckAccessAsync(string httpMethod, string route, IEnumerable<string> userRoles);

    /// <summary>
    /// Invalidate the authorization cache
    /// Call this after updating endpoint permissions
    /// </summary>
    Task InvalidateCacheAsync();

    /// <summary>
    /// Get all endpoints with their role permissions
    /// </summary>
    Task<List<DTOs.EndpointRegistryDto>> GetAllEndpointsAsync();

    /// <summary>
    /// Get endpoint by ID
    /// </summary>
    Task<DTOs.EndpointRegistryDto?> GetEndpointByIdAsync(int endpointId);

    /// <summary>
    /// Sync endpoints from code to database (for initial setup or updates)
    /// </summary>
    Task SyncEndpointsAsync();
}
