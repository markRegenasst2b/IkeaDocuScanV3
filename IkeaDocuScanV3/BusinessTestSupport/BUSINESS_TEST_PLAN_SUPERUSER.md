# IkeaDocuScan V3 - Business Test Plan: SUPERUSER ROLE

**Version:** 1.0
**Date:** 2025-11-14
**Test Role:** SuperUser (Administrator)

---

## SuperUser Role Overview

### Capabilities

The **SuperUser** role has **full administrative** access:

✅ **Includes All Publisher & Reader Capabilities**
✅ **Plus:**
- Delete documents and scanned files
- Manage user permissions (create, edit, delete)
- Create/edit/delete all reference data:
  - Countries
  - Currencies
  - Document Types
  - Document Names
  - Counter Parties
- Access system configuration (SMTP, email templates, recipients)
- View audit trail
- See ALL documents (no permission filtering)

---

## Critical Test Scenarios

### User Permission Management

#### TC-S-001: Create New User with Permissions

**Objective:** Verify SuperUser can create users and assign permissions

**Test Steps:**
1. Navigate to "User Permissions"
2. Click "Add User"
3. Enter Account Name: `DOMAIN\new_user`
4. Set IsSuperUser: false
5. Add permissions:
   - Document Type: Invoice
   - Country: Sweden
   - Counter Party: ACME
6. Save

**Expected Result:**
- User created in DocuScanUser table
- Permissions created in UserPermissions table
- User can now log in and see only permitted documents
- Audit trail entry created

**Actual Result:**
[To be filled]

**Status:** ⬜

---

#### TC-S-002: Grant SuperUser Privileges

**Objective:** Verify granting SuperUser flag

**Test Steps:**
1. Edit existing user
2. Check "IsSuperUser" checkbox
3. Save

**Expected Result:**
- User's IsSuperUser flag set to true
- User now has full access (ignores UserPermissions)
- Audit trail records this critical change

**Actual Result:**
[To be filled]

**Status:** ⬜

**Critical:** This grants full system access

---

#### TC-S-003: Delete User with Existing Permissions

**Objective:** Verify user deletion handles orphaned permissions

**Pre-conditions:**
- User exists with multiple UserPermission records

**Test Steps:**
1. Delete user

**Expected Result:**
- User deleted from DocuScanUser
- Associated UserPermissions automatically deleted (CASCADE) OR
- Error: "Cannot delete - has existing permissions. Remove permissions first"
- No orphaned permission records

**Actual Result:**
[To be filled]

**Status:** ⬜

---

#### TC-S-004: User with Conflicting Permissions

**Objective:** Test adding contradictory permissions

**Test Steps:**
1. Edit user
2. Add permission: Type=Invoice, Country=(NULL), Party=(NULL) [All invoices]
3. Add permission: Type=(NULL), Country=Germany, Party=(NULL) [All docs in Germany]
4. Save

**Expected Result:**
- Both permissions saved
- User sees UNION of both (invoices anywhere + all Germany docs)
- No conflicts or errors

**Actual Result:**
[To be filled]

**Status:** ⬜

---

### Reference Data Management

#### TC-S-010: Create New Country

**Objective:** Verify creating reference data

**Test Steps:**
1. Navigate to "Country" (Admin menu)
2. Click "Add Country"
3. Enter Code: `NO`
4. Enter Name: `Norway`
5. Save

**Expected Result:**
- Country created
- Available in dropdowns throughout app
- Audit trail entry created

**Actual Result:**
[To be filled]

**Status:** ⬜

---

#### TC-S-011: Delete Country - Not In Use

**Objective:** Delete country with no references

**Pre-conditions:**
- Country "TestCountry" exists, not used by any documents/permissions

**Test Steps:**
1. Delete "TestCountry"

**Expected Result:**
- Country deleted successfully
- Removed from dropdowns
- Audit trail entry

**Actual Result:**
[To be filled]

**Status:** ⬜

---

#### TC-S-012: Delete Country - In Use (Should Fail)

**Objective:** Verify deletion blocked if country is referenced

**Pre-conditions:**
- Country "Sweden" used by:
  - 50 documents
  - 10 user permissions
  - 5 counter parties

**Test Steps:**
1. Attempt to delete "Sweden"

**Expected Result:**
- Error: "Cannot delete - in use by X documents, Y permissions, Z counter parties"
- Deletion blocked
- Foreign key constraint prevents deletion

**Actual Result:**
[To be filled]

**Status:** ⬜

**Critical:** Prevents data corruption

---

#### TC-S-013: Edit Country - Change Code

**Objective:** Verify impact of changing country code (PK)

**Pre-conditions:**
- Country "SE" (Sweden) used by documents

**Test Steps:**
1. Edit country "SE"
2. Change code to "SW"
3. Save

**Expected Result:**
- Option A: Code is immutable (cannot change PK)
- Option B: CASCADE update changes all references
- No broken references

**Actual Result:**
[To be filled]

**Status:** ⬜

**Critical:** Referential integrity

---

#### TC-S-014: Duplicate Reference Data

