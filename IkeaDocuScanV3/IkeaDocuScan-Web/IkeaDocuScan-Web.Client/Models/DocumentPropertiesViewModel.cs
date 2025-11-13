using System.Text.Json.Serialization;
using IkeaDocuScan_Web.Client.Pages;
using Microsoft.Extensions.Logging;

namespace IkeaDocuScan_Web.Client.Models;

/// <summary>
/// View model for the Document Properties form.
/// Contains all 40+ fields organized into 4 main sections:
/// 1. Document Section (14 fields)
/// 2. Action Section (4 fields)
/// 3. Flags Section (4 fields)
/// 4. Additional Info Section (10 fields)
/// Plus header fields and metadata.
/// </summary>
public class DocumentPropertiesViewModel
{
    // ========================================
    // INJECTED DEPENDENCIES
    // ========================================

    /// <summary>
    /// Logger instance (injected from component)
    /// Excluded from copy/paste - runtime dependency
    /// </summary>
    [JsonIgnore]
    public ILogger<DocumentPropertiesPage>? Logger { get; set; }
    // ========================================
    // HEADER FIELDS
    // ========================================

    /// <summary>
    /// Document ID (primary key)
    /// Excluded from copy/paste - each document should have unique ID
    /// </summary>
    [JsonIgnore]
    public int? Id { get; set; }

    /// <summary>
    /// Document barcode (unique identifier)
    /// - Editable in Register mode
    /// - Read-only in Edit and Check-in modes
    /// </summary>
    public string BarCode { get; set; } = string.Empty;

    /// <summary>
    /// Associated file name (e.g., "12345.pdf")
    /// - Clickable link if file exists
    /// - Shows "(none)" in Register mode
    /// </summary>
    public string? FileName { get; set; }

    /// <summary>
    /// Document name (auto-generated or user-provided)
    /// </summary>
    public string? Name { get; set; }

    // ========================================
    // DOCUMENT SECTION (14 fields)
    // ========================================

    /// <summary>
    /// Document Type ID (foreign key)
    /// Triggers field visibility configuration when changed
    /// </summary>
    public int? DocumentTypeId { get; set; }

    /// <summary>
    /// Document Type name (for display)
    /// </summary>
    public string? DocumentTypeName { get; set; }

    /// <summary>
    /// Counter Party alpha-numeric code (user entry field)
    /// Triggers auto-lookup of Counter Party on blur/change
    /// </summary>
    public string? CounterPartyNoAlpha { get; set; }

    /// <summary>
    /// Counter Party ID (foreign key, auto-populated from lookup)
    /// </summary>
    public string? CounterPartyId { get; set; }

    /// <summary>
    /// Counter Party name (for display)
    /// </summary>
    public string? CounterPartyName { get; set; }

    /// <summary>
    /// Location (read-only, auto-filled from CounterParty: "City, Country")
    /// </summary>
    public string? Location { get; set; }

    /// <summary>
    /// Affiliated to (read-only, auto-filled from CounterParty)
    /// </summary>
    public string? AffiliatedTo { get; set; }

    /// <summary>
    /// Selected third party IDs (semicolon-separated in database)
    /// Managed via dual-listbox selector
    /// </summary>
    public List<string> SelectedThirdPartyIds { get; set; } = new();

    /// <summary>
    /// Selected third party names (semicolon-separated in database)
    /// Stored for display purposes
    /// </summary>
    public List<string> SelectedThirdPartyNames { get; set; } = new();

    /// <summary>
    /// Date of Contract (required)
    /// </summary>
    public DateTime? DateOfContract { get; set; }

    /// <summary>
    /// Receiving Date (required)
    /// </summary>
    public DateTime? ReceivingDate { get; set; }

    /// <summary>
    /// Sending Out Date (required) - NEW FIELD
    /// When document was sent out to external parties
    /// </summary>
    public DateTime? SendingOutDate { get; set; }

    /// <summary>
    /// Forwarded To Signatories Date (required) - NEW FIELD
    /// When document was forwarded for signatures
    /// </summary>
    public DateTime? ForwardedToSignatoriesDate { get; set; }

    /// <summary>
    /// Dispatch Date (conditionally required based on Property Set)
    /// - Disabled in Property Set 1 (Register mode)
    /// - Enabled and required in Property Set 2 (Edit/Check-in modes)
    /// </summary>
    public DateTime? DispatchDate { get; set; }

    /// <summary>
    /// Comment (required, max 255 characters, 3 rows)
    /// </summary>
    public string? Comment { get; set; }

    // ========================================
    // ACTION SECTION (4 fields)
    // ========================================

