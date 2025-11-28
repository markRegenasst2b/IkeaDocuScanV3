namespace IkeaDocuScan.Shared.DTOs.AccessAudit;

/// <summary>
/// Response DTO for document type access audit - shows all users with access to a specific document type
/// </summary>
public class DocumentTypeAccessAuditDto
{
    public int DocumentTypeId { get; set; }
    public string DocumentTypeName { get; set; } = string.Empty;
    public int TotalUsersWithAccess { get; set; }
    public int GlobalAccessUserCount { get; set; }
    public int DirectAccessUserCount { get; set; }
    public List<AccessAuditUserDto> GlobalAccessUsers { get; set; } = new();
    public List<AccessAuditUserDto> DirectAccessUsers { get; set; } = new();
}

/// <summary>
/// User information for access audit
/// </summary>
public class AccessAuditUserDto
{
    public int UserId { get; set; }
    public string AccountName { get; set; } = string.Empty;
    public DateTime? LastLogon { get; set; }
    public bool IsSuperUser { get; set; }
    public int PermissionId { get; set; }
}
