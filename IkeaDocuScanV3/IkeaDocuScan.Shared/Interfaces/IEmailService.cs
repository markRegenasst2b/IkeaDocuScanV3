using IkeaDocuScan.Shared.Models.Email;

namespace IkeaDocuScan.Shared.Interfaces;

/// <summary>
/// Service for sending emails
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Send notification to admin when user requests access
    /// </summary>
    /// <param name="username">Username requesting access</param>
    /// <param name="reason">Optional reason for access request</param>
    Task SendAccessRequestNotificationAsync(string username, string? reason = null);

    /// <summary>
    /// Send confirmation to user after access request
    /// </summary>
    /// <param name="userEmail">User's email address</param>
    /// <param name="username">Username that requested access</param>
    Task SendAccessRequestConfirmationAsync(string userEmail, string username);

    /// <summary>
    /// Send document link to recipient
    /// </summary>
    /// <param name="recipientEmail">Recipient email address</param>
    /// <param name="documentBarCode">Document bar code</param>
    /// <param name="documentLink">Link to document</param>
    /// <param name="message">Optional message to include</param>
    Task SendDocumentLinkAsync(
        string recipientEmail,
        string documentBarCode,
        string documentLink,
        string? message = null);

    /// <summary>
    /// Send document as attachment
    /// </summary>
    /// <param name="recipientEmail">Recipient email address</param>
    /// <param name="documentBarCode">Document bar code</param>
    /// <param name="attachmentData">Document file data</param>
    /// <param name="fileName">File name with extension</param>
    /// <param name="message">Optional message to include</param>
    Task SendDocumentAttachmentAsync(
        string recipientEmail,
        string documentBarCode,
        byte[] attachmentData,
        string fileName,
        string? message = null);

    /// <summary>
    /// Send multiple document links
    /// </summary>
    /// <param name="recipientEmail">Recipient email address</param>
    /// <param name="documents">Collection of documents with bar codes and links</param>
    /// <param name="message">Optional message to include</param>
    Task SendDocumentLinksAsync(
        string recipientEmail,
        IEnumerable<(string BarCode, string Link)> documents,
        string? message = null);

    /// <summary>
    /// Send multiple documents as attachments
    /// </summary>
    /// <param name="recipientEmail">Recipient email address</param>
    /// <param name="documents">Collection of documents with data and file names</param>
    /// <param name="message">Optional message to include</param>
    Task SendDocumentAttachmentsAsync(
        string recipientEmail,
        IEnumerable<(string BarCode, byte[]? Data, string? FileName)> documents,
        string? message = null);

    /// <summary>
    /// Generic email send method
    /// </summary>
    /// <param name="toEmail">Recipient email address</param>
    /// <param name="subject">Email subject</param>
    /// <param name="htmlBody">HTML body content</param>
    /// <param name="plainTextBody">Optional plain text body (fallback)</param>
    /// <param name="attachments">Optional attachments</param>
    Task SendEmailAsync(
        string toEmail,
        string subject,
        string htmlBody,
        string? plainTextBody = null,
        IEnumerable<EmailAttachment>? attachments = null);

    /// <summary>
    /// Send email to multiple recipients
    /// </summary>
    /// <param name="toEmails">Collection of recipient email addresses</param>
    /// <param name="subject">Email subject</param>
    /// <param name="htmlBody">HTML body content</param>
    /// <param name="plainTextBody">Optional plain text body (fallback)</param>
    /// <param name="attachments">Optional attachments</param>
    Task SendEmailToMultipleRecipientsAsync(
        IEnumerable<string> toEmails,
        string subject,
        string htmlBody,
        string? plainTextBody = null,
        IEnumerable<EmailAttachment>? attachments = null);
}
