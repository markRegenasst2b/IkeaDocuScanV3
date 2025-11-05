using System.ComponentModel.DataAnnotations;

namespace IkeaDocuScan.Shared.DTOs.UserPermissions;

/// <summary>
/// DTO for creating a new DocuScanUser
/// </summary>
public class CreateDocuScanUserDto
{
    [Required(ErrorMessage = "Account name is required")]
    [StringLength(255, ErrorMessage = "Account name cannot exceed 255 characters")]
    public string AccountName { get; set; } = string.Empty;

    [Required(ErrorMessage = "User identifier is required")]
    [StringLength(255, ErrorMessage = "User identifier cannot exceed 255 characters")]
    public string UserIdentifier { get; set; } = string.Empty;

    public bool IsSuperUser { get; set; } = false;
}
