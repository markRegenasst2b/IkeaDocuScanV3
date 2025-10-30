# Search Document Page - Implementation Plan

**Route:** `/documents/search`
**Navigation:** Main menu item "Search Documents"
**Last Updated:** 2025-10-30

---

## 📋 Implementation Phases

### Phase 1: DTOs and Service Layer
**Status:** 🟢 Completed
**Estimated Effort:** Medium
**Completed:** 2025-10-30

#### Tasks:
- [x] Create `DocumentSearchRequestDto` with all filter properties
  - [x] General filters (SearchString, Barcode, DocumentTypes, DocumentName, etc.)
  - [x] Counterparty filters (Name, No, Country, City)
  - [x] Document attributes (Fax, OriginalReceived, Confidential, BankConfirmation, Authorisation)
  - [x] Financial filters (Amount range, Currency)
  - [x] Date filters (6 date range filters)
  - [x] Pagination properties (PageNumber, PageSize)
  - [x] Sorting properties (SortColumn, SortDirection)
- [x] Create `DocumentSearchResultDto`
  - [x] TotalCount property
  - [x] Items collection (List<DocumentSearchItemDto>)
  - [x] Pagination metadata (CurrentPage, TotalPages, PageSize)
- [x] Create `DocumentSearchItemDto` with display columns
  - [x] All 30+ columns from specification
  - [x] Formatted display values
- [x] Add `SearchAsync()` method to `DocumentService.cs`
  - [x] Build dynamic LINQ query based on filters
  - [x] Apply sorting before result limit
  - [x] Apply 1000-result limit (configurable)
  - [x] Handle full-text search via iFilter (TODO: implementation pending)
  - [x] Return paginated results
- [x] Create HTTP client service method `DocumentHttpService.SearchAsync()`
- [x] Add search endpoint in `DocumentEndpoints.cs`
  - [x] POST `/api/documents/search`
  - [x] Authorization check
  - [x] Validation

**Dependencies:** None

**Files Created:** ✅
- `IkeaDocuScan.Shared/DTOs/Documents/DocumentSearchRequestDto.cs`
- `IkeaDocuScan.Shared/DTOs/Documents/DocumentSearchResultDto.cs`
- `IkeaDocuScan.Shared/DTOs/Documents/DocumentSearchItemDto.cs`

**Files Modified:** ✅
- `IkeaDocuScan-Web/IkeaDocuScan-Web/Services/DocumentService.cs`
- `IkeaDocuScan-Web.Client/Services/DocumentHttpService.cs`
- `IkeaDocuScan-Web/IkeaDocuScan-Web/Endpoints/DocumentEndpoints.cs`
- `IkeaDocuScan.Shared/Interfaces/IDocumentService.cs`

**Notes:**
- Full-text PDF search (iFilter) marked as TODO - requires full-text indexing setup on DocumentFile.Bytes
- Max results limit hardcoded to 1000 - should be moved to configuration in Phase 2

---

### Phase 2: Configuration and Shared Components
**Status:** 🟢 Completed
**Estimated Effort:** Small
**Completed:** 2025-10-30

#### Tasks:
- [x] Add email configuration to `appsettings.json`
  - [x] `Email:SearchResults:DefaultRecipient`
  - [x] `Email:SearchResults:AttachSubjectTemplate`
  - [x] `Email:SearchResults:AttachBodyTemplate`
  - [x] `Email:SearchResults:LinkSubjectTemplate`
  - [x] `Email:SearchResults:LinkBodyTemplate`
- [x] Add search configuration
  - [x] `DocumentSearch:MaxResults` (default: 1000)
  - [x] `DocumentSearch:DefaultPageSize` (default: 25)
  - [x] `DocumentSearch:PageSizeOptions` (array: [10, 25, 100])
- [x] Create configuration classes for new config
- [x] Register configuration options in Program.cs
- [x] Update DocumentService to use configuration

**Dependencies:** None

