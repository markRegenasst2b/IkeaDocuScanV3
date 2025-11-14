# Blazor Migration Specification
## Document Properties Management Module

### Executive Summary

This specification covers the migration of two ASP.NET WebForms pages (`EditProperties.aspx` and `EnterProperties.aspx`) and their shared user control (`DocumentPropertyControl.ascx`) to a modern Blazor application using Blazorize and Entity Framework Core.

**Recommendation: Consolidate into ONE Blazor component** with different modes:
- **Edit Mode** - Edit existing document properties
- **Register Mode** - Create new document metadata only
- **Check-in Mode** - Associate scanned file with properties

---

## Current Architecture Analysis

### Pages Overview

| Page | Purpose | URL Parameters | Property Set |
|------|---------|----------------|--------------|
| **EditProperties.aspx** | Edit existing document properties | `barcode` (int) | 1 or 2 based on file presence |
| **EnterProperties.aspx** | Register new OR check-in scanned document | `filename` (string)<br>`id` (string: "new" or barcode) | 1 (register) or 2 (check-in) |

### Property Set Differences

**Property Set 1** (Metadata Registration Only):
- DispatchDate field is **disabled** and not mandatory
- Used when registering document without file

**Property Set 2** (Full Check-in):
- DispatchDate field is **enabled** and mandatory
- Used when checking in scanned file or editing existing document

### Page-Specific Features

#### EditProperties.aspx
- **Header Fields:**
  - Bar Code (read-only label)
  - File Name (clickable link to view PDF)

- **Actions:**
  - "Save Changes" button
  - "Compare with Standard Contract" button (enabled only if DocumentType selected)
  - "Cancel" button (closes window)

- **Behavior:**
  - Opens existing document by barcode
  - Updates `ModifiedBy` and `ModifiedOn` fields
  - Logs to AuditTrail with action "Edit"
  - Closes window after save
  - Refreshes parent window on save

#### EnterProperties.aspx
- **Header Fields:**
  - Bar Code (editable textbox in Register mode, read-only in Check-in mode)
  - File Name (clickable link OR "(none)" in Register mode)

- **Actions:**
  - "Check-in Document" OR "Register Document" button (text changes based on mode)
  - "Check-in confirmed" button (hidden, used for duplicate confirmation)
  - "Compare with Standard Contract" button (visible only in Check-in mode)
  - "Cancel" button (closes window)

- **Behavior:**
  - **Register Mode** (`filename=none`, `id=new`):
    - Allows entering barcode manually
    - Validates barcode is unique and integer
    - Does NOT attach file
    - Auto-focuses barcode field after save (allows continuous registration)
    - Shows alert and stays on page for next entry

  - **Check-in Mode** (`filename=<name>`, `id=<barcode>`):
    - Barcode is read-only (light gray background)
    - Reads PDF file from CheckinDirectory
    - Stores file bytes in database
    - Deletes file from CheckinDirectory after successful check-in
    - Closes window after save

  - **Duplicate Detection** (December 2020 enhancement):
    - Checks if document with same DocumentType, DocumentNo, and VersionNo exists
    - Shows confirmation dialog with list of existing barcodes (max 5)
    - User can confirm to proceed or cancel
    - Uses hidden button trick for confirmation flow

---

## Form Structure

### DocumentPropertyControl Layout

The control is organized into **4 main sections** using `<fieldset>` elements:

#### 1. Document Section (Left Column, Spans 3 Rows)

