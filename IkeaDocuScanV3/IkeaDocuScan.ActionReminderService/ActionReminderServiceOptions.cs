namespace IkeaDocuScan.ActionReminderService;

/// <summary>
/// Configuration options for the Action Reminder Service
/// </summary>
public class ActionReminderServiceOptions
{
    /// <summary>
    /// Time of day to run the daily check (format: HH:mm)
    /// </summary>
    public string ScheduleTime { get; set; } = "08:00";

    /// <summary>
    /// How often to check if it's time to run (in minutes)
    /// </summary>
    public int CheckIntervalMinutes { get; set; } = 60;

    /// <summary>
    /// Enable or disable the service
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Email addresses to send action reminders to
    /// </summary>
    public string[] RecipientEmails { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Subject template for action reminder emails
    /// Use {Count} placeholder for number of actions
    /// </summary>
    public string EmailSubject { get; set; } = "Action Reminders Due Today - {Count} Items";

    /// <summary>
    /// Whether to send empty notifications (when no actions are due)
    /// </summary>
    public bool SendEmptyNotifications { get; set; } = false;

    /// <summary>
    /// Number of days to look ahead for upcoming actions (default: 0 = today only)
    /// </summary>
    public int DaysAhead { get; set; } = 0;
}
