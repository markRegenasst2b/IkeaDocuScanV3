namespace IkeaDocuScan.Shared.Configuration;

/// <summary>
/// SMTP security mode options
/// </summary>
public enum SmtpSecurityMode
{
    /// <summary>
    /// Automatically determine based on port (recommended)
    /// Port 25: None, Port 587: StartTls, Port 465: SslOnConnect
    /// </summary>
    Auto = 0,

    /// <summary>
    /// No SSL/TLS encryption (plain text)
    /// Use for port 25 without STARTTLS support
    /// </summary>
    None = 1,

    /// <summary>
    /// Use STARTTLS after initial connection
    /// Recommended for port 587
    /// </summary>
    StartTls = 2,

    /// <summary>
    /// Use SSL/TLS from the start of connection (implicit SSL)
    /// Required for port 465
    /// </summary>
    SslOnConnect = 3
}

/// <summary>
/// Configuration options for email service
/// Bound from appsettings.json "Email" section
/// </summary>
public class EmailOptions
{
    /// <summary>
    /// Configuration section name in appsettings.json
    /// </summary>
    public const string SectionName = "Email";

    /// <summary>
    /// SMTP server host address
    /// </summary>
    public string SmtpHost { get; set; } = "smtp.company.com";

    /// <summary>
    /// SMTP server port (587 for TLS, 465 for SSL, 25 for unencrypted)
    /// </summary>
    public int SmtpPort { get; set; } = 587;

    /// <summary>
    /// SMTP security mode (Auto, None, StartTls, SslOnConnect)
    /// Auto mode intelligently selects based on port:
    /// - Port 25: None
    /// - Port 587: StartTls
    /// - Port 465: SslOnConnect
    /// </summary>
    public SmtpSecurityMode SecurityMode { get; set; } = SmtpSecurityMode.Auto;

    /// <summary>
    /// [DEPRECATED] Use SecurityMode instead.
    /// Legacy property: Use SSL/TLS for SMTP connection.
    /// Only used when SecurityMode is Auto and port doesn't match standard ports.
    /// </summary>
    [Obsolete("Use SecurityMode property instead for more precise control")]
    public bool UseSsl { get; set; } = true;

    /// <summary>
    /// SMTP authentication username
    /// </summary>
    public string SmtpUsername { get; set; } = string.Empty;

    /// <summary>
    /// SMTP authentication password (should be encrypted or from environment variable)
    /// </summary>
    public string SmtpPassword { get; set; } = string.Empty;

    /// <summary>
    /// Email address to send from
    /// </summary>
    public string FromAddress { get; set; } = "noreply@company.com";

    /// <summary>
    /// Display name for sender
    /// </summary>
    public string FromDisplayName { get; set; } = "IKEA DocuScan System";

    /// <summary>
    /// Primary administrator email address
    /// </summary>
    public string AdminEmail { get; set; } = "admin@company.com";

    /// <summary>
    /// Additional administrator email addresses to CC
    /// </summary>
    public string[] AdditionalAdminEmails { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Master switch to enable/disable all email notifications
    /// </summary>
    public bool EnableEmailNotifications { get; set; } = true;

    /// <summary>
    /// Enable access request notification emails
    /// </summary>
    public bool SendAccessRequestNotifications { get; set; } = true;

    /// <summary>
    /// Enable document-related notification emails
    /// </summary>
    public bool SendDocumentNotifications { get; set; } = true;

    /// <summary>
    /// Subject line for access request notifications to admin
    /// </summary>
    public string AccessRequestSubject { get; set; } = "New Access Request - IKEA DocuScan";

    /// <summary>
    /// Subject line for access request confirmation to user
    /// </summary>
    public string AccessRequestConfirmationSubject { get; set; } = "Your Access Request Has Been Received";

    /// <summary>
    /// Application URL for links in emails
    /// </summary>
    public string ApplicationUrl { get; set; } = "https://docuscan.company.com";

    /// <summary>
    /// Timeout in seconds for SMTP operations
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Validate configuration options
    /// </summary>
    public void Validate()
    {
        if (EnableEmailNotifications)
        {
            if (string.IsNullOrWhiteSpace(SmtpHost))
            {
                throw new InvalidOperationException(
                    "SmtpHost is required when email notifications are enabled");
            }

            if (SmtpPort <= 0 || SmtpPort > 65535)
            {
                throw new InvalidOperationException(
                    "SmtpPort must be between 1 and 65535");
            }

            if (string.IsNullOrWhiteSpace(FromAddress))
            {
                throw new InvalidOperationException(
                    "FromAddress is required when email notifications are enabled");
            }

            if (string.IsNullOrWhiteSpace(AdminEmail))
            {
                throw new InvalidOperationException(
                    "AdminEmail is required when email notifications are enabled");
            }
        }
    }
}
