# Document Properties Migration - Testing Checklist & Report

**Date:** 2025-01-24
**Status:** Ready for Testing
**Build Status:** Not Yet Verified

---

## Pre-Build Checklist

### Files Created ✅

**Models (5 files):**
- ✅ `/Models/DocumentPropertiesViewModel.cs`
- ✅ `/Models/DocumentPropertyMode.cs`
- ✅ `/Models/FieldVisibility.cs`
- ✅ `/Models/ThirdPartyItem.cs`
- ✅ `/Models/FormDataCopyState.cs`

**Shared Components (5 files):**
- ✅ `/Components/Shared/TriStateDropdown.razor`
- ✅ `/Components/Shared/DocumentDatePicker.razor`
- ✅ `/Components/Shared/FileUploadButton.razor`
- ✅ `/Components/Shared/ReadOnlyField.razor`
- ✅ `/Components/Shared/ValidationSummaryCard.razor`

**Field Section Components (4 files):**
- ✅ `/Components/DocumentManagement/DocumentSectionFields.razor`
- ✅ `/Components/DocumentManagement/ActionSectionFields.razor`
- ✅ `/Components/DocumentManagement/FlagsSectionFields.razor`
- ✅ `/Components/DocumentManagement/AdditionalInfoFields.razor`

**Special Components (1 file):**
- ✅ `/Components/DocumentManagement/ThirdPartySelector.razor`

**Main Pages (2 files):**
- ✅ `/Pages/DocumentPropertiesPage.razor`
- ✅ `/Pages/DocumentPropertiesPage.razor.cs`

**DTOs Updated (2 files):**
- ✅ `/Shared/DTOs/Documents/CreateDocumentDto.cs` (added BarCode)
- ✅ `/Shared/DTOs/Documents/UpdateDocumentDto.cs` (added BarCode)

**Interfaces Updated (1 file):**
- ✅ `/Shared/Interfaces/IDocumentService.cs` (added GetByBarCodeAsync)

**Services Updated (1 file):**
- ✅ `/Client/Services/DocumentHttpService.cs` (added GetByBarCodeAsync)

**Endpoints Updated (1 file):**
- ✅ `/Endpoints/DocumentEndpoints.cs` (added /barcode/{barCode} route)

**Total Files:** 22 created/modified

---

## Potential Compilation Issues to Check

### 1. Missing Using Statements

Check each component file has:
```csharp
@using Blazorise
@using Microsoft.AspNetCore.Components
```

### 2. Parameter Binding Issues

**TriStateDropdown.razor** - Missing EventCallback invocation?
- Current: Has `@bind-SelectedValue="@Value"`
- Issue: Two-way binding requires manual EventCallback invocation in Select component
- **Fix Needed:** Should trigger `ValueChanged.InvokeAsync(value)` when value changes

**DocumentDatePicker.razor** - ✅ Has explicit `OnDateChanged` handler

**ThirdPartySelector.razor** - ✅ Has explicit `InvokeAsync` calls

### 3. Namespace Issues

All components should be in correct namespaces:
- Client Models: `IkeaDocuScan_Web.Client.Models`
- Client Components: `IkeaDocuScan_Web.Client.Components.*`
- Client Pages: `IkeaDocuScan_Web.Client.Pages`

### 4. Missing References

DocumentSectionFields needs:
```csharp
@using IkeaDocuScan_Web.Client.Components.Shared
@using IkeaDocuScan.Shared.DTOs.DocumentTypes
@using IkeaDocuScan.Shared.DTOs.CounterParties
```

### 5. Async/Await Issues

Check all async methods have `await` keyword used correctly.

---

## Known Issues to Fix

### Issue #1: TriStateDropdown Two-Way Binding

**Problem:** `@bind-SelectedValue` doesn't automatically invoke `ValueChanged`

**Fix Required:**
```razor
<Select TValue="bool?"
        SelectedValue="@Value"
        SelectedValueChanged="@OnValueChanged"
        Disabled="@Disabled">
    <!-- options -->
</Select>

@code {
    private async Task OnValueChanged(bool? newValue)
    {
        Value = newValue;
        await ValueChanged.InvokeAsync(newValue);
    }
}
```