| Field | Type | Validation | Notes |
|-------|------|------------|-------|
| **Document Type** | DropDown | Required | Auto-postback, triggers field visibility logic |
| **Counterparty No.** | TextBox | Custom (must exist) | Alpha-numeric, max 32 chars, auto-postback |
| **Counterparty** | DropDown | Required | Populated from DB, auto-postback |
| **Location** | TextBox | - | Read-only, auto-filled from CounterParty (City, Country) |
| **Affiliated to** | TextBox | - | Read-only, auto-filled from CounterParty |
| **Available Third Parties** | ListBox (Multi-select) | - | 7 rows, move items to Selected |
| **Add/Remove Buttons** | Buttons | - | JavaScript-based list manipulation |
| **Selected Third Parties** | ListBox (Multi-select) | - | **5 rows** (updated), persisted to hidden fields |
| **Date of Contract** | TextBox + Calendar | Required, Date format | Flexible date parsing, calendar picker |
| **Receiving Date** | TextBox + Calendar | Required, Date format | Flexible date parsing, calendar picker |
| **Sending Out Date** | TextBox + Calendar | Required, Date format | **NEW FIELD** - Calendar picker |
| **Forwarded To Signatories Date** | TextBox + Calendar | Required, Date format | **NEW FIELD** - Calendar picker |
| **Dispatch Date** | TextBox + Calendar | Conditional Required, Date format | Enabled only in Property Set 2 |
| **Comment** | TextArea (MultiLine) | Required, Max 255 chars | **3 rows** (updated from 4) |

**⚠️ NOTE:** The new date fields reference `Constants.LabelSendingOutDate` and `Constants.LabelForwardedToSignatoriesDate` which need to be added to Constants.cs.

#### 2. Action Section (Right Column, Top)

| Field | Type | Validation | Notes |
|-------|------|------------|-------|
| **Action Date** | TextBox + Calendar | Conditional (all or none), Date format | Calendar picker |
| **Action Description** | TextArea | Conditional (all or none), Max 255 chars | 4 rows |
| **E-Mail Reminder Group** | DropDown | Conditional (hidden now) | Hidden via `display:none`, loaded from LDAP |
| **Distribution List Label** | Label | - | Shows configured email distribution list |

**Validation Logic:** If ANY action field is filled, ALL must be filled (Action Date + Action Description).

#### 3. Flags Section (Right Column, Middle)

| Field | Type | Validation | Notes |
|-------|------|------------|-------|
| **Fax** | DropDown (Yes/No/Empty) | Must choose Yes or No | Tri-state boolean |
| **Original Received** | DropDown (Yes/No/Empty) | Must choose Yes or No | Tri-state boolean |
| **Translation Received** | DropDown (Yes/No/Empty) | Must choose Yes or No | Tri-state boolean |
| **Confidential** | DropDown (Yes/No/Empty) | Must choose Yes or No | Tri-state boolean |

**Values:** Empty string (""), "TRUE", "FALSE" (mapped to BOOL enum)

#### 4. Additional Info Section (Right Column, Bottom)

| Field | Type | Validation | Notes |
|-------|------|------------|-------|
| **Document Name** | DropDown | - | Loaded based on selected DocumentType |
| **Document No.** | TextBox | Required, Max 255 chars | Free text |
| **Version No.** | TextBox | Required, Max 255 chars | Free text |
| **Associated to PUA/Agreement No.** | TextBox | Required, Max 255 chars | Free text |
| **Associated to Appendix No.** | TextBox | Required, Max 255 chars | Free text |
| **Valid Until/As Of** | TextBox + Calendar | Required, Date format | Calendar picker |
| **Amount** | TextBox | Conditional Required, Decimal format | Validated based on Currency decimal places |
| **Currency** | DropDown | Required if Amount entered | Loaded from DB |
| **Authorisation to** | TextBox | Required, Max 255 chars | Free text |
| **Bank Confirmation** | DropDown (Yes/No/Empty) | Must choose Yes or No | Tri-state boolean |

**Amount Validation:**
- If Amount entered, Currency MUST be selected
- Decimal places validated based on Currency.DecimalPlaces
- Regex patterns: `@"^\d+$"` (integral) or `@"^\d+(\.\d{1,N})?$"` (N decimals)

#### 5. Form Data Section (Bottom, Full Width)

| Field | Type | Action |
|-------|------|--------|
| **Copy Button** | Button | Copies all form values to session cookie (XML serialized) |
| **Paste Button** | Button | Pastes form values from session cookie, enabled only if cookie exists |

**Copy/Paste Functionality:**
- Allows copying all field values from one document to another
- Uses XML serialization stored in HTTP cookie
- Cookie expires after 10 days
- Paste button auto-disabled when no copied data available
- Useful for entering multiple similar documents

---

## Recent Changes Summary

### Updates Identified in Latest Version

