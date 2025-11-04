namespace IkeaDocuScan.Infrastructure.Entities.Configuration;

/// <summary>
/// Master configuration table for storing application settings in database
/// </summary>
public class SystemConfiguration
{
    public int ConfigurationId { get; set; }

    /// <summary>
    /// Unique key for the configuration setting (e.g., "SmtpHost", "AdminEmail")
    /// </summary>
    public string ConfigKey { get; set; } = string.Empty;

    /// <summary>
    /// Configuration section (e.g., "Email", "ActionReminderService")
    /// </summary>
    public string ConfigSection { get; set; } = string.Empty;

    /// <summary>
    /// Serialized configuration value (JSON for complex types)
    /// </summary>
    public string ConfigValue { get; set; } = string.Empty;

    /// <summary>
    /// Type of value: String, StringArray, Int, Bool, Json
    /// </summary>
    public string ValueType { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable description of this setting
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Whether this configuration is currently active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// If true, database value overrides appsettings.json; if false, use file config
    /// </summary>
    public bool IsOverride { get; set; } = true;

    /// <summary>
    /// Username who created this configuration
    /// </summary>
    public string CreatedBy { get; set; } = string.Empty;

    /// <summary>
    /// When this configuration was created
    /// </summary>
    public DateTime CreatedDate { get; set; }

    /// <summary>
    /// Username who last modified this configuration
    /// </summary>
    public string? ModifiedBy { get; set; }

    /// <summary>
    /// When this configuration was last modified
    /// </summary>
    public DateTime? ModifiedDate { get; set; }

    /// <summary>
    /// Audit trail for this configuration
    /// </summary>
    public ICollection<SystemConfigurationAudit> AuditTrail { get; set; } = new List<SystemConfigurationAudit>();
}
