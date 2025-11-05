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
    /// Email subject (DEPRECATED: Template system now generates subject automatically)
    /// </summary>
    [Obsolete("Subject is now generated from database template. Use AdditionalMessage instead.")]
    public string Subject { get; set; } = string.Empty;

    /// <summary>
    /// HTML email body (DEPRECATED: Template system now generates body automatically)
    /// </summary>
    [Obsolete("HtmlBody is now generated from database template. Use AdditionalMessage instead.")]
    public string HtmlBody { get; set; } = string.Empty;

    /// <summary>
    /// Optional plain text body (DEPRECATED: Template system now generates plain text automatically)
    /// </summary>
    [Obsolete("PlainTextBody is now generated from database template.")]
    public string? PlainTextBody { get; set; }

    /// <summary>
    /// Optional additional message to include in the email template
    /// This will be rendered in the {{Message}} placeholder of the DocumentAttachment template
    /// </summary>
    public string? AdditionalMessage { get; set; }

    /// <summary>
    /// List of document IDs to attach
    /// </summary>
    public List<int> DocumentIds { get; set; } = new();
}
