using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IkeaDocuScan.Infrastructure.Entities;

/// <summary>
/// Catalog of all API endpoints for dynamic authorization
/// </summary>
[Table("EndpointRegistry")]
public class EndpointRegistry
{
    /// <summary>
    /// Primary key
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int EndpointId { get; set; }

    /// <summary>
    /// HTTP method (GET, POST, PUT, DELETE, etc.)
    /// </summary>
    [Required]
    [MaxLength(10)]
    public string HttpMethod { get; set; } = string.Empty;

    /// <summary>
    /// API route (e.g., /api/documents/, /api/documents/{id})
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string Route { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable endpoint name (e.g., GetAllDocuments, CreateDocument)
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string EndpointName { get; set; } = string.Empty;

    /// <summary>
    /// Description of what the endpoint does
    /// </summary>
    [MaxLength(500)]
    public string? Description { get; set; }

    /// <summary>
    /// Category for grouping endpoints (e.g., Documents, CounterParties, Configuration)
    /// </summary>
    [MaxLength(100)]
    public string? Category { get; set; }

    /// <summary>
    /// Whether the endpoint is currently active/enabled
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// When the endpoint was registered
    /// </summary>
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When the endpoint registration was last modified
    /// </summary>
    public DateTime? ModifiedOn { get; set; }

    /// <summary>
    /// Navigation property: Role permissions for this endpoint
    /// </summary>
    public virtual ICollection<EndpointRolePermission> RolePermissions { get; set; } = new List<EndpointRolePermission>();

    /// <summary>
    /// Navigation property: Audit log entries for this endpoint
    /// </summary>
    public virtual ICollection<PermissionChangeAuditLog> AuditLogs { get; set; } = new List<PermissionChangeAuditLog>();
}