1. **NEW DATE FIELDS ADDED:**
   - **Sending Out Date** (`tbSendingOutDate`) - Lines 151-157
     - Required field with date validation
     - Calendar picker included
     - Validators: `vldMandatory16`, `vldDate16`

   - **Forwarded To Signatories Date** (`tbForwardedToSignatoriesDate`) - Lines 160-166
     - Required field with date validation
     - Calendar picker included
     - Validators: `vldMandatory17`, `vldDate17`

2. **UI ADJUSTMENTS:**
   - Selected Third Parties listbox: Changed from 7 rows to **5 rows** (line 131)
   - Comment textarea: Changed from 4 rows to **3 rows** (line 182)

3. **MISSING CONSTANTS:**
   - `Constants.LabelSendingOutDate` - Referenced but not defined in Constants.cs
   - `Constants.LabelForwardedToSignatoriesDate` - Referenced but not defined in Constants.cs
   - **Action Required:** Add these constants to Constants.cs before deployment

---

## Field Visibility and Validation Rules

### Dynamic Field Behavior (based on DocumentType selection)

When DocumentType dropdown changes:

1. **No selection (index 0):**
   - ALL validators are disabled
   - User can freely navigate away

2. **DocumentType selected:**
   - System queries `DocumentType` entity for field configuration
   - Each field has one of three states:
     - **NA (Not Applicable):** Field disabled, grayed out, value cleared
     - **Optional:** Field enabled, no required validator
     - **Mandatory:** Field enabled, required validator enabled

**Implementation:** `DocumentType.FieldType` enum with properties per field name:
- Returns: `FieldType.NA`, `FieldType.Optional`, or `FieldType.Mandatory`
- Controls both field enable/disable state and validator enable/disable state

### Date Field Parsing

**Supported Formats:**
- Full: `31-Jan-2004`, `31-1-2004`
- Month omitted: `Jan-2004`, `1-2004` (assumes first day)
- Short year: `31-Jan-4`, `31-1-4`

**Implementation:** `ConversionUtility.ParseDate(string value, Utility.DefaultDay defaultDay)`
- `DefaultDay.FirstDay` - when day omitted, use 1st
- `DefaultDay.LastDay` - when day omitted, use last day of month

### Counterparty Field Dependencies

**Cascade Flow:**
1. User enters **Counterparty No.** (alpha-numeric)
   - OnTextChanged event fires (auto-postback)
   - Looks up CounterParty by `CounterPartyNoAlpha`
   - If found:
     - Sets **Counterparty** dropdown to matching item
     - Populates **Location** (City + Country)
     - Populates **Affiliated to**
   - If not found:
     - Clears all fields
     - Sets custom validator to invalid

2. User selects **Counterparty** dropdown
   - OnSelectedIndexChanged event fires (auto-postback)
   - If index 0 (empty): clears Location, Counterparty No, Affiliated to
   - Otherwise: populates from selected CounterParty entity

**Validation:** Custom validator checks `CounterPartyNoAlphaIsValid` stored in ViewState

### Third Party Selection

**Behavior:**
- Two listboxes side-by-side
- Items can be double-clicked OR use Add/Remove buttons
- Selected items persisted to two hidden fields:
  - `selected3rdPartyId` - semicolon-separated IDs
  - `selected3rdPartyName` - semicolon-separated names
- JavaScript functions: `MoveItem()`, `MoveOneItem()`, `PersistOptionList()`, `DisplayOptionList()`
- On page load, `DisplayOptionList()` restores selection from hidden fields

---

## Data Model

### Document Entity (Updated with New Fields)

