using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IkeaDocuScan.Infrastructure.Entities;

/// <summary>
/// Audit trail for endpoint permission changes
/// </summary>
[Table("PermissionChangeAuditLog")]
public class PermissionChangeAuditLog
{
    /// <summary>
    /// Primary key
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int AuditId { get; set; }

    /// <summary>
    /// Foreign key to EndpointRegistry
    /// </summary>
    [Required]
    public int EndpointId { get; set; }

    /// <summary>
    /// Who made the change (username)
    /// </summary>
    [Required]
    [MaxLength(255)]
    public string ChangedBy { get; set; } = string.Empty;

    /// <summary>
    /// Type of change (RoleAdded, RoleRemoved, EndpointCreated, EndpointModified, EndpointDeactivated, EndpointReactivated)
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string ChangeType { get; set; } = string.Empty;

    /// <summary>
    /// Old value before change (JSON or plain text)
    /// </summary>
    [Column(TypeName = "nvarchar(max)")]
    public string? OldValue { get; set; }

    /// <summary>
    /// New value after change (JSON or plain text)
    /// </summary>
    [Column(TypeName = "nvarchar(max)")]
    public string? NewValue { get; set; }

    /// <summary>
    /// Reason for the change
    /// </summary>
    [MaxLength(500)]
    public string? ChangeReason { get; set; }

    /// <summary>
    /// When the change was made
    /// </summary>
    public DateTime ChangedOn { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Navigation property: The endpoint this audit log is for
    /// </summary>
    [ForeignKey(nameof(EndpointId))]
    public virtual EndpointRegistry Endpoint { get; set; } = null!;
}
