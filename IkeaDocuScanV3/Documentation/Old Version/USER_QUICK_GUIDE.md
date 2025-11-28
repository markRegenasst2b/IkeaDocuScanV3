# IkeaDocuScan User Quick Guide

**5-Minute Overview for Users Familiar with Document Management**

---

## 🔐 USER ROLES & ACCESS MATRIX

### Role-Based Navigation Menu Access

| Navigation Menu Item | Reader | Publisher | SuperUser |
|---------------------|---------|-----------|-----------|
| **DOCUMENT MANAGEMENT** | | | |
| Register Document |❌ No Access | ✅ Create/Edit | ✅ All |
| Search Documents | ✅ View | ✅ View/Email | ✅ All/Delete |
| Check-in Scanned | ❌ No Access | ✅ Check-in | ✅ All/Delete |
| Action Reminders | ❌ No Access | ✅ View/Export | ✅ All |
| **SPECIAL REPORTS** | | | |
| Barcode Gaps | ✅ View/Export | ✅ View/Export | ✅ All |
| Duplicate Documents | ✅ View/Export | ✅ View/Export | ✅ All |
| Unlinked Registrations | ✅ View/Export | ✅ View/Export | ✅ All |
| Scan Copies | ✅ View/Export | ✅ View/Export | ✅ All |
| Suppliers | ✅ View/Export | ✅ View/Export | ✅ All |
| **ADMINISTRATION** | | | |
| User Permissions | ❌ No Access | ❌ No Access | ✅ Full Access |
| Currency | ❌ No Access | ❌ No Access | ✅ Create/Edit/Delete |
| Country | ❌ No Access | ❌ No Access | ✅ Create/Edit/Delete |
| Document Type | ❌ No Access | ❌ No Access | ✅ Create/Edit/Delete |
| Counter Party | ❌ No Access | ❌ No Access | ✅ Create/Edit/Delete |
| Document Names |❌ No Access | ❌ No Access | ✅ Create/Edit/Delete |
| **SETTINGS** | | | |
| Configuration | ❌ No Access | ❌ No Access | ✅ Full Access |
| Audit Trail | ❌ No Access | ❌ No Access | ✅ Full Access |

### Role Capabilities Summary

**Reader (View-Only):**
- Search and view documents (filtered by permissions)
- View all reference data
- Export reports to Excel
- View action reminders
- View audit trail
- **Cannot:** Create, edit, delete, or send emails

**Publisher (Content Manager):**
- All Reader capabilities
- Create and edit documents
- Check-in scanned files
- Send emails with document attachments
- **Cannot:** Delete documents, manage users, modify reference data

**SuperUser (Administrator):**
- All Reader and Publisher capabilities
- Delete documents and scanned files
- Manage all reference data (currencies, countries, document types, counter parties)
- Manage user permissions
- Configure system settings
- View all data without permission filters

### Permission Filtering

**Document Access:**
- Reader & Publisher: See only documents matching their assigned permissions (Document Type, Country, Counter Party)
- SuperUser: See all documents regardless of permissions

**Reference Data:**
- All roles can view currencies, countries, document types, counter parties
- Only SuperUser can create, edit, or delete reference data

---

## 📄 DOCUMENT MANAGEMENT

### Document Properties Page (`/documents/register`, `/documents/edit/{barcode}`, `/documents/checkin/{filename}`)

**Purpose:** Create, edit, or check-in documents with comprehensive metadata. This page operates in three distinct modes:

#### **Mode 1: Register Mode** (`/documents/register`)
Create a new document **without a file** - manual barcode entry required.

**Use Case:** Pre-register a document before the physical file arrives or is scanned.

**Workflow:**
1. Navigate to `/documents/register` (or click "Register Document" in nav menu)
2. **Manually enter barcode** in the barcode field (no auto-generation)
3. Select document type (affects required fields - mandatory fields marked with red asterisk)
4. Choose counter party and third party (autocomplete search - start typing)
5. Fill in financial details: currency, amount, dates (contract, receiving, action)
6. Set document flags: Fax, Original, Confidential, Bank Confirmation (True/False radio buttons)
7. Add comments, action description, version number
8. Click Save (file can be attached later via Edit mode or Check-in)

**Key Point:** Barcode must be entered manually. No file is attached in this mode.

---

#### **Mode 2: Check-In Mode** (`/documents/checkin/{filename}`)
Register a new document **with a scanned file** - barcode extracted from filename.

**Use Case:** Physical document has been scanned to network folder and needs to be registered.

