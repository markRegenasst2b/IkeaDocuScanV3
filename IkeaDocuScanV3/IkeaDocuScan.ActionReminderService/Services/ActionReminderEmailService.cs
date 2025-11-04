using IkeaDocuScan.Infrastructure.Data;
using IkeaDocuScan.Shared.DTOs.ActionReminders;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Text;

namespace IkeaDocuScan.ActionReminderService.Services;

/// <summary>
/// Service implementation for fetching action reminders and sending email notifications
/// </summary>
public class ActionReminderEmailService : IActionReminderEmailService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    private readonly IEmailSender _emailSender;
    private readonly ActionReminderServiceOptions _options;
    private readonly ILogger<ActionReminderEmailService> _logger;

    public ActionReminderEmailService(
        IDbContextFactory<AppDbContext> contextFactory,
        IEmailSender emailSender,
        IOptions<ActionReminderServiceOptions> options,
        ILogger<ActionReminderEmailService> logger)
    {
        _contextFactory = contextFactory;
        _emailSender = emailSender;
        _options = options.Value;
        _logger = logger;
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
            DocumentName = d.DocumentName,
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
        if (_options.RecipientEmails == null || _options.RecipientEmails.Length == 0)
        {
            _logger.LogWarning("No recipient emails configured. Skipping email send.");
            return;
        }

        var subject = _options.EmailSubject.Replace("{Count}", dueActions.Count.ToString());
        var (htmlBody, plainTextBody) = BuildActionReminderEmail(dueActions);

        _logger.LogInformation("Sending action reminder email to {Count} recipient(s)", _options.RecipientEmails.Length);

        await _emailSender.SendEmailAsync(
            _options.RecipientEmails,
            subject,
            htmlBody,
            plainTextBody,
            cancellationToken);

        _logger.LogInformation("Action reminder email sent successfully");
    }

    /// <summary>
    /// Send empty notification when no actions are due
    /// </summary>
    private async Task SendEmptyNotificationAsync(CancellationToken cancellationToken)
    {
        if (_options.RecipientEmails == null || _options.RecipientEmails.Length == 0)
        {
            return;
        }

        var subject = "Action Reminders - No Items Due Today";
        var htmlBody = @"
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

        var plainTextBody = "IKEA DocuScan - Action Reminders\n\nGood news! There are no action reminders due today.";

        await _emailSender.SendEmailAsync(
            _options.RecipientEmails,
            subject,
            htmlBody,
            plainTextBody,
            cancellationToken);

        _logger.LogInformation("Empty notification sent");
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
}
