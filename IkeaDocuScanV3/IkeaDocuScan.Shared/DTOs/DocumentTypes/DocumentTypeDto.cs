namespace IkeaDocuScan.Shared.DTOs.DocumentTypes;

/// <summary>
/// Data transfer object for DocumentType
/// </summary>
public class DocumentTypeDto
{
    public int DtId { get; set; }
    public string DtName { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }

    // Field configuration properties (M=Mandatory, O=Optional, N=Not applicable)
    public string BarCode { get; set; } = "N";
    public string CounterParty { get; set; } = "N";
    public string DateOfContract { get; set; } = "N";
    public string Comment { get; set; } = "N";
    public string ReceivingDate { get; set; } = "N";
    public string DispatchDate { get; set; } = "N";
    public string Fax { get; set; } = "N";
    public string OriginalReceived { get; set; } = "N";
    public string DocumentNo { get; set; } = "N";
    public string AssociatedToPua { get; set; } = "N";
    public string VersionNo { get; set; } = "N";
    public string AssociatedToAppendix { get; set; } = "N";
    public string ValidUntil { get; set; } = "N";
    public string Currency { get; set; } = "N";
    public string Amount { get; set; } = "N";
    public string Authorisation { get; set; } = "N";
    public string BankConfirmation { get; set; } = "N";
    public string TranslatedVersionReceived { get; set; } = "N";
    public string ActionDate { get; set; } = "N";
    public string ActionDescription { get; set; } = "N";
    public string ReminderGroup { get; set; } = "N";
    public string Confidential { get; set; } = "N";
    public string CounterPartyAlpha { get; set; } = "N";
    public string SendingOutDate { get; set; } = "N";
    public string ForwardedToSignatoriesDate { get; set; } = "N";
    public bool IsAppendix { get; set; }
}