**Workflow:**
1. Scanner saves PDF to network folder (filename should contain barcode, e.g., `12345.pdf` or `Invoice_12345.pdf`)
2. Navigate to "Check-in Scanned" page (`/checkin-scanned`)
3. Search for your file in the file list
4. Click "Check In" button on the file row
5. System extracts barcode from filename (e.g., `12345` from `12345.pdf`)
6. Page opens in Check-In mode with:
   - **Barcode field pre-filled** (read-only, extracted from filename)
   - **File already attached** (name shown, clickable to preview)
   - Empty form fields ready for data entry
7. Fill in document metadata (same fields as Register mode)
8. Click Save to create document record with file attached

**Two Scenarios:**
- **Scenario A:** Barcode doesn't exist in system → Creates new document with file (warning shown)
- **Scenario B:** Barcode exists without file → Loads existing document data, attaches file on save (success message shown)
- **Scenario C:** Barcode exists with file → Error, cannot overwrite existing file

**Key Point:** Barcode comes from filename, not manual entry. File is attached immediately upon save.

---

#### **Mode 3: Edit Mode** (`/documents/edit/{barcode}`)
Modify an existing document's properties.

**Use Case:** Update metadata for an already-registered document.

**Workflow:**
1. Search for document in "Search Documents" page
2. Click document row or barcode link to open properties
3. Page opens with barcode **read-only** (cannot change)
4. Modify any field (document type, counter party, dates, flags, comments)
5. Click Save to update (audit trail logs all changes)

**Key Point:** Barcode cannot be changed. Existing file (if any) is preserved.

---

**Common Features Across All Modes:**
- **Dynamic Field Validation:** Required fields change based on document type selection
- **Counter Party/Third Party Autocomplete:** Keyboard navigation (arrow keys, Enter)
- **Duplicate Detection:** System warns if similar documents exist before saving
- **Copy/Paste:** Copy data from one document to another (stored in browser for 10 days)
- **Field Visibility Rules:** Some fields hidden/shown based on document type configuration
- **Unsaved Changes Warning:** Browser warns if you try to navigate away without saving
- **Real-time Validation:** Instant feedback on required fields and format errors

---

### Search Documents (`/documents/search`)

**Purpose:** Advanced multi-criteria document search with export capabilities.

**Key Features:**
- **PDF Content Search:** Full-text search within PDF files
- **Barcode Lookup:** Search single or multiple barcodes (comma-separated)
- **Multi-Select Filters:** Document types, counter parties, countries, currencies
- **Date Range Filters:** Contract date, receiving date, action date
- **Boolean Filters:** Fax, Original, Confidential, Bank Confirmation (with clear buttons)
- **Advanced Options:** Version number, document number, comment search
- **Result Actions:**
  - Open document properties (view/edit)
  - Preview PDF inline
  - Send email with document
  - Export results to Excel
- **Keyboard Navigation:** Use arrow keys in dropdowns, Enter to select

**Search Tips:**
- **Leave filters empty to show all documents** (limited to first 1,000 results - refine search if limit reached)
- Use partial names for counter party search
- PDF search queries database index (instant results)
- Click column headers to sort results
- Clear individual filters with ✕ button
- If "Max limit reached (1000)" badge appears, add more filters to narrow results

---

### Check-in Scanned (`/checkin-scanned`)

**Purpose:** Import scanned files from network folder into document system.

**Key Features:**
- **File Browser:** View all files in configured scanned folder
- **File Preview:** See file details (size, modified date, type)
- **Search & Sort:** Filter files by name, sort by any column
- **Barcode Extraction:** Automatically detect barcode from filename
- **Link to Document:** Associate scanned file with existing document record
- **File Actions:** Preview, check-in, or delete scanned files
- **Pagination:** Navigate large file lists efficiently
- **Refresh:** Manually reload file list (60-second cache)

**Common Workflow:**
1. Scanner saves PDF to network folder
2. Open Check-in Scanned page
3. Search for file by name or barcode
4. Click "View Details" to preview
5. System suggests matching barcode from filename
6. Confirm or edit barcode
7. Check-in attaches file to document record

---

### Action Reminders (`/action-reminders`)

**Purpose:** Track documents with upcoming or overdue action dates.

**Business Rules - Which Documents Appear:**
- Document MUST have an ActionDate set (not null)
- ActionDate MUST be on or after ReceivingDate (validation rule)

**Visual Status Indicators:**
- 🔴 **Overdue** (Red row + Red badge): ActionDate < Today (past due)
- 🟡 **Due Today** (Yellow row + Yellow badge): ActionDate = Today
- 🔵 **Due This Week** (Blue badge): ActionDate between tomorrow and 7 days from today
- ⚫ **Future** (Gray badge): ActionDate more than 7 days in the future

**Quick Filters:**
- **Today:** Shows only actions due today (excludes overdue and future)
- **Overdue:** Shows only past-due actions (ActionDate < Today)
- **This Week:** Sunday to Saturday of current week (includes overdue, today, and upcoming)
- **This Month:** 1st to last day of current month (includes overdue, today, and upcoming)