```csharp
public partial class Document : EntityObject
{
    // Primary Key
    public int BarCode { get; set; }

    // Document Classification
    public int? DT_ID { get; set; }  // DocumentType foreign key
    public virtual DocumentType DocumentType { get; set; }
    public int? DocumentNameId { get; set; }
    public virtual DocumentName DocumentName { get; set; }

    // Counterparty Information
    public string CounterPartyId { get; set; }  // String! (can be int or alpha)
    public virtual CounterParty CounterParty { get; set; }
    public string ThirdPartyId { get; set; }  // Semicolon-separated
    public string ThirdParty { get; set; }     // Semicolon-separated names

    // Dates
    public DateTime DateOfContract { get; set; }
    public DateTime ReceivingDate { get; set; }
    public DateTime SendingOutDate { get; set; }              // NEW FIELD
    public DateTime ForwardedToSignatoriesDate { get; set; }  // NEW FIELD
    public DateTime DispatchDate { get; set; }
    public DateTime ActionDate { get; set; }
    public DateTime ValidUntil { get; set; }

    // Text Fields
    public string Comment { get; set; }
    public string ActionDescription { get; set; }
    public string DocumentNo { get; set; }
    public string VersionNo { get; set; }
    public string AssociatedToPUA { get; set; }
    public string AssociatedToAppendix { get; set; }
    public string Authorisation { get; set; }
    public string ReminderGroup { get; set; }

    // Financial
    public decimal? Amount { get; set; }
    public string CurrencyCode { get; set; }
    public virtual Currency Currency { get; set; }

    // Flags (nullable bool)
    public bool? Fax { get; set; }
    public bool? OriginalReceived { get; set; }
    public bool? TranslatedVersionReceived { get; set; }
    public bool? Confidential { get; set; }
    public bool? BankConfirmation { get; set; }

    // File Association
    public int? FileId { get; set; }
    public virtual DocumentFile DocumentFile { get; set; }

    // Audit Fields
    public DateTime CreatedOn { get; set; }
    public string CreatedBy { get; set; }
    public DateTime ModifiedOn { get; set; }
    public string ModifiedBy { get; set; }
    public string Name { get; set; }

    // Computed Properties (not in DB)
    public long FileSizeInKb => DocumentFile?.Bytes.Length / 1024 + 1 ?? 0;
    public int CounterPartyNo => CounterParty?.CounterPartyNo ?? int.MinValue;
    public string CounterPartyNoAlpha => CounterParty?.CounterPartyNoAlpha ?? string.Empty;
    public string Country => CounterParty?.CountryCode;
    public string City => CounterParty?.City;
    public string AffiliatedTo => CounterParty?.AffiliatedTo;
    public string Filename => $"{BarCode}.pdf";
}
```

### Related Entities

**DocumentFile**
```csharp
public partial class DocumentFile : EntityObject
{
    public int Id { get; set; }
    public byte[] Bytes { get; set; }  // PDF binary content
    public string FileName { get; set; }
}
```

**DocumentType**
```csharp
public partial class DocumentType : EntityObject
{
    public int Id { get; set; }
    public string Name { get; set; }
    // Field configuration for each property (NA/Optional/Mandatory)
    public FieldType GetFieldType(string propertyName) { ... }
}
```

**CounterParty**
```csharp
public partial class CounterParty : EntityObject
{
    public string CounterPartyId { get; set; }  // String (can be int or alpha)
    public int CounterPartyNo { get; set; }      // Integer version
    public string CounterPartyNoAlpha { get; set; }  // Alpha-numeric version
    public string Name { get; set; }
    public string City { get; set; }
    public string CountryCode { get; set; }
    public string AffiliatedTo { get; set; }
    public virtual Country Country { get; set; }
}
```

**Currency**
```csharp
public partial class Currency : EntityObject
{
    public string CurrencyCode { get; set; }  // e.g., "USD", "EUR"
    public int DecimalPlaces { get; set; }     // e.g., 2 for USD, 0 for JPY
}
```

---

## Business Logic

### Data Operations (DataFacade Methods)

```csharp
// Document CRUD
Document LoadDocumentByBarcode(int barcode);
Document LoadDocumentWithFileByBarcode(int barcode);
void UpdateDocument(Document document);
bool DoesDocumentExist(int barcode);
int[] GetSimilarRegistrations(int? documentTypeId, string documentNo, string versionNo);

// Lookups
ICollection LoadAllDocumentTypes();
ICollection LoadAllCounterParties();
ICollection LoadAllCurrencies();
ICollection LoadDocumentNamesByDocumentTypeId(int documentTypeId);

// Single entity loads
DocumentType LoadDocumentTypeById(int id);
CounterParty LoadCounterPartyById(string id);
CounterParty LoadCounterPartyById(int id);
CounterParty LoadCounterPartyByCounterpartyNo(int counterPartyNo);
CounterParty LoadCounterPartyByCounterpartyNoAlpha(string counterPartyNoAlpha);
Currency LoadCurrencyByCode(string code);
DocumentName LoadDocumentNameByDocumentNameId(int id);

// Permissions
bool HasPermission(string userName, Document document);

// Audit
void InsertToAuditTrail(string user, string action, string details, string barcode);
```

