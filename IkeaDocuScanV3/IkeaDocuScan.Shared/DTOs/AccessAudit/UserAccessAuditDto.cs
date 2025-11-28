namespace IkeaDocuScan.Shared.DTOs.AccessAudit;

/// <summary>
/// Response DTO for user access audit - shows all document types a user has access to
/// </summary>
public class UserAccessAuditDto
{
    public int UserId { get; set; }
    public string AccountName { get; set; } = string.Empty;
    public DateTime? LastLogon { get; set; }
    public bool IsSuperUser { get; set; }
    public bool HasGlobalAccess { get; set; }
    public int? GlobalAccessPermissionId { get; set; }
    public int TotalDocumentTypesWithAccess { get; set; }
    public int TotalDocumentTypesWithoutAccess { get; set; }
    public List<AccessAuditDocumentTypeDto> DocumentTypesWithAccess { get; set; } = new();
    public List<AccessAuditDocumentTypeDto> DocumentTypesWithoutAccess { get; set; } = new();
}

/// <summary>
/// Document type information for access audit
/// </summary>
public class AccessAuditDocumentTypeDto
{
    public int DocumentTypeId { get; set; }
    public string DocumentTypeName { get; set; } = string.Empty;
    public int? PermissionId { get; set; }
}