**Key Features:**
- **Custom Date Range:** Filter by specific date range (DateFrom - DateTo)
- **Search Filters:** Document type, counter party, barcode, comment, action description
- **Clickable Barcodes:** Click any barcode to open document properties page
- **Excel Export:** Download filtered results as spreadsheet
- **Collapsible Filters:** Hide filter panel for more screen space
- **Real-time Count:** Badge shows total action reminders matching filters

**Default View on Page Load:**
- Opens with "Today" filter active (shows actions due today only)

**Use Cases:**
- Daily review: Check "Today" and "Overdue" filters each morning
- Follow-up: Use "Overdue" filter to identify past-due documents
- Weekly planning: "This Week" shows all actions in current week
- Reporting: Export filtered results for team meetings

---

## 📊 SPECIAL REPORTS

All reports accessible via **Excel Preview** page with export to Excel functionality.

### Barcode Gaps (`/excel-preview?reportType=barcode-gaps`)

**Purpose:** Identify missing barcodes in sequential numbering system.

**Business Rule:** Uses SQL LEAD() window function to find gaps where `BarCode + 1 ≠ NextBarcode`.

**Report Shows:**
- GapStart: First missing barcode in sequence
- GapEnd: Last missing barcode in sequence
- GapSize: Number of missing barcodes
- PreviousBarcode: Last used barcode before gap
- NextBarcode: First used barcode after gap

**Use Cases:**
- Reclaim unused barcode numbers for new documents
- Detect data issues (skipped numbers, deleted records)
- Verify sequential numbering integrity

---

### Duplicate Documents (`/excel-preview?reportType=duplicate-documents`)

**Purpose:** Identify documents with identical key properties that may be duplicate registrations.

**Business Rule:** Groups documents by (Document Type + Document No + Version No + Counter Party), returns groups with COUNT > 1.

**Report Shows:**
- Document Type, Document No, Version No
- Counter Party (No and Name)
- Count: Number of documents in duplicate group

**Important:** Documents are only considered duplicates if ALL four criteria match (type, number, version, AND counter party).

**Use Cases:**
- Data cleanup: Find accidentally double-registered documents
- Quality control: Identify potential data entry errors
- Audit: Verify uniqueness of document identifiers

---

### Unlinked Registrations (`/excel-preview?reportType=unlinked-registrations`)

**Purpose:** Find documents registered in system but without attached files.

**Business Rule:** Selects documents where `FileId IS NULL`.

**Report Shows:**
- Barcode, Document Type, Document Name
- Document No, Counter Party (Name and No)

**Use Cases:**
- Follow-up on incomplete registrations (waiting for file check-in)
- Identify documents registered before scanning
- Track pending file attachments

---

### Scan Copies (`/excel-preview?reportType=scan-copies`)

**Purpose:** Track fax copies waiting for physical original documents.

**Business Rule (Critical):** Selects documents where `Fax = 1 AND OriginalReceived = 0`.

**Report Shows:**
- Barcode, Document Type, Document Name
- Document No, Counter Party (Name and No)

**Business Scenario:**
1. Fax copy received and scanned (Fax = True)
2. Original physical document not yet arrived (OriginalReceived = False)
3. Document appears in this report until original is received

**Use Cases:**
- Follow-up: Track which originals are still pending
- Reminders: Contact counter parties for missing originals
- Compliance: Ensure originals received for legal/audit purposes
- **NOT** a general file inventory - only fax copies awaiting originals

---

### Suppliers (`/excel-preview?reportType=suppliers`)

**Purpose:** List counter parties available in check-in dropdown for document registration.

**Business Rule (Critical):** Selects counter parties where `DisplayAtCheckIn = 1`.

**Report Shows:**
- Counter Party No (Alpha), Name
- Country, Affiliated To

**Important:** This is NOT all counter parties in the system - only those flagged for display during document check-in.

**Use Cases:**
- Reference list: Which suppliers appear in check-in dropdown
- Configuration audit: Verify correct suppliers are enabled
- Training: Show users available counter party options
- **NOT** for vendor analysis or counting documents per supplier

---

**Report Features (All Reports):**
- **Preview Before Export:** See data grid with paging and sorting
- **Column Metadata:** View data types and formats
- **Excel Download:** One-click export with IKEA blue headers
- **Real-time Data:** Reports query live database (no caching)
- **No Filters:** Reports show all matching data (no user-applied filters)

---

## ⚙️ ADMINISTRATION

### User Permissions (`/edit-userpermissions`)

**Purpose:** Manage user access to counter parties, countries, and document types.

