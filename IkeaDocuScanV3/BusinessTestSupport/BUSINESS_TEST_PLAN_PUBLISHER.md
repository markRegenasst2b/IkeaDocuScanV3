# IkeaDocuScan V3 - Business Test Plan: PUBLISHER ROLE

**Version:** 1.0
**Date:** 2025-11-14
**Test Role:** Publisher (Content Manager)

---

## Publisher Role Overview

### Capabilities

The **Publisher** role has **create/edit** access with the following capabilities:

✅ **Includes All Reader Capabilities** (see BUSINESS_TEST_PLAN_READER.md)
✅ **Plus:**
- Register new documents (without PDF)
- Edit existing documents (within permissions)
- Check-in scanned PDF files and link to documents
- Send emails with document attachments or links
- View and export action reminders

❌ **Cannot Do:**
- Delete documents (SuperUser only)
- Manage user permissions
- Create/edit/delete reference data
- Access system configuration
- View audit trail

### Permission Filtering

Publisher sees ONLY documents matching their UserPermission records (same as Reader).

---

## Critical Test Scenarios

### Document Registration

#### TC-P-001: Register New Document (Happy Path)

**Objective:** Verify Publisher can register a new document successfully

**Pre-conditions:**
- Logged in as Publisher
- Know next available barcode

**Test Steps:**
1. Navigate to "Register Document"
2. Enter Barcode: `2025001234`
3. Select Document Type: `Invoice`
4. Select Counter Party: `ACME Corp` (autocomplete)
5. Select Country: `Sweden`
6. Enter Document No: `INV-2025-001`
7. Enter Amount: `1500.00`
8. Select Currency: `SEK`
9. Set Receiving Date: Today
10. Add Comment: `Test invoice registration`
11. Click "Save"

**Expected Result:**
- Success message: "Document saved successfully"
- Redirect to search or document details
- Document appears in search results
- Audit trail entry created
- Real-time update sent to other users

**Actual Result:**
[To be filled]

**Status:** ⬜

---

#### TC-P-002: Register with All Optional Fields

**Objective:** Verify all fields save correctly

**Test Steps:**
1. Register document with ALL fields populated:
   - All mandatory fields
   - Third Party (multi-select)
   - Document Name
   - All dates (Contract, Action, Sending Out, Forwarded to Signatories)
   - Version Number
   - Action Description
   - All boolean flags (Fax, Original, Confidential, Bank Confirmation)

**Expected Result:**
- All fields save correctly
- All data displays in view mode
- No truncation or data loss

**Actual Result:**
[To be filled]

**Status:** ⬜

---

#### TC-P-003: Duplicate Barcode Detection

**Objective:** Verify system prevents duplicate barcodes

**Pre-conditions:**
- Document exists with Barcode: `12345678`

**Test Steps:**
1. Attempt to register new document
2. Enter existing Barcode: `12345678`
3. Fill other required fields
4. Click "Save"

**Expected Result:**
- Validation error: "Barcode already exists"
- Document NOT saved
- User can correct barcode

**Actual Result:**
[To be filled]

**Status:** ⬜

**Critical:** This prevents data corruption

---

#### TC-P-004: Similar Document Warning

**Objective:** Verify warning for similar documents (same type + number + version)

**Pre-conditions:**
- Document exists: Type=Invoice, DocNo=INV-001, Version=1

**Test Steps:**
1. Register new document
2. Select Type: Invoice
3. Enter DocNo: INV-001
4. Enter Version: 1
5. Attempt to save

**Expected Result:**
- Warning modal: "Similar documents exist" with list of matching barcodes
- User can: "Continue Anyway" or "Cancel"
- If continue, document saves with new barcode

**Actual Result:**
[To be filled]

**Status:** ⬜

---

#### TC-P-005: Invalid Data - Missing Required Fields

**Objective:** Verify validation for required fields

**Test Steps:**
1. Navigate to Register
2. Leave Barcode empty
3. Leave Document Type unselected
4. Click "Save"

**Expected Result:**
- Validation errors display
- Fields with red asterisk (*) are required
- Document NOT saved
- Error messages clear and helpful

**Actual Result:**
[To be filled]

**Status:** ⬜

---

#### TC-P-006: Invalid Data - Special Characters in Barcode

**Objective:** Verify barcode accepts only valid characters

