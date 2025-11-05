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
/// Enhanced with database-driven configuration and templating
/// </summary>
public class EmailService : IEmailService
{
    private readonly EmailOptions _options;
    private readonly ILogger<EmailService> _logger;
    private readonly ISystemConfigurationManager _configManager;
    private readonly IEmailTemplateService _templateService;

    public EmailService(
        IOptions<EmailOptions> options,
        ILogger<EmailService> logger,
        ISystemConfigurationManager configManager,
        IEmailTemplateService templateService)
    {
        _options = options.Value;
        _logger = logger;
        _configManager = configManager;
        _templateService = templateService;
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
            // Get admin email recipients from database (with fallback to config)
            var adminEmails = await _configManager.GetEmailRecipientsAsync("AdminEmails");

            if (adminEmails.Length == 0)
            {
                // Fallback to configuration file
                _logger.LogInformation("Using admin emails from configuration file");
                var emailList = new List<string>();
                if (!string.IsNullOrEmpty(_options.AdminEmail))
                    emailList.Add(_options.AdminEmail);
                if (_options.AdditionalAdminEmails?.Length > 0)
                    emailList.AddRange(_options.AdditionalAdminEmails);
                adminEmails = emailList.ToArray();
            }

            if (adminEmails.Length == 0)
            {
                _logger.LogWarning("No admin email addresses configured. Cannot send access request notification.");
                return;
            }

            // Try to get email template from database
            var template = await _configManager.GetEmailTemplateAsync("AccessRequestNotification");
            string htmlBody, plainText, subject;

            if (template != null)
            {
                // Use database template
                _logger.LogInformation("Using AccessRequestNotification template from database");

                var data = new Dictionary<string, object>
                {
                    { "Username", username },
                    { "Reason", reason ?? "No reason provided" },
                    { "ApplicationUrl", _options.ApplicationUrl },
                    { "Date", DateTime.Now }
                };

                htmlBody = _templateService.RenderTemplate(template.HtmlBody, data);
                plainText = !string.IsNullOrEmpty(template.PlainTextBody)
                    ? _templateService.RenderTemplate(template.PlainTextBody, data)
                    : $"Access Request from {username}\nReason: {reason ?? "No reason provided"}\nDate: {DateTime.Now:dd/MM/yyyy HH:mm}";
                subject = _templateService.RenderTemplate(template.Subject, data);
            }
            else
            {
                // Fallback to hard-coded template
                _logger.LogInformation("Using hard-coded AccessRequestNotification template");
                (htmlBody, plainText) = EmailTemplates.BuildAccessRequestNotification(
                    username,
                    reason,
                    _options.ApplicationUrl);
                subject = _options.AccessRequestSubject;
            }

            // Send to all admin emails
            await SendEmailToMultipleRecipientsAsync(
                adminEmails,
                subject,
                htmlBody,
                plainText);

            _logger.LogInformation("Access request notification sent to {Count} admin(s) for user {Username}",
                adminEmails.Length, username);
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
            // Try to get email template from database
            var template = await _configManager.GetEmailTemplateAsync("AccessRequestConfirmation");
            string htmlBody, plainText, subject;

            if (template != null)
            {
                // Use database template
                _logger.LogInformation("Using AccessRequestConfirmation template from database");

                var data = new Dictionary<string, object>
                {
                    { "Username", username },
                    { "AdminEmail", _options.AdminEmail },
                    { "ApplicationUrl", _options.ApplicationUrl },
                    { "Date", DateTime.Now }
                };

                htmlBody = _templateService.RenderTemplate(template.HtmlBody, data);
                plainText = !string.IsNullOrEmpty(template.PlainTextBody)
                    ? _templateService.RenderTemplate(template.PlainTextBody, data)
                    : $"Your access request has been received.\nUsername: {username}\nDate: {DateTime.Now:dd/MM/yyyy HH:mm}";
                subject = _templateService.RenderTemplate(template.Subject, data);
            }
            else
            {
                // Fallback to hard-coded template
                _logger.LogInformation("Using hard-coded AccessRequestConfirmation template");
                (htmlBody, plainText) = EmailTemplates.BuildAccessRequestConfirmation(
                    username,
                    _options.AdminEmail);
                subject = _options.AccessRequestConfirmationSubject;
            }

            await SendEmailAsync(
                userEmail,
                subject,
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
            // Try to get email template from database
            var template = await _configManager.GetEmailTemplateAsync("DocumentLink");
            string htmlBody, plainText, subject;

            if (template != null)
            {
                // Use database template
                _logger.LogInformation("Using DocumentLink template from database");

                var data = new Dictionary<string, object>
                {
                    { "BarCode", documentBarCode },
                    { "DocumentLink", documentLink },
                    { "Message", message ?? string.Empty },
                    { "Date", DateTime.Now }
                };

                htmlBody = _templateService.RenderTemplate(template.HtmlBody, data);
                plainText = !string.IsNullOrEmpty(template.PlainTextBody)
                    ? _templateService.RenderTemplate(template.PlainTextBody, data)
                    : $"Document Shared: {documentBarCode}\nLink: {documentLink}\nMessage: {message ?? "N/A"}";
                subject = _templateService.RenderTemplate(template.Subject, data);
            }
            else
            {
                // Fallback to hard-coded template
                _logger.LogInformation("Using hard-coded DocumentLink template");
                (htmlBody, plainText) = EmailTemplates.BuildDocumentLink(
                    documentBarCode,
                    documentLink,
                    message);
                subject = $"Document Shared: {documentBarCode}";
            }

            await SendEmailAsync(
                recipientEmail,
                subject,
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
            // Try to get email template from database
            var template = await _configManager.GetEmailTemplateAsync("DocumentAttachment");
            string htmlBody, plainText, subject;

            if (template != null)
            {
                // Use database template
                _logger.LogInformation("Using DocumentAttachment template from database");

                var data = new Dictionary<string, object>
                {
                    { "BarCode", documentBarCode },
                    { "FileName", fileName },
                    { "Message", message ?? string.Empty },
                    { "Date", DateTime.Now }
                };

                htmlBody = _templateService.RenderTemplate(template.HtmlBody, data);
                plainText = !string.IsNullOrEmpty(template.PlainTextBody)
                    ? _templateService.RenderTemplate(template.PlainTextBody, data)
                    : $"Document: {documentBarCode}\nFile: {fileName}\nMessage: {message ?? "N/A"}";
                subject = _templateService.RenderTemplate(template.Subject, data);
            }
            else
            {
                // Fallback to hard-coded template
                _logger.LogInformation("Using hard-coded DocumentAttachment template");
                (htmlBody, plainText) = EmailTemplates.BuildDocumentAttachment(
                    documentBarCode,
                    fileName,
                    message);
                subject = $"Document: {documentBarCode}";
            }

            var attachment = new EmailAttachment
            {
                FileName = fileName,
                Content = attachmentData,
                ContentType = GetContentType(fileName)
            };

            await SendEmailAsync(
                recipientEmail,
                subject,
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

            // Try to get email template from database
            var template = await _configManager.GetEmailTemplateAsync("DocumentLinks");
            string htmlBody, plainText, subject;

            if (template != null)
            {
                // Use database template with loop support
                _logger.LogInformation("Using DocumentLinks template from database");

                var data = new Dictionary<string, object>
                {
                    { "Count", documentList.Count },
                    { "DocumentCount", documentList.Count }, // Alternative placeholder name
                    { "Barcodes", string.Join(", ", documentList.Select(d => d.BarCode)) }, // Comma-separated barcodes
                    { "Message", message ?? string.Empty },
                    { "Date", DateTime.Now }
                };

                var loops = new Dictionary<string, List<Dictionary<string, object>>>
                {
                    { "DocumentRows", documentList.Select(d => new Dictionary<string, object>
                        {
                            { "BarCode", d.BarCode },
                            { "Link", d.Link }
                        }).ToList()
                    }
                };

                htmlBody = _templateService.RenderTemplateWithLoops(template.HtmlBody, data, loops);
                plainText = !string.IsNullOrEmpty(template.PlainTextBody)
                    ? _templateService.RenderTemplateWithLoops(template.PlainTextBody, data, loops)
                    : $"{documentList.Count} Documents Shared\n" + string.Join("\n", documentList.Select(d => $"- {d.BarCode}: {d.Link}"));
                subject = _templateService.RenderTemplate(template.Subject, data);
            }
            else
            {
                // Fallback to hard-coded template
                _logger.LogInformation("Using hard-coded DocumentLinks template");
                (htmlBody, plainText) = EmailTemplates.BuildDocumentLinks(documentList, message);
                subject = $"{documentList.Count} Documents Shared";
            }

            await SendEmailAsync(
                recipientEmail,
                subject,
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

            // Try to get email template from database
            var template = await _configManager.GetEmailTemplateAsync("DocumentAttachments");
            string htmlBody, plainText, subject;

            if (template != null)
            {
                // Use database template with loop support
                _logger.LogInformation("Using DocumentAttachments template from database");

                var data = new Dictionary<string, object>
                {
                    { "Count", documentList.Count },
                    { "DocumentCount", documentList.Count }, // Alternative placeholder name
                    { "Barcodes", string.Join(", ", documentList.Select(d => d.BarCode)) }, // Comma-separated barcodes
                    { "Message", message ?? string.Empty },
                    { "Date", DateTime.Now }
                };

                var loops = new Dictionary<string, List<Dictionary<string, object>>>
                {
                    { "DocumentRows", documentList.Select(d => new Dictionary<string, object>
                        {
                            { "BarCode", d.BarCode },
                            { "FileName", d.FileName }
                        }).ToList()
                    }
                };

                htmlBody = _templateService.RenderTemplateWithLoops(template.HtmlBody, data, loops);
                plainText = !string.IsNullOrEmpty(template.PlainTextBody)
                    ? _templateService.RenderTemplateWithLoops(template.PlainTextBody, data, loops)
                    : $"{documentList.Count} Documents Attached\n" + string.Join("\n", documentList.Select(d => $"- {d.BarCode}: {d.FileName}"));
                subject = _templateService.RenderTemplate(template.Subject, data);
            }
            else
            {
                // Fallback to hard-coded template
                _logger.LogInformation("Using hard-coded DocumentAttachments template");
                var documentInfo = documentList.Select(d => (d.BarCode, d.FileName));
                (htmlBody, plainText) = EmailTemplates.BuildDocumentAttachments(documentInfo, message);
                subject = $"{documentList.Count} Documents Attached";
            }

            await SendEmailAsync(
                recipientEmail,
                subject,
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