**Files Created:** ✅
- `IkeaDocuScan.Shared/Configuration/DocumentSearchOptions.cs`
- `IkeaDocuScan.Shared/Configuration/EmailSearchResultsOptions.cs`

**Files Modified:** ✅
- `IkeaDocuScan-Web/IkeaDocuScan-Web/appsettings.json`
- `IkeaDocuScan-Web/IkeaDocuScan-Web/Program.cs`
- `IkeaDocuScan-Web/IkeaDocuScan-Web/Services/DocumentService.cs`

**Notes:**
- Email templates support placeholders: {DocumentCount}, {Barcodes}, {Links}
- Configuration includes validation methods
- DateRangePicker component will be created as needed in Phase 3

---

### Phase 3: Filter Panel UI
**Status:** 🟢 Completed
**Estimated Effort:** Large
**Completed:** 2025-10-30

#### Tasks:
- [x] Create `SearchDocuments.razor` page component
- [x] Create `SearchDocuments.razor.cs` code-behind
- [x] Build filter panel structure with sections
  - [x] General Filters section (all 8 inputs implemented)
  - [x] Counterparty Filters section (all 4 inputs implemented)
  - [x] Document Attributes section (all 5 inputs implemented)
  - [x] Financial Filters section (all 3 inputs implemented)
  - [x] Date Filters section (all 6 date ranges implemented)
- [x] Implement "Search" button with loading state
- [x] Implement "Clear Filters" button
- [x] Load reference data (DocumentTypes, Countries, Currencies, CounterParties, DocumentNames)
- [x] Implement Document Name filtering logic (dynamic based on selected types)
- [x] Apply Bootstrap styling with custom CSS
- [x] Empty state messages implemented
- [x] Error handling implemented

**Dependencies:** Phase 1 (DTOs), Phase 2 (Config)

**Files Created:** ✅
- `IkeaDocuScan-Web.Client/Pages/SearchDocuments.razor`
- `IkeaDocuScan-Web.Client/Pages/SearchDocuments.razor.cs`

**Notes:**
- All filter sections implemented with Bootstrap 5 styling
- Reference data loaded in parallel for performance
- Document Name dropdown dynamically filtered by selected Document Types
- Search and Clear buttons fully functional
- Empty states: initial, no results, and error states
- Placeholder for Phase 4 results table UI

---

### Phase 4: Results List UI
**Status:** 🟢 Completed
**Estimated Effort:** Large
**Completed:** 2025-10-30

#### Tasks:
- [x] Create results table component structure
- [x] Implement empty states
  - [x] Initial state: "Start your search by entering the criteria above."
  - [x] No results: "No documents found matching your criteria."
- [x] Add selection column with checkboxes
- [x] Implement display columns (17 columns)
  - [x] Bar Code (link to edit page)
  - [x] Document Type
  - [x] Document Name
  - [x] Counterparty
  - [x] Counterparty No.
  - [x] Country
  - [x] Third Party
  - [x] Date of Contract
  - [x] Comment (truncated with tooltip)
  - [x] Fax (Yes/No)
  - [x] Original Received (Yes/No)
  - [x] Document No.
  - [x] Associated to PUA/Agreement No.
  - [x] Version No.
  - [x] Currency
  - [x] Amount
  - [x] Actions dropdown menu
- [x] Implement sortable column headers
  - [x] Click to sort ascending
  - [x] Click again to sort descending → click third time to clear sort
  - [x] Visual indicator (▲/▼ arrow symbols)
  - [x] Single-column sort only
  - [x] Sorting persists across pagination
- [x] Implement pagination controls
  - [x] Previous/Next buttons
  - [x] Page number display (shows 5 pages at a time)
  - [x] Page size dropdown (10, 25, 100)
  - [x] Total results count display ("Showing X to Y of Z results")
- [x] Implement selection controls
  - [x] Select All checkbox in header
  - [x] Select All button (current page)
  - [x] Deselect All button (all pages)
  - [x] Invert Selection button (current page)
  - [x] Selected count display
  - [x] Visual feedback for selected rows (table-active class)
