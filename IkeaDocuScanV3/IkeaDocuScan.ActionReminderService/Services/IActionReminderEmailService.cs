namespace IkeaDocuScan.ActionReminderService.Services;

/// <summary>
/// Service for fetching action reminders and sending email notifications
/// </summary>
public interface IActionReminderEmailService
{
    /// <summary>
    /// Send daily action reminder emails for documents due today (or within specified days ahead)
    /// </summary>
    /// <param name="daysAhead">Number of days to look ahead (0 = today only)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task SendDailyActionRemindersAsync(int daysAhead = 0, CancellationToken cancellationToken = default);
}
