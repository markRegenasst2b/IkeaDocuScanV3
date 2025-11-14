# IkeaDocuScan V3 - Business Test Plan: READER ROLE

**Version:** 1.0
**Date:** 2025-11-14
**Test Role:** Reader (View-Only Access)

---

## Table of Contents

1. [Reader Role Overview](#reader-role-overview)
2. [Pre-Test Setup](#pre-test-setup)
3. [Test Scenarios](#test-scenarios)
   - [Authentication & Navigation](#authentication--navigation)
   - [Document Search & Filtering](#document-search--filtering)
   - [Document Viewing](#document-viewing)
   - [Excel Export](#excel-export)
   - [Permission Boundaries](#permission-boundaries)
   - [Real-Time Updates](#real-time-updates)
   - [Edge Cases](#edge-cases)
4. [Test Results Summary](#test-results-summary)

---

## Reader Role Overview

### Capabilities

The **Reader** role has **view-only** access to the system with the following capabilities:

✅ **Can Do:**
- Login to the application
- Search and view documents (filtered by UserPermissions)
- View document details and attached PDFs
- Export search results to Excel
- View reports (Barcode Gaps, Duplicates, Scan Copies, Suppliers)
- View action reminders (filtered by permissions)
- View reference data (countries, currencies, document types, counter parties)

❌ **Cannot Do:**
- Create or edit documents
- Delete documents
- Check-in scanned files
- Send emails with documents
- Manage user permissions
- Create/edit/delete reference data
- Access system configuration
- View audit trail (SuperUser only)

### Permission Filtering

**Critical:** Reader sees ONLY documents matching their UserPermission records:
- Filter by: `Document Type`, `Country`, `Counter Party`
- Documents outside their permissions are **invisible** (not just read-only)

---

## Pre-Test Setup

### Test User Account

**Username:** `DOMAIN\test_reader`
**Role:** Reader
**AD Group:** `ADGroup.Builtin.Reader` (if using AD groups)

### Database Setup

```sql
-- Verify test user exists
SELECT * FROM DocuScanUser WHERE AccountName = 'DOMAIN\test_reader'

-- Verify user has HasAccess = true (not just IsSuperUser)
-- Should have specific permissions assigned

SELECT
    u.AccountName,
    u.IsSuperUser,
    dt.DtName as DocumentType,
    c.CountryName,
    cp.Name as CounterParty
FROM DocuScanUser u
LEFT JOIN UserPermissions up ON u.UserId = up.UserId
LEFT JOIN DocumentTypes dt ON up.DocumentTypeId = dt.DocumentTypeId
LEFT JOIN Countries c ON up.CountryCode = c.CountryCode
LEFT JOIN CounterParties cp ON up.CounterPartyId = cp.CounterPartyId
WHERE u.AccountName = 'DOMAIN\test_reader'
```

### Test Data Requirements

Create the following UserPermissions for test_reader:

| Document Type | Country | Counter Party | Purpose |
|---------------|---------|---------------|---------|
| Invoice | Sweden | ACME Corp | Can see invoices for ACME in Sweden |
| Contract | Denmark | TechCo | Can see contracts for TechCo in Denmark |
| (None) | Norway | (All) | Can see all document types for Norway |

Also create documents that Reader **CANNOT** see:
- Invoice for Germany (Reader has no Germany permission)
- Purchase Order for any country (Reader has no PO permission)

---

## Test Scenarios

###Authentication & Navigation

#### TC-R-001: Successful Login

**Objective:** Verify Reader can log in with Windows Authentication

**Pre-conditions:**
- Test user `test_reader` exists in DocuScanUser table
- IsSuperUser = false
- Has at least one UserPermission record

**Test Steps:**
1. Navigate to application URL
2. Windows Authentication should auto-authenticate
3. Application loads

**Expected Result:**
- User automatically logged in
- Home page displays
- Navigation menu shows only Reader-accessible items:
  - Search Documents
  - Barcode Gaps Report
  - Duplicate Documents Report
  - Unlinked Registrations Report
  - Scan Copies Report
  - Suppliers Report

**Actual Result:**
[To be filled]

**Status:** ⬜ Not Run | ✅ Pass | ❌ Fail

**Notes:**

---

#### TC-R-002: Menu Access Restrictions

**Objective:** Verify Reader cannot see restricted menu items

**Pre-conditions:**
- Logged in as Reader

**Test Steps:**
1. Examine navigation menu

**Expected Result:**
Reader should **NOT** see:
- ❌ Register Document
- ❌ Check-in Scanned
- ❌ User Permissions
- ❌ Currency (Admin)
- ❌ Country (Admin)
- ❌ Document Type (Admin)
- ❌ Counter Party (Admin)
- ❌ Document Names (Admin)
- ❌ Configuration
- ❌ Audit Trail

**Actual Result:**
[To be filled]

**Status:** ⬜ Not Run | ✅ Pass | ❌ Fail

**Notes:**

---

#### TC-R-003: Direct URL Access to Restricted Pages

**Objective:** Verify Reader cannot access restricted pages via direct URL

**Pre-conditions:**
- Logged in as Reader

**Test Steps:**
1. Attempt to navigate to `/documents/register`
2. Attempt to navigate to `/documents/checkin/testfile.pdf`
3. Attempt to navigate to `/admin/users`
4. Attempt to navigate to `/admin/configuration`
5. Attempt to navigate to `/admin/audit`

**Expected Result:**
- All attempts should result in:
  - Access Denied error message
  - OR redirect to home/unauthorized page
  - OR 403 Forbidden response

**Actual Result:**
[To be filled]

**Status:** ⬜ Not Run | ✅ Pass | ❌ Fail

**Notes:**

---

### Document Search & Filtering

#### TC-R-010: Basic Document Search

**Objective:** Verify Reader can search for documents within their permissions

**Pre-conditions:**
- Logged in as Reader
- Test documents exist that match Reader's permissions

**Test Steps:**
1. Navigate to "Search Documents"
2. Leave all filters empty
3. Click "Search"

**Expected Result:**
- Results display only documents matching Reader's UserPermissions
- Should see invoices for Sweden/ACME and contracts for Denmark/TechCo
- Should NOT see documents outside permissions (Germany invoices, POs, etc.)

**Actual Result:**
[To be filled]

**Status:** ⬜ Not Run | ✅ Pass | ❌ Fail

**Notes:**

---

#### TC-R-011: Search by Barcode

**Objective:** Verify search by specific barcode

**Pre-conditions:**
- Logged in as Reader
- Know barcode of document Reader has permission to see: `12345678`
- Know barcode of document Reader does NOT have permission: `99999999`

**Test Steps:**
1. Navigate to "Search Documents"
2. Enter barcode `12345678` in barcode field
3. Click "Search"
4. Note results
5. Clear search
6. Enter barcode `99999999`
7. Click "Search"

**Expected Result:**
- First search: Document `12345678` displays (if within permissions)
- Second search: No results (document outside permissions is hidden)

**Actual Result:**
[To be filled]

**Status:** ⬜ Not Run | ✅ Pass | ❌ Fail

**Notes:**

---

#### TC-R-012: Filter by Document Type

**Objective:** Verify filtering by document type respects permissions

**Pre-conditions:**
- Logged in as Reader
- Reader has permission for "Invoice" but NOT "Purchase Order"

**Test Steps:**
1. Navigate to "Search Documents"
2. Select "Invoice" from Document Type dropdown
3. Click "Search"
4. Note results
5. Clear search
6. Select "Purchase Order" from Document Type dropdown
7. Click "Search"

**Expected Result:**
- Invoice search: Shows invoices within Reader's country/counter party permissions
- Purchase Order search: No results (Reader doesn't have PO permissions)

**Actual Result:**
[To be filled]

**Status:** ⬜ Not Run | ✅ Pass | ❌ Fail

**Notes:**

---

#### TC-R-013: Advanced Search - Date Range

**Objective:** Verify date range filtering works correctly

**Pre-conditions:**
- Logged in as Reader

**Test Steps:**
1. Navigate to "Search Documents"
2. Set Receiving Date "From": 2025-01-01
3. Set Receiving Date "To": 2025-01-31
4. Click "Search"

**Expected Result:**
- Results show only documents received in January 2025
- Still filtered by Reader's permissions
- Date filtering works correctly

**Actual Result:**
[To be filled]

**Status:** ⬜ Not Run | ✅ Pass | ❌ Fail

**Notes:**

---

#### TC-R-014: Search with Special Characters

**Objective:** Verify search handles special characters without errors

**Pre-conditions:**
- Logged in as Reader

**Test Steps:**
1. Navigate to "Search Documents"
2. Enter in Document No field: `INV-2025/001`
3. Click "Search"
4. Try another search with: `O'Brien Contract`
5. Try another search with: `Test & Co.`

**Expected Result:**
- No SQL errors
- Search handles special characters correctly
- Results match if documents exist

**Actual Result:**
[To be filled]

**Status:** ⬜ Not Run | ✅ Pass | ❌ Fail

**Notes:**

---

### Document Viewing

#### TC-R-020: View Document Details

**Objective:** Verify Reader can view document details

**Pre-conditions:**
- Logged in as Reader
- Know barcode of document within Reader's permissions

**Test Steps:**
1. Search for document
2. Click on document row or View button
3. Document details modal/page opens

**Expected Result:**
- All document fields display correctly:
  - Barcode, Document Type, Document Number
  - Counter Party, Third Party
  - Dates, Amounts, Currency
  - Comments, Action Description
  - Flags (Fax, Original, Confidential, etc.)
- No edit fields (all read-only)
- No Save/Delete buttons
- Download PDF button present (if file attached)

**Actual Result:**
[To be filled]

**Status:** ⬜ Not Run | ✅ Pass | ❌ Fail

**Notes:**

---

#### TC-R-021: View/Download Attached PDF

**Objective:** Verify Reader can view and download attached PDF

**Pre-conditions:**
- Logged in as Reader
- Document with attached PDF exists within permissions

**Test Steps:**
1. Search for document with PDF
2. Open document details
3. Click "View PDF" or "Download" button

**Expected Result:**
- PDF opens in browser OR downloads
- File is correct PDF for that document
- No errors

**Actual Result:**
[To be filled]

**Status:** ⬜ Not Run | ✅ Pass | ❌ Fail

**Notes:**

---

#### TC-R-022: Attempt to View Document Outside Permissions

**Objective:** Verify Reader cannot access documents outside their permissions

**Pre-conditions:**
- Logged in as Reader
- Know barcode of document outside Reader's permissions: `99999999`

**Test Steps:**
1. Manually navigate to URL: `/documents/edit/99999999`
2. Or attempt API call directly (if testing API)

**Expected Result:**
- Access Denied error
- Document details do NOT display
- No information leaked about the document

**Actual Result:**
[To be filled]

**Status:** ⬜ Not Run | ✅ Pass | ❌ Fail

**Notes:**

---

### Excel Export

#### TC-R-030: Export Search Results to Excel

**Objective:** Verify Reader can export filtered search results

**Pre-conditions:**
- Logged in as Reader
- Search returns at least 10 results

**Test Steps:**
1. Navigate to "Search Documents"
2. Perform search (results display)
3. Click "Export to Excel" button

**Expected Result:**
- Excel file downloads: `Documents_Export_YYYYMMDD_HHMMSS.xlsx`
- File contains:
  - All columns from search results
  - Only documents Reader has permission to see
  - Formatted headers
  - Data matches what's on screen
- File opens in Excel without errors

**Actual Result:**
[To be filled]

**Status:** ⬜ Not Run | ✅ Pass | ❌ Fail

**Notes:**

---

#### TC-R-031: Export Large Result Set

**Objective:** Verify export handles large data volumes

**Pre-conditions:**
- Logged in as Reader
- Search returns 500+ results

**Test Steps:**
1. Perform search with many results
2. Click "Export to Excel"

**Expected Result:**
- Export completes (may take time)
- Excel file downloads successfully
- All records present in export
- No timeout errors

**Actual Result:**
[To be filled]

**Status:** ⬜ Not Run | ✅ Pass | ❌ Fail

**Notes:**

---

#### TC-R-032: Export with No Results

**Objective:** Verify export handles empty result set

**Pre-conditions:**
- Logged in as Reader

**Test Steps:**
1. Perform search that returns 0 results
2. Attempt to click "Export to Excel"

**Expected Result:**
- Export button disabled OR
- Export creates Excel with headers only (no data rows) OR
- Warning message: "No results to export"

**Actual Result:**
[To be filled]

**Status:** ⬜ Not Run | ✅ Pass | ❌ Fail

**Notes:**

---

### Permission Boundaries

#### TC-R-040: Attempt to Register Document

**Objective:** Verify Reader cannot access document registration

**Pre-conditions:**
- Logged in as Reader

**Test Steps:**
1. Verify "Register Document" not in menu
2. Attempt direct URL: `/documents/register`
3. Attempt API call: `POST /api/documents`

**Expected Result:**
- Menu item not visible
- Direct URL redirects or shows Access Denied
- API call returns 403 Forbidden

**Actual Result:**
[To be filled]

**Status:** ⬜ Not Run | ✅ Pass | ❌ Fail

**Notes:**

---

#### TC-R-041: Attempt to Edit Document

**Objective:** Verify Reader cannot edit documents

**Pre-conditions:**
- Logged in as Reader
- Know barcode of document within permissions: `12345678`

**Test Steps:**
1. View document details
2. Look for Edit/Save buttons
3. Attempt direct URL: `/documents/edit/12345678`
4. Attempt API call: `PUT /api/documents/12345678`

**Expected Result:**
- No Edit/Save buttons in UI (all read-only)
- Direct URL shows read-only view OR Access Denied
- API call returns 403 Forbidden

**Actual Result:**
[To be filled]

**Status:** ⬜ Not Run | ✅ Pass | ❌ Fail

**Notes:**

---

#### TC-R-042: Attempt to Delete Document

**Objective:** Verify Reader cannot delete documents

**Pre-conditions:**
- Logged in as Reader

**Test Steps:**
1. View document details
2. Look for Delete button
3. Attempt API call: `DELETE /api/documents/12345678`

**Expected Result:**
- No Delete button visible
- API call returns 403 Forbidden
- Document remains in database

**Actual Result:**
[To be filled]

**Status:** ⬜ Not Run | ✅ Pass | ❌ Fail

**Notes:**

---

#### TC-R-043: Attempt to Check-in PDF

**Objective:** Verify Reader cannot access check-in functionality

**Pre-conditions:**
- Logged in as Reader

**Test Steps:**
1. Verify "Check-in Scanned" not in menu
2. Attempt direct URL: `/documents/checkin/testfile.pdf`
3. Attempt to access scanned file list

**Expected Result:**
- Menu item not visible
- Direct URL redirects or shows Access Denied
- Cannot access check-in functionality

**Actual Result:**
[To be filled]

**Status:** ⬜ Not Run | ✅ Pass | ❌ Fail

**Notes:**

---

#### TC-R-044: Attempt to Send Email

**Objective:** Verify Reader cannot send emails with documents

**Pre-conditions:**
- Logged in as Reader
- On search results page

**Test Steps:**
1. Select documents in search results
2. Look for "Send Email" or "Email Selected" button

**Expected Result:**
- No email functionality visible
- Cannot send emails with documents

**Actual Result:**
[To be filled]

**Status:** ⬜ Not Run | ✅ Pass | ❌ Fail

**Notes:**

---

#### TC-R-045: Attempt to Access User Permissions

**Objective:** Verify Reader cannot access user management

**Pre-conditions:**
- Logged in as Reader

**Test Steps:**
1. Verify "User Permissions" not in menu
2. Attempt direct URL: `/admin/users`
3. Attempt API call: `GET /api/userpermissions`

**Expected Result:**
- Menu item not visible
- Direct URL shows Access Denied
- API call returns 403 Forbidden

**Actual Result:**
[To be filled]

**Status:** ⬜ Not Run | ✅ Pass | ❌ Fail

**Notes:**

---

#### TC-R-046: Attempt to Manage Reference Data

**Objective:** Verify Reader cannot create/edit/delete reference data

**Pre-conditions:**
- Logged in as Reader

**Test Steps:**
1. Navigate to any reference data page (if visible for viewing)
2. Look for Create/Edit/Delete buttons
3. Attempt API calls:
   - `POST /api/countries`
   - `PUT /api/currencies/USD`
   - `DELETE /api/counterparties/1`

**Expected Result:**
- Reference data visible for viewing only (if accessible)
- No Create/Edit/Delete buttons
- All API modification calls return 403 Forbidden

**Actual Result:**
[To be filled]

**Status:** ⬜ Not Run | ✅ Pass | ❌ Fail

**Notes:**

---

### Real-Time Updates

#### TC-R-050: Receive Real-Time Updates (SignalR)

**Objective:** Verify Reader receives real-time updates when data changes

**Pre-conditions:**
- Logged in as Reader (Browser 1)
- Second user logged in as Publisher (Browser 2)
- Both viewing same search results

**Test Steps:**
1. Reader: Perform search, results display
2. Publisher: Edit a document visible to Reader
3. Publisher: Save changes
4. Observe Reader's screen

**Expected Result:**
- Reader's search results update automatically (SignalR push)
- Changed document reflects new values
- No page refresh required

**Actual Result:**
[To be filled]

**Status:** ⬜ Not Run | ✅ Pass | ❌ Fail

**Notes:**

---

#### TC-R-051: SignalR Connection Loss and Reconnect

**Objective:** Verify Reader's SignalR reconnection works

**Pre-conditions:**
- Logged in as Reader
- SignalR connection established

**Test Steps:**
1. Open browser dev tools, Network tab
2. Simulate network interruption (disconnect WiFi briefly)
3. Reconnect network
4. Have another user make changes

**Expected Result:**
- SignalR attempts automatic reconnection
- Connection re-establishes
- Updates resume after reconnection
- No errors visible to user

**Actual Result:**
[To be filled]

**Status:** ⬜ Not Run | ✅ Pass | ❌ Fail

**Notes:**

---

### Edge Cases

#### TC-R-060: User with No Permissions

**Objective:** Test Reader with zero UserPermission records

**Pre-conditions:**
- Create test user with HasAccess = true but zero UserPermissions

**Test Steps:**
1. Login as this user
2. Navigate to Search Documents
3. Perform search

**Expected Result:**
- Search returns zero results
- Message: "No documents found" or "You have no document permissions"
- No errors
- Application doesn't crash

**Actual Result:**
[To be filled]

**Status:** ⬜ Not Run | ✅ Pass | ❌ Fail

**Notes:**

---

#### TC-R-061: Permission Change During Active Session

**Objective:** Verify behavior when Reader's permissions change mid-session

**Pre-conditions:**
- Logged in as Reader
- Reader has permissions for Invoice/Sweden

**Test Steps:**
1. Reader: Perform search, see invoices
2. SuperUser: Remove Reader's Invoice permission via User Permissions page
3. Reader: Perform search again (without logging out)

**Expected Result:**
- After permission change, Reader sees fewer documents
- Change takes effect within cache timeout (5 minutes max)
- OR Reader must log out/in to see change (document expected behavior)

**Actual Result:**
[To be filled]

**Status:** ⬜ Not Run | ✅ Pass | ❌ Fail

**Notes:**

---

#### TC-R-062: Very Long Search Results

**Objective:** Test UI with 1000+ search results

**Pre-conditions:**
- Database has 1000+ documents within Reader's permissions

**Test Steps:**
1. Perform search that returns 1000+ results
2. Scroll through results
3. Observe pagination or virtual scrolling

**Expected Result:**
- Results display (may be paginated)
- UI remains responsive
- No browser crash or freeze
- Export still works

**Actual Result:**
[To be filled]

**Status:** ⬜ Not Run | ✅ Pass | ❌ Fail

**Notes:**

---

#### TC-R-063: SQL Injection Attempt in Search

**Objective:** Verify search fields are protected against SQL injection

**Pre-conditions:**
- Logged in as Reader

**Test Steps:**
1. Navigate to Search Documents
2. Enter in Document No field: `'; DROP TABLE Documents; --`
3. Enter in Comment field: `' OR '1'='1`
4. Click Search

**Expected Result:**
- No SQL error
- No database corruption
- Search treats input as literal string
- Either no results or valid search results (if any match)

**Actual Result:**
[To be filled]

**Status:** ⬜ Not Run | ✅ Pass | ❌ Fail

**Notes:**

---

#### TC-R-064: XSS Attempt in Search Results

**Objective:** Verify search results don't execute injected scripts

**Pre-conditions:**
- Document exists with Comment: `<script>alert('XSS')</script>`

**Test Steps:**
1. Search for document with scripted comment
2. View search results
3. Open document details

**Expected Result:**
- Script tag displayed as text (HTML encoded)
- No JavaScript alert executes
- No XSS vulnerability

**Actual Result:**
[To be filled]

**Status:** ⬜ Not Run | ✅ Pass | ❌ Fail

**Notes:**

---

#### TC-R-065: Concurrent Sessions - Same User

**Objective:** Test Reader logged in from multiple browsers

**Pre-conditions:**
- Same Reader account

**Test Steps:**
1. Login as Reader in Chrome
2. Login as same Reader in Firefox
3. Perform actions in both browsers
4. Check for conflicts

**Expected Result:**
- Both sessions work independently
- No session conflicts
- Both receive SignalR updates

**Actual Result:**
[To be filled]

**Status:** ⬜ Not Run | ✅ Pass | ❌ Fail

**Notes:**

---

#### TC-R-066: Session Timeout

**Objective:** Verify behavior after session timeout

**Pre-conditions:**
- Logged in as Reader

**Test Steps:**
1. Login
2. Leave browser idle for session timeout period (e.g., 30 minutes)
3. Attempt to perform search

**Expected Result:**
- Redirect to login page OR
- Session expired message
- After re-authentication, can continue working

**Actual Result:**
[To be filled]

**Status:** ⬜ Not Run | ✅ Pass | ❌ Fail

**Notes:**

---

#### TC-R-067: Special Characters in Document Data

**Objective:** Verify display of documents with special characters

**Pre-conditions:**
- Documents exist with special characters:
  - Counter Party: `O'Brien & Associates`
  - Document No: `INV/2025-001`
  - Comment: `€1,000.50 payment due`

**Test Steps:**
1. Search for these documents
2. View in search results
3. Open document details
4. Export to Excel

**Expected Result:**
- All special characters display correctly
- No encoding issues
- Excel export shows special characters correctly

**Actual Result:**
[To be filled]

**Status:** ⬜ Not Run | ✅ Pass | ❌ Fail

**Notes:**

---

#### TC-R-068: Unicode Characters in Search

**Objective:** Verify support for international characters

**Pre-conditions:**
- Documents with Unicode characters exist (Swedish: åäö, Danish: æøå)

**Test Steps:**
1. Search using Unicode characters
2. View results

**Expected Result:**
- Search works correctly with Unicode
- Results display Unicode characters properly

**Actual Result:**
[To be filled]

**Status:** ⬜ Not Run | ✅ Pass | ❌ Fail

**Notes:**

---

## Test Results Summary

### Test Execution Summary

| Category | Total | Pass | Fail | Blocked | Not Run |
|----------|-------|------|------|---------|---------|
| Authentication & Navigation | 3 | | | | |
| Document Search & Filtering | 5 | | | | |
| Document Viewing | 3 | | | | |
| Excel Export | 3 | | | | |
| Permission Boundaries | 7 | | | | |
| Real-Time Updates | 2 | | | | |
| Edge Cases | 9 | | | | |
| **TOTAL** | **32** | | | | |

### Critical Defects

| Defect ID | Severity | Description | Status |
|-----------|----------|-------------|--------|
| | | | |

### Sign-Off

**Tested By:** _______________________ **Date:** _____________

**Approved By:** _______________________ **Date:** _____________

**Notes:**

---

**Next Steps:** Proceed to BUSINESS_TEST_PLAN_PUBLISHER.md