### Save Operations

#### EditProperties - Save Changes

**Location:** EditProperties.aspx.cs:134-145

```csharp
private string DoCheckin()
{
    Document document = documentProperties.Document;
    document.BarCode = Int32.Parse(lblDocument.Text);
    document.ModifiedOn = DateTime.Now;
    document.ModifiedBy = HttpContext.Current.User.Identity.Name;

    Audit.Log(Audit.Action.Edit, document.BarCode.ToString(), document.ToString());
    DataFacade.UpdateDocument(document);

    return "Successfully changed the document properties!";
}
```

#### EnterProperties - Check-in/Register

**Location:** EnterProperties.aspx.cs:351-390

```csharp
private string DoCheckin()
{
    int id = Int32.Parse(tbBarCode.Text.Trim());

    // Read file from disk (if not register mode)
    byte[] buffer = ReadFileFromDisk(out string fullName);

    Document document = DocumentProperties.Document;
    document.BarCode = id;

    // Prevent duplicate file upload
    if (document.DocumentFile != null)
    {
        return $"Could not check in file for document with barcode '{document.BarCode}'. " +
               $"A document with name '{document.DocumentFile.FileName}' was already loaded.";
    }

    string fileName = fullName == null ? $"{id}.pdf" : BtnDocumentFile.Text;

    if (buffer.Length > 0)
    {
        document.DocumentFile = new DocumentFile
        {
            Bytes = buffer,
            FileName = fileName
        };
    }

    document.Name = document.Name ?? fileName;
    document.CreatedOn = DateTime.Now;
    document.CreatedBy = HttpContext.Current.User.Identity.Name;

    DataFacade.UpdateDocument(document);

    Audit.Log(Register ? Audit.Action.Register : Audit.Action.CheckIn,
              document.BarCode.ToString(), document.ToString());

    // Delete file from checkin directory
    if (fullName != null) File.Delete(fullName);

    return $"Successfully {(Register ? "registered" : "checked in")} document!";
}
```

---

## UI/UX Design

### Color Scheme