**Key Features:**
- **User Search:** Find users by account name (Active Directory)
- **Add New User:** Register users who aren't in system yet
- **Permission Assignment:**
  - Counter Parties: Grant access to specific business entities
  - Countries: Restrict by geographic region
  - Document Types: Control access by document classification
- **Registration Requests:** Filter to show users without permissions
- **Bulk Actions:** Assign multiple permissions at once
- **Active Directory Integration:** Auto-populate user details from domain

**Common Tasks:**
- New employee: Add user → Assign counter parties
- Role change: Edit user → Update document type permissions
- Geographic restriction: Limit user to specific countries
- Access review: Search user → View all permissions

---

### Reference Data Administration

**Currency** (`/currency-administration`)
- Add/edit/delete currency codes
- Set currency name and symbol
- Manage active currencies for document registration

**Country** (`/country-administration`)
- Maintain country list for document classification
- Edit country names and codes
- Control available countries in dropdowns

**Document Type** (`/documenttype-administration`)
- Define document categories (Contract, Invoice, etc.)
- Configure field visibility rules (mandatory/optional/hidden)
- Set document type descriptions
- Control registration form behavior by type

**Counter Party** (`/counterparty-administration`)
- Manage business entities (suppliers, customers, partners)
- Edit counter party names and contact details
- Maintain relationships between entities
- Bulk import/export capabilities

**Document Names** (`/manage-document-names`)
- Predefined document name templates
- Link to specific document types
- Speed up registration with name autocomplete
- Maintain naming consistency

---

## 🔧 SETTINGS

### Configuration Management (`/configuration-management`)

**Purpose:** System-wide settings and application configuration.

**Key Features:**
- View application settings
- Edit configuration values (admin only)
- Manage file paths (scanned folder, archive location)
- Email notification settings
- Audit trail retention policies
- Performance tuning options

**Common Settings:**
- Scanned files path (network folder location)
- SMTP server for email notifications
- Default barcode starting number
- File size limits and allowed extensions
- Active Directory group mappings

---

## 🔑 KEYBOARD SHORTCUTS

**Counter Party / Third Party Selectors:**
- `Arrow Down/Up`: Navigate suggestions
- `Enter`: Select highlighted item
- `Escape`: Close dropdown
- `Tab`: Close dropdown and move to next field

**Search Page:**
- `Ctrl+F`: Focus search field (browser default)
- `Tab`: Navigate through filters
- `Enter`: Apply search (when in text field)

**Data Grids:**
- Click column headers to sort
- Use pagination controls at bottom
- Search box filters current page results

---

## 💡 TIPS & BEST PRACTICES

1. **Barcode Format:** Use consistent numbering (e.g., 5-6 digits)
2. **Counter Party Search:** Type 2-3 letters for autocomplete suggestions
3. **Document Type Selection:** Choose type first to see required fields
4. **File Naming:** Include barcode in scanned filenames for auto-detection
5. **Action Dates:** Set action dates for follow-up reminders
6. **Excel Export:** Use for reporting, analysis, and archival
7. **Clear Filters:** Use ✕ buttons to reset search criteria
8. **Mandatory Fields:** Red asterisk (*) indicates required data
9. **Audit Trail:** All changes are logged with username and timestamp
10. **Real-time Updates:** Refresh icon indicates live data synchronization

---

## 🆘 COMMON WORKFLOWS

**Daily Operations:**
1. Check Action Reminders (overdue + today)
2. Check-in overnight scanned files
3. Register new documents received
4. Update action dates on completed tasks

**Weekly Tasks:**
1. Review "This Week" action reminders
2. Run Unlinked Registrations report
3. Export document lists for team review
4. Check barcode gaps for number management

**Monthly Tasks:**
1. Run Duplicate Documents report (cleanup)
2. Export Suppliers report (business analysis)
3. Review user permissions (access control)
4. Archive completed documents

**Ad-hoc Searches:**
1. Search by counter party (vendor inquiries)
2. Search by date range (period reports)
3. PDF content search (find specific clauses)
4. Barcode lookup (quick document access)

---

## 📱 BROWSER COMPATIBILITY

- **Chrome/Edge:** Full support (recommended)
- **Firefox:** Full support
- **Safari:** Full support (macOS/iOS)
- **Mobile:** Responsive design, touch-friendly

---

## 🔒 SECURITY NOTES

- **Windows Authentication:** Login with domain credentials
- **Role-Based Access:** HasAccess policy required for all features
- **Audit Trail:** All actions logged with user identity
- **File Validation:** Only allowed file types accepted (.pdf, .jpg, .png, .doc, .xls)
- **Path Security:** Automatic validation prevents unauthorized file access

---

**Version:** 3.0
**Last Updated:** January 2025
**Application:** IkeaDocuScan V3 - Blazor WebAssembly + Server

For technical support or feature requests, contact your system administrator.
