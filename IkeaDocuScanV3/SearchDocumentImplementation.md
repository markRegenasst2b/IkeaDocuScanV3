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
**Status:** 🔴 Not Started
**Estimated Effort:** Large

#### Tasks:
- [ ] Create results table component structure
- [ ] Implement empty states
  - [ ] Initial state: "Start your search by entering the criteria above."
  - [ ] No results: "No documents found matching your criteria."
- [ ] Add selection column with checkboxes
- [ ] Implement display columns (30+ columns)
  - [ ] Bar Code (link to edit page)
  - [ ] Document Type
  - [ ] Document Name
  - [ ] Counterparty
  - [ ] Counterparty No.
  - [ ] Country
  - [ ] Third Party (comma-separated)
  - [ ] Date of Contract
  - [ ] Comment
  - [ ] Fax (Yes/No)
  - [ ] Original Received (Yes/No)
  - [ ] Document No.
  - [ ] Associated to PUA/Agreement No.
  - [ ] Version No.
  - [ ] Currency
  - [ ] Amount
  - [ ] (Add remaining columns from spec)
- [ ] Implement sortable column headers
  - [ ] Click to sort ascending
  - [ ] Click again to sort descending
  - [ ] Visual indicator (arrow icons)
  - [ ] Single-column sort only
- [ ] Implement pagination controls
  - [ ] Previous/Next buttons
  - [ ] Page number display
  - [ ] Page size dropdown (10, 25, 100)
  - [ ] Total results count display
- [ ] Implement selection controls
  - [ ] Select All (current page)
  - [ ] Deselect All (current page)
  - [ ] Invert Selection (current page)
  - [ ] Selected count display
- [ ] Apply responsive table styling
- [ ] Add loading indicator

**Dependencies:** Phase 1 (DTOs), Phase 3 (Filter Panel)
**Files to Modify:**
- `IkeaDocuScan-Web.Client/Pages/SearchDocuments.razor`
- `IkeaDocuScan-Web.Client/Pages/SearchDocuments.razor.cs`

---

### Phase 5: Per-Row Actions
**Status:** 🔴 Not Started
**Estimated Effort:** Medium

#### Tasks:
- [ ] Add action menu column to results table
- [ ] Implement action menu dropdown (per row)
- [ ] Implement "Open" action
  - [ ] Download PDF from `documentfile.bytes`
  - [ ] Open in new browser tab
- [ ] Implement "View Properties" action
  - [ ] Create read-only properties modal component
  - [ ] Display all document metadata
  - [ ] Format dates, currencies, yes/no values
- [ ] Implement "Edit Properties" action
  - [ ] Navigate to `/documents/edit/{barcode}`
- [ ] Implement "Send as Email (Attach)" action
  - [ ] Generate `mailto:` link
  - [ ] Use config for recipient, subject, body
  - [ ] Include document as attachment reference
- [ ] Implement "Send as Email (Link)" action
  - [ ] Generate `mailto:` link
  - [ ] Use config for recipient, subject, body
  - [ ] Include download link in body
- [ ] Implement "Delete" action
  - [ ] Show confirmation dialog with barcode + filename
  - [ ] Call delete API
  - [ ] Refresh results after deletion

**Dependencies:** Phase 4 (Results List)
**Files to Create:**
- `IkeaDocuScan-Web.Client/Components/DocumentManagement/ViewPropertiesModal.razor`
- `IkeaDocuScan-Web.Client/Components/DocumentManagement/ViewPropertiesModal.razor.cs`

**Files to Modify:**
- `IkeaDocuScan-Web.Client/Pages/SearchDocuments.razor`
- `IkeaDocuScan-Web.Client/Pages/SearchDocuments.razor.cs`

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
**Completed:** 3
**In Progress:** 0
**Not Started:** 5

**Overall Status:** 🟡 In Progress (37.5%)

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
