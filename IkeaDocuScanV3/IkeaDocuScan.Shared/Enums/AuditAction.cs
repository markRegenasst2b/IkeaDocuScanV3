namespace IkeaDocuScan.Shared.Enums;

/// <summary>
/// Represents the types of actions that can be audited in the system
/// </summary>
public enum AuditAction
{
    /// <summary>
    /// User edited a document
    /// </summary>
    Edit,

    /// <summary>
    /// User registered a new document
    /// </summary>
    Register,

    /// <summary>
    /// User checked in a document
    /// </summary>
    CheckIn,

    /// <summary>
    /// User deleted a document
    /// </summary>
    Delete,

    /// <summary>
    /// User sent a single link
    /// </summary>
    SendLink,

    /// <summary>
    /// User sent a single attachment
    /// </summary>
    SendAttachment,

    /// <summary>
    /// User sent multiple links
    /// </summary>
    SendLinks,

    /// <summary>
    /// User sent multiple attachments
    /// </summary>
    SendAttachments
}
