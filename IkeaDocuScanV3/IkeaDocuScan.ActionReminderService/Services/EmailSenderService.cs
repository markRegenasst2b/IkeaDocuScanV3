using IkeaDocuScan.Shared.Configuration;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace IkeaDocuScan.ActionReminderService.Services;

/// <summary>
/// Email sender service implementation using MailKit
/// </summary>
public class EmailSenderService : IEmailSender
{
    private readonly EmailOptions _options;
    private readonly ILogger<EmailSenderService> _logger;

    public EmailSenderService(
        IOptions<EmailOptions> options,
        ILogger<EmailSenderService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    /// <summary>
    /// Determine the appropriate SecureSocketOptions based on configuration
    /// </summary>
    private SecureSocketOptions DetermineSecureSocketOptions()
    {
        // If explicit mode is set, use it
        if (_options.SecurityMode != SmtpSecurityMode.Auto)
        {
            return _options.SecurityMode switch
            {
                SmtpSecurityMode.None => SecureSocketOptions.None,
                SmtpSecurityMode.StartTls => SecureSocketOptions.StartTls,
                SmtpSecurityMode.SslOnConnect => SecureSocketOptions.SslOnConnect,
                _ => SecureSocketOptions.Auto
            };
        }

        // Auto mode: Intelligently determine based on port
        return _options.SmtpPort switch
        {
            25 => SecureSocketOptions.None,           // Standard SMTP port (no encryption)
            587 => SecureSocketOptions.StartTls,      // Submission port (STARTTLS)
            465 => SecureSocketOptions.SslOnConnect,  // Secure SMTP port (implicit SSL)
            _ => _options.UseSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None
        };
    }

    public async Task SendEmailAsync(
        IEnumerable<string> toEmails,
        string subject,
        string htmlBody,
        string? plainTextBody = null,
        CancellationToken cancellationToken = default)
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
            message.Body = bodyBuilder.ToMessageBody();

            // Send email using SMTP
            using var client = new SmtpClient();

            // Set timeout
            client.Timeout = _options.TimeoutSeconds * 1000;

            // Determine secure socket options
            var secureSocketOptions = DetermineSecureSocketOptions();

            _logger.LogDebug("Connecting to SMTP server {Host}:{Port} with security mode: {SecurityMode}",
                _options.SmtpHost, _options.SmtpPort, secureSocketOptions);

            // Connect to SMTP server
            await client.ConnectAsync(
                _options.SmtpHost,
                _options.SmtpPort,
                secureSocketOptions,
                cancellationToken);

            // Authenticate if credentials provided
            if (!string.IsNullOrWhiteSpace(_options.SmtpUsername))
            {
                await client.AuthenticateAsync(_options.SmtpUsername, _options.SmtpPassword, cancellationToken);
            }

            // Send message
            await client.SendAsync(message, cancellationToken);

            // Disconnect
            await client.DisconnectAsync(true, cancellationToken);

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
}