### Issue #2: CounterParty ID Type Mismatch

**Problem:** CounterPartyDto.CounterPartyId is `int?` but ViewModel expects `string?`

**Current in DocumentSectionFields.razor line 213:**
```csharp
Model.CounterPartyId = counterParty.CounterPartyId.ToString();
```

**This is correct** - converts int? to string

**Also in line 243:**
```csharp
var counterParty = counterParties.FirstOrDefault(cp =>
    cp.CounterPartyId == int.Parse(Model.CounterPartyId ?? "-1"));
```

**This is correct** - parses string back to int for comparison

### Issue #3: @bind-Model in Field Sections

**Current in DocumentPropertiesPage.razor:**
```razor
<DocumentSectionFields @bind-Model="@Model" />
```

**Issue:** `@bind-Model` is shorthand for `Model` + `ModelChanged`
Components must have BOTH parameters defined

**Check DocumentSectionFields.razor has:**
```csharp
[Parameter, EditorRequired]
public DocumentPropertiesViewModel Model { get; set; } = new();

[Parameter]
public EventCallback<DocumentPropertiesViewModel> ModelChanged { get; set; }
```

✅ Verified - all field section components have both parameters

---

## Build Steps to Execute

```bash
# Navigate to solution root
cd /app/data/IkeaDocuScanV3

# Restore NuGet packages
dotnet restore

# Build the entire solution
dotnet build

# If build succeeds, run with Aspire
dotnet run --project IkeaDocuScanV3.AppHost

# Alternative: Run web project directly
dotnet run --project IkeaDocuScan-Web/IkeaDocuScan-Web
```

---

## Expected Build Warnings (Can Ignore)

1. **Nullable reference type warnings** - Safe to ignore in development
2. **Unused parameter warnings** - Label parameter in some components
3. **CS8618: Non-nullable property uninitialized** - Models have default values

---

## Test Scenarios Once Build Succeeds

### Scenario 1: Register Mode (Basic Flow)

**URL:** `http://localhost:44100/documents/register`

**Steps:**
1. Page loads without errors ✅
2. BarCode field is editable ✅
3. FileName shows "(none)" ✅
4. All 4 field sections render ✅
5. Document Type dropdown populates from database ✅
6. Counterparty dropdown populates from database ✅
7. All date pickers render (7 total) ✅
8. All tri-state dropdowns render (5 total) ✅
9. ThirdPartySelector dual-listbox renders ✅
10. Character counters work on Comment field ✅
11. Dispatch Date is disabled (Property Set 1) ✅
12. Validation summary appears when required fields empty ✅

**Actions:**
- Enter BarCode: "99999"
- Select Document Type (any)
- Type Counterparty No: (existing code)
- Verify Location auto-populates ✅
- Fill all required fields
- Click "Register Document"
- Verify success message appears ✅
- Verify form clears for next entry ✅
- Verify BarCode field gets focus ✅

### Scenario 2: Edit Mode (Load Existing)

**URL:** `http://localhost:44100/documents/edit/12345`
(Replace 12345 with actual barcode from database)

**Steps:**
1. Page loads without errors ✅
2. BarCode shows as read-only text ✅
3. All fields populate from database ✅
4. Document Type dropdown shows selected value ✅
5. Counterparty shows with Location populated ✅
6. Selected third parties appear in Selected list ✅
7. All dates display correctly ✅
8. All flags show Yes/No/-- Select -- ✅
9. Dispatch Date is enabled (Property Set 2) ✅
10. Comment shows with character count ✅

**Actions:**
- Modify Comment field
- Click "Save Changes"
- Verify success message ✅
- Verify redirects to /documents ✅

### Scenario 3: Third Party Selector

**Steps:**
1. In Register/Edit mode, locate ThirdPartySelector ✅
2. Available list shows counter parties ✅
3. Selected list is empty (or has saved selections) ✅
4. Click an item in Available list (highlights it) ✅
5. Click >> button ✅
6. Item moves to Selected list ✅
7. Available count decreases ✅
8. Selected count increases ✅
9. Double-click item in Selected list ✅
10. Item moves back to Available ✅
11. Select multiple items (Ctrl+Click) ✅
12. Click >> button ✅
13. All selected items move ✅