- [x] Apply responsive table styling
  - [x] Bootstrap 5 table classes
  - [x] Sticky header
  - [x] Small text for better density
  - [x] Horizontal scrolling for overflow
- [x] Add loading indicator
  - [x] Search button shows loading state
  - [x] Reference data loading spinner

**Dependencies:** Phase 1 (DTOs), Phase 3 (Filter Panel)

**Files Modified:** ✅
- `IkeaDocuScan-Web.Client/Pages/SearchDocuments.razor`
- `IkeaDocuScan-Web.Client/Pages/SearchDocuments.razor.cs`

**Implementation Details:**

**Code-Behind Methods Added:**
- Sorting: `ToggleSort()`, `GetSortIcon()`
- Selection: `ToggleSelectDocument()`, `ToggleSelectAll()`, `SelectAllOnPage()`, `DeselectAll()`, `InvertSelection()`, `IsDocumentSelected()`
- Pagination: `GoToPage()`, `OnPageSizeChanged()`, `GetStartPage()`, `GetEndPage()`
- Formatting: `FormatBoolean()`, `FormatAmount()`, `FormatDate()`, `FormatThirdParty()`

**State Variables Added:**
- `HashSet<int> selectedDocumentIds` - Tracks selected document IDs across pages
- `string? currentSortColumn` - Current sort column name
- `string? currentSortDirection` - Current sort direction ("asc"/"desc"/null)

**UI Features:**
- Selection controls in card header (Select All, Deselect All, Invert)
- Bulk action bar shown when ≥1 document selected (placeholder buttons disabled)
- Sortable column headers with visual indicators
- Action dropdown per row with 7 actions (most disabled as placeholders)
- Pagination with dynamic page range (shows current ± 2 pages)
- Bootstrap Icons for visual enhancements
- Responsive table with horizontal scrolling

**Notes:**
- Action menu items (Open PDF, View Properties, Email, Delete) are disabled placeholders for Phase 5
- Bulk action buttons (Delete Selected, Print, Email) are disabled placeholders for Phase 6
- Selection state persists across pagination
- Sort is applied server-side before result limit

---

### Phase 5: Per-Row Actions
**Status:** 🟢 Completed
**Estimated Effort:** Medium
**Completed:** 2025-10-30

#### Tasks:
- [x] Add action menu column to results table (completed in Phase 4)
- [x] Implement action menu dropdown (per row) (completed in Phase 4)
- [x] Implement "Open PDF" action
  - [x] Created `GET /api/documents/{id}/download` endpoint
  - [x] Added `GetDocumentFileAsync` method to IDocumentService
  - [x] Implemented server-side PDF download with proper content type
  - [x] Opens PDF in new browser tab
- [x] Implement "View Properties" action
  - [x] Create read-only properties modal component
  - [x] Display all document metadata (organized in 6 sections)
  - [x] Format dates, currencies, yes/no values
  - [x] Fetch full document details via GetByIdAsync
- [x] Implement "Edit Properties" action
  - [x] Navigate to `/documents/edit/{barcode}` (already working)
- [x] Implement "Send as Email (Attach)" action
  - [x] Generate `mailto:` link with document details
  - [x] Subject and body from EmailSearchResultsOptions configuration
  - [x] Falls back to default template if configuration unavailable
- [x] Implement "Send as Email (Link)" action
  - [x] Generate `mailto:` link with download URL
  - [x] Subject and body from EmailSearchResultsOptions configuration
  - [x] Falls back to default template if configuration unavailable
- [x] Implement "Delete" action
  - [x] Show confirmation dialog with barcode
  - [x] Call delete API (DocumentService.DeleteAsync)
  - [x] Refresh results after deletion
  - [x] Remove from selection if deleted
  - [x] Loading indicator during deletion

**Dependencies:** Phase 4 (Results List) ✅

**Files Created:** ✅
- `IkeaDocuScan.Shared/DTOs/Documents/DocumentFileDto.cs` - DTO for file download
- `IkeaDocuScan-Web.Client/Components/ViewPropertiesModal.razor` - Read-only properties modal
- `IkeaDocuScan-Web.Client/Components/DeleteConfirmationModal.razor` - Delete confirmation dialog

