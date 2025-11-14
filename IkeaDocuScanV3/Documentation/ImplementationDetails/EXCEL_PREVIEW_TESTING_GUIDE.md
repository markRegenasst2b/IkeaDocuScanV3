# Excel Preview Component Testing Guide

## Overview

This guide provides step-by-step instructions for testing the ExcelPreview component integration with the IkeaDocuScan application.

## Prerequisites

- IkeaDocuScan-Web application running (either via Visual Studio, VS Code, or `dotnet run`)
- Web browser (Chrome, Edge, Firefox, or Safari)
- Sample documents in the database to search and export

## Test Scenarios

### 1. Basic Navigation Test

**Objective**: Verify that the "Export to Excel" button appears and navigates correctly

**Steps**:
1. Navigate to the Search Documents page: `/documents/search`
2. Verify that the search form loads correctly
3. Enter search criteria (e.g., search term or select a document type)
4. Click the **Search** button
5. Verify that search results are displayed
6. Locate the **Export to Excel** button (green button with spreadsheet icon)
7. Verify the button is enabled when search results are present
8. Click the **Export to Excel** button

**Expected Results**:
- Button should be visible and styled with green (success) color
- Button should be disabled when no search results are available
- Clicking the button should navigate to `/excel-preview` with query parameters
- Browser URL should contain search parameters (e.g., `?searchTerm=...&pageSize=...`)

---

### 2. Excel Preview Page Load Test

**Objective**: Verify that the ExcelPreview page loads and displays data correctly

**Steps**:
1. Follow steps 1-8 from Test 1 to navigate to the preview page
2. Wait for the page to load

**Expected Results**:
- Page title should be "Excel Preview"
- Loading spinner should appear briefly
- Filter context box should display at the top with:
  - Active filters (search term, document type, etc.)
  - Export date timestamp
- Data summary should show:
  - Total rows count
  - Column count
