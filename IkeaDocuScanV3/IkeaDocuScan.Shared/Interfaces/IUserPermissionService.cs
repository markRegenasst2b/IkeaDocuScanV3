using IkeaDocuScan.Shared.DTOs.UserPermissions;

namespace IkeaDocuScan.Shared.Interfaces;

/// <summary>
/// Service interface for UserPermission operations
/// </summary>
public interface IUserPermissionService
{
    /// <summary>
    /// Get all DocuScan users, optionally filtered by account name
    /// </summary>
    Task<List<DocuScanUserDto>> GetAllUsersAsync(string? accountNameFilter = null);

    /// <summary>
    /// Get all user permissions, optionally filtered by account name
    /// </summary>
    Task<List<UserPermissionDto>> GetAllAsync(string? accountNameFilter = null);

    /// <summary>
    /// Get a user permission by ID
    /// </summary>
    Task<UserPermissionDto?> GetByIdAsync(int id);

    /// <summary>
    /// Get all permissions for a specific user
    /// </summary>
    Task<List<UserPermissionDto>> GetByUserIdAsync(int userId);

    /// <summary>
    /// Create a new user permission
    /// </summary>
    Task<UserPermissionDto> CreateAsync(CreateUserPermissionDto dto);

    /// <summary>
    /// Update an existing user permission
    /// </summary>
    Task<UserPermissionDto> UpdateAsync(UpdateUserPermissionDto dto);

    /// <summary>
    /// Delete a user permission
    /// </summary>
    Task DeleteAsync(int id);
}