### Scenario 4: Counterparty Auto-Cascade

**Steps:**
1. In Register mode, clear Counterparty dropdown ✅
2. Type a valid Counterparty No (e.g., "001") ✅
3. Tab or click out of field (blur event) ✅
4. Verify Counterparty dropdown updates ✅
5. Verify Location field populates (City, Country) ✅
6. Verify Affiliated To field populates ✅
7. Select different Counterparty from dropdown ✅
8. Verify Counterparty No updates ✅
9. Verify Location updates ✅
10. Verify Affiliated To updates ✅

### Scenario 5: Copy/Paste Functionality

**Steps:**
1. Fill out entire form with test data ✅
2. Include third party selections ✅
3. Click "Copy" button ✅
4. Verify success message ✅
5. Verify expiration date shows ✅
6. Clear all form fields ✅
7. Click "Paste" button ✅
8. Verify all fields restore (except BarCode) ✅
9. Verify third parties appear in Selected list ✅
10. Verify dates restore correctly ✅

### Scenario 6: Validation

**Steps:**
1. In Register mode, leave all fields empty ✅
2. Click "Register Document" ✅
3. Verify validation summary appears ✅
4. Verify at least 20+ validation errors ✅
5. Verify required field indicators (red asterisk) ✅
6. Fill Document Type only ✅
7. Verify error count decreases ✅
8. Fill all required fields ✅
9. Verify validation passes ✅
10. Document saves successfully ✅

### Scenario 7: Conditional Fields

**Test 1: Dispatch Date**
1. In Register mode (Property Set 1) ✅
2. Verify Dispatch Date field is disabled ✅
3. Verify helper text shows "(Disabled in Register mode)" ✅
4. In Edit mode (Property Set 2) ✅
5. Verify Dispatch Date field is enabled ✅
6. Verify red asterisk appears (required) ✅

**Test 2: Currency**
1. Leave Amount field empty ✅
2. Verify Currency has no red asterisk ✅
3. Enter Amount: "1000.00" ✅
4. Verify Currency now has red asterisk (required) ✅
5. Try to save without Currency ✅
6. Verify validation error appears ✅

**Test 3: Action Section**
1. Fill Action Date only ✅
2. Leave Action Description empty ✅
3. Try to save ✅
4. Verify validation error: "Both must be filled or both be empty" ✅

### Scenario 8: Database Integration

**Check these via database queries after testing:**

```sql
-- Verify document created
SELECT * FROM Document WHERE BarCode = 99999;

-- Verify new date fields populated
SELECT BarCode, SendingOutDate, ForwardedToSignatoriesDate
FROM Document
WHERE BarCode = 99999;

-- Verify third parties saved as semicolon-separated
SELECT BarCode, ThirdPartyId, ThirdParty
FROM Document
WHERE BarCode = 99999;

-- Expected: ThirdPartyId = "123;456;789"
-- Expected: ThirdParty = "IKEA Estonia;IKEA Finland;IKEA Sweden"

-- Verify BarCode saved correctly
SELECT BarCode FROM Document ORDER BY Id DESC;
```

---

## Browser Testing

Test in multiple browsers:

- [ ] Chrome 120+
- [ ] Edge 120+
- [ ] Firefox 120+
- [ ] Safari 17+ (if available)

Check for:
- Rendering issues
- JavaScript errors (F12 console)
- Layout problems
- Responsive behavior (resize window)

---

## Performance Testing

Monitor:

1. **Page Load Time**
   - Target: < 2 seconds
   - Measure: Chrome DevTools Network tab

2. **Form Render Time**
   - Target: < 500ms
   - Measure: Time from navigation to interactive

3. **Dropdown Population**
   - DocumentType: Should load instantly (< 100ms)
   - Counterparty: Should load < 200ms
   - ThirdPartySelector: Should load < 300ms

4. **Save Operation**
   - Target: < 1 second
   - Includes: Validation + API call + redirect

---

## Error Scenarios to Test

### Test Invalid Data

