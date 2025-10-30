namespace IkeaDocuScan.Shared.DTOs.Documents;

/// <summary>
/// Request DTO for document search with comprehensive filter criteria
/// </summary>
public class DocumentSearchRequestDto
{
    // ========================================
    // GENERAL FILTERS
    // ========================================

    /// <summary>
    /// Full-text search in document file bytes (PDF content via iFilter)
    /// </summary>
    public string? SearchString { get; set; }

    /// <summary>
    /// Comma-separated list of barcodes (OR logic, max 256 chars)
    /// Example: "12345,67890,11111"
    /// </summary>
    public string? Barcodes { get; set; }

    /// <summary>
    /// Selected document type IDs (multi-select, OR logic)
    /// </summary>
    public List<int> DocumentTypeIds { get; set; } = new();

    /// <summary>
    /// Document name ID (single select, filtered by document types)
    /// </summary>
    public int? DocumentNameId { get; set; }

    /// <summary>
    /// Document number (contains search)
    /// </summary>
    public string? DocumentNumber { get; set; }

    /// <summary>
    /// Version number (contains search)
    /// </summary>
    public string? VersionNo { get; set; }

    /// <summary>
    /// Associated to PUA/Agreement number (contains search)
    /// </summary>
    public string? AssociatedToPua { get; set; }

    /// <summary>
    /// Associated to Appendix number (contains search)
    /// </summary>
    public string? AssociatedToAppendix { get; set; }

    // ========================================
    // COUNTERPARTY FILTERS
    // ========================================

    /// <summary>
    /// Counterparty name (free-text search in counterparty and third-party names)
    /// </summary>
    public string? CounterpartyName { get; set; }

    /// <summary>
    /// Counterparty number (exact match on CounterPartyNoAlpha)
    /// </summary>
    public string? CounterpartyNo { get; set; }

    /// <summary>
    /// Counterparty country (exact match)
    /// </summary>
    public string? CounterpartyCountry { get; set; }

    /// <summary>
    /// Counterparty city (free-text search)
    /// </summary>
    public string? CounterpartyCity { get; set; }

    // ========================================
    // DOCUMENT ATTRIBUTES
    // ========================================

    /// <summary>
    /// Fax received (Yes/No filter)
    /// </summary>
    public bool? Fax { get; set; }

    /// <summary>
    /// Original received (Yes/No filter)
    /// </summary>
    public bool? OriginalReceived { get; set; }

    /// <summary>
    /// Confidential (Yes/No filter)
    /// </summary>
    public bool? Confidential { get; set; }

    /// <summary>
    /// Bank confirmation (Yes/No filter)
    /// </summary>
    public bool? BankConfirmation { get; set; }

    /// <summary>
    /// Authorisation to (contains search)
    /// </summary>
    public string? Authorisation { get; set; }

    // ========================================
    // FINANCIAL FILTERS
    // ========================================

    /// <summary>
    /// Minimum amount (inclusive, open-ended)
    /// </summary>
    public decimal? AmountFrom { get; set; }

    /// <summary>
    /// Maximum amount (inclusive, open-ended)
    /// </summary>
    public decimal? AmountTo { get; set; }

    /// <summary>
    /// Currency code (exact match)
    /// </summary>
    public string? CurrencyCode { get; set; }

    // ========================================
    // DATE FILTERS (all open-ended ranges)
    // ========================================

    /// <summary>
    /// Date of Contract - From (inclusive)
    /// </summary>
    public DateTime? DateOfContractFrom { get; set; }

    /// <summary>
    /// Date of Contract - To (inclusive)
    /// </summary>
    public DateTime? DateOfContractTo { get; set; }

    /// <summary>
    /// Receiving Date - From (inclusive)
    /// </summary>
    public DateTime? ReceivingDateFrom { get; set; }

    /// <summary>
    /// Receiving Date - To (inclusive)
    /// </summary>
    public DateTime? ReceivingDateTo { get; set; }

