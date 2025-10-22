using IkeaDocuScan.Shared.Configuration;
using IkeaDocuScan.Shared.Interfaces;
using IkeaDocuScan.Shared.Models.Email;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace IkeaDocuScan_Web.Services;

/// <summary>
/// Email service implementation using MailKit
/// </summary>
public class EmailService : IEmailService
{
    private readonly EmailOptions _options;
    private readonly ILogger<EmailService> _logger;

    public EmailService(
        IOptions<EmailOptions> options,
        ILogger<EmailService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task SendAccessRequestNotificationAsync(string username, string? reason = null)
    {
        if (!ShouldSendEmail(_options.SendAccessRequestNotifications))
        {
            return;
        }

        try
        {
            var (htmlBody, plainText) = EmailTemplates.BuildAccessRequestNotification(
                username,
                reason,
                _options.ApplicationUrl);

            // Send to primary admin
            await SendEmailAsync(
                _options.AdminEmail,
                _options.AccessRequestSubject,
                htmlBody,
                plainText);

            // Send to additional admins if configured
            if (_options.AdditionalAdminEmails?.Length > 0)
            {
                foreach (var adminEmail in _options.AdditionalAdminEmails.Where(e => !string.IsNullOrWhiteSpace(e)))
                {
                    await SendEmailAsync(
                        adminEmail,
                        _options.AccessRequestSubject,
                        htmlBody,
                        plainText);
                }
            }

            _logger.LogInformation("Access request notification sent for user {Username}", username);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send access request notification for user {Username}", username);
            // Don't throw - email failures shouldn't break the application
        }
    }

    /// <inheritdoc />
    public async Task SendAccessRequestConfirmationAsync(string userEmail, string username)
    {
        if (!ShouldSendEmail(_options.SendAccessRequestNotifications))
        {
            return;
        }

        try
        {
            var (htmlBody, plainText) = EmailTemplates.BuildAccessRequestConfirmation(
                username,
                _options.AdminEmail);

            await SendEmailAsync(
                userEmail,
                _options.AccessRequestConfirmationSubject,
                htmlBody,
                plainText);

            _logger.LogInformation("Access request confirmation sent to {UserEmail}", userEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send access request confirmation to {UserEmail}", userEmail);
        }
    }

    /// <inheritdoc />
    public async Task SendDocumentLinkAsync(
        string recipientEmail,
        string documentBarCode,
        string documentLink,
        string? message = null)
    {
        if (!ShouldSendEmail(_options.SendDocumentNotifications))
        {
            return;
        }

        try
        {
            var (htmlBody, plainText) = EmailTemplates.BuildDocumentLink(
                documentBarCode,
                documentLink,
                message);

            await SendEmailAsync(
                recipientEmail,
                $"Document Shared: {documentBarCode}",
                htmlBody,
                plainText);

            _logger.LogInformation("Document link sent to {RecipientEmail} for {BarCode}",
                recipientEmail, documentBarCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send document link to {RecipientEmail} for {BarCode}",
                recipientEmail, documentBarCode);
        }
    }

    /// <inheritdoc />
    public async Task SendDocumentAttachmentAsync(
        string recipientEmail,
        string documentBarCode,
        byte[] attachmentData,
        string fileName,
        string? message = null)
    {
        if (!ShouldSendEmail(_options.SendDocumentNotifications))
        {
            return;
        }

        try
        {
            var (htmlBody, plainText) = EmailTemplates.BuildDocumentAttachment(
                documentBarCode,
                fileName,
                message);

            var attachment = new EmailAttachment
            {
                FileName = fileName,
                Content = attachmentData,
                ContentType = GetContentType(fileName)
            };

            await SendEmailAsync(
                recipientEmail,
                $"Document: {documentBarCode}",
                htmlBody,
                plainText,
                new[] { attachment });

            _logger.LogInformation("Document attachment sent to {RecipientEmail} for {BarCode}",
                recipientEmail, documentBarCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send document attachment to {RecipientEmail} for {BarCode}",
                recipientEmail, documentBarCode);
        }
    }

    /// <inheritdoc />
    public async Task SendDocumentLinksAsync(
        string recipientEmail,
        IEnumerable<(string BarCode, string Link)> documents,
        string? message = null)
    {
        if (!ShouldSendEmail(_options.SendDocumentNotifications))
        {
            return;
        }

        try
        {
            var documentList = documents.ToList();
            var (htmlBody, plainText) = EmailTemplates.BuildDocumentLinks(documentList, message);

            await SendEmailAsync(
                recipientEmail,
                $"{documentList.Count} Documents Shared",
                htmlBody,
                plainText);

            _logger.LogInformation("Document links sent to {RecipientEmail}, count: {Count}",
                recipientEmail, documentList.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send document links to {RecipientEmail}", recipientEmail);
        }
    }

    /// <inheritdoc />
    public async Task SendDocumentAttachmentsAsync(
        string recipientEmail,
        IEnumerable<(string BarCode, byte[] Data, string FileName)> documents,
        string? message = null)
    {
        if (!ShouldSendEmail(_options.SendDocumentNotifications))
        {
            return;
        }

        try
        {
            var documentList = documents.ToList();
            var attachments = documentList.Select(d => new EmailAttachment
            {
                FileName = d.FileName,
                Content = d.Data,
                ContentType = GetContentType(d.FileName)
            }).ToList();

            var documentInfo = documentList.Select(d => (d.BarCode, d.FileName));
            var (htmlBody, plainText) = EmailTemplates.BuildDocumentAttachments(documentInfo, message);

            await SendEmailAsync(
                recipientEmail,
                $"{documentList.Count} Documents Attached",
                htmlBody,
                plainText,
                attachments);

            _logger.LogInformation("Document attachments sent to {RecipientEmail}, count: {Count}",
                recipientEmail, documentList.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send document attachments to {RecipientEmail}", recipientEmail);
        }
    }

    /// <inheritdoc />
    public async Task SendEmailAsync(
        string toEmail,
        string subject,
        string htmlBody,
        string? plainTextBody = null,
        IEnumerable<EmailAttachment>? attachments = null)
    {
        await SendEmailToMultipleRecipientsAsync(
            new[] { toEmail },
            subject,
            htmlBody,
            plainTextBody,
            attachments);
    }

    /// <inheritdoc />
    public async Task SendEmailToMultipleRecipientsAsync(
        IEnumerable<string> toEmails,
        string subject,
        string htmlBody,
        string? plainTextBody = null,
        IEnumerable<EmailAttachment>? attachments = null)
    {
        if (!_options.EnableEmailNotifications)
        {
            _logger.LogInformation("Email notifications are disabled. Would have sent: {Subject}", subject);
            return;
        }

        var recipients = toEmails.Where(e => !string.IsNullOrWhiteSpace(e)).ToList();
        if (recipients.Count == 0)
        {
            _logger.LogWarning("No valid recipients for email: {Subject}", subject);
            return;
        }

        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_options.FromDisplayName, _options.FromAddress));

            foreach (var recipient in recipients)
            {
                message.To.Add(MailboxAddress.Parse(recipient));
            }

            message.Subject = subject;

            // Build message body
            var bodyBuilder = new BodyBuilder();

            if (!string.IsNullOrWhiteSpace(plainTextBody))
            {
                bodyBuilder.TextBody = plainTextBody;
            }

            bodyBuilder.HtmlBody = htmlBody;

            // Add attachments if provided
            if (attachments != null)
            {
                foreach (var attachment in attachments)
                {
                    if (attachment.Content?.Length > 0)
                    {
                        bodyBuilder.Attachments.Add(
                            attachment.FileName,
                            attachment.Content,
                            ContentType.Parse(attachment.ContentType));
                    }
                }
            }

            message.Body = bodyBuilder.ToMessageBody();

            // Send email using SMTP
            using var client = new SmtpClient();

            // Set timeout
            client.Timeout = _options.TimeoutSeconds * 1000;

            // Connect to SMTP server
            await client.ConnectAsync(
                _options.SmtpHost,
                _options.SmtpPort,
                _options.UseSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None);

            // Authenticate if credentials provided
            if (!string.IsNullOrWhiteSpace(_options.SmtpUsername))
            {
                await client.AuthenticateAsync(_options.SmtpUsername, _options.SmtpPassword);
            }

            // Send message
            await client.SendAsync(message);

            // Disconnect
            await client.DisconnectAsync(true);

            _logger.LogInformation("Email sent successfully: {Subject} to {RecipientCount} recipient(s)",
                subject, recipients.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email: {Subject} to {RecipientCount} recipient(s)",
                subject, recipients.Count);
            throw;
        }
    }

    /// <summary>
    /// Check if emails should be sent based on configuration
    /// </summary>
    private bool ShouldSendEmail(bool featureEnabled)
    {
        if (!_options.EnableEmailNotifications)
        {
            _logger.LogDebug("Email notifications are globally disabled");
            return false;
        }

        if (!featureEnabled)
        {
            _logger.LogDebug("This email feature is disabled");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Get MIME content type based on file extension
    /// </summary>
    private static string GetContentType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();

        return extension switch
        {
            ".pdf" => "application/pdf",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".bmp" => "image/bmp",
            ".tif" or ".tiff" => "image/tiff",
            ".txt" => "text/plain",
            ".html" or ".htm" => "text/html",
            ".xml" => "text/xml",
            ".json" => "application/json",
            ".zip" => "application/zip",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".xls" => "application/vnd.ms-excel",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            _ => "application/octet-stream"
        };
    }
}
