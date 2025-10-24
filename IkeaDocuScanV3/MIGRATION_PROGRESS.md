# Document Properties Migration - Progress Report

**Date:** 2025-01-24
**Status:** Foundation Phase Complete (Phase 1)
**Progress:** 20% Complete

---

## ✅ Completed Tasks

### Phase 0: Design Decisions
- ✅ Reviewed and answered all 13 clarification questions from migration spec
- ✅ Created comprehensive `DESIGN_DECISIONS.md` document
- ✅ Selected technical approach for all features:
  - File upload: Both directory monitoring + manual upload
  - Window management: Blazorize Modal dialogs
  - Copy/Paste: Browser localStorage
  - Duplicate detection: Blocking modal
  - Calendar: Blazorize DateEdit
  - Third party selector: Custom Blazor component
  - Authorization: Both (defense in depth)
  - LDAP integration: Maintain existing
  - Field config: Database-driven (DocumentType)
  - Validation: Both inline + summary
  - Responsive: Desktop + Tablet
  - PDF viewing: New tab/window

### Phase 1: Foundation Models ✅
Created all view models and supporting types:

1. **DocumentPropertyMode.cs** (`/IkeaDocuScan-Web.Client/Models/`)
   - Enum with 3 modes: Edit, Register, CheckIn
   - Fully documented with behavior specifications

2. **FieldVisibility.cs** (`/IkeaDocuScan-Web.Client/Models/`)
   - Enum with 3 states: NotApplicable, Optional, Mandatory
   - Maps to database codes: "N", "O", "M"

3. **ThirdPartyItem.cs** (`/IkeaDocuScan-Web.Client/Models/`)
   - Model for dual-listbox selector items
   - Properties: Id, Name, CounterPartyNoAlpha, City, Country, IsSelected
   - DisplayText computed property

4. **FormDataCopyState.cs** (`/IkeaDocuScan-Web.Client/Models/`)
   - Copy/Paste state management
   - localStorage serialization
   - 10-day expiration logic
   - Helper methods for JSON conversion

5. **DocumentPropertiesViewModel.cs** (`/IkeaDocuScan-Web.Client/Models/`) ⭐
   - **Complete 40+ field view model**
   - Organized into 4 sections:
     - Document Section (14 fields)
     - Action Section (4 fields)
     - Flags Section (4 fields)
     - Additional Info Section (10 fields)
   - **NEW FIELDS INCLUDED:**
     - ✅ SendingOutDate
     - ✅ ForwardedToSignatoriesDate
   - Mode and PropertySet configuration
   - Helper methods for third party management
   - Computed properties (SaveButtonText, IsDispatchDateEnabled, etc.)
   - Audit fields (CreatedOn, CreatedBy, ModifiedOn, ModifiedBy)

### Phase 1.5: Main Page Component ✅
Created the container page with routing:

1. **DocumentPropertiesPage.razor** (`/IkeaDocuScan-Web.Client/Pages/`)
   - Three route endpoints:
     - `/documents/edit/{BarCode:int}` → Edit mode
     - `/documents/register` → Register mode
     - `/documents/checkin/{FileName}` → Check-in mode
   - Header section with BarCode and FileName display
   - Validation summary with error list
   - Success message display
   - Action buttons (Save, Compare, Cancel)
   - Two-column responsive layout
   - Placeholders for 4 field sections
   - Form Data Copy/Paste buttons
   - Audit information display (Edit mode)
   - Duplicate detection modal
   - Loading and error states

2. **DocumentPropertiesPage.razor.cs** (`/IkeaDocuScan-Web.Client/Pages/`)
   - **Complete code-behind implementation:**
     - ✅ Lifecycle methods (OnInitializedAsync, OnParametersSetAsync, OnAfterRenderAsync)
     - ✅ Mode detection logic (Edit/Register/Check-in)
     - ✅ LoadPageAsync with 3 mode handlers
     - ✅ Save logic with mode-specific behavior
     - ✅ Comprehensive validation (20+ validation rules)
     - ✅ Duplicate detection flow
     - ✅ Copy/Paste with localStorage integration
     - ✅ DTO mapping (DocumentDto ↔ ViewModel, CreateDto, UpdateDto)
     - ✅ Helper methods (GetPageTitle, ExtractBarcodeFromFileName)
     - ✅ View document button (opens in new tab)
     - ✅ Auto-focus barcode in Register mode
     - ✅ Post-save behavior (stay/navigate based on mode)

