namespace IkeaDocuScan.ActionReminderService.Services;

/// <summary>
/// Service for sending emails using MailKit
/// </summary>
public interface IEmailSender
{
    /// <summary>
    /// Send an email to multiple recipients
    /// </summary>
    /// <param name="toEmails">Collection of recipient email addresses</param>
    /// <param name="subject">Email subject</param>
    /// <param name="htmlBody">HTML body content</param>
    /// <param name="plainTextBody">Optional plain text body (fallback)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task SendEmailAsync(
        IEnumerable<string> toEmails,
        string subject,
        string htmlBody,
        string? plainTextBody = null,
        CancellationToken cancellationToken = default);
}