**Files Modified:** ✅
- `IkeaDocuScan-Web/IkeaDocuScan-Web/Endpoints/DocumentEndpoints.cs` - Added download endpoint
- `IkeaDocuScan-Web/IkeaDocuScan-Web/Services/DocumentService.cs` - Implemented GetDocumentFileAsync
- `IkeaDocuScan.Shared/Interfaces/IDocumentService.cs` - Added interface method
- `IkeaDocuScan-Web.Client/Services/DocumentHttpService.cs` - Added client method
- `IkeaDocuScan-Web.Client/Pages/SearchDocuments.razor` - Added modals, enabled action menu items
- `IkeaDocuScan-Web.Client/Pages/SearchDocuments.razor.cs` - Fully implemented all action methods

**Implementation Details:**

**Download PDF Endpoint:**
- Route: `GET /api/documents/{id}/download`
- Returns file with proper content type (application/pdf, etc.)
- Supports multiple file types: PDF, DOC, DOCX, XLS, XLSX, JPG, PNG, TIF
- Includes proper file name in response
- Content type determined by file extension

**ViewPropertiesModal Component:**
- Bootstrap modal with scrollable content
- 6 organized sections: General, Counterparty, Financial, Attributes, Dates, References, Additional, Audit
- Formatted display: dates (yyyy-MM-dd), booleans (Yes/No/-), amounts (with currency)
- Third party formatting (semicolon to comma-separated)
- Close button with event callback

**DeleteConfirmationModal Component:**
- Danger-themed modal with warning styling
- Shows document identifier (barcode)
- Confirmation required before delete
- Loading indicator during deletion process
- Cancel and Delete buttons
- Disabled state during operation

**Action Methods:**
- `OpenPdf(int documentId)` - Opens PDF in new browser tab via download endpoint
- `ViewProperties(int documentId)` - Fetches document details and shows modal
- `SendEmailAttach(int documentId)` - Generates mailto: link with configuration templates
- `SendEmailLink(int documentId)` - Generates mailto: link with download URL
- `DeleteDocument(int documentId, int barcode)` - Shows confirmation modal
- `ConfirmDelete()` - Executes deletion, refreshes results
- `CloseViewPropertiesModal()` - Closes modal and clears state
- `CancelDelete()` - Cancels deletion and closes modal

**Bug Fixes:**
- Fixed pagination navigation (changed from `<a href="#">` to `<button type="button">`)
- Fixed sortable headers (changed to `<span>` with cursor:pointer styling)
- Prevented unwanted hash navigation

**Notes:**
- Email configuration loaded from EmailSearchResultsOptions (optional injection)
- Fallback to default templates if configuration not available
- All 7 action menu items now fully functional
- Proper error handling with user-friendly messages
- Comprehensive logging for all operations

---

### Phase 6: Bulk Actions
**Status:** 🔴 Not Started
**Estimated Effort:** Medium

#### Tasks:
- [ ] Add bulk actions toolbar (visible when ≥1 document selected)
- [ ] Implement "Delete Selected" action
  - [ ] Show confirmation with count + list of barcodes/filenames
  - [ ] Call delete API for each selected document
  - [ ] Refresh results after deletion
- [ ] Implement "Print Summary" action
  - [ ] Generate summary report (placeholder/stub for now)
  - [ ] Open print dialog or new tab
- [ ] Implement "Print Detailed" action
  - [ ] Generate detailed report (placeholder/stub for now)
  - [ ] Open print dialog or new tab
- [ ] Implement "Send as Email (Attach)" bulk action
  - [ ] Generate `mailto:` link
  - [ ] Use config template with placeholders
  - [ ] Include multiple document references
  - [ ] Handle attachment limit considerations
- [ ] Implement "Send as Email (Link)" bulk action
  - [ ] Generate `mailto:` link
  - [ ] Use config template with placeholders
  - [ ] Include multiple download links in body