**Test Steps:**
1. Attempt to enter Barcode: `ABC-123@#$`
2. Attempt to save

**Expected Result:**
- Validation error or character rejection
- Only numeric barcodes accepted (based on system rules)

**Actual Result:**
[To be filled]

**Status:** ⬜

---

#### TC-P-007: SQL Injection Attempt in Registration

**Objective:** Verify input sanitization

**Test Steps:**
1. Register document
2. Enter in Document No: `'; DROP TABLE Documents; --`
3. Enter in Comment: `' OR '1'='1`
4. Save

**Expected Result:**
- No SQL error
- Data saved as literal strings
- No database corruption

**Actual Result:**
[To be filled]

**Status:** ⬜

**Critical:** Security test

---

#### TC-P-008: XSS Attempt in Registration

**Objective:** Verify XSS protection

**Test Steps:**
1. Register document
2. Enter in Comment: `<script>alert('XSS')</script>`
3. Save
4. View document

**Expected Result:**
- Script tag stored as text (HTML encoded)
- No script execution when viewing
- Display shows literal `<script>` text

**Actual Result:**
[To be filled]

**Status:** ⬜

**Critical:** Security test

---

### Document Editing

#### TC-P-020: Edit Existing Document

**Objective:** Verify Publisher can edit documents within permissions

**Pre-conditions:**
- Document exists within Publisher's permissions: Barcode `12345678`

**Test Steps:**
1. Search for document `12345678`
2. Click Edit
3. Change Amount from `1000` to `1500`
4. Change Comment
5. Click "Save"

**Expected Result:**
- Success message
- Changes saved to database
- Audit trail entry created (old value → new value)
- Real-time update sent to other users

**Actual Result:**
[To be filled]

**Status:** ⬜

---

#### TC-P-021: Cannot Edit Document Outside Permissions

**Objective:** Verify permission boundary in edit mode

**Pre-conditions:**
- Document exists OUTSIDE Publisher's permissions: Barcode `99999999`

**Test Steps:**
1. Attempt to navigate to `/documents/edit/99999999`
2. Or attempt API call: `PUT /api/documents/99999999`

**Expected Result:**
- Access Denied
- Cannot edit document outside permissions
- No data leaked

**Actual Result:**
[To be filled]

**Status:** ⬜

---

#### TC-P-022: Concurrent Edit Detection

**Objective:** Verify behavior when two users edit same document

**Pre-conditions:**
- Two Publishers logged in (Browser 1 & 2)

**Test Steps:**
1. Both open same document for edit
2. Publisher 1: Change Amount to 1500, Save
3. Publisher 2: Change Comment, attempt to Save (without refreshing)

