namespace IkeaDocuScan.Shared.DTOs.UserPermissions;

/// <summary>
/// Data transfer object for DocuScanUser
/// </summary>
public class DocuScanUserDto
{
    public int UserId { get; set; }
    public string AccountName { get; set; } = string.Empty;
    public string UserIdentifier { get; set; } = string.Empty;
    public DateTime? LastLogon { get; set; }
    public bool IsSuperUser { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public int PermissionCount { get; set; }
}