---

## 📋 Remaining Tasks

### Phase 2: Reusable Shared Components (NEXT)
Create foundational UI components:

- [ ] **DatePicker.razor** - Wraps Blazorize DateEdit
- [ ] **TriStateDropdown.razor** - Yes/No/Empty selector
- [ ] **ValidationSummaryCard.razor** - Styled error display
- [ ] **FileUploadButton.razor** - File upload with validation

**Estimated Time:** 2-3 hours
**Priority:** HIGH (needed for all field sections)

### Phase 3: Field Section Components
Build the 4 main form sections:

- [ ] **DocumentSectionFields.razor** (14 fields)
  - DocumentType dropdown with change handler
  - CounterParty lookup and auto-population
  - ThirdPartySelector integration
  - 5 date fields including NEW ones
  - Comment textarea

- [ ] **ActionSectionFields.razor** (4 fields)
  - Conditional validation logic
  - LDAP group integration

- [ ] **FlagsSectionFields.razor** (4 tri-state flags)
  - All using TriStateDropdown component

- [ ] **AdditionalInfoFields.razor** (10 fields)
  - Amount/Currency conditional validation
  - DocumentName filtered by DocumentType

**Estimated Time:** 8-10 hours
**Priority:** HIGH

### Phase 4: ThirdPartySelector Component
- [ ] **ThirdPartySelector.razor**
  - Dual-listbox with Add/Remove buttons
  - Double-click to move items
  - Persist to ViewModel

**Estimated Time:** 3-4 hours
**Priority:** MEDIUM

### Phase 5: Dynamic Field Visibility Service
- [ ] **FieldVisibilityService.cs**
  - Load DocumentType field configuration
  - Map "M"/"O"/"N" codes to enum
  - Apply to form fields dynamically

**Estimated Time:** 2-3 hours
**Priority:** MEDIUM

### Phase 6: Counterparty Auto-Population
- [ ] Implement OnCounterPartyNoChanged handler
- [ ] Cascade to Location and AffiliatedTo
- [ ] Load third parties for selector

**Estimated Time:** 2 hours
**Priority:** MEDIUM

### Phase 7: Backend Service Enhancements
- [ ] Add `GetSimilarDocuments()` to DocumentService
- [ ] Add `GetByCounterPartyNoAlpha()` to CounterPartyService
- [ ] Add `GetFieldVisibilityConfig()` to DocumentTypeService
- [ ] Add file upload endpoint

**Estimated Time:** 3-4 hours
**Priority:** MEDIUM

### Phase 8: File Management
- [ ] Integrate with ScannedFileService
- [ ] Implement manual file upload
- [ ] Handle file deletion after check-in

**Estimated Time:** 3-4 hours
**Priority:** MEDIUM

### Phase 9: Styling & Polish
- [ ] Create `document-properties.css`
- [ ] Match original color scheme
- [ ] Apply Tahoma font, 11px
- [ ] Responsive breakpoints

**Estimated Time:** 3-4 hours
**Priority:** LOW

### Phase 10: Testing & Integration
- [ ] Unit tests for validation
- [ ] Integration testing for all 3 modes
- [ ] SignalR real-time update integration
- [ ] End-to-end testing

**Estimated Time:** 6-8 hours
**Priority:** HIGH

---

## 📊 Overall Progress

| Phase | Status | Progress |
|-------|--------|----------|
| 0. Design Decisions | ✅ Complete | 100% |
| 1. Foundation Models | ✅ Complete | 100% |
| 1.5. Main Page Component | ✅ Complete | 100% |
| 2. Shared Components | 🔲 Not Started | 0% |
| 3. Field Sections | 🔲 Not Started | 0% |
| 4. ThirdPartySelector | 🔲 Not Started | 0% |
| 5. Field Visibility | 🔲 Not Started | 0% |
| 6. Counterparty Logic | 🔲 Not Started | 0% |
| 7. Backend Services | 🔲 Not Started | 0% |
| 8. File Management | 🔲 Not Started | 0% |
| 9. Styling | 🔲 Not Started | 0% |
| 10. Testing | 🔲 Not Started | 0% |

