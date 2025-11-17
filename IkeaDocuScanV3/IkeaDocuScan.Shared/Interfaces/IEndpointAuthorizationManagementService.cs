namespace IkeaDocuScan.Shared.Interfaces;

/// <summary>
/// Service for managing endpoint authorization permissions
/// Used by admin UI and management endpoints
/// </summary>
public interface IEndpointAuthorizationManagementService
{
    /// <summary>
    /// Get all endpoints with their role permissions
    /// </summary>
    Task<List<DTOs.EndpointRegistryDto>> GetAllEndpointsAsync();

    /// <summary>
    /// Get endpoint by ID
    /// </summary>
    Task<DTOs.EndpointRegistryDto?> GetEndpointByIdAsync(int endpointId);

    /// <summary>
    /// Get endpoint by HTTP method and route
    /// </summary>
    Task<DTOs.EndpointRegistryDto?> GetEndpointByRouteAsync(string httpMethod, string route);

    /// <summary>
    /// Get all role names that have access to a specific endpoint
    /// </summary>
    Task<List<string>> GetEndpointRolesAsync(int endpointId);

    /// <summary>
    /// Update the roles that can access an endpoint
    /// </summary>
    /// <param name="endpointId">Endpoint ID</param>
    /// <param name="roleNames">New list of role names</param>
    /// <param name="changedBy">Username of person making the change</param>
    /// <param name="changeReason">Reason for the change</param>
    Task UpdateEndpointRolesAsync(int endpointId, List<string> roleNames, string changedBy, string? changeReason = null);

    /// <summary>
    /// Create a new endpoint registry entry
    /// </summary>
    Task<DTOs.EndpointRegistryDto> CreateEndpointAsync(DTOs.CreateEndpointRegistryDto dto, string createdBy);

    /// <summary>
    /// Update an existing endpoint registry entry
    /// </summary>
    Task<DTOs.EndpointRegistryDto> UpdateEndpointAsync(int endpointId, DTOs.UpdateEndpointRegistryDto dto, string modifiedBy);

    /// <summary>
    /// Deactivate an endpoint (soft delete)
    /// </summary>
    Task DeactivateEndpointAsync(int endpointId, string deactivatedBy, string? reason = null);

    /// <summary>
    /// Reactivate a deactivated endpoint
    /// </summary>
    Task ReactivateEndpointAsync(int endpointId, string reactivatedBy, string? reason = null);

    /// <summary>
    /// Get all available role names
    /// </summary>
    Task<List<string>> GetAvailableRolesAsync();

    /// <summary>
    /// Get audit log for endpoint permission changes
    /// </summary>
    Task<List<DTOs.PermissionChangeAuditLogDto>> GetAuditLogAsync(int? endpointId = null, DateTime? fromDate = null, DateTime? toDate = null);

    /// <summary>
    /// Invalidate the authorization cache after permission changes
    /// </summary>
    Task InvalidateCacheAsync();

    /// <summary>
    /// Sync endpoints from code to database
    /// This will create or update endpoint registry entries based on code-defined endpoints
    /// </summary>
    Task SyncEndpointsFromCodeAsync();

    /// <summary>
    /// Validate a permission change before applying it
    /// Returns validation errors if any
    /// </summary>
    Task<List<string>> ValidatePermissionChangeAsync(int endpointId, List<string> newRoles);
}
