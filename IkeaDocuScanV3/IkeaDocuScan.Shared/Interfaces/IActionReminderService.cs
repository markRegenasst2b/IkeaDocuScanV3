using IkeaDocuScan.Shared.DTOs.ActionReminders;

namespace IkeaDocuScan.Shared.Interfaces;

/// <summary>
/// Service interface for managing action reminders
/// </summary>
public interface IActionReminderService
{
    /// <summary>
    /// Get all due actions with optional filtering
    /// </summary>
    /// <param name="request">Search criteria (optional)</param>
    /// <returns>List of action reminders</returns>
    Task<List<ActionReminderDto>> GetDueActionsAsync(ActionReminderSearchRequestDto? request = null);

    /// <summary>
    /// Get actions due on a specific date (for email notifications)
    /// </summary>
    /// <param name="date">The date to filter by</param>
    /// <returns>List of action reminders for the specified date</returns>
    Task<List<ActionReminderDto>> GetActionsDueOnDateAsync(DateTime date);

    /// <summary>
    /// Get count of currently due actions (for dashboard/badges)
    /// </summary>
    /// <returns>Count of due actions</returns>
    Task<int> GetDueActionsCountAsync();
}
