using ExcelReporting.Models;
using ExcelReporting.Services;
using IkeaDocuScan.Infrastructure.Data;
using IkeaDocuScan.Shared.DTOs.AccessAudit;
using IkeaDocuScan.Shared.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IkeaDocuScan_Web.Services;

/// <summary>
/// Service for access audit functionality - provides read-only audit views of document type permissions
/// </summary>
public class AccessAuditService : IAccessAuditService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    private readonly IExcelExportService _excelExportService;
    private readonly ILogger<AccessAuditService> _logger;

    public AccessAuditService(
        IDbContextFactory<AppDbContext> contextFactory,
        IExcelExportService excelExportService,
        ILogger<AccessAuditService> logger)
    {
        _contextFactory = contextFactory;
        _excelExportService = excelExportService;
        _logger = logger;
    }

    public async Task<DocumentTypeAccessAuditDto> GetDocumentTypeAccessAsync(
        int documentTypeId,
        bool? showOnlyActiveUsers = null,
        bool? showOnlySuperUsers = null,
        string? accountNameFilter = null,
        int activeDaysThreshold = 90)
    {
        _logger.LogInformation(
            "Getting access audit for document type {DocumentTypeId} with filters: ActiveOnly={ActiveOnly}, SuperUserOnly={SuperUserOnly}, NameFilter={NameFilter}",
            documentTypeId, showOnlyActiveUsers, showOnlySuperUsers, accountNameFilter);

        await using var context = await _contextFactory.CreateDbContextAsync();

        // Get the document type name
        var documentType = await context.DocumentTypes
            .AsNoTracking()
            .FirstOrDefaultAsync(dt => dt.DtId == documentTypeId);

        if (documentType == null)
        {
            _logger.LogWarning("Document type {DocumentTypeId} not found", documentTypeId);
            return new DocumentTypeAccessAuditDto
            {
                DocumentTypeId = documentTypeId,
                DocumentTypeName = "Unknown"
            };
        }

        var activeThresholdDate = DateTime.UtcNow.AddDays(-activeDaysThreshold);

        // Get users with GLOBAL access (DocumentTypeId is null)
        var globalAccessQuery = context.UserPermissions
            .Include(up => up.User)
            .AsNoTracking()
            .Where(up => up.DocumentTypeId == null);

        // Get users with DIRECT access to this specific document type
        var directAccessQuery = context.UserPermissions
            .Include(up => up.User)
            .AsNoTracking()
            .Where(up => up.DocumentTypeId == documentTypeId);

        // Apply filters to both queries
        if (showOnlyActiveUsers == true)
        {
            globalAccessQuery = globalAccessQuery.Where(up => up.User.LastLogon >= activeThresholdDate);
            directAccessQuery = directAccessQuery.Where(up => up.User.LastLogon >= activeThresholdDate);
        }

        if (showOnlySuperUsers == true)
        {
            globalAccessQuery = globalAccessQuery.Where(up => up.User.IsSuperUser);
            directAccessQuery = directAccessQuery.Where(up => up.User.IsSuperUser);
        }

        if (!string.IsNullOrWhiteSpace(accountNameFilter))
        {
            var filter = accountNameFilter.ToLower();
            globalAccessQuery = globalAccessQuery.Where(up => up.User.AccountName.ToLower().Contains(filter));
            directAccessQuery = directAccessQuery.Where(up => up.User.AccountName.ToLower().Contains(filter));
        }

        // Execute queries
        var globalAccessUsers = await globalAccessQuery
            .OrderByDescending(up => up.User.LastLogon)
            .ThenBy(up => up.User.AccountName)
            .Select(up => new AccessAuditUserDto
            {
                UserId = up.UserId,
                AccountName = up.User.AccountName,
                LastLogon = up.User.LastLogon,
                IsSuperUser = up.User.IsSuperUser,
                PermissionId = up.Id
            })
            .ToListAsync();

        var directAccessUsers = await directAccessQuery
            .OrderByDescending(up => up.User.LastLogon)
            .ThenBy(up => up.User.AccountName)
            .Select(up => new AccessAuditUserDto
            {
                UserId = up.UserId,
                AccountName = up.User.AccountName,
                LastLogon = up.User.LastLogon,
                IsSuperUser = up.User.IsSuperUser,
                PermissionId = up.Id
            })
            .ToListAsync();

        var result = new DocumentTypeAccessAuditDto
        {
            DocumentTypeId = documentTypeId,
            DocumentTypeName = documentType.DtName,
            GlobalAccessUsers = globalAccessUsers,
            DirectAccessUsers = directAccessUsers,
            GlobalAccessUserCount = globalAccessUsers.Count,
            DirectAccessUserCount = directAccessUsers.Count,
            TotalUsersWithAccess = globalAccessUsers.Count + directAccessUsers.Count
        };

        _logger.LogInformation(
            "Access audit for document type {DocumentTypeName}: {GlobalCount} global, {DirectCount} direct, {TotalCount} total",
            documentType.DtName, result.GlobalAccessUserCount, result.DirectAccessUserCount, result.TotalUsersWithAccess);

        return result;
    }

    public async Task<UserAccessAuditDto?> GetUserAccessAsync(int userId)
    {
        _logger.LogInformation("Getting access audit for user {UserId}", userId);

        await using var context = await _contextFactory.CreateDbContextAsync();

        // Get the user
        var user = await context.DocuScanUsers
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.UserId == userId);

        if (user == null)
        {
            _logger.LogWarning("User {UserId} not found", userId);
            return null;
        }

        // Get user's permissions
        var userPermissions = await context.UserPermissions
            .AsNoTracking()
            .Where(up => up.UserId == userId)
            .ToListAsync();

        // Check for global access (DocumentTypeId is null)
        var globalAccessPermission = userPermissions.FirstOrDefault(up => up.DocumentTypeId == null);
        var hasGlobalAccess = globalAccessPermission != null;

        // Get all enabled document types
        var allDocumentTypes = await context.DocumentTypes
            .AsNoTracking()
            .Where(dt => dt.IsEnabled)
            .OrderBy(dt => dt.DtName)
            .ToListAsync();

        // Get document types with direct access
        var directAccessDocTypeIds = userPermissions
            .Where(up => up.DocumentTypeId.HasValue)
            .Select(up => up.DocumentTypeId!.Value)
            .ToHashSet();

        List<AccessAuditDocumentTypeDto> documentTypesWithAccess;
        List<AccessAuditDocumentTypeDto> documentTypesWithoutAccess;

        if (hasGlobalAccess)
        {
            // User has access to ALL document types via global permission
            documentTypesWithAccess = allDocumentTypes
                .Select(dt => new AccessAuditDocumentTypeDto
                {
                    DocumentTypeId = dt.DtId,
                    DocumentTypeName = dt.DtName,
                    PermissionId = globalAccessPermission!.Id
                })
                .ToList();

            documentTypesWithoutAccess = new List<AccessAuditDocumentTypeDto>();
        }
        else
        {
            // User has specific document type permissions
            documentTypesWithAccess = allDocumentTypes
                .Where(dt => directAccessDocTypeIds.Contains(dt.DtId))
                .Select(dt => new AccessAuditDocumentTypeDto
                {
                    DocumentTypeId = dt.DtId,
                    DocumentTypeName = dt.DtName,
                    PermissionId = userPermissions.First(up => up.DocumentTypeId == dt.DtId).Id
                })
                .ToList();

            documentTypesWithoutAccess = allDocumentTypes
                .Where(dt => !directAccessDocTypeIds.Contains(dt.DtId))
                .Select(dt => new AccessAuditDocumentTypeDto
                {
                    DocumentTypeId = dt.DtId,
                    DocumentTypeName = dt.DtName,
                    PermissionId = null
                })
                .ToList();
        }

        var result = new UserAccessAuditDto
        {
            UserId = user.UserId,
            AccountName = user.AccountName,
            LastLogon = user.LastLogon,
            IsSuperUser = user.IsSuperUser,
            HasGlobalAccess = hasGlobalAccess,
            GlobalAccessPermissionId = globalAccessPermission?.Id,
            DocumentTypesWithAccess = documentTypesWithAccess,
            DocumentTypesWithoutAccess = documentTypesWithoutAccess,
            TotalDocumentTypesWithAccess = documentTypesWithAccess.Count,
            TotalDocumentTypesWithoutAccess = documentTypesWithoutAccess.Count
        };

        _logger.LogInformation(
            "Access audit for user {AccountName}: GlobalAccess={GlobalAccess}, WithAccess={WithAccess}, WithoutAccess={WithoutAccess}",
            user.AccountName, hasGlobalAccess, result.TotalDocumentTypesWithAccess, result.TotalDocumentTypesWithoutAccess);

        return result;
    }

    public async Task<List<AccessAuditUserSearchDto>> SearchUsersAsync(AccessAuditUserSearchRequest request)
    {
        _logger.LogInformation(
            "Searching users with filters: NameFilter={NameFilter}, ActiveOnly={ActiveOnly}, SuperUserOnly={SuperUserOnly}, GlobalAccessOnly={GlobalAccessOnly}",
            request.AccountNameFilter, request.ShowOnlyActiveUsers, request.ShowOnlySuperUsers, request.ShowOnlyGlobalAccess);

        await using var context = await _contextFactory.CreateDbContextAsync();

        var activeDaysThreshold = request.ActiveDaysThreshold ?? 90;
        var activeThresholdDate = DateTime.UtcNow.AddDays(-activeDaysThreshold);

        var query = context.DocuScanUsers
            .Include(u => u.UserPermissions)
            .AsNoTracking();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(request.AccountNameFilter) && request.AccountNameFilter.Length >= 3)
        {
            var filter = request.AccountNameFilter.ToLower();
            query = query.Where(u => u.AccountName.ToLower().Contains(filter));
        }

        if (request.ShowOnlyActiveUsers == true)
        {
            query = query.Where(u => u.LastLogon >= activeThresholdDate);
        }

        if (request.ShowOnlySuperUsers == true)
        {
            query = query.Where(u => u.IsSuperUser);
        }

        if (request.ShowOnlyGlobalAccess == true)
        {
            query = query.Where(u => u.UserPermissions.Any(up => up.DocumentTypeId == null));
        }

        var users = await query
            .OrderByDescending(u => u.LastLogon)
            .ThenBy(u => u.AccountName)
            .Take(100) // Limit results
            .ToListAsync();

        var result = users.Select(u => new AccessAuditUserSearchDto
        {
            UserId = u.UserId,
            AccountName = u.AccountName,
            LastLogon = u.LastLogon,
            IsSuperUser = u.IsSuperUser,
            HasGlobalAccess = u.UserPermissions.Any(up => up.DocumentTypeId == null),
            PermissionCount = u.UserPermissions.Count
        }).ToList();

        _logger.LogInformation("User search returned {Count} results", result.Count);

        return result;
    }

    public async Task<byte[]> ExportDocumentTypeAccessToExcelAsync(
        int documentTypeId,
        bool? showOnlyActiveUsers = null,
        bool? showOnlySuperUsers = null,
        string? accountNameFilter = null,
        int activeDaysThreshold = 90)
    {
        _logger.LogInformation("Exporting document type access audit to Excel for document type {DocumentTypeId}", documentTypeId);

        var auditData = await GetDocumentTypeAccessAsync(
            documentTypeId,
            showOnlyActiveUsers,
            showOnlySuperUsers,
            accountNameFilter,
            activeDaysThreshold);

        var exportData = new List<DocumentTypeAccessExportDto>();

        // Add global access users
        foreach (var user in auditData.GlobalAccessUsers)
        {
            exportData.Add(new DocumentTypeAccessExportDto
            {
                AccountName = user.AccountName,
                LastLogon = user.LastLogon,
                IsSuperUser = user.IsSuperUser,
                AccessType = "Global (All Document Types)",
                DocumentTypeName = auditData.DocumentTypeName
            });
        }

        // Add direct access users
        foreach (var user in auditData.DirectAccessUsers)
        {
            exportData.Add(new DocumentTypeAccessExportDto
            {
                AccountName = user.AccountName,
                LastLogon = user.LastLogon,
                IsSuperUser = user.IsSuperUser,
                AccessType = "Direct",
                DocumentTypeName = auditData.DocumentTypeName
            });
        }

        // Prepare all records for export
        foreach (var record in exportData)
        {
            record.PrepareForExport();
        }

        // Sanitize sheet name - Excel doesn't allow: \ / ? * [ ] and max 31 chars
        var sanitizedName = SanitizeSheetName(auditData.DocumentTypeName);

        var options = new ExcelExportOptions
        {
            SheetName = $"Access - {sanitizedName}",
            IncludeHeader = true,
            AutoFitColumns = true
        };

        using var stream = await _excelExportService.GenerateExcelAsync(exportData, options);
        return stream.ToArray();
    }

    public async Task<byte[]> ExportUserAccessToExcelAsync(int userId)
    {
        _logger.LogInformation("Exporting user access audit to Excel for user {UserId}", userId);

        var auditData = await GetUserAccessAsync(userId);
        if (auditData == null)
        {
            return Array.Empty<byte>();
        }

        var exportData = new List<UserAccessExportDto>();

        // Add document types with access
        foreach (var docType in auditData.DocumentTypesWithAccess)
        {
            exportData.Add(new UserAccessExportDto
            {
                DocumentTypeName = docType.DocumentTypeName,
                HasAccess = true,
                AccessType = auditData.HasGlobalAccess ? "Global (All Document Types)" : "Direct",
                AccountName = auditData.AccountName
            });
        }

        // Add document types without access
        foreach (var docType in auditData.DocumentTypesWithoutAccess)
        {
            exportData.Add(new UserAccessExportDto
            {
                DocumentTypeName = docType.DocumentTypeName,
                HasAccess = false,
                AccessType = "No Access",
                AccountName = auditData.AccountName
            });
        }

        // Prepare all records for export
        foreach (var record in exportData)
        {
            record.PrepareForExport();
        }

        // Sanitize sheet name - Excel doesn't allow: \ / ? * [ ] and max 31 chars
        var sanitizedName = SanitizeSheetName(auditData.AccountName);

        var options = new ExcelExportOptions
        {
            SheetName = $"Access - {sanitizedName}",
            IncludeHeader = true,
            AutoFitColumns = true
        };

        using var stream = await _excelExportService.GenerateExcelAsync(exportData, options);
        return stream.ToArray();
    }

    /// <summary>
    /// Sanitizes a string for use as an Excel sheet name
    /// Excel sheet names cannot contain: \ / ? * [ ] : and must be max 31 characters
    /// </summary>
    private static string SanitizeSheetName(string name)
    {
        if (string.IsNullOrEmpty(name))
            return "Sheet";

        // Replace invalid characters
        var sanitized = name
            .Replace("\\", "_")
            .Replace("/", "_")
            .Replace("?", "_")
            .Replace("*", "_")
            .Replace("[", "_")
            .Replace("]", "_")
            .Replace(":", "_");

        // Truncate to fit within 31 char limit (accounting for "Access - " prefix = 9 chars, so max 22)
        if (sanitized.Length > 22)
            sanitized = sanitized.Substring(0, 22);

        return sanitized;
    }
}
