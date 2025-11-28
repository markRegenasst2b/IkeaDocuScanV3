using IkeaDocuScan.Shared.DTOs.AccessAudit;

namespace IkeaDocuScan.Shared.Interfaces;

/// <summary>
/// Service interface for access audit functionality
/// </summary>
public interface IAccessAuditService
{
    /// <summary>
    /// Get all users who have access to a specific document type
    /// </summary>
    /// <param name="documentTypeId">The document type ID to check</param>
    /// <param name="showOnlyActiveUsers">Filter to only show users active within threshold</param>
    /// <param name="showOnlySuperUsers">Filter to only show super users</param>
    /// <param name="accountNameFilter">Filter by account name (contains)</param>
    /// <param name="activeDaysThreshold">Number of days to consider a user active (default 90)</param>
    Task<DocumentTypeAccessAuditDto> GetDocumentTypeAccessAsync(
        int documentTypeId,
        bool? showOnlyActiveUsers = null,
        bool? showOnlySuperUsers = null,
        string? accountNameFilter = null,
        int activeDaysThreshold = 90);

    /// <summary>
    /// Get all document types a specific user has access to
    /// </summary>
    /// <param name="userId">The user ID to check</param>
    Task<UserAccessAuditDto?> GetUserAccessAsync(int userId);

    /// <summary>
    /// Search users with optional filters
    /// </summary>
    Task<List<AccessAuditUserSearchDto>> SearchUsersAsync(AccessAuditUserSearchRequest request);

    /// <summary>
    /// Export document type access audit to Excel bytes
    /// </summary>
    Task<byte[]> ExportDocumentTypeAccessToExcelAsync(
        int documentTypeId,
        bool? showOnlyActiveUsers = null,
        bool? showOnlySuperUsers = null,
        string? accountNameFilter = null,
        int activeDaysThreshold = 90);

    /// <summary>
    /// Export user access audit to Excel bytes
    /// </summary>
    Task<byte[]> ExportUserAccessToExcelAsync(int userId);
}