    /// <summary>
    /// Action Date (conditionally required)
    /// If ANY action field is filled, ALL must be filled
    /// </summary>
    public DateTime? ActionDate { get; set; }

    /// <summary>
    /// Action Description (conditionally required, max 255 characters, 4 rows)
    /// If ANY action field is filled, ALL must be filled
    /// </summary>
    public string? ActionDescription { get; set; }

    /// <summary>
    /// Email Reminder Group (LDAP group name)
    /// Hidden by default via display:none
    /// Loaded from Active Directory
    /// </summary>
    public string? EmailReminderGroup { get; set; }

    /// <summary>
    /// Distribution list label (read-only display)
    /// Shows configured email distribution list
    /// </summary>
    public string? DistributionListLabel { get; set; }

    // ========================================
    // FLAGS SECTION (4 tri-state booleans)
    // ========================================

    /// <summary>
    /// Fax received? (required: must choose Yes or No)
    /// </summary>
    public bool? Fax { get; set; }

    /// <summary>
    /// Original document received? (required: must choose Yes or No)
    /// </summary>
    public bool? OriginalReceived { get; set; }

    /// <summary>
    /// Translation received? (required: must choose Yes or No)
    /// </summary>
    public bool? TranslationReceived { get; set; }

    /// <summary>
    /// Document is confidential? (required: must choose Yes or No)
    /// </summary>
    public bool? Confidential { get; set; }

    // ========================================
    // ADDITIONAL INFO SECTION (10 fields)
    // ========================================

    /// <summary>
    /// Document Name ID (foreign key)
    /// Dropdown filtered by selected DocumentType
    /// </summary>
    public int? DocumentNameId { get; set; }

    /// <summary>
    /// Document Name (for display)
    /// </summary>
    public string? DocumentNameText { get; set; }

    /// <summary>
    /// Document Number (required, max 255 characters)
    /// Free text, used for duplicate detection
    /// </summary>
    public string? DocumentNo { get; set; }

    /// <summary>
    /// Version Number (required, max 255 characters)
    /// Free text, used for duplicate detection
    /// </summary>
    public string? VersionNo { get; set; }

    /// <summary>
    /// Associated to PUA/Agreement Number (required, max 255 characters)
    /// </summary>
    public string? AssociatedToPUA { get; set; }

    /// <summary>
    /// Associated to Appendix Number (required, max 255 characters)
    /// </summary>
    public string? AssociatedToAppendix { get; set; }

    /// <summary>
    /// Valid Until / As Of date (required)
    /// </summary>
    public DateTime? ValidUntil { get; set; }

    /// <summary>
    /// Amount (conditionally required - if entered, Currency must be selected)
    /// Decimal places validated based on Currency.DecimalPlaces
    /// </summary>
    public decimal? Amount { get; set; }

    /// <summary>
    /// Currency Code (required if Amount entered)
    /// </summary>
    public string? CurrencyCode { get; set; }

    /// <summary>
    /// Currency name (for display)
    /// </summary>
    public string? CurrencyName { get; set; }

    /// <summary>
    /// Authorisation to (required, max 255 characters)
    /// </summary>
    public string? Authorisation { get; set; }

    /// <summary>
    /// Bank Confirmation received? (required: must choose Yes or No)
    /// </summary>
    public bool? BankConfirmation { get; set; }

    // ========================================
    // METADATA & CONFIGURATION
    // ========================================

    /// <summary>
    /// Current operational mode (Edit/Register/Check-in)
    /// Determines UI behavior and validation rules
    /// Excluded from copy/paste - runtime state
    /// </summary>
    [JsonIgnore]
    public DocumentPropertyMode Mode { get; set; } = DocumentPropertyMode.Register;

    /// <summary>
    /// Property Set Number (1 or 2)
    /// - Property Set 1: DispatchDate disabled (Register mode)
    /// - Property Set 2: DispatchDate enabled (Edit/Check-in modes)
    /// Excluded from copy/paste - runtime configuration
    /// </summary>
    [JsonIgnore]
    public int PropertySetNumber { get; set; } = 1;

    /// <summary>
    /// Field visibility configuration based on selected DocumentType
    /// Key: field name, Value: visibility state (NA/Optional/Mandatory)
    /// Excluded from copy/paste - dynamic configuration dictionary
    /// </summary>
    [JsonIgnore]
    public Dictionary<string, FieldVisibility> FieldConfig { get; set; } = new();

    /// <summary>
    /// File bytes for upload (Check-in mode)
    /// Populated from CheckinDirectory or manual upload
    /// Excluded from copy/paste - large binary data (up to 50MB), file-specific
    /// </summary>
    [JsonIgnore]
    public byte[]? FileBytes { get; set; }