**Background:**
- Page: `lightgrey` (#D3D3D3)
- Fieldsets: White with gray border
- Disabled fields: `LightGray` (#D3D3D3)
- Enabled fields: `White`

**Text:**
- Body: `#000000` (black)
- Labels: `#000000` (black, 11px Tahoma)
- Subcaptions: `#666666` (gray, bold)
- Errors: `#FF0000` (red, 11px)
- Validators: `#FF0000` (red, asterisk)

**Borders:**
- Fieldsets: Gray border, 3px padding
- Input fields: `solid 1px gray`
- Buttons: `solid 1px gray`

**Fonts:**
- All: `Tahoma, 11px`
- Legend: `12px`

### Layout Specifications

**Main Table:**
- Fixed layout
- Margin: 10px top, 10px left
- Columns: 17px | 123px | 375px | (flexible)

**Fieldset Widths:**
- Document section: 50% width (left column, rowspan 3)
- Action section: 50% width (right column, top)
- Flags section: 50% width (right column, middle)
- Additional Info: 50% width (right column, bottom)
- Form Data: 100% width (bottom row, colspan 2)

**Input Widths:**
- Standard textbox: 280px
- Amount: 120px
- Currency dropdown: 80px
- Boolean dropdowns: 80px
- Available Third Party listbox: 280px, 7 rows
- Selected Third Party listbox: 280px, **5 rows** (updated)
- TextAreas: 280px width
  - Comment: **3 rows** (updated from 4)
  - Action Description: 4 rows

**Calendar Button:**
- 16px × 16px
- Background: calendar.gif icon
- Position: Immediately right of date textbox

### Responsive Behavior

**Current State:**
- Fixed-width layout (~930px wide × 700px height)
- Opens in popup window
- NOT responsive

**Blazor Recommendation:**
- Use Blazorize Grid system
- Mobile: Stack sections vertically
- Tablet/Desktop: Two-column layout

---

## Blazor Component Design

### Proposed Component Structure

```
📁 Components/
├── 📁 DocumentManagement/
│   ├── DocumentPropertiesPage.razor          # Main page component
│   ├── DocumentPropertiesPage.razor.cs       # Code-behind
│   ├── DocumentPropertiesForm.razor          # Reusable form component
│   ├── DocumentSectionFields.razor           # Document fieldset (14 fields now)
│   ├── ActionSectionFields.razor             # Action fieldset
│   ├── FlagsSectionFields.razor              # Flags fieldset
│   ├── AdditionalInfoFields.razor            # Additional info fieldset
│   ├── ThirdPartySelector.razor              # Dual-listbox component
│   └── FormDataActions.razor                 # Copy/Paste buttons
│
├── 📁 Shared/
│   ├── DatePicker.razor                      # Reusable date picker
│   ├── TriStateDropdown.razor                # Yes/No/Empty dropdown
│   └── ValidationSummaryCard.razor           # Error display
│
└── 📁 Models/
    ├── DocumentPropertiesViewModel.cs        # Form model
    ├── DocumentPropertyMode.cs               # Enum: Edit/Register/CheckIn
    └── FormDataCopyState.cs                  # Copy/paste state
```

### Main Page Component Example

**DocumentPropertiesPage.razor**

```razor
@page "/documents/properties/{Mode}/{Id:int?}"
@using Blazorise

<Container Fluid>
    <Div Background="Background.Light" Padding="Padding.Is3">

        @* Header Section *@
        <Row MarginBottom="Margin.Is3">
            <Column>
                <Card>
                    <CardBody>
                        <Row>
                            <Column ColumnSize="ColumnSize.Is6">
                                <Field Horizontal>
                                    <FieldLabel ColumnSize="ColumnSize.Is3">Bar Code:</FieldLabel>
                                    <FieldBody ColumnSize="ColumnSize.Is9">
                                        @if (IsRegisterMode)
                                        {
                                            <TextEdit @bind-Text="@Model.BarCode"
                                                      ElementId="barcodeInput" />
                                        }
                                        else
                                        {
                                            <Text>@Model.BarCode</Text>
                                        }
                                    </FieldBody>
                                </Field>
                            </Column>
                            <Column ColumnSize="ColumnSize.Is6">
                                <Field Horizontal>
                                    <FieldLabel ColumnSize="ColumnSize.Is3">File Name:</FieldLabel>
                                    <FieldBody ColumnSize="ColumnSize.Is9">
                                        @if (HasFile)
                                        {
                                            <Button Color="Color.Link" Clicked="@ViewDocument">
                                                @Model.FileName
                                            </Button>
                                        }
                                        else
                                        {
                                            <Text>(none)</Text>
                                        }
                                    </FieldBody>
                                </Field>
                            </Column>
                        </Row>
                    </CardBody>
                </Card>
            </Column>
        </Row>

        @* Action Buttons *@
        <Row MarginBottom="Margin.Is3">
            <Column>
                <Buttons>
                    <Button Color="Color.Primary" Clicked="@SaveDocument">
                        @SaveButtonText
                    </Button>
                    @if (!IsRegisterMode)
                    {
                        <Button Color="Color.Secondary"
                                Clicked="@CompareWithStandard"
                                Disabled="@(Model.DocumentTypeId == null)">
                            Compare with Standard Contract
                        </Button>
                    }
                    <Button Color="Color.Light" Clicked="@Cancel">
                        Cancel
                    </Button>
                </Buttons>
            </Column>
        </Row>

        @* Main Form *@
        <DocumentPropertiesForm @bind-Model="@Model"
                                PropertySetNumber="@PropertySetNumber" />
    </Div>
</Container>
```

### ViewModel (Updated with New Fields)

**DocumentPropertiesViewModel.cs**

```csharp
public class DocumentPropertiesViewModel
{
    // Header
    public string BarCode { get; set; }
    public string FileName { get; set; }

    // Document Section
    public int? DocumentTypeId { get; set; }
    public string CounterPartyNoAlpha { get; set; }
    public string CounterPartyId { get; set; }
    public string Location { get; set; }  // Read-only
    public string AffiliatedTo { get; set; }  // Read-only
    public List<string> SelectedThirdPartyIds { get; set; } = new();
    public DateTime? DateOfContract { get; set; }
    public DateTime? ReceivingDate { get; set; }
    public DateTime? SendingOutDate { get; set; }              // NEW FIELD
    public DateTime? ForwardedToSignatoriesDate { get; set; }  // NEW FIELD
    public DateTime? DispatchDate { get; set; }
    public string Comment { get; set; }

    // Action Section
    public DateTime? ActionDate { get; set; }
    public string ActionDescription { get; set; }
    public string EmailReminderGroup { get; set; }

    // Flags Section
    public bool? Fax { get; set; }
    public bool? OriginalReceived { get; set; }
    public bool? TranslationReceived { get; set; }
    public bool? Confidential { get; set; }

    // Additional Info Section
    public int? DocumentNameId { get; set; }
    public string DocumentNo { get; set; }
    public string VersionNo { get; set; }
    public string AssociatedToPUA { get; set; }
    public string AssociatedToAppendix { get; set; }
    public DateTime? ValidUntil { get; set; }
    public decimal? Amount { get; set; }
    public string CurrencyCode { get; set; }
    public string Authorisation { get; set; }
    public bool? BankConfirmation { get; set; }

    // Metadata
    public DocumentPropertyMode Mode { get; set; }
    public int PropertySetNumber { get; set; }
    public Dictionary<string, FieldVisibility> FieldConfig { get; set; } = new();
}

public enum DocumentPropertyMode
{
    Edit,
    Register,
    CheckIn
}

public enum FieldVisibility
{
    NotApplicable,  // Disabled, grayed out
    Optional,       // Enabled, not required
    Mandatory       // Enabled, required
}
```

---

## Migration Prerequisites

### Required Database Updates

1. **Add New Columns to Document Table:**
```sql
ALTER TABLE Document
ADD SendingOutDate DATETIME NULL;

ALTER TABLE Document
ADD ForwardedToSignatoriesDate DATETIME NULL;
```

2. **Add Constants to Constants.cs:**
```csharp
public const string LabelSendingOutDate = "Sending Out Date:";
public const string LabelForwardedToSignatoriesDate = "Forwarded To Signatories Date:";

// Also add captions for search/display
public const string CaptionSendingOutDate = "Sending Out Date";
public const string CaptionForwardedToSignatoriesDate = "Forwarded To Signatories Date";
```

3. **Update DocumentType Configuration:**
   - Ensure DocumentType.GetFieldType() handles new fields
   - Add field visibility rules for SendingOutDate and ForwardedToSignatoriesDate

---

## Questions for Clarification

Before proceeding with implementation, please clarify:

### 1. New Date Fields Purpose
**Q:** What is the business purpose of the new date fields?
- **Sending Out Date**: When document was sent out? To whom?
- **Forwarded To Signatories Date**: When forwarded for signatures?
- Should these be conditional/optional based on DocumentType?

### 2. File Upload Mechanism
**Q:** How should file upload work in Blazor?
- **Option A:** Upload via web form (user browses for PDF)
- **Option B:** Continue monitoring checkin directory
- **Option C:** Both options

**Current:** Files dropped into monitored directory.

### 3. Window Management
**Q:** Should Blazor maintain popup window behavior?
- **Option A:** Popup windows (JavaScript window.open)
- **Option B:** Modal dialogs (Blazorize Modal)
- **Option C:** In-page navigation

**Current:** Popup windows that close after save.

### 4. Copy/Paste Functionality
**Q:** How should form data copying work?
- **Option A:** Browser localStorage
- **Option B:** Server-side session storage
- **Option C:** In-memory state

**Current:** HTTP cookie with 10-day expiration.

### 5. Duplicate Detection
**Q:** Should duplicate detection be:
- **Option A:** Blocking modal dialog
- **Option B:** Warning message with checkbox
- **Option C:** Toast notification

**Current:** JavaScript `confirm()` dialog.

### 6. Calendar Component
**Q:** Which date picker?
- **Option A:** Blazorize DateEdit
- **Option B:** Third-party component
- **Option C:** Custom component

**Current:** Custom JavaScript calendar with flexible parsing.

### 7. Third Party Selector
**Q:** Should dual-listbox be:
- **Option A:** Custom Blazor component
- **Option B:** Existing library
- **Option C:** Simple multi-select dropdown

**Current:** Custom dual-listbox with Add/Remove buttons.

### 8. Authorization
**Q:** How should permissions be enforced?
- **Option A:** Existing HasPermission stored procedure
- **Option B:** .NET authorization policies
- **Option C:** Both (defense in depth)

### 9. LDAP Integration
**Q:** Should reminder groups use LDAP?
- **Option A:** Yes, maintain LDAP
- **Option B:** Move to database
- **Option C:** Configurable

**Current:** Loads from Active Directory.

### 10. Property Set Configuration
**Q:** Should DocumentType field configs be:
- **Option A:** Stored in database
- **Option B:** JSON/configuration file
- **Option C:** Hardcoded

**Current:** DocumentType.GetFieldType() method.

### 11. Validation Display
**Q:** How should validation errors show?
- **Option A:** Inline next to each field
- **Option B:** Summary at top only
- **Option C:** Both inline and summary

**Current:** Asterisk (*) + summary at top.

### 12. Responsive Design
**Q:** What devices should be supported?
- **Option A:** Desktop only
- **Option B:** Desktop + Tablet
- **Option C:** Fully responsive (all devices)

**Current:** Desktop only (~930px fixed width).

### 13. PDF Viewing
**Q:** How should "View Document" work?
- **Option A:** Open in new tab/window
- **Option B:** Inline PDF viewer in modal
- **Option C:** Download to device

**Current:** Opens in popup window.

---

## Implementation Phases

### Phase 1: Foundation (Week 1-2)
- Set up Blazor Server with Blazorize
- Configure Entity Framework Core
- **Add new date fields to database schema**
- **Add new constants to Constants.cs**
- Create entity models and DbContext
- Implement repository pattern
- Create service layer
- Set up authentication/authorization

### Phase 2: Core Form (Week 3-4)
- Build DocumentPropertiesForm component
- Implement four field sections with **14 date fields** (updated)
- Create reusable sub-components
- Implement dynamic field visibility
- Add FluentValidation rules for all fields including new date fields

### Phase 3: Mode Logic (Week 5)
- Implement Edit mode
- Implement Register mode
- Implement Check-in mode
- Add duplicate detection
- Implement barcode validation

### Phase 4: Advanced Features (Week 6)
- Third Party dual-listbox (5 rows for selected)
- Copy/Paste functionality
- Calendar date picker for **all 7 date fields**
- File upload/download
- Compare with Standard Contract

### Phase 5: Styling & UX (Week 7)
- Apply Blazorize theme
- Match original color scheme
- Create responsive layouts
- Add loading indicators
- Implement error handling

### Phase 6: Testing (Week 8)
- Unit tests
- Integration tests
- End-to-end testing
- Performance optimization
- Bug fixes

---

## Success Criteria

✓ All three modes work correctly
✓ All validation rules match original
✓ Dynamic field visibility works
✓ **New date fields integrated and functional**
✓ Data saves correctly
✓ Audit logging maintained
✓ Permission checking enforced
✓ Look and feel matches original
✓ No regressions
✓ Performance acceptable (< 2s load)
✓ Code is maintainable

---

## Technical Stack

**Frontend:**
- Blazor Server (.NET 6+)
- Blazorize UI Framework
- Custom CSS
- JavaScript Interop

**Backend:**
- .NET 6+ / C# 10+
- Entity Framework Core 6+
- FluentValidation
- AutoMapper

**Database:**
- SQL Server
- EF Core migrations

**Testing:**
- xUnit
- bUnit
- Moq
- FluentAssertions

---

**Document Version:** 1.1 (Updated)
**Created:** 2025
**Last Updated:** 2025 - Added Sending Out Date and Forwarded To Signatories Date fields
