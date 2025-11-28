namespace IkeaDocuScan.Shared.DTOs.AccessAudit;

/// <summary>
/// DTO for user search results in access audit
/// </summary>
public class AccessAuditUserSearchDto
{
    public int UserId { get; set; }
    public string AccountName { get; set; } = string.Empty;
    public DateTime? LastLogon { get; set; }
    public bool IsSuperUser { get; set; }
    public bool HasGlobalAccess { get; set; }
    public int PermissionCount { get; set; }
}

/// <summary>
/// Search/filter parameters for user search
/// </summary>
public class AccessAuditUserSearchRequest
{
    public string? AccountNameFilter { get; set; }
    public bool? ShowOnlyActiveUsers { get; set; }
    public bool? ShowOnlySuperUsers { get; set; }
    public bool? ShowOnlyGlobalAccess { get; set; }
    public int? ActiveDaysThreshold { get; set; } = 90;
}