    /// <summary>
    /// Source file path (Check-in mode)
    /// Used to delete file from CheckinDirectory after successful save
    /// Excluded from copy/paste - file system specific path
    /// </summary>
    [JsonIgnore]
    public string? SourceFilePath { get; set; }

    // ========================================
    // AUDIT FIELDS (read-only, display only)
    // ========================================

    /// <summary>
    /// Created on timestamp
    /// Excluded from copy/paste - audit field
    /// </summary>
    [JsonIgnore]
    public DateTime? CreatedOn { get; set; }

    /// <summary>
    /// Created by user
    /// Excluded from copy/paste - audit field
    /// </summary>
    [JsonIgnore]
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Modified on timestamp
    /// Excluded from copy/paste - audit field
    /// </summary>
    [JsonIgnore]
    public DateTime? ModifiedOn { get; set; }

    /// <summary>
    /// Modified by user
    /// Excluded from copy/paste - audit field
    /// </summary>
    [JsonIgnore]
    public string? ModifiedBy { get; set; }

    // ========================================
    // HELPER PROPERTIES
    // ========================================

    /// <summary>
    /// Indicates if a file is attached to this document
    /// </summary>
    public bool HasFile => FileId != null;

    /// <summary>
    /// Indicates if this is a new document (no ID assigned yet)
    /// </summary>
    public bool IsNewDocument => !Id.HasValue;

    /// <summary>
    /// Save button text based on mode
    /// </summary>
    public string SaveButtonText => Mode switch
    {
        DocumentPropertyMode.Edit => "Save Changes",
        DocumentPropertyMode.Register => "Register Document",
        DocumentPropertyMode.CheckIn => "Check-in Document",
        _ => "Save"
    };

    /// <summary>
    /// Indicates if DispatchDate field should be enabled
    /// </summary>
    public bool IsDispatchDateEnabled => PropertySetNumber == 2;

    public int? FileId { get; internal set; }

    /// <summary>
    /// Converts selected third party IDs to semicolon-separated string for database
    /// </summary>
    public string GetThirdPartyIdString() => string.Join(";", SelectedThirdPartyIds);

    /// <summary>
    /// Converts selected third party names to semicolon-separated string for database
    /// </summary>
    public string GetThirdPartyNameString() => string.Join(";", SelectedThirdPartyNames);

    /// <summary>
    /// Parses semicolon-separated third party IDs from database
    /// </summary>
    public void SetThirdPartyIdsFromString(string? thirdPartyIds)
    {
        if (string.IsNullOrWhiteSpace(thirdPartyIds))
        {
            SelectedThirdPartyIds.Clear();
        }
        else
        {
            SelectedThirdPartyIds = thirdPartyIds
                .Split(';', StringSplitOptions.RemoveEmptyEntries)
                .Select(id => id.Trim())
                .ToList();
        }
    }

    /// <summary>
    /// Parses semicolon-separated third party names from database
    /// </summary>
    public void SetThirdPartyNamesFromString(string? thirdPartyNames)
    {
        if (string.IsNullOrWhiteSpace(thirdPartyNames))
        {
            SelectedThirdPartyNames.Clear();
        }
        else
        {
            SelectedThirdPartyNames = thirdPartyNames
                .Split(';', StringSplitOptions.RemoveEmptyEntries)
                .Select(name => name.Trim())
                .ToList();
        }
    }

    /// <summary>
    /// Gets the field visibility for a given field name
    /// </summary>
    public FieldVisibility GetFieldVisibility(string fieldName)
    {
        if (FieldConfig.TryGetValue(fieldName, out var visibility)) {
            return visibility;
        }

        // Suppress warning if DocumentTypeId is not set yet (common during initial page load)
        if (DocumentTypeId.HasValue)
        {
            Logger?.LogWarning($"Field visibility for '{fieldName}' not found in configuration. Defaulting to Optional.");
        }

        return FieldVisibility.Optional; // Default to Optional if not configured
    }

    /// <summary>
    /// Checks if a field is mandatory based on its configuration
    /// </summary>
    public bool IsFieldMandatory(string fieldName)
    {
        return GetFieldVisibility(fieldName) == FieldVisibility.Mandatory;
    }

    /// <summary>
    /// Checks if a field is disabled (Not Applicable) based on its configuration
    /// </summary>
    public bool IsFieldDisabled(string fieldName)
    {
        return GetFieldVisibility(fieldName) == FieldVisibility.NotApplicable;
    }
}
