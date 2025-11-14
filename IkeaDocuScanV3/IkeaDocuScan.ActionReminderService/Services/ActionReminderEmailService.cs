using IkeaDocuScan.Infrastructure.Data;
using IkeaDocuScan.Shared.DTOs.ActionReminders;
using IkeaDocuScan.Shared.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Text;

namespace IkeaDocuScan.ActionReminderService.Services;

/// <summary>
/// Service implementation for fetching action reminders and sending email notifications
/// Enhanced with database-driven configuration and templating
/// </summary>
public class ActionReminderEmailService : IActionReminderEmailService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    private readonly IEmailSender _emailSender;
    private readonly ActionReminderServiceOptions _options;
    private readonly ILogger<ActionReminderEmailService> _logger;
    private readonly ISystemConfigurationManager _configManager;
    private readonly IEmailTemplateService _templateService;

    public ActionReminderEmailService(
        IDbContextFactory<AppDbContext> contextFactory,
        IEmailSender emailSender,
        IOptions<ActionReminderServiceOptions> options,
        ILogger<ActionReminderEmailService> logger,
        ISystemConfigurationManager configManager,
        IEmailTemplateService templateService)
    {
        _contextFactory = contextFactory;
        _emailSender = emailSender;
        _options = options.Value;
        _logger = logger;
        _configManager = configManager;
        _templateService = templateService;
    }

    public async Task SendDailyActionRemindersAsync(int daysAhead = 0, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching action reminders for today (+ {DaysAhead} days)", daysAhead);

        try
        {
            // Get due actions from database
            var dueActions = await GetDueActionsAsync(daysAhead, cancellationToken);

            if (dueActions == null || dueActions.Count == 0)
            {
                _logger.LogInformation("No action reminders due today");

                if (_options.SendEmptyNotifications)
                {
                    await SendEmptyNotificationAsync(cancellationToken);
                }

                return;
            }

            _logger.LogInformation("Found {Count} action reminder(s) due", dueActions.Count);

            // Send email with action reminders
            await SendActionReminderEmailAsync(dueActions, cancellationToken);

            _logger.LogInformation("Successfully sent action reminder emails");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending daily action reminders");
            throw;
        }
    }

    /// <summary>
    /// Fetch due actions from the database
    /// </summary>
    private async Task<List<ActionReminderDto>> GetDueActionsAsync(int daysAhead, CancellationToken cancellationToken)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var today = DateTime.Today;
        var endDate = today.AddDays(daysAhead);

        _logger.LogDebug("Querying documents with ActionDate between {StartDate} and {EndDate}", today, endDate);

        // Build query to fetch documents with action dates due today (or within daysAhead)
        var query = context.Documents
            .AsNoTracking()
            .Where(d => d.ActionDate != null &&
                       d.ActionDate >= d.ReceivingDate &&
                       d.ActionDate >= today &&
                       d.ActionDate <= endDate)
            .Select(d => new
            {
                BarCode = d.BarCode.ToString(),
                DocumentType = d.Dt != null ? d.Dt.DtName : "",
                DocumentName = d.DocumentName != null ? d.DocumentName.Name : "",
                d.DocumentNo,
                CounterParty = d.CounterParty != null ? d.CounterParty.Name ?? "" : "",
                CounterPartyNo = d.CounterParty != null ? (int?)d.CounterParty.CounterPartyId : null,
                d.ActionDate,
                d.ReceivingDate,
                d.ActionDescription,
                d.Comment
            })
            .OrderBy(d => d.ActionDate)
            .ThenBy(d => d.BarCode);

        var results = await query.Select(d => new ActionReminderDto
        {
            BarCode = d.BarCode,
            DocumentType = d.DocumentType,
            DocumentName = d.DocumentName ?? string.Empty,
            DocumentNo = d.DocumentNo,
            CounterParty = d.CounterParty,
            CounterPartyNo = d.CounterPartyNo,
            ActionDate = d.ActionDate,
            ReceivingDate = d.ReceivingDate,
            ActionDescription = d.ActionDescription,
            Comment = d.Comment
        }).ToListAsync(cancellationToken);

        _logger.LogDebug("Retrieved {Count} due actions from database", results.Count);

        return results;
    }

    /// <summary>
    /// Send email with action reminders
    /// </summary>
    private async Task SendActionReminderEmailAsync(List<ActionReminderDto> dueActions, CancellationToken cancellationToken)
    {
        try
        {
            // Get recipients from database (with fallback to config)
            var recipientEmails = await _configManager.GetEmailRecipientsAsync("ActionReminderRecipients");

            if (recipientEmails.Length == 0)
            {
                // Fallback to configuration file
                _logger.LogInformation("Using recipient emails from configuration file");
                if (_options.RecipientEmails == null || _options.RecipientEmails.Length == 0)
                {
                    _logger.LogWarning("No recipient emails configured. Skipping email send.");
                    return;
                }
                recipientEmails = _options.RecipientEmails;
            }

            // Try to get email template from database
            _logger.LogDebug("Attempting to retrieve ActionReminderDaily template from database");
            var template = await _configManager.GetEmailTemplateAsync("ActionReminderDaily");
            string htmlBody, plainTextBody, subject;

            if (template != null)
            {
                // Use database template with loop support
                _logger.LogInformation("Using ActionReminderDaily template from database");
                _logger.LogDebug("Template details - Subject length: {SubjectLen}, HtmlBody length: {HtmlLen}, PlainText length: {PlainLen}",
                    template.Subject?.Length ?? 0,
                    template.HtmlBody?.Length ?? 0,
                    template.PlainTextBody?.Length ?? 0);

                // Log first 200 chars of HTML template to verify content
                if (!string.IsNullOrEmpty(template.HtmlBody) && template.HtmlBody.Length > 0)
                {
                    var preview = template.HtmlBody.Length > 200 ? template.HtmlBody.Substring(0, 200) : template.HtmlBody;
                    _logger.LogDebug("Template HTML preview: {Preview}...", preview);
                }

                var data = new Dictionary<string, object>
                {
                    { "Count", dueActions.Count },
                    { "Date", DateTime.Now }
                };

                _logger.LogDebug("Template data prepared: Count={Count}, Date={Date}", dueActions.Count, DateTime.Now);

                var loops = new Dictionary<string, List<Dictionary<string, object>>>
                {
                    { "ActionRows", dueActions.Select(action => new Dictionary<string, object>
                        {
                            { "BarCode", action.BarCode },
                            { "DocumentType", action.DocumentType },
                            { "DocumentName", action.DocumentName },
                            { "DocumentNo", action.DocumentNo ?? string.Empty },
                            { "CounterParty", action.CounterParty },
                            { "CounterPartyNo", action.CounterPartyNo?.ToString() ?? string.Empty },
                            { "ActionDate", (object?)action.ActionDate ?? string.Empty },
                            { "ReceivingDate", (object?)action.ReceivingDate ?? string.Empty },
                            { "ActionDescription", action.ActionDescription ?? string.Empty },
                            { "Comment", action.Comment ?? string.Empty },
                            { "IsOverdue", action.ActionDate.HasValue && action.ActionDate.Value < DateTime.Today }
                        }).ToList()
                    }
                };

                _logger.LogDebug("Loop data prepared: ActionRows with {RowCount} items", loops["ActionRows"].Count);
                if (loops["ActionRows"].Count > 0)
                {
                    var firstRow = loops["ActionRows"][0];
                    _logger.LogDebug("First ActionRow keys: {Keys}", string.Join(", ", firstRow.Keys));
                }

                _logger.LogDebug("Rendering HTML body with loops...");
                htmlBody = _templateService.RenderTemplateWithLoops(template.HtmlBody ?? string.Empty, data, loops);
                _logger.LogDebug("HTML body rendered. Length: {Length}", htmlBody?.Length ?? 0);

                _logger.LogDebug("Rendering plain text body...");
                plainTextBody = !string.IsNullOrEmpty(template.PlainTextBody)
                    ? _templateService.RenderTemplateWithLoops(template.PlainTextBody, data, loops)
                    : BuildPlainTextActionReminder(dueActions);
                _logger.LogDebug("Plain text body rendered. Length: {Length}", plainTextBody?.Length ?? 0);

                _logger.LogDebug("Rendering subject...");
                subject = _templateService.RenderTemplate(template.Subject ?? "Action Reminder", data);
                _logger.LogDebug("Subject rendered: {Subject}", subject);
            }
            else
            {
                // Fallback to hard-coded template
                _logger.LogInformation("Using hard-coded ActionReminderDaily template");
                (htmlBody, plainTextBody) = BuildActionReminderEmail(dueActions);
                subject = _options.EmailSubject.Replace("{Count}", dueActions.Count.ToString());
            }

            _logger.LogInformation("Sending action reminder email to {Count} recipient(s): {Recipients}",
                recipientEmails.Length, string.Join(", ", recipientEmails));
            _logger.LogDebug("Email details - Subject: {Subject}, HTML length: {HtmlLen}, PlainText length: {PlainLen}",
                subject, htmlBody?.Length ?? 0, plainTextBody?.Length ?? 0);

            await _emailSender.SendEmailAsync(
                recipientEmails,
                subject ?? "Action Reminder",
                htmlBody ?? string.Empty,
                plainTextBody,
                cancellationToken);

            _logger.LogInformation("Action reminder email sent successfully to {Count} recipient(s)", recipientEmails.Length);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send action reminder email");
            throw;
        }
    }

    /// <summary>
    /// Send empty notification when no actions are due
    /// </summary>
    private async Task SendEmptyNotificationAsync(CancellationToken cancellationToken)
    {
        try
        {
            // Get recipients from database (with fallback to config)
            var recipientEmails = await _configManager.GetEmailRecipientsAsync("ActionReminderRecipients");

            if (recipientEmails.Length == 0)
            {
                // Fallback to configuration file
                if (_options.RecipientEmails == null || _options.RecipientEmails.Length == 0)
                {
                    return;
                }
                recipientEmails = _options.RecipientEmails;
            }

            // Try to get email template from database
            var template = await _configManager.GetEmailTemplateAsync("ActionReminderEmpty");
            string htmlBody, plainTextBody, subject;

            if (template != null)
            {
                // Use database template
                _logger.LogInformation("Using ActionReminderEmpty template from database");

                var data = new Dictionary<string, object>
                {
                    { "Date", DateTime.Now }
                };

                htmlBody = _templateService.RenderTemplate(template.HtmlBody, data);
                plainTextBody = !string.IsNullOrEmpty(template.PlainTextBody)
                    ? _templateService.RenderTemplate(template.PlainTextBody, data)
                    : "IKEA DocuScan - Action Reminders\n\nGood news! There are no action reminders due today.";
                subject = _templateService.RenderTemplate(template.Subject, data);
            }
            else
            {
                // Fallback to hard-coded template
                _logger.LogInformation("Using hard-coded ActionReminderEmpty template");
                subject = "Action Reminders - No Items Due Today";
                htmlBody = @"
<html>
<body style='font-family: Arial, sans-serif;'>
    <h2 style='color: #0051BA;'>IKEA DocuScan - Action Reminders</h2>
    <p>Good news! There are no action reminders due today.</p>
    <hr style='border: 1px solid #FFDA1A;' />
    <p style='font-size: 12px; color: #666;'>
        This is an automated message from the IKEA DocuScan Action Reminder Service.
    </p>
</body>
</html>";
                plainTextBody = "IKEA DocuScan - Action Reminders\n\nGood news! There are no action reminders due today.";
            }

            await _emailSender.SendEmailAsync(
                recipientEmails,
                subject,
                htmlBody,
                plainTextBody,
                cancellationToken);

            _logger.LogInformation("Empty notification sent");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send empty notification");
            // Don't throw - email failures shouldn't break the service
        }
    }

    /// <summary>
    /// Build HTML and plain text email bodies for action reminders
    /// </summary>
    private (string HtmlBody, string PlainTextBody) BuildActionReminderEmail(List<ActionReminderDto> dueActions)
    {
        var htmlBuilder = new StringBuilder();
        var textBuilder = new StringBuilder();

        // HTML version
        htmlBuilder.AppendLine(@"
<html>
<head>
    <style>
        body { font-family: Arial, sans-serif; }
        h2 { color: #0051BA; }
        table { border-collapse: collapse; width: 100%; margin-top: 20px; }
        th { background-color: #0051BA; color: white; padding: 10px; text-align: left; border: 1px solid #ddd; }
        td { padding: 8px; border: 1px solid #ddd; }
        tr:nth-child(even) { background-color: #f2f2f2; }
        .overdue { background-color: #ffcccc !important; }
        .footer { font-size: 12px; color: #666; margin-top: 20px; border-top: 1px solid #FFDA1A; padding-top: 10px; }
    </style>
</head>
<body>
    <h2>IKEA DocuScan - Action Reminders Due Today</h2>
    <p>The following documents have actions that are due today:</p>
    <table>
        <thead>
            <tr>
                <th>Barcode</th>
                <th>Document Type</th>
                <th>Document Name</th>
                <th>Document No</th>
                <th>Counterparty</th>
                <th>Action Date</th>
                <th>Receiving Date</th>
                <th>Action Description</th>
            </tr>
        </thead>
        <tbody>");

        // Plain text version header
        textBuilder.AppendLine("IKEA DocuScan - Action Reminders Due Today");
        textBuilder.AppendLine("==========================================");
        textBuilder.AppendLine();
        textBuilder.AppendLine("The following documents have actions that are due today:");
        textBuilder.AppendLine();

        // Add rows
        foreach (var action in dueActions)
        {
            var rowClass = action.ActionDate.HasValue && action.ActionDate.Value < DateTime.Today ? " class='overdue'" : "";

            htmlBuilder.AppendLine($@"
            <tr{rowClass}>
                <td><strong>{action.BarCode}</strong></td>
                <td>{action.DocumentType}</td>
                <td>{action.DocumentName}</td>
                <td>{action.DocumentNo ?? ""}</td>
                <td>{action.CounterParty}</td>
                <td>{action.ActionDate?.ToString("dd/MM/yyyy") ?? ""}</td>
                <td>{action.ReceivingDate?.ToString("dd/MM/yyyy") ?? ""}</td>
                <td>{action.ActionDescription ?? ""}</td>
            </tr>");

            textBuilder.AppendLine($"Barcode: {action.BarCode}");
            textBuilder.AppendLine($"  Document Type: {action.DocumentType}");
            textBuilder.AppendLine($"  Document Name: {action.DocumentName}");
            if (!string.IsNullOrEmpty(action.DocumentNo))
                textBuilder.AppendLine($"  Document No: {action.DocumentNo}");
            textBuilder.AppendLine($"  Counterparty: {action.CounterParty}");
            textBuilder.AppendLine($"  Action Date: {action.ActionDate?.ToString("dd/MM/yyyy") ?? ""}");
            if (action.ReceivingDate.HasValue)
                textBuilder.AppendLine($"  Receiving Date: {action.ReceivingDate.Value:dd/MM/yyyy}");
            if (!string.IsNullOrEmpty(action.ActionDescription))
                textBuilder.AppendLine($"  Action Description: {action.ActionDescription}");
            textBuilder.AppendLine();
        }

        // HTML footer
        htmlBuilder.AppendLine(@"
        </tbody>
    </table>
    <div class='footer'>
        <p>This is an automated message from the IKEA DocuScan Action Reminder Service.</p>
        <p>Please log in to the DocuScan system to view and manage these actions.</p>
    </div>
</body>
</html>");

        // Plain text footer
        textBuilder.AppendLine("==========================================");
        textBuilder.AppendLine("This is an automated message from the IKEA DocuScan Action Reminder Service.");
        textBuilder.AppendLine("Please log in to the DocuScan system to view and manage these actions.");

        return (htmlBuilder.ToString(), textBuilder.ToString());
    }

    /// <summary>
    /// Build plain text email body for action reminders (fallback for templates)
    /// </summary>
    private string BuildPlainTextActionReminder(List<ActionReminderDto> dueActions)
    {
        var textBuilder = new StringBuilder();

        textBuilder.AppendLine("IKEA DocuScan - Action Reminders Due Today");
        textBuilder.AppendLine("==========================================");
        textBuilder.AppendLine();
        textBuilder.AppendLine("The following documents have actions that are due today:");
        textBuilder.AppendLine();

        foreach (var action in dueActions)
        {
            textBuilder.AppendLine($"Barcode: {action.BarCode}");
            textBuilder.AppendLine($"  Document Type: {action.DocumentType}");
            textBuilder.AppendLine($"  Document Name: {action.DocumentName}");
            if (!string.IsNullOrEmpty(action.DocumentNo))
                textBuilder.AppendLine($"  Document No: {action.DocumentNo}");
            textBuilder.AppendLine($"  Counterparty: {action.CounterParty}");
            textBuilder.AppendLine($"  Action Date: {action.ActionDate?.ToString("dd/MM/yyyy") ?? ""}");
            if (action.ReceivingDate.HasValue)
                textBuilder.AppendLine($"  Receiving Date: {action.ReceivingDate.Value:dd/MM/yyyy}");
            if (!string.IsNullOrEmpty(action.ActionDescription))
                textBuilder.AppendLine($"  Action Description: {action.ActionDescription}");
            textBuilder.AppendLine();
        }

        textBuilder.AppendLine("==========================================");
        textBuilder.AppendLine("This is an automated message from the IKEA DocuScan Action Reminder Service.");
        textBuilder.AppendLine("Please log in to the DocuScan system to view and manage these actions.");

        return textBuilder.ToString();
    }
}
