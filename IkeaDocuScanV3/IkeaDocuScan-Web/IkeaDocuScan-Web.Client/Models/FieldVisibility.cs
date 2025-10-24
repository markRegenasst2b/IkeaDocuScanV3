namespace IkeaDocuScan_Web.Client.Models;

/// <summary>
/// Represents the visibility and validation state of a form field
/// based on DocumentType configuration.
/// Maps to database single-character codes: "N", "O", "M"
/// </summary>
public enum FieldVisibility
{
    /// <summary>
    /// Field is not applicable for this DocumentType.
    /// - Field is disabled and grayed out
    /// - Value is cleared
    /// - No validation applied
    /// - Database code: "N"
    /// </summary>
    NotApplicable,

    /// <summary>
    /// Field is optional for this DocumentType.
    /// - Field is enabled
    /// - No required validation
    /// - User can leave empty
    /// - Database code: "O"
    /// </summary>
    Optional,

    /// <summary>
    /// Field is mandatory for this DocumentType.
    /// - Field is enabled
    /// - Required validator is active
    /// - User must provide value
    /// - Database code: "M"
    /// </summary>
    Mandatory
}
