using IkeaDocuScan.Shared.Models.Authorization;
using IkeaDocuScan.Shared.DTOs.Authorization;

namespace IkeaDocuScan.Shared.Interfaces;

/// <summary>
/// Service for accessing current user information and permissions
/// Scoped per request - loads once and caches for the request duration
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Get the current authenticated user with permissions
    /// </summary>
    Task<CurrentUser> GetCurrentUserAsync();

    /// <summary>
    /// Check if current user is a super user (synchronous, uses cached data)
    /// </summary>
    bool IsSuperUser { get; }

    /// <summary>
    /// Check if current user has any access to the system
    /// </summary>
    bool HasAccess { get; }

    /// <summary>
    /// Get current user ID (synchronous, uses cached data)
    /// </summary>
    int? UserId { get; }

    /// <summary>
    /// Request access for a user (creates DocuScanUser record if not exists)
    /// </summary>
    Task<AccessRequestResult> RequestAccessAsync(string username, string? reason = null);

    /// <summary>
    /// Update last logon timestamp for current user
    /// </summary>
    Task UpdateLastLogonAsync();

    /// <summary>
    /// Invalidate cached user data (use when permissions change)
    /// </summary>
    void InvalidateCache();
}
