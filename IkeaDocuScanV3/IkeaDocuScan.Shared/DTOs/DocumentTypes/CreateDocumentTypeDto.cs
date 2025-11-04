using System.ComponentModel.DataAnnotations;

namespace IkeaDocuScan.Shared.DTOs.DocumentTypes;

/// <summary>
/// DTO for creating a new document type
/// </summary>
public class CreateDocumentTypeDto
{
    [Required(ErrorMessage = "Document type name is required")]
    [StringLength(255, ErrorMessage = "Document type name cannot exceed 255 characters")]
    public string DtName { get; set; } = string.Empty;

    public bool IsEnabled { get; set; } = true;

    // Field configuration properties (M=Mandatory, O=Optional, N=Not applicable)
    [StringLength(1)]
    [RegularExpression("^[MON]$", ErrorMessage = "Value must be M, O, or N")]
    public string BarCode { get; set; } = "N";

    [StringLength(1)]
    [RegularExpression("^[MON]$", ErrorMessage = "Value must be M, O, or N")]
    public string CounterParty { get; set; } = "N";

    [StringLength(1)]
    [RegularExpression("^[MON]$", ErrorMessage = "Value must be M, O, or N")]
    public string DateOfContract { get; set; } = "N";

    [StringLength(1)]
    [RegularExpression("^[MON]$", ErrorMessage = "Value must be M, O, or N")]
    public string Comment { get; set; } = "N";

    [StringLength(1)]
    [RegularExpression("^[MON]$", ErrorMessage = "Value must be M, O, or N")]
    public string ReceivingDate { get; set; } = "N";

    [StringLength(1)]
    [RegularExpression("^[MON]$", ErrorMessage = "Value must be M, O, or N")]
    public string DispatchDate { get; set; } = "N";

    [StringLength(1)]
    [RegularExpression("^[MON]$", ErrorMessage = "Value must be M, O, or N")]
    public string Fax { get; set; } = "N";

    [StringLength(1)]
    [RegularExpression("^[MON]$", ErrorMessage = "Value must be M, O, or N")]
    public string OriginalReceived { get; set; } = "N";

    [StringLength(1)]
    [RegularExpression("^[MON]$", ErrorMessage = "Value must be M, O, or N")]
    public string DocumentNo { get; set; } = "N";

    [StringLength(1)]
    [RegularExpression("^[MON]$", ErrorMessage = "Value must be M, O, or N")]
    public string AssociatedToPua { get; set; } = "N";

    [StringLength(1)]
    [RegularExpression("^[MON]$", ErrorMessage = "Value must be M, O, or N")]
    public string VersionNo { get; set; } = "N";

    [StringLength(1)]
    [RegularExpression("^[MON]$", ErrorMessage = "Value must be M, O, or N")]
    public string AssociatedToAppendix { get; set; } = "N";

    [StringLength(1)]
    [RegularExpression("^[MON]$", ErrorMessage = "Value must be M, O, or N")]
    public string ValidUntil { get; set; } = "N";

    [StringLength(1)]
    [RegularExpression("^[MON]$", ErrorMessage = "Value must be M, O, or N")]
    public string Currency { get; set; } = "N";

    [StringLength(1)]
    [RegularExpression("^[MON]$", ErrorMessage = "Value must be M, O, or N")]
    public string Amount { get; set; } = "N";

    [StringLength(1)]
    [RegularExpression("^[MON]$", ErrorMessage = "Value must be M, O, or N")]
    public string Authorisation { get; set; } = "N";

    [StringLength(1)]
    [RegularExpression("^[MON]$", ErrorMessage = "Value must be M, O, or N")]
    public string BankConfirmation { get; set; } = "N";

    [StringLength(1)]
    [RegularExpression("^[MON]$", ErrorMessage = "Value must be M, O, or N")]
    public string TranslatedVersionReceived { get; set; } = "N";

    [StringLength(1)]
    [RegularExpression("^[MON]$", ErrorMessage = "Value must be M, O, or N")]
    public string ActionDate { get; set; } = "N";

    [StringLength(1)]
    [RegularExpression("^[MON]$", ErrorMessage = "Value must be M, O, or N")]
    public string ActionDescription { get; set; } = "N";

    [StringLength(1)]
    [RegularExpression("^[MON]$", ErrorMessage = "Value must be M, O, or N")]
    public string ReminderGroup { get; set; } = "N";

    [StringLength(1)]
    [RegularExpression("^[MON]$", ErrorMessage = "Value must be M, O, or N")]
    public string Confidential { get; set; } = "N";

    [StringLength(1)]
    [RegularExpression("^[MON]$", ErrorMessage = "Value must be M, O, or N")]
    public string CounterPartyAlpha { get; set; } = "N";

    [StringLength(1)]
    [RegularExpression("^[MON]$", ErrorMessage = "Value must be M, O, or N")]
    public string SendingOutDate { get; set; } = "N";

    [StringLength(1)]
    [RegularExpression("^[MON]$", ErrorMessage = "Value must be M, O, or N")]
    public string ForwardedToSignatoriesDate { get; set; } = "N";

    public bool IsAppendix { get; set; } = false;
}
