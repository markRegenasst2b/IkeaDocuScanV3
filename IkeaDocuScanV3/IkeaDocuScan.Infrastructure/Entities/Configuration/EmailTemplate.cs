namespace IkeaDocuScan.Infrastructure.Entities.Configuration;

/// <summary>
/// Email templates with placeholder support for dynamic content
/// </summary>
public class EmailTemplate
{
    public int TemplateId { get; set; }

    /// <summary>
    /// Display name for the template
    /// </summary>
    public string TemplateName { get; set; } = string.Empty;

    /// <summary>
    /// Unique key to identify the template (e.g., "ActionReminderDaily", "AccessRequestNotification")
    /// </summary>
    public string TemplateKey { get; set; } = string.Empty;

    /// <summary>
    /// Email subject line (supports placeholders like {{Count}}, {{Username}})
    /// </summary>
    public string Subject { get; set; } = string.Empty;

    /// <summary>
    /// HTML body content with placeholder support
    /// Placeholders: {{Username}}, {{Count}}, {{Date}}, {{ApplicationUrl}}, {{Reason}}, {{ActionRows}}, etc.
    /// </summary>
    public string HtmlBody { get; set; } = string.Empty;

    /// <summary>
    /// Plain text alternative for email clients that don't support HTML
    /// </summary>
    public string? PlainTextBody { get; set; }

    /// <summary>
    /// JSON array of placeholder definitions for documentation
    /// Example: [{"name": "Username", "description": "User's account name", "required": true}]
    /// </summary>
    public string? PlaceholderDefinitions { get; set; }

    /// <summary>
    /// Category for grouping templates (e.g., "Notifications", "Reminders", "System")
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// Whether this template is currently active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Whether this is the default template for its key
    /// </summary>
    public bool IsDefault { get; set; } = false;

    /// <summary>
    /// Username who created this template
    /// </summary>
    public string CreatedBy { get; set; } = string.Empty;

    /// <summary>
    /// When this template was created
    /// </summary>
    public DateTime CreatedDate { get; set; }

    /// <summary>
    /// Username who last modified this template
    /// </summary>
    public string? ModifiedBy { get; set; }

    /// <summary>
    /// When this template was last modified
    /// </summary>
    public DateTime? ModifiedDate { get; set; }
}