**Expected Result:**
- Option A: Last write wins (Publisher 2 overwrites Publisher 1's changes - potential issue)
- Option B: Concurrency conflict detected, error message
- Option C: Real-time lock prevents second user from editing

**Document Expected Behavior:**

**Actual Result:**
[To be filled]

**Status:** ⬜

**Critical:** Potential data corruption issue

---

### Check-In PDF

#### TC-P-030: Check-In PDF - Link to Existing Document

**Objective:** Verify check-in workflow links PDF to pre-registered document

**Pre-conditions:**
- Scanned PDF file in `CheckinDirectory`: `12345678.pdf`
- Document pre-registered with Barcode: `12345678` (no file attached)

**Test Steps:**
1. Navigate to "Check-in Scanned"
2. Scanned file list displays `12345678.pdf`
3. Click "Check-in" next to file
4. System extracts barcode `12345678` from filename
5. System finds matching document in database
6. Document details display (pre-populated)
7. Verify details are correct
8. Click "Save"

**Expected Result:**
- PDF linked to document `12345678`
- File moved from `CheckinDirectory` to `ScannedFilesPath`
- Renamed to permanent barcode-based name
- Document now has PDF attachment
- Audit trail entry created
- Real-time update sent

**Actual Result:**
[To be filled]

**Status:** ⬜

**Critical:** Core workflow

---

#### TC-P-031: Check-In PDF - Barcode Not Found

**Objective:** Verify handling when barcode doesn't exist in database

**Pre-conditions:**
- PDF file: `99999999.pdf` in CheckinDirectory
- No document with Barcode `99999999` in database

**Test Steps:**
1. Navigate to "Check-in Scanned"
2. Click "Check-in" next to `99999999.pdf`

**Expected Result:**
- Error message: "Document with barcode 99999999 not found"
- OR System prompts to create new document
- PDF NOT moved until document exists

**Actual Result:**
[To be filled]

**Status:** ⬜

---

#### TC-P-032: Check-In PDF - Invalid Barcode Format

**Objective:** Verify handling of invalid filename

**Pre-conditions:**
- PDF file: `invalid_file.pdf` in CheckinDirectory

**Test Steps:**
1. Navigate to "Check-in Scanned"
2. Attempt to check-in `invalid_file.pdf`

**Expected Result:**
- Error: "Cannot extract barcode from filename"
- File NOT processed
- Remains in CheckinDirectory

**Actual Result:**
[To be filled]

**Status:** ⬜

---

#### TC-P-033: Check-In PDF - Overwrite Existing File

**Objective:** Verify behavior when document already has PDF attached

**Pre-conditions:**
- Document `12345678` already has PDF attached
- New file `12345678.pdf` in CheckinDirectory

**Test Steps:**
1. Attempt to check-in `12345678.pdf`

**Expected Result:**
- Warning: "Document already has file attached. Overwrite?"
- User chooses: Yes/No
- If Yes: Old file backed up or deleted, new file attached
- If No: Check-in cancelled

**Actual Result:**
[To be filled]

**Status:** ⬜

**Critical:** Prevents accidental file loss

---

#### TC-P-034: Check-In PDF - File Size Limit

**Objective:** Verify handling of oversized PDFs

**Pre-conditions:**
- PDF file: `12345678.pdf` size = 60MB (over 50MB limit)

**Test Steps:**
1. Attempt to check-in oversized file

**Expected Result:**
- Error: "File exceeds maximum size of 50MB"
- File NOT processed

**Actual Result:**
[To be filled]

**Status:** ⬜

---

#### TC-P-035: Check-In PDF - Invalid File Type

**Objective:** Verify file extension validation

**Pre-conditions:**
- File: `12345678.exe` in CheckinDirectory

**Test Steps:**
1. Attempt to check-in `.exe` file

**Expected Result:**
- Error: "Invalid file type. Only PDF files allowed"
- File NOT processed

**Actual Result:**
[To be filled]

**Status:** ⬜

**Critical:** Security - prevents malicious file upload

---

#### TC-P-036: Check-In PDF - Path Traversal Attempt

**Objective:** Verify protection against path traversal

**Pre-conditions:**
- File: `..\\..\\12345678.pdf` in CheckinDirectory

**Test Steps:**
1. System attempts to process file with path traversal characters

**Expected Result:**
- Path sanitized or rejected
- File saved to correct directory only
- Cannot escape `ScannedFilesPath`

**Actual Result:**
[To be filled]

**Status:** ⬜

**Critical:** Security test

---

### Email Functionality

#### TC-P-040: Send Email with Attachments

**Objective:** Verify email sending with PDF attachments

**Pre-conditions:**
- Search results with 3 documents (all have PDFs)
- SMTP configured

**Test Steps:**
1. Search for documents
2. Select 3 documents (checkboxes)
3. Click "Send Email"
4. Enter recipient: `test@company.com`
5. Select "Attach PDFs"
6. Click "Send"

**Expected Result:**
- Email sent successfully
- Recipient receives email with 3 PDF attachments
- Email contains document details (barcodes, types, etc.)
- Uses configured email template
- Success confirmation message

**Actual Result:**
[To be filled]

**Status:** ⬜

---

#### TC-P-041: Send Email with Links

**Objective:** Verify email with document links (no attachments)

**Test Steps:**
1. Select documents
2. Choose "Send as Links" option
3. Send email

**Expected Result:**
- Email contains clickable links to documents
- Links format: `https://server/documents/edit/{barcode}`
- No file attachments
- Recipient can click links to view (if they have access)

**Actual Result:**
[To be filled]

**Status:** ⬜

---

#### TC-P-042: Email - Invalid Recipient

**Objective:** Verify validation of email addresses

**Test Steps:**
1. Attempt to send email
2. Enter recipient: `invalid-email`
3. Click Send

**Expected Result:**
- Validation error: "Invalid email address"
- Email NOT sent

**Actual Result:**
[To be filled]

**Status:** ⬜

---

#### TC-P-043: Email - SMTP Failure

**Objective:** Verify handling of email delivery failure

**Pre-conditions:**
- SMTP server unreachable or credentials invalid

**Test Steps:**
1. Attempt to send email

**Expected Result:**
- Error message: "Failed to send email: [reason]"
- User notified of failure
- No silent failure

**Actual Result:**
[To be filled]

**Status:** ⬜

---

### Action Reminders

#### TC-P-050: View Action Reminders

**Objective:** Verify Publisher can view action reminders

**Pre-conditions:**
- Documents exist with ActionDate = today

**Test Steps:**
1. Navigate to "Action Reminders"
2. View list of due actions

**Expected Result:**
- List displays documents with ActionDate = today
- Filtered by Publisher's permissions
- Shows: Barcode, Type, Action Date, Description
- Can export to Excel

**Actual Result:**
[To be filled]

**Status:** ⬜

---

#### TC-P-051: Action Reminder Edge Case - ActionDate Before ReceivingDate

**Objective:** Verify validation of date logic

**Pre-conditions:**
- None

**Test Steps:**
1. Register or edit document
2. Set ReceivingDate: 2025-11-14
3. Set ActionDate: 2025-11-01 (earlier than receiving)
4. Save

**Expected Result:**
- Validation error: "Action date cannot be before receiving date"
- OR System allows but doesn't show in action reminders
- Document expected behavior

**Actual Result:**
[To be filled]

**Status:** ⬜

**Critical:** Data quality check

---

### Edge Cases & Data Corruption Scenarios

#### TC-P-060: Very Long Text Fields

**Objective:** Verify handling of maximum field lengths

**Test Steps:**
1. Register document
2. Enter Comment: 5000 characters
3. Enter DocumentNo: 500 characters
4. Enter ActionDescription: 2000 characters
5. Save

**Expected Result:**
- Fields either truncate with warning OR
- Validation error if over limit
- No database error
- Data saved correctly or rejected cleanly

**Actual Result:**
[To be filled]

**Status:** ⬜

---

#### TC-P-061: NULL Values in Optional Fields

**Objective:** Verify handling of NULL/empty values

**Test Steps:**
1. Register document with only required fields
2. Leave all optional fields empty
3. Save

**Expected Result:**
- Document saves successfully
- NULL or empty values stored correctly
- No errors when viewing/editing later

**Actual Result:**
[To be filled]

**Status:** ⬜

---

#### TC-P-062: Delete In-Use Counter Party

**Objective:** Attempt to delete counter party referenced by documents

**Note:** Publisher cannot delete reference data, but test via API or SuperUser

**Test Steps:**
1. Counter Party "ACME" used by 10 documents
2. Attempt to delete "ACME"

**Expected Result:**
- Error: "Cannot delete - in use by X documents"
- Deletion blocked (foreign key constraint)

**Actual Result:**
[To be filled]

**Status:** ⬜

**Critical:** Prevents referential integrity violation

---

#### TC-P-063: Rapid Successive Saves

**Objective:** Test concurrent save operations

**Test Steps:**
1. Edit document
2. Click Save button rapidly 5 times

**Expected Result:**
- Only one save processed
- Button disabled after first click
- No duplicate audit entries
- No database deadlock

**Actual Result:**
[To be filled]

**Status:** ⬜

---

#### TC-P-064: Network Interruption During Save

**Objective:** Verify data integrity if network fails mid-save

**Test Steps:**
1. Edit document
2. Click Save
3. Immediately disconnect network (WiFi off)

**Expected Result:**
- Save either succeeds (already sent) OR
- Error message: "Network error - please retry"
- No partial save / data corruption
- User can retry when network returns

**Actual Result:**
[To be filled]

**Status:** ⬜

**Critical:** Data integrity

---

## Test Results Summary

| Category | Total | Pass | Fail | Blocked | Not Run |
|----------|-------|------|------|---------|---------|
| Registration | 8 | | | | |
| Editing | 3 | | | | |
| Check-In | 7 | | | | |
| Email | 4 | | | | |
| Action Reminders | 2 | | | | |
| Edge Cases | 5 | | | | |
| **TOTAL** | **29** | | | | |

**Plus:** All Reader tests (32) should also pass for Publisher

**Sign-Off:**

**Tested By:** _______________________ **Date:** _____________

---

**Next:** BUSINESS_TEST_PLAN_SUPERUSER.md
