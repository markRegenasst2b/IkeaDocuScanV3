namespace IkeaDocuScan.Infrastructure.Entities.Configuration;

/// <summary>
/// Individual email recipient within a group
/// </summary>
public class EmailRecipient
{
    public int RecipientId { get; set; }

    /// <summary>
    /// Reference to the group this recipient belongs to
    /// </summary>
    public int GroupId { get; set; }

    /// <summary>
    /// Email address of the recipient
    /// </summary>
    public string EmailAddress { get; set; } = string.Empty;

    /// <summary>
    /// Display name for the recipient (optional)
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// Whether this recipient is currently active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Sort order within the group
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// Username who added this recipient
    /// </summary>
    public string CreatedBy { get; set; } = string.Empty;

    /// <summary>
    /// When this recipient was added
    /// </summary>
    public DateTime CreatedDate { get; set; }

    /// <summary>
    /// Navigation property to the group
    /// </summary>
    public EmailRecipientGroup Group { get; set; } = null!;
}