- Table should render with:
  - IKEA blue (#0051BA) header row
  - All visible columns from metadata
  - Data rows matching search criteria
- Paging controls should be visible at the top of the table

---

### 3. Data Display Verification

**Objective**: Verify that data is correctly formatted and displayed

**Steps**:
1. Navigate to the Excel Preview page with search results
2. Examine the table columns and data

**Expected Results**:
- Column headers should match the metadata display names:
  - Document ID, Bar Code, Document Type, Document Name, etc.
- Data formatting should be correct:
  - **Dates**: yyyy-MM-dd format
  - **Numbers**: Right-aligned
  - **Currency**: Right-aligned with 2 decimal places
  - **Booleans**: "Yes" or "No"
- All 22 columns should be visible (may require horizontal scrolling)
- Data should match the search results from the Search Documents page

---

### 4. Paging Controls Test

**Objective**: Verify that paging controls work correctly

**Steps**:
1. Navigate to the Excel Preview page with more than 25 results
2. Locate the paging controls at the top of the table
3. Test the following actions:
   - Click **Next** button (should move to page 2)
   - Click **Previous** button (should return to page 1)
   - Click **Last** button (should jump to last page)
   - Click **First** button (should return to page 1)
   - Change "Rows per page" dropdown (test 10, 25, 100)
4. Verify page indicator shows correct page numbers (e.g., "Page 1 of 5")

**Expected Results**:
- Navigation buttons should enable/disable appropriately
- Page changes should be instant (no loading spinner)
- Changing rows per page should reset to page 1
- Page indicator should update correctly
- Row count indicator should show correct range (e.g., "Showing 1-25 of 120")

---

### 5. Excel Export Test

**Objective**: Verify that Excel export downloads correctly

**Steps**:
1. Navigate to the Excel Preview page
2. Scroll down to the "Export Actions" section at the bottom
3. Click the **Download Excel File** button
4. Wait for the download to complete
5. Open the downloaded Excel file

**Expected Results**:
- Button should show loading spinner during export
- JavaScript console should log: "Excel file '...' download initiated"
- Alert should appear: "Excel file '...' has been downloaded successfully!"
- File name should be in format: `Documents_Export_YYYYMMDD_HHmmss.xlsx`
- Excel file should contain:
  - **Sheet name**: "Documents Export"
  - **Header row**: IKEA blue (#0051BA) with white text
  - **Filter context**: First few rows showing active filters
  - **Data rows**: All search results (not just the preview page)
  - **Proper formatting**: Dates, numbers, currency formatted correctly
  - **22 columns**: All exportable properties

---

### 6. Filter Context Display Test

**Objective**: Verify that filter context is displayed correctly

**Steps**:
1. Navigate to Search Documents page
2. Enter multiple filter criteria:
   - Search term: "contract"
   - Select a document type
   - Enter date range (optional)
3. Click Search
4. Click **Export to Excel**
5. Examine the filter context box at the top of the preview

**Expected Results**:
- Filter context box should be visible (light blue background)
- All active filters should be displayed as badges
- Export date should be shown in format: "yyyy-MM-dd HH:mm:ss"
- Filter badges should be readable and styled consistently

---

### 7. Cancel Button Test

**Objective**: Verify that the Cancel button returns to search page

**Steps**:
1. Navigate to the Excel Preview page
2. Click the **Cancel** button at the bottom
3. Verify navigation

**Expected Results**:
- Should navigate back to `/search-documents`
- Search results should still be visible (not cleared)
- All filter criteria should be preserved

---

### 8. Validation Test - Large Exports

**Objective**: Verify validation warnings for large exports

**Steps**:
1. Navigate to Search Documents page
2. Perform a search that returns more than 10,000 results
3. Click **Export to Excel**
4. Check for validation message on the preview page

**Expected Results**:
- If results > 10,000 but ≤ 50,000: Warning message should appear
  - "Warning: This export contains X rows which exceeds the recommended limit..."
- If results > 50,000: Error message should appear
  - "This export exceeds the maximum allowed rows..."
  - Export button should be disabled

---

### 9. Responsive Design Test

**Objective**: Verify that the page works on different screen sizes

**Steps**:
1. Navigate to the Excel Preview page
2. Resize browser window to mobile width (< 768px)
3. Test all functionality

**Expected Results**:
- Table should be horizontally scrollable
- Paging controls should stack vertically
- Export button should be full-width on mobile
- All text should be readable
- No horizontal page scrolling (only table scrolling)

---

### 10. Error Handling Test

**Objective**: Verify error handling for edge cases

**Test Cases**:

**10a. No Search Results**:
1. Navigate to `/excel-preview` directly without search results
2. Verify error handling

**Expected**: Error message or redirect to search page

**10b. Invalid Query Parameters**:
1. Navigate to `/excel-preview?pageSize=invalid`
2. Verify error handling

**Expected**: Should use default page size or show error

**10c. API Failure**:
1. Stop the IkeaDocuScan-Web server
2. Try to export from a cached preview page
3. Verify error handling

**Expected**: Error message displayed, export button remains enabled for retry

---

## Browser Console Tests

### JavaScript Tests

Open browser developer tools (F12) and check console logs:

**Expected Console Messages**:
```
Excel download helper loaded
Excel file 'Documents_Export_YYYYMMDD_HHmmss.xlsx' download initiated
```

**Check for Errors**:
- No JavaScript errors should appear
- No 404 errors for missing resources
- No CORS errors

---

## Performance Tests

### Large Dataset Test

**Objective**: Verify performance with large datasets

**Steps**:
1. Search for criteria that returns 1,000+ results
2. Navigate to Excel Preview
3. Measure load time
4. Test paging responsiveness
5. Export to Excel and measure export time

**Expected Results**:
- Preview page should load in < 3 seconds
- Paging should be instant (client-side)
- Excel export should complete in < 10 seconds for 1,000 rows
- UI should remain responsive during export

---

## Integration Tests

### Audit Trail Test

**Objective**: Verify that exports are logged to audit trail

**Steps**:
1. Navigate to Excel Preview and export a file
2. Check the audit trail log (database or UI)
3. Verify audit entry was created

**Expected Results**:
- Audit action: `ExportExcel`
- Barcode: "BULKEXPORT"
- Details: "Exported X documents to Excel"
- Username: Current user
- Timestamp: Current time

---

## Accessibility Tests

### Keyboard Navigation

**Steps**:
1. Navigate to Excel Preview page
2. Use only keyboard to navigate:
   - Tab through controls
   - Use Enter/Space to activate buttons
   - Test paging controls with keyboard

**Expected Results**:
- All interactive elements should be keyboard accessible
- Focus indicators should be visible
- Logical tab order

### Screen Reader Test

**Steps**:
1. Enable screen reader (NVDA, JAWS, or VoiceOver)
2. Navigate through the page
3. Verify announcements

**Expected Results**:
- Page structure should be announced correctly
- Table headers should be associated with data
- Buttons should have descriptive labels

---

## Sign-Off Checklist

Complete this checklist before marking the feature as production-ready:

- [ ] Navigation from Search Documents works correctly
- [ ] Filter context displays active filters
- [ ] Data displays with correct formatting
- [ ] Paging controls work smoothly
- [ ] Excel export downloads successfully
- [ ] Excel file contains correct data and formatting
- [ ] Validation warnings display for large exports
- [ ] Cancel button returns to search page
- [ ] Error handling works for edge cases
- [ ] Responsive design works on mobile devices
- [ ] No browser console errors
- [ ] Performance is acceptable for large datasets
- [ ] Audit trail logs exports correctly
- [ ] Keyboard navigation works
- [ ] Screen reader announces content correctly

---

## Known Issues / Limitations

Document any issues found during testing:

1. **Multiple Document Types**: Currently only the first selected document type is passed to the preview when multiple types are selected in the search. Enhancement needed to pass all selected types.

2. **Counter Party Filter**: The counter party filter is not currently passed from the search page to the preview (would need to be added to `DocumentSearchRequestDto` and navigation query string).

3. **Other Filters**: Date ranges, boolean flags, and other advanced filters are not passed to the preview. Only search term and document type are currently supported.

---

## Troubleshooting

### Issue: "Export to Excel" button is disabled

**Cause**: No search results available
**Solution**: Perform a search that returns results before attempting export

### Issue: Excel file is empty or contains no data

**Cause**: API endpoint returned no data
**Solution**:
- Check server logs for errors
- Verify search criteria in filter context
- Check database has matching documents

### Issue: JavaScript error: "downloadFileFromBytes is not defined"

**Cause**: excelDownload.js not loaded
**Solution**:
- Verify script tag in App.razor: `<script src="js/excelDownload.js"></script>`
- Check browser network tab to ensure script loaded (200 status)
- Clear browser cache and reload

### Issue: Preview page shows loading spinner indefinitely

**Cause**: API request failed or timed out
**Solution**:
- Check browser console for errors
- Check browser network tab for failed requests
- Verify IkeaDocuScan-Web server is running
- Check server logs for exceptions

---

## Additional Notes

- The preview page uses client-side paging, so all data is loaded upfront (up to 1,000 rows by default)
- The Excel export uses the original search criteria and may include more rows than shown in preview
- Large exports (>10,000 rows) show a warning but are still allowed up to 50,000 rows
- The component uses IKEA blue (#0051BA) for branding consistency

---

## Test Environment

Document your test environment:

- **Browser**: _______________
- **Browser Version**: _______________
- **Operating System**: _______________
- **Screen Resolution**: _______________
- **Date Tested**: _______________
- **Tester Name**: _______________

---

## Test Results Summary

Record overall test results:

| Test Scenario | Status | Notes |
|--------------|--------|-------|
| Basic Navigation | ⬜ Pass / ❌ Fail | |
| Page Load | ⬜ Pass / ❌ Fail | |
| Data Display | ⬜ Pass / ❌ Fail | |
| Paging Controls | ⬜ Pass / ❌ Fail | |
| Excel Export | ⬜ Pass / ❌ Fail | |
| Filter Context | ⬜ Pass / ❌ Fail | |
| Cancel Button | ⬜ Pass / ❌ Fail | |
| Validation | ⬜ Pass / ❌ Fail | |
| Responsive Design | ⬜ Pass / ❌ Fail | |
| Error Handling | ⬜ Pass / ❌ Fail | |
| Performance | ⬜ Pass / ❌ Fail | |
| Audit Trail | ⬜ Pass / ❌ Fail | |
| Accessibility | ⬜ Pass / ❌ Fail | |

**Overall Status**: ⬜ APPROVED / ❌ NEEDS WORK

---

## Next Steps

After testing is complete:

1. **If all tests pass**:
   - Mark feature as production-ready
   - Update release notes
   - Consider enhancements (multiple document types, more filters)

2. **If issues found**:
   - Document issues in the "Known Issues" section
   - Create bug tickets for each issue
   - Re-test after fixes are applied

3. **Enhancements to consider**:
   - Support passing all selected document types to preview
   - Add counter party filter support
   - Add date range filter support
   - Add support for other search criteria
   - Add "Export Selected" button on search page for bulk operations
   - Add email export option (send Excel file via email)
   - Add scheduled exports functionality
