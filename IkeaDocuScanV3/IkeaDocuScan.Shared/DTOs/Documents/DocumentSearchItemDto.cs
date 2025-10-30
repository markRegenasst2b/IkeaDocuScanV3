namespace IkeaDocuScan.Shared.DTOs.Documents;

/// <summary>
/// Single document item in search results
/// Contains all display columns for the search results table
/// </summary>
public class DocumentSearchItemDto
{
    /// <summary>
    /// Document ID (for actions)
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Document barcode (unique identifier)
    /// </summary>
    public int BarCode { get; set; }

    /// <summary>
    /// Document type name
    /// </summary>
    public string? DocumentType { get; set; }

    /// <summary>
    /// Document name
    /// </summary>
    public string? DocumentName { get; set; }

    /// <summary>
    /// Counterparty name
    /// </summary>
    public string? Counterparty { get; set; }

    /// <summary>
    /// Counterparty alpha-numeric code
    /// </summary>
    public string? CounterpartyNo { get; set; }

    /// <summary>
    /// Counterparty country
    /// </summary>
    public string? Country { get; set; }

    /// <summary>
    /// Third party names (comma-separated)
    /// </summary>
    public string? ThirdParty { get; set; }

    /// <summary>
    /// Date of contract
    /// </summary>
    public DateTime? DateOfContract { get; set; }

    /// <summary>
    /// Receiving date
    /// </summary>
    public DateTime? ReceivingDate { get; set; }

    /// <summary>
    /// Sending out date
    /// </summary>
    public DateTime? SendingOutDate { get; set; }

    /// <summary>
    /// Forwarded to signatories date
    /// </summary>
    public DateTime? ForwardedToSignatoriesDate { get; set; }

    /// <summary>
    /// Dispatch date
    /// </summary>
    public DateTime? DispatchDate { get; set; }

    /// <summary>
    /// Action date
    /// </summary>
    public DateTime? ActionDate { get; set; }

    /// <summary>
    /// Comment/notes
    /// </summary>
    public string? Comment { get; set; }

    /// <summary>
    /// Fax received (Yes/No)
    /// </summary>
    public bool? Fax { get; set; }

    /// <summary>
    /// Original received (Yes/No)
    /// </summary>
    public bool? OriginalReceived { get; set; }

    /// <summary>
    /// Translation received (Yes/No)
    /// </summary>
    public bool? TranslationReceived { get; set; }

    /// <summary>
    /// Confidential (Yes/No)
    /// </summary>
    public bool? Confidential { get; set; }

    /// <summary>
    /// Document number
    /// </summary>
    public string? DocumentNo { get; set; }

    /// <summary>
    /// Associated to PUA/Agreement number
    /// </summary>
    public string? AssociatedToPua { get; set; }

    /// <summary>
    /// Associated to Appendix number
    /// </summary>
    public string? AssociatedToAppendix { get; set; }

    /// <summary>
    /// Version number
    /// </summary>
    public string? VersionNo { get; set; }

    /// <summary>
    /// Valid until / As of date
    /// </summary>
    public DateTime? ValidUntil { get; set; }

    /// <summary>
    /// Currency code
    /// </summary>
    public string? CurrencyCode { get; set; }

    /// <summary>
    /// Amount
    /// </summary>
    public decimal? Amount { get; set; }

    /// <summary>
    /// Authorisation to
    /// </summary>
    public string? Authorisation { get; set; }

    /// <summary>
    /// Bank confirmation (Yes/No)
    /// </summary>
    public bool? BankConfirmation { get; set; }

    /// <summary>
    /// Counterparty city
    /// </summary>
    public string? City { get; set; }

    /// <summary>
    /// Affiliated to
    /// </summary>
    public string? AffiliatedTo { get; set; }

    /// <summary>
    /// Action description
    /// </summary>
    public string? ActionDescription { get; set; }

    /// <summary>
    /// Has associated file
    /// </summary>
    public bool HasFile { get; set; }

    /// <summary>
    /// File ID (for download/open actions)
    /// </summary>
    public int? FileId { get; set; }

    /// <summary>
    /// Document name (for display purposes)
    /// </summary>
    public string? Name { get; set; }
}
