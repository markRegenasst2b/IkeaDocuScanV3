using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IkeaDocuScan.Infrastructure.Entities;

/// <summary>
/// Maps roles to endpoints for dynamic authorization
/// </summary>
[Table("EndpointRolePermission")]
public class EndpointRolePermission
{
    /// <summary>
    /// Primary key
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int PermissionId { get; set; }

    /// <summary>
    /// Foreign key to EndpointRegistry
    /// </summary>
    [Required]
    public int EndpointId { get; set; }

    /// <summary>
    /// Role name (Reader, Publisher, ADAdmin, SuperUser)
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string RoleName { get; set; } = string.Empty;

    /// <summary>
    /// When the permission was granted
    /// </summary>
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Who granted the permission (username or SYSTEM)
    /// </summary>
    [MaxLength(255)]
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Navigation property: The endpoint this permission is for
    /// </summary>
    [ForeignKey(nameof(EndpointId))]
    public virtual EndpointRegistry Endpoint { get; set; } = null!;
}
