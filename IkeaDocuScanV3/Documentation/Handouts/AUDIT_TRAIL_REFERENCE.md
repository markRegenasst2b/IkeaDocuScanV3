# IkeaDocuScan Audit Trail Reference

## Overview

The IkeaDocuScan system maintains a comprehensive audit trail of user actions for compliance and accountability purposes. Every tracked action records:

- **Who** - The Windows-authenticated username
- **What** - The action performed
- **When** - Timestamp of the action
- **Which Document** - The document barcode (or special marker for system operations)
- **Details** - Additional context about the action

---

## Tracked Actions

### Document Lifecycle

| Action | Description | When Triggered |
|--------|-------------|----------------|
| **Register** | New document created in the system | User creates and saves a new document |
| **Edit** | Existing document properties modified | User saves changes to document properties |
| **Delete** | Document permanently removed | User deletes a document |
| **CheckIn** | File attached to document | User uploads/associates a file with a document |

### Email Operations

| Action | Description | When Triggered |
|--------|-------------|----------------|
| **SendLink** | Document link emailed | User sends a single document link via email |
| **SendAttachment** | Document file emailed | User sends a single document as email attachment |
| **SendLinks** | Multiple document links emailed | User sends links to multiple documents in one email |
| **SendAttachments** | Multiple document files emailed | User sends multiple documents as email attachments |

### Export Operations

| Action | Description | When Triggered |
|--------|-------------|----------------|
| **ExportExcel** | Documents exported to Excel | User exports document list to Excel spreadsheet |

### System Administration

| Action | Description | When Triggered |
|--------|-------------|----------------|
| **ViewLogs** | System logs accessed | User searches or views system log entries |
| **ExportLogs** | System logs exported | User exports system logs to CSV or JSON file |

---

## Audit Entry Details by Action

### Register (New Document)

**Recorded When:** A user creates a new document in the system.

**Details Captured:**
- Document name
- Document barcode (auto-generated)
- Creating user

**Example Entry:**
```
User: domain\jsmith
Action: Register
BarCode: 1234567890
Details: Document registered: Invoice-2025-001
Timestamp: 2025-01-15 09:23:45
```

---

### Edit (Document Modified)

**Recorded When:** A user saves changes to an existing document's properties.

**Details Captured:**
- Fields that were changed (when available)
- Document barcode
- Modifying user

**Example Entry:**
```
User: domain\jsmith
Action: Edit
BarCode: 1234567890
Details: Document edited: CounterParty, Amount, DocumentDate
Timestamp: 2025-01-15 10:15:22
```

---

### Delete (Document Removed)

**Recorded When:** A user permanently deletes a document from the system.

**Details Captured:**
- Document name (captured before deletion)
- Document barcode
- Deleting user

**Example Entry:**
```
User: domain\jsmith
Action: Delete
BarCode: 1234567890
Details: Document deleted: Invoice-2025-001
Timestamp: 2025-01-15 11:30:00
```

---

### CheckIn (File Attached)

**Recorded When:** A user uploads or associates a file with a document.

**Details Captured:**
- File name
- File size in bytes
- Document barcode
- User performing check-in

**Example Entry:**
```
User: domain\jsmith
Action: CheckIn
BarCode: 1234567890
Details: File checked in: Invoice_scan.pdf (245760 bytes)
Timestamp: 2025-01-15 09:25:00
```

---

### SendLink (Single Document Link)

**Recorded When:** A user sends a document link via email to a recipient.

**Details Captured:**
- Recipient email address
- Document barcode
- Sending user

**Example Entry:**
```
User: domain\jsmith
Action: SendLink
BarCode: 1234567890
Details: Link sent to recipient@example.com
Timestamp: 2025-01-15 14:00:00
```

---

### SendAttachment (Single Document Attachment)

**Recorded When:** A user sends a document file as an email attachment.

**Details Captured:**
- Recipient email address
- Document barcode
- Sending user

