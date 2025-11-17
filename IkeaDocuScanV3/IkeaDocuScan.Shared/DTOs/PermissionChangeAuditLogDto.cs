namespace IkeaDocuScan.Shared.DTOs;

/// <summary>
/// DTO for PermissionChangeAuditLog entity
/// </summary>
public class PermissionChangeAuditLogDto
{
    public int AuditId { get; set; }
    public int EndpointId { get; set; }
    public string ChangedBy { get; set; } = string.Empty;
    public string ChangeType { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string? ChangeReason { get; set; }
    public DateTime ChangedOn { get; set; }

    // Navigation properties
    public string? EndpointRoute { get; set; }
    public string? HttpMethod { get; set; }
    public string? EndpointName { get; set; }
}

/// <summary>
/// DTO for creating a permission change audit log entry
/// </summary>
public class CreatePermissionChangeAuditLogDto
{
    public int EndpointId { get; set; }
    public string ChangedBy { get; set; } = string.Empty;
    public string ChangeType { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string? ChangeReason { get; set; }
}