- [ ] Add visual feedback for bulk operations
- [ ] Handle errors gracefully (partial success scenarios)

**Dependencies:** Phase 5 (Per-Row Actions)
**Files to Modify:**
- `IkeaDocuScan-Web.Client/Pages/SearchDocuments.razor`
- `IkeaDocuScan-Web.Client/Pages/SearchDocuments.razor.cs`

---

### Phase 7: Navigation Integration
**Status:** 🔴 Not Started
**Estimated Effort:** Small

#### Tasks:
- [ ] Add "Search Documents" to main navigation menu
  - [ ] Menu label: "Search Documents"
  - [ ] Route: `/documents/search`
  - [ ] Icon (optional)
  - [ ] Placement in menu structure
- [ ] Update route configuration
- [ ] Add authorization policy (if needed)
- [ ] Test navigation from various pages

**Dependencies:** Phase 3 (Basic page exists)
**Files to Modify:**
- `IkeaDocuScan-Web.Client/Components/Layout/NavMenu.razor` (or equivalent)
- `IkeaDocuScan-Web.Client/Components/Routes.razor` (if needed)

---

### Phase 8: Testing and Polish
**Status:** 🔴 Not Started
**Estimated Effort:** Medium

#### Tasks:
- [ ] Test all filter combinations
- [ ] Test sorting with pagination
- [ ] Test selection controls edge cases
- [ ] Test email generation with various document counts
- [ ] Test delete confirmations
- [ ] Verify 1000-result limit behavior
- [ ] Test empty states
- [ ] Test responsive layout on different screen sizes
- [ ] Verify accessibility (keyboard navigation, screen readers)
- [ ] Add loading indicators where appropriate
- [ ] Add error handling and user-friendly messages
- [ ] Performance testing with large result sets
- [ ] Add logging for search queries (audit trail)

**Dependencies:** All previous phases
**Files to Modify:** Various (as needed)

---

## 📊 Overall Progress

**Total Phases:** 8
**Completed:** 5
**In Progress:** 0
**Not Started:** 3

**Overall Status:** 🟡 In Progress (62.5%)

---

## 🔑 Key Technical Notes

### Database Search (iFilter)
- Full-text search on `documentfile.bytes` handled by SQL Server iFilter
- Search query should use `CONTAINS()` or `FREETEXT()` SQL functions
- Performance consideration: Index required on document file content

### Barcode Filter
- Parse comma-separated string: `"12345,67890,11111"` → `[12345, 67890, 11111]`
- Validate each token is valid integer
- Use `WHERE BarCode IN (...)` SQL query

### Sorting Before Limit
```
Query → Filter → Sort → TAKE(1000) → Paginate
```

### Email Templates
Example configuration:
```json
{
  "Email": {
    "SearchResults": {
      "DefaultRecipient": "legal@ikea.com",
      "AttachSubjectTemplate": "IKEA Document(s): {DocumentCount} file(s)",
      "AttachBodyTemplate": "Please find attached {DocumentCount} document(s) with barcodes: {Barcodes}",
      "LinkSubjectTemplate": "IKEA Document Links: {DocumentCount} file(s)",
      "LinkBodyTemplate": "Access the following documents:\n{Links}"
    }
  }
}
```

### Third Party Display
- Multiple third parties stored as semicolon-separated in DB
- Display as comma-separated in UI: `"Party A, Party B, Party C"`
- Handle null/empty gracefully

---

## 🚀 Next Steps

1. **Start Phase 1**: Create DTOs and service layer methods
2. **Review**: Have DTOs reviewed before proceeding to UI
3. **Iterate**: Build incrementally, testing each phase before moving to next

---

## 📝 Notes and Questions

- **Column Visibility**: Should all 30 columns be visible by default, or implement show/hide columns feature?
- **Print Reports**: Templates/format to be defined in separate specification
- **Download Links**: How should download links be generated? Temporary signed URLs? Authentication required?

---

**Document Version:** 1.0
**Last Updated:** 2025-10-30
