namespace IkeaDocuScan.Infrastructure.Entities.Configuration;

/// <summary>
/// Audit trail for tracking configuration changes
/// </summary>
public class SystemConfigurationAudit
{
    public int AuditId { get; set; }

    /// <summary>
    /// Reference to the configuration that was changed
    /// </summary>
    public int ConfigurationId { get; set; }

    /// <summary>
    /// Configuration key for easy searching
    /// </summary>
    public string ConfigKey { get; set; } = string.Empty;

    /// <summary>
    /// Previous value before the change
    /// </summary>
    public string? OldValue { get; set; }

    /// <summary>
    /// New value after the change
    /// </summary>
    public string? NewValue { get; set; }

    /// <summary>
    /// Username who made the change
    /// </summary>
    public string ChangedBy { get; set; } = string.Empty;

    /// <summary>
    /// When the change was made
    /// </summary>
    public DateTime ChangedDate { get; set; }

    /// <summary>
    /// Optional reason for the change
    /// </summary>
    public string? ChangeReason { get; set; }

    /// <summary>
    /// Navigation property to the configuration
    /// </summary>
    public SystemConfiguration Configuration { get; set; } = null!;
}