**Example Entry:**
```
User: domain\jsmith
Action: SendAttachment
BarCode: 1234567890
Details: Attachment sent to recipient@example.com
Timestamp: 2025-01-15 14:05:00
```

---

### SendLinks (Multiple Document Links)

**Recorded When:** A user sends links to multiple documents in a single email.

**Details Captured:**
- Recipient email address
- All document barcodes (one audit entry per document)
- Sending user

**Example Entry (one entry created per document):**
```
User: domain\jsmith
Action: SendLinks
BarCode: 1234567890
Details: Links sent to recipient@example.com
Timestamp: 2025-01-15 14:10:00
```

---

### SendAttachments (Multiple Document Attachments)

**Recorded When:** A user sends multiple document files as email attachments.

**Details Captured:**
- Recipient email address
- All document barcodes (one audit entry per document)
- Sending user

**Example Entry (one entry created per document):**
```
User: domain\jsmith
Action: SendAttachments
BarCode: 1234567890
Details: Attachments sent to recipient@example.com
Timestamp: 2025-01-15 14:15:00
```

---

### ExportExcel (Document Export)

**Recorded When:** A user exports a list of documents to an Excel spreadsheet.

**Details Captured:**
- Number of documents exported
- Exporting user

**Example Entry:**
```
User: domain\jsmith
Action: ExportExcel
BarCode: BULKEXPORT
Details: Exported 150 documents to Excel
Timestamp: 2025-01-15 15:00:00
```

Note: The barcode "BULKEXPORT" is a special marker indicating this is a bulk operation not tied to a single document.

---

### ViewLogs (Log Access)

**Recorded When:** A user searches or views system log entries.

**Details Captured:**
- Search filters applied (log level, date range, search text)
- Viewing user

**Example Entry:**
```
User: domain\admin
Action: ViewLogs
BarCode: LOGVIEWER
Details: Viewed logs: Level=Error, From=2025-01-01, To=2025-01-15, Search=timeout
Timestamp: 2025-01-15 16:00:00
```

Note: The barcode "LOGVIEWER" is a special marker for log viewing operations.

---

### ExportLogs (Log Export)

**Recorded When:** A user exports system logs to a file (CSV or JSON format).

**Details Captured:**
- Export format (CSV or JSON)
- Search filters applied
- Exporting user

**Example Entry:**
```
User: domain\admin
Action: ExportLogs
BarCode: LOGEXPORT
Details: Exported logs as CSV: Level=Warning, From=2025-01-01, To=2025-01-15
Timestamp: 2025-01-15 16:30:00
```

Note: The barcode "LOGEXPORT" is a special marker for log export operations.

---

## Special Barcode Values

Some audit entries use special barcode markers for operations not tied to a specific document:

| Marker | Used For |
|--------|----------|
| BULKEXPORT | Excel export operations |
| LOGVIEWER | System log viewing |
| LOGEXPORT | System log exports |

---

## Querying the Audit Trail

The system provides several ways to query audit trail data:

1. **By Document Barcode** - View all actions performed on a specific document
2. **By User** - View all actions performed by a specific user
3. **By Date Range** - View all actions within a time period
4. **Recent Activity** - View the most recent audit entries

---

## Data Retention

Audit trail entries are stored permanently in the database and are not automatically purged. This ensures a complete history of document activity is maintained for compliance purposes.

---

## Summary Table

| Action | Category | Creates Entry Per |
|--------|----------|-------------------|
| Register | Document Lifecycle | Document |
| Edit | Document Lifecycle | Document |
| Delete | Document Lifecycle | Document |
| CheckIn | Document Lifecycle | Document |
| SendLink | Email | Document |
| SendAttachment | Email | Document |
| SendLinks | Email | Each Document |
| SendAttachments | Email | Each Document |
| ExportExcel | Export | Operation (bulk marker) |
| ViewLogs | Administration | Operation (special marker) |
| ExportLogs | Administration | Operation (special marker) |

---

*Document generated: 2025-01-28*
*IkeaDocuScan V3*