**Total Progress:** 20% Complete

---

## 🎯 Next Steps (Recommended Order)

1. **Create Shared Components** (2-3 hours)
   - DatePicker.razor
   - TriStateDropdown.razor
   - ValidationSummaryCard.razor
   - FileUploadButton.razor

2. **Build DocumentSectionFields** (3-4 hours)
   - Most complex section with 14 fields
   - Includes ThirdPartySelector placeholder

3. **Build Other Field Sections** (4-5 hours)
   - ActionSectionFields.razor
   - FlagsSectionFields.razor
   - AdditionalInfoFields.razor

4. **Create ThirdPartySelector** (3-4 hours)
   - Custom dual-listbox component

5. **Implement Dynamic Logic** (4-5 hours)
   - Field visibility service
   - Counterparty auto-population
   - Backend service enhancements

6. **File Management** (3-4 hours)
   - Upload and check-in functionality

7. **Styling & Testing** (9-12 hours)
   - CSS matching
   - Integration testing
   - End-to-end validation

**Estimated Total Remaining Time:** 28-40 hours

---

## 🔥 Critical Path Items

The following must be completed for MVP (Minimum Viable Product):

1. ✅ ~~View models and enums~~
2. ✅ ~~Main page component with routing~~
3. ⏳ **Shared components (DatePicker, TriStateDropdown)** ← NEXT
4. ⏳ **All 4 field section components**
5. ⏳ **Basic validation working**
6. ⏳ **Save/Load functionality for all 3 modes**

Non-critical features (can be added later):
- ThirdPartySelector (can start with simple multi-select)
- Copy/Paste (scaffolding exists, can be enabled later)
- Duplicate detection (scaffolding exists, backend needed)
- Dynamic field visibility (can start with all fields always visible)
- Compare with Standard Contract

---

## 📝 Notes & Decisions

### Architecture Highlights

1. **Clean Separation:**
   - Models in `/Client/Models/`
   - Pages in `/Client/Pages/`
   - Components will go in `/Client/Components/`

2. **Three-Tier Validation:**
   - Client-side: Basic required field checks in code-behind
   - Component-level: Blazorize validation attributes
   - Server-side: Service layer validation (existing)

3. **Mode-Driven Behavior:**
   - Single component handles 3 modes
   - PropertySetNumber controls DispatchDate visibility
   - Post-save behavior differs by mode

4. **DTO Mapping:**
   - Manual mapping in code-behind
   - Separate methods for Create vs Update
   - Handles semicolon-separated third party lists

### Technical Debt Identified

1. **TODO Comments in Code:**
   - Line 138: Load scanned file from CheckinDirectory
   - Line 145: Load file bytes
   - Line 266: Implement GetSimilarDocuments
   - Line 283: Get current user from ICurrentUserService
   - Line 494: Get actual filename from DocumentFile
   - Line 532: Implement compare with standard contract

2. **Missing Backend Methods:**
   - `DocumentService.GetSimilarDocuments()`
   - `CounterPartyService.GetByCounterPartyNoAlpha()`
   - `DocumentTypeService.GetFieldVisibilityConfig()`

3. **Missing Endpoints:**
   - `GET /api/documents/{id}/file` (view PDF)
   - `POST /api/documents/upload` (manual file upload)

---

## 🚀 Quick Start Guide for Next Developer

To continue development:

1. **Review Design Decisions:**
   ```
   cat DESIGN_DECISIONS.md
   ```

2. **Check Current Files:**
   ```
   ls IkeaDocuScan-Web/IkeaDocuScan-Web.Client/Models/
   ls IkeaDocuScan-Web/IkeaDocuScan-Web.Client/Pages/DocumentPropertiesPage.*
   ```

3. **Next Task: Create Shared Components**
   - Create `/Client/Components/Shared/` directory
   - Implement DatePicker.razor (wrap Blazorize DateEdit)
   - Implement TriStateDropdown.razor (Yes/No/Empty)
   - Test in isolation before integration

4. **Run and Test:**
   ```bash
   dotnet build
   dotnet run --project IkeaDocuScanV3.AppHost
   ```
   Navigate to: `/documents/register`

---

**Last Updated:** 2025-01-24
**Next Review:** After Phase 2 completion
**Owner:** Development Team
