namespace IkeaDocuScan.Shared.DTOs.Email;

/// <summary>
/// Request model for sending an email with document attachments
/// </summary>
public class SendEmailWithAttachmentsRequest
{
    /// <summary>
    /// Recipient email address
    /// </summary>
    public string ToEmail { get; set; } = string.Empty;

    /// <summary>
    /// Email subject
    /// </summary>
    public string Subject { get; set; } = string.Empty;

    /// <summary>
    /// HTML email body
    /// </summary>
    public string HtmlBody { get; set; } = string.Empty;

    /// <summary>
    /// Optional plain text body (fallback for email clients that don't support HTML)
    /// </summary>
    public string? PlainTextBody { get; set; }

    /// <summary>
    /// List of document IDs to attach
    /// </summary>
    public List<int> DocumentIds { get; set; } = new();
}