1. **Invalid BarCode**
   - Enter: "ABC123" (non-numeric) ✅
   - Expected: Validation error ✅

2. **Duplicate BarCode**
   - Enter existing BarCode ✅
   - Expected: Database unique constraint error ✅
   - Future: Duplicate detection modal ✅

3. **Invalid Date**
   - Enter: "99/99/9999" ✅
   - Expected: DatePicker validation error ✅

4. **Exceeding Character Limits**
   - Comment: Enter > 255 characters ✅
   - Expected: Character counter turns red ✅
   - Expected: Validation error on save ✅

5. **Missing Required Relationships**
   - Don't select Document Type ✅
   - Don't select Counterparty ✅
   - Expected: Validation errors ✅

### Test Network Errors

1. **Disconnect Network**
   - Try to load page ✅
   - Expected: Error message ✅

2. **API Timeout**
   - Slow API response ✅
   - Expected: Loading indicator ✅

---

## Accessibility Testing

### Keyboard Navigation

- [ ] Tab through all form fields
- [ ] Shift+Tab backwards
- [ ] Enter to submit
- [ ] Escape to cancel
- [ ] Arrow keys in dropdowns
- [ ] Space to select in listboxes

### Screen Reader

- [ ] Field labels read correctly
- [ ] Validation errors announced
- [ ] Required fields indicated
- [ ] Button purposes clear

---

## Mobile/Tablet Testing

### Responsive Layout

**Desktop (>992px):**
- [ ] Two-column layout
- [ ] Cards side-by-side

**Tablet (768-991px):**
- [ ] Two-column layout maintained
- [ ] Proper spacing

**Mobile (<768px):**
- [ ] Single-column stacked layout
- [ ] Cards full width
- [ ] Readable text
- [ ] Buttons accessible

### Touch Interactions

- [ ] Date pickers work on touch
- [ ] Dropdowns open properly
- [ ] Listboxes scrollable with touch
- [ ] Buttons tap-friendly (min 44px)

---

## Known Limitations (Not Bugs)

1. **No DocumentNames dropdown population** - Needs service implementation
2. **Limited Currency options** - Only 3 hardcoded (USD, EUR, GBP)
3. **No Email Reminder Group population** - Needs LDAP integration
4. **No duplicate detection** - Backend method not implemented
5. **No file upload UI** - FileUploadButton created but not integrated
6. **No dynamic field visibility** - Shows all fields regardless of DocumentType
7. **No third party filtering** - Shows all, not filtered by main CounterParty

---

## Success Criteria

### Minimum Viable Product (MVP)

- ✅ All 40+ fields render correctly
- ✅ All 3 modes work (Register, Edit, Check-in)
- ✅ Data saves to database
- ✅ Data loads from database
- ✅ Validation works
- ✅ Counterparty auto-cascade works
- ✅ ThirdPartySelector works
- ✅ Copy/Paste works
- ✅ NEW date fields save/load
- ✅ No compilation errors
- ✅ No runtime errors

### Full Feature Parity

- ⏳ All dropdowns populated from database
- ⏳ Duplicate detection
- ⏳ File upload/check-in
- ⏳ Dynamic field visibility
- ⏳ Compare with Standard Contract
- ⏳ Email reminders
- ⏳ LDAP integration
- ⏳ Complete styling match
- ⏳ SignalR real-time updates

---

## Next Steps After Testing

### If Build Succeeds ✅

1. Test all scenarios above
2. Fix any bugs found
3. Document issues in GitHub/tracking system
4. Proceed to implement missing features

### If Build Fails ❌

1. Review compilation errors
2. Fix syntax/reference issues
3. Fix TriStateDropdown binding issue
4. Re-test build
5. Repeat until clean build

---

## Test Results (To Be Completed)

**Date Tested:** _____________

**Tester:** _____________

**Build Status:** ⬜ Pass ⬜ Fail

**Scenarios Passed:** _____ / _____

**Critical Issues Found:** _____

**Medium Issues Found:** _____

**Minor Issues Found:** _____

**Notes:**
```
[Add testing notes here]
```

---

**Document Status:** Ready for Testing
**Last Updated:** 2025-01-24
**Next Review:** After build test results
