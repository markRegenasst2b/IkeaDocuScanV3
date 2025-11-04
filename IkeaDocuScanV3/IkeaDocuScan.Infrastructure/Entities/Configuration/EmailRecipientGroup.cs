namespace IkeaDocuScan.Infrastructure.Entities.Configuration;

/// <summary>
/// Group of email recipients for organizing distribution lists
/// </summary>
public class EmailRecipientGroup
{
    public int GroupId { get; set; }

    /// <summary>
    /// Display name for the group
    /// </summary>
    public string GroupName { get; set; } = string.Empty;

    /// <summary>
    /// Unique key to identify the group (e.g., "ActionReminderRecipients", "AdminEmails")
    /// </summary>
    public string GroupKey { get; set; } = string.Empty;

    /// <summary>
    /// Description of the group's purpose
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Whether this group is currently active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Username who created this group
    /// </summary>
    public string CreatedBy { get; set; } = string.Empty;

    /// <summary>
    /// When this group was created
    /// </summary>
    public DateTime CreatedDate { get; set; }

    /// <summary>
    /// Collection of recipients in this group
    /// </summary>
    public ICollection<EmailRecipient> Recipients { get; set; } = new List<EmailRecipient>();
}