    /// <summary>
    /// Sending Out Date - From (inclusive)
    /// </summary>
    public DateTime? SendingOutDateFrom { get; set; }

    /// <summary>
    /// Sending Out Date - To (inclusive)
    /// </summary>
    public DateTime? SendingOutDateTo { get; set; }

    /// <summary>
    /// Forwarded to Signatories Date - From (inclusive)
    /// </summary>
    public DateTime? ForwardedToSignatoriesDateFrom { get; set; }

    /// <summary>
    /// Forwarded to Signatories Date - To (inclusive)
    /// </summary>
    public DateTime? ForwardedToSignatoriesDateTo { get; set; }

    /// <summary>
    /// Dispatch Date - From (inclusive)
    /// </summary>
    public DateTime? DispatchDateFrom { get; set; }

    /// <summary>
    /// Dispatch Date - To (inclusive)
    /// </summary>
    public DateTime? DispatchDateTo { get; set; }

    /// <summary>
    /// Action Date - From (inclusive)
    /// </summary>
    public DateTime? ActionDateFrom { get; set; }

    /// <summary>
    /// Action Date - To (inclusive)
    /// </summary>
    public DateTime? ActionDateTo { get; set; }

    // ========================================
    // PAGINATION & SORTING
    // ========================================

    /// <summary>
    /// Current page number (1-based)
    /// </summary>
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// Number of results per page (default: 25)
    /// </summary>
    public int PageSize { get; set; } = 25;

    /// <summary>
    /// Column to sort by (null = no sorting)
    /// </summary>
    public string? SortColumn { get; set; }

    /// <summary>
    /// Sort direction: "asc" or "desc"
    /// </summary>
    public string? SortDirection { get; set; }

    // ========================================
    // HELPER METHODS
    // ========================================

    /// <summary>
    /// Parses the comma-separated barcode string into a list of integers
    /// </summary>
    public List<int> GetBarcodeList()
    {
        if (string.IsNullOrWhiteSpace(Barcodes))
            return new List<int>();

        return Barcodes
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(b => b.Trim())
            .Where(b => int.TryParse(b, out _))
            .Select(int.Parse)
            .Distinct()
            .ToList();
    }

    /// <summary>
    /// Checks if any filter criteria is set
    /// </summary>
    public bool HasAnyFilter()
    {
        return !string.IsNullOrWhiteSpace(SearchString)
            || !string.IsNullOrWhiteSpace(Barcodes)
            || DocumentTypeIds.Any()
            || DocumentNameId.HasValue
            || !string.IsNullOrWhiteSpace(DocumentNumber)
            || !string.IsNullOrWhiteSpace(VersionNo)
            || !string.IsNullOrWhiteSpace(AssociatedToPua)
            || !string.IsNullOrWhiteSpace(AssociatedToAppendix)
            || !string.IsNullOrWhiteSpace(CounterpartyName)
            || !string.IsNullOrWhiteSpace(CounterpartyNo)
            || !string.IsNullOrWhiteSpace(CounterpartyCountry)
            || !string.IsNullOrWhiteSpace(CounterpartyCity)
            || Fax.HasValue
            || OriginalReceived.HasValue
            || Confidential.HasValue
            || BankConfirmation.HasValue
            || !string.IsNullOrWhiteSpace(Authorisation)
            || AmountFrom.HasValue
            || AmountTo.HasValue
            || !string.IsNullOrWhiteSpace(CurrencyCode)
            || DateOfContractFrom.HasValue
            || DateOfContractTo.HasValue
            || ReceivingDateFrom.HasValue
            || ReceivingDateTo.HasValue
            || SendingOutDateFrom.HasValue
            || SendingOutDateTo.HasValue
            || ForwardedToSignatoriesDateFrom.HasValue
            || ForwardedToSignatoriesDateTo.HasValue
            || DispatchDateFrom.HasValue
            || DispatchDateTo.HasValue
            || ActionDateFrom.HasValue
            || ActionDateTo.HasValue;
    }
}