**Objective:** Verify duplicate prevention

**Pre-conditions:**
- Currency "USD" exists

**Test Steps:**
1. Attempt to create another currency with code "USD"

**Expected Result:**
- Validation error: "Currency code already exists"
- Duplicate not created

**Actual Result:**
[To be filled]

**Status:** ⬜

---

#### TC-S-015: Create Counter Party with Special Characters

**Objective:** Verify support for special characters in names

**Test Steps:**
1. Create Counter Party
2. Name: `O'Brien & Associates (Ö&Ä)`
3. Save

**Expected Result:**
- Saved correctly
- Displays correctly in dropdowns and search
- No encoding issues

**Actual Result:**
[To be filled]

**Status:** ⬜

---

### Document Type Field Configuration

#### TC-S-020: Configure Document Type - Required Fields

**Objective:** Verify field configuration affects registration UI

**Test Steps:**
1. Edit Document Type "Invoice"
2. Set field configurations:
   - Amount: Required
   - Currency: Required
   - ActionDate: Optional
3. Save
4. As Publisher, register new Invoice document

**Expected Result:**
- Amount and Currency show red asterisk (*)
- Cannot save without Amount and Currency
- ActionDate is optional (can be empty)

**Actual Result:**
[To be filled]

**Status:** ⬜

---

#### TC-S-021: Configure Document Type - Field Visibility

**Objective:** Verify hiding fields for certain document types

**Test Steps:**
1. Edit Document Type "Contract"
2. Disable "Amount" field
3. Save
4. Register new Contract

**Expected Result:**
- Amount field not visible for Contracts
- Other document types still show Amount
- No validation errors

**Actual Result:**
[To be filled]

**Status:** ⬜

---

### Configuration Management

#### TC-S-030: Update SMTP Configuration

**Objective:** Verify SMTP configuration with auto-test

**Test Steps:**
1. Navigate to "Configuration" → "SMTP Settings"
2. Update:
   - SmtpHost: `smtp.test.com`
   - SmtpPort: `587`
   - Username: `test@test.com`
   - Password: `password`
3. Click "Save" (triggers SMTP test)

**Expected Result:**
- System tests SMTP connection before saving
- If test succeeds: Settings saved
- If test fails: Error message, settings NOT saved (rollback)
- Audit trail entry

**Actual Result:**
[To be filled]

**Status:** ⬜

**Critical:** Prevents saving broken SMTP config

---

#### TC-S-031: Update Email Template

**Objective:** Verify email template customization

**Test Steps:**
1. Navigate to "Configuration" → "Email Templates"
2. Edit "ActionReminderDaily" template
3. Modify HTML body (change header color)
4. Add placeholder: `{{TestPlaceholder}}`
5. Save

**Expected Result:**
- Template saved
- System validates HTML structure
- Warning if placeholder undefined
- Cached template cleared
- Next email uses new template

**Actual Result:**
[To be filled]

**Status:** ⬜

---

#### TC-S-032: Invalid Email Template

**Objective:** Verify validation of malformed templates

**Test Steps:**
1. Edit email template
2. Enter HTML with mismatched braces: `{{Count}}`
3. Or invalid HTML: `<div>No closing tag`
4. Save

**Expected Result:**
- Validation warning or error
- Option to save anyway or fix
- No system crash when rendering

**Actual Result:**
[To be filled]

**Status:** ⬜

---

#### TC-S-033: Update Email Recipients

**Objective:** Verify email recipient group management

**Test Steps:**
1. Navigate to "Configuration" → "Email Recipients"
2. Find "ActionReminderRecipients" group
3. Add email: `new@company.com`
4. Remove email: `old@company.com`
5. Save

**Expected Result:**
- Recipients updated
- Cache cleared
- Next action reminder email goes to new list
- Audit trail entry

**Actual Result:**
[To be filled]

**Status:** ⬜

---

### Delete Operations

#### TC-S-040: Delete Document

**Objective:** Verify document deletion

**Pre-conditions:**
- Document exists: Barcode `12345678`

**Test Steps:**
1. Search for document
2. Open document details
3. Click "Delete" button
4. Confirm deletion

**Expected Result:**
- Confirmation prompt: "Are you sure?"
- Document deleted from database
- Attached PDF deleted from file system
- Audit trail entry: "Deleted document 12345678"
- Real-time update sent to other users
- Document no longer appears in searches

**Actual Result:**
[To be filled]

**Status:** ⬜

**Critical:** Permanent data loss

---

#### TC-S-041: Delete Document - Cancel

**Objective:** Verify canceling deletion doesn't delete

**Test Steps:**
1. Attempt to delete document
2. Click "Cancel" in confirmation dialog

**Expected Result:**
- Document NOT deleted
- Still exists in database
- No audit entry

**Actual Result:**
[To be filled]

**Status:** ⬜

---

#### TC-S-042: Delete Scanned File Separately

**Objective:** Verify deleting PDF without deleting document record

**Pre-conditions:**
- Document with attached PDF

