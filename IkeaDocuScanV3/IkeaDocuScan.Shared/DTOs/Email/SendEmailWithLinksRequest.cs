namespace IkeaDocuScan.Shared.DTOs.Email;

/// <summary>
/// Request model for sending an email with document links
/// </summary>
public class SendEmailWithLinksRequest
{
    /// <summary>
    /// Recipient email address
    /// </summary>
    public string ToEmail { get; set; } = string.Empty;

    /// <summary>
    /// Optional additional message to include in the email template
    /// This will be rendered in the {{Message}} placeholder of the DocumentLinks template
    /// </summary>
    public string? AdditionalMessage { get; set; }

    /// <summary>
    /// List of document IDs to include as links
    /// </summary>
    public List<int> DocumentIds { get; set; } = new();
}
