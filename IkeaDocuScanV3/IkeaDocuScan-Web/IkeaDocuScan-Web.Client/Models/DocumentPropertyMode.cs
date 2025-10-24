namespace IkeaDocuScan_Web.Client.Models;

/// <summary>
/// Represents the operational mode for the Document Properties page.
/// Determines behavior, validation rules, and UI state.
/// </summary>
public enum DocumentPropertyMode
{
    /// <summary>
    /// Edit existing document properties.
    /// - BarCode is read-only
    /// - Loads existing document data
    /// - Updates ModifiedBy/ModifiedOn
    /// - Uses Property Set 2 (DispatchDate enabled)
    /// - Closes window after save
    /// </summary>
    Edit,

    /// <summary>
    /// Register new document metadata only (no file attachment).
    /// - BarCode is editable (user enters manually)
    /// - No file attached
    /// - Uses Property Set 1 (DispatchDate disabled)
    /// - Auto-focuses BarCode after save for continuous entry
    /// - Stays on page after save
    /// </summary>
    Register,

    /// <summary>
    /// Check-in scanned document with file attachment.
    /// - BarCode is read-only (from scanned file or database)
    /// - Attaches PDF file from CheckinDirectory or upload
    /// - Uses Property Set 2 (DispatchDate enabled)
    /// - Deletes file from CheckinDirectory after successful save
    /// - Closes window after save
    /// </summary>
    CheckIn
}
