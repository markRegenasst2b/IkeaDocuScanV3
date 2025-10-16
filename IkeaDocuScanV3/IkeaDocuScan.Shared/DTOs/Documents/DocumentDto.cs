namespace IkeaDocuScan.Shared.DTOs.Documents;

public class DocumentDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int BarCode { get; set; }

    // Related entities
    public int? DocumentTypeId { get; set; }
    public string? DocumentTypeName { get; set; }
    public int? CounterPartyId { get; set; }
    public string? CounterPartyName { get; set; }
    public int? DocumentNameId { get; set; }
    public string? DocumentNameText { get; set; }
    public int? FileId { get; set; }

    // Dates
    public DateTime? DateOfContract { get; set; }
    public DateTime? ReceivingDate { get; set; }
    public DateTime? DispatchDate { get; set; }
    public DateTime? ActionDate { get; set; }
    public DateTime? ValidUntil { get; set; }
    public DateTime? SendingOutDate { get; set; }
    public DateTime? ForwardedToSignatoriesDate { get; set; }

    // Text fields
    public string? Comment { get; set; }
    public string? ActionDescription { get; set; }
    public string? ReminderGroup { get; set; }
    public string? DocumentNo { get; set; }
    public string? AssociatedToPua { get; set; }
    public string? VersionNo { get; set; }
    public string? AssociatedToAppendix { get; set; }
    public string? Authorisation { get; set; }
    public string? ThirdParty { get; set; }
    public string? ThirdPartyId { get; set; }

    // Financial
    public string? CurrencyCode { get; set; }
    public decimal? Amount { get; set; }

    // Boolean flags
    public bool? Fax { get; set; }
    public bool? OriginalReceived { get; set; }
    public bool? BankConfirmation { get; set; }
    public bool? TranslatedVersionReceived { get; set; }
    public bool? Confidential { get; set; }

    // Audit fields
    public DateTime CreatedOn { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? ModifiedOn { get; set; }
    public string? ModifiedBy { get; set; }
}