**Test Steps:**
1. View document with PDF
2. Click "Delete File" (if available) OR
3. Use scanned file management page

**Expected Result:**
- PDF deleted from file system
- Document record remains (without file link)
- Audit trail entry
- Can re-attach different PDF later

**Actual Result:**
[To be filled]

**Status:** ⬜

---

### Audit Trail

#### TC-S-050: View Audit Trail

**Objective:** Verify SuperUser can view all audit entries

**Test Steps:**
1. Navigate to "Audit Trail"
2. View list of all changes

**Expected Result:**
- All audit entries visible
- Shows: Who, When, What changed, Old/New values
- Can filter by user, date range, entity type
- Can export to Excel

**Actual Result:**
[To be filled]

**Status:** ⬜

---

#### TC-S-051: Audit Trail Completeness

**Objective:** Verify all data modifications are audited

**Test Steps:**
1. As SuperUser, perform:
   - Create document
   - Edit document
   - Delete document
   - Create counter party
   - Edit user permissions
   - Update configuration
2. Check audit trail

**Expected Result:**
- All 6 operations have audit entries
- Each entry shows old and new values
- No gaps in audit trail

**Actual Result:**
[To be filled]

**Status:** ⬜

**Critical:** Compliance requirement

---

### No Permission Filtering

#### TC-S-060: SuperUser Sees All Documents

**Objective:** Verify SuperUser bypasses permission filtering

**Pre-conditions:**
- Database has:
  - 100 documents total
  - SuperUser has NO UserPermission records
  - OR SuperUser has limited permissions

**Test Steps:**
1. Login as SuperUser
2. Search all documents

**Expected Result:**
- SuperUser sees ALL 100 documents
- No filtering by UserPermissions
- Can view/edit any document regardless of type/country/party

**Actual Result:**
[To be filled]

**Status:** ⬜

**Critical:** SuperUser must have full access

---

### Edge Cases & Data Corruption

#### TC-S-070: Circular Reference Prevention

**Objective:** Verify prevention of circular dependencies

**Note:** May not be applicable depending on schema

**Test Steps:**
1. If ThirdParty can reference CounterParty:
   - Create CP1 with ThirdParty = CP2
   - Create CP2 with ThirdParty = CP1

**Expected Result:**
- Either: Circular reference allowed (not a problem)
- Or: Validation prevents saving circular reference

**Actual Result:**
[To be filled]

**Status:** ⬜

---

#### TC-S-071: Delete While Document is Being Edited

**Objective:** Test concurrent delete/edit scenario

**Pre-conditions:**
- Two SuperUsers logged in

**Test Steps:**
1. SuperUser1: Opens document for edit
2. SuperUser2: Deletes same document
3. SuperUser1: Attempts to save changes

**Expected Result:**
- Error: "Document no longer exists"
- Save fails gracefully
- No database error

**Actual Result:**
[To be filled]

**Status:** ⬜

**Critical:** Concurrent operation safety

---

#### TC-S-072: Mass Delete Attempt

**Objective:** Test deleting many documents quickly

**Test Steps:**
1. Select 50 documents
2. Click "Delete Selected" (if available)
3. Confirm

**Expected Result:**
- All 50 documents deleted
- All 50 PDF files deleted
- 50 audit trail entries created
- No database timeout
- Transaction succeeds or rolls back completely

**Actual Result:**
[To be filled]

**Status:** ⬜

---

#### TC-S-073: Configuration Rollback on SMTP Failure

**Objective:** Verify automatic rollback if SMTP test fails

**Test Steps:**
1. Note current SMTP settings (working)
2. Update to invalid settings:
   - SmtpHost: `invalid.smtp.server`
   - Port: `999`
3. Click Save (triggers test)

**Expected Result:**
- SMTP test fails
- Error: "SMTP test failed - settings not saved"
- Database rolled back to previous settings
- Current settings unchanged
- Audit trail shows attempted change with "Rolled back" note

**Actual Result:**
[To be filled]

**Status:** ⬜

**Critical:** Prevents breaking email functionality

---

#### TC-S-074: Large Configuration Value

**Objective:** Test storing very large email template

**Test Steps:**
1. Edit email template
2. Paste 100KB HTML template
3. Save

**Expected Result:**
- Template saves (nvarchar(MAX) field)
- No truncation
- Template renders correctly

**Actual Result:**
[To be filled]

**Status:** ⬜

---

## Test Results Summary

| Category | Total | Pass | Fail | Blocked | Not Run |
|----------|-------|------|------|---------|---------|
| User Permissions | 4 | | | | |
| Reference Data | 6 | | | | |
| Document Type Config | 2 | | | | |
| Configuration | 4 | | | | |
| Delete Operations | 3 | | | | |
| Audit Trail | 2 | | | | |
| No Filtering | 1 | | | | |
| Edge Cases | 5 | | | | |
| **TOTAL** | **27** | | | | |

**Plus:** All Publisher tests (29) and Reader tests (32) should also pass

**Sign-Off:**

**Tested By:** _______________________ **Date:** _____________

---

**Next:** SECURITY_TEST_PLAN.md
