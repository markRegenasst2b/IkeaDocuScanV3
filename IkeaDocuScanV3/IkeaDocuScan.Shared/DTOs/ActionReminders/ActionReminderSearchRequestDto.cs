namespace IkeaDocuScan.Shared.DTOs.ActionReminders;

/// <summary>
/// DTO for filtering action reminders
/// </summary>
public class ActionReminderSearchRequestDto
{
    /// <summary>
    /// Filter by ActionDate >= DateFrom
    /// </summary>
    public DateTime? DateFrom { get; set; }

    /// <summary>
    /// Filter by ActionDate <= DateTo
    /// </summary>
    public DateTime? DateTo { get; set; }

    /// <summary>
    /// Filter by document types
    /// </summary>
    public List<int>? DocumentTypeIds { get; set; }

    /// <summary>
    /// Filter by counter parties
    /// </summary>
    public List<int>? CounterPartyIds { get; set; }

    /// <summary>
    /// Search in counter party name (LIKE search)
    /// </summary>
    public string? CounterPartySearch { get; set; }

    /// <summary>
    /// Search in BarCode, DocumentName, and Comment fields
    /// </summary>
    public string? SearchString { get; set; }

    /// <summary>
    /// Include actions with future dates (default: true)
    /// </summary>
    public bool IncludeFutureActions { get; set; } = true;

    /// <summary>
    /// Only show overdue actions (where ActionDate < today)
    /// </summary>
    public bool IncludeOverdueOnly { get; set; }
}
