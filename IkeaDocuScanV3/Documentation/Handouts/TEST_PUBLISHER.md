# IkeaDocuScan Test Cases - Publisher Role

**Test Profile:** publisher
**Prerequisites:** User has Publisher role, has UserPermissions
**Note:** Reader tests are not repeated here. Run TEST_READER.md first.

---

## 1. Document Registration

### 1.1 Register New Document
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Register Document | Registration form opens |
| 2 | Select Document Type | Document Name dropdown populates |
| 3 | Leave mandatory fields empty, click Save | Validation errors shown for each required field |
| 4 | Fill all mandatory fields | Save button enables |
| 5 | Click Save | Document created, barcode auto-generated |
| 6 | Note the new barcode | Barcode displayed in success message |

### 1.2 Mandatory Field Validation
| Field | Test | Expected |
|-------|------|----------|
| Document Type | Leave empty | Required field error |
| Document Name | Leave empty | Required field error |
| CounterParty | Leave empty | Required field error |
| Country | Leave empty | Required field error |
| Receiving Date | Leave empty | Required field error (if mandatory) |

### 1.3 Duplicate Barcode Prevention
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Note an existing barcode from search | Barcode exists |
| 2 | In Register, manually enter that barcode | Warning: "Barcode already exists" |
| 3 | Attempt to save | Save prevented |

---

## 2. Edit Document Properties

### 2.1 Edit Existing Document
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Search and find a document | Document in results |
| 2 | Click Edit button | Properties page opens in edit mode |
| 3 | Modify a field (e.g., Comment) | Field becomes dirty |
| 4 | Click Save | Changes saved, success message |
| 5 | Refresh and verify | Changes persisted |

### 2.2 Unsaved Changes Warning
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Open document for edit | Edit mode active |
| 2 | Modify any field | Page tracks unsaved changes |
| 3 | Click browser back or navigate away | Warning: "Unsaved changes will be lost" |
| 4 | Click Cancel on warning | Stays on page |
| 5 | Click Confirm on warning | Navigates away, changes lost |

---

## 3. Check-in Scanned Documents

### 3.1 View Scanned Files
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Check-in Scanned | List of PDF files in check-in folder |
| 2 | Files display filename (barcode) | Filename format: {barcode}.pdf |
| 3 | Click Preview on a file | PDF viewer shows document |

### 3.2 Check-in with Existing Metadata
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Pre-register a document (get barcode) | Document exists without PDF |
| 2 | Place file named {barcode}.pdf in check-in folder | File appears in list |
| 3 | Click Check-in on that file | Properties page loads with existing metadata |
| 4 | Review/modify properties, click Save | Document saved WITH PDF attached |
| 5 | Return to Check-in list | File no longer in list (deleted from folder) |
| 6 | Search for document | PDF icon now active |

### 3.3 Check-in without Existing Metadata
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Place file named {newbarcode}.pdf (unused barcode) | File in check-in folder |
| 2 | Click Check-in | Warning: "Barcode not registered, enter metadata now" |
| 3 | Fill all required fields | Form validates |
| 4 | Click Save | New document created WITH PDF |
| 5 | File deleted from check-in folder | File gone |

### 3.4 Check-in Duplicate (Document Already Has PDF)
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Find document that already has PDF attached | Has PDF icon |
| 2 | Place file named {thatbarcode}.pdf in check-in | File appears |
| 3 | Click Check-in | Error: "Document already has file attached" |
| 4 | File NOT deleted | File remains in check-in folder |

---

## 4. Send Email with Documents

### 4.1 Send Document Links
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Search and select 1-3 documents | Checkboxes selected |
| 2 | Click "Send Links" button | Email dialog opens |
| 3 | Enter recipient email | Valid email format accepted |
| 4 | Add optional message | Message field accepts text |
| 5 | Click Send | Success: "Email sent" |
| 6 | Check recipient inbox | Email contains clickable links to documents |

### 4.2 Send Document Attachments
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Select documents WITH PDFs | Documents with PDF icons |
| 2 | Click "Send Attachments" | Email dialog opens |
| 3 | Enter recipient, click Send | Success message |
| 4 | Check recipient inbox | Email has PDF attachments |

### 4.3 Send Attachment without PDF
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Select document WITHOUT PDF | No PDF attached |
| 2 | Click "Send Attachments" | Warning or disabled: "No PDF to attach" |

---

## 5. View Reference Data (Read-Only)

### 5.1 View Metadata Definitions
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Currencies | List displays, no edit buttons |
| 2 | Navigate to Countries | List displays, no edit buttons |
| 3 | Navigate to Document Types | List displays, no edit buttons |
| 4 | Navigate to CounterParties | List displays, no edit buttons |
| 5 | Navigate to Document Names | List displays, no edit buttons |

---

## 6. View Audit Logs

### 6.1 Browse Audit Entries
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Audit Trail | Recent entries display |
| 2 | Filter by Barcode | Shows entries for that document |
| 3 | Filter by User | Shows entries for that user |
| 4 | Filter by Date Range | Shows entries within range |
| 5 | Filter by Action (e.g., CheckIn) | Shows only CheckIn actions |

---

## 7. Action Reminders

### 7.1 View Action Reminders
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Action Reminders | Documents with action dates display |
| 2 | View "Today" filter | Shows documents with ActionDate = today |
| 3 | View "This Week" filter | Shows documents with ActionDate within 7 days |
| 4 | View "This Month" filter | Shows documents within current month |
| 5 | View "Overdue" filter | Shows documents with ActionDate < today |
| 6 | Set custom date range | Results filtered to range |

---

## 8. Special Reports

### 8.1 Barcode Gaps Report
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Reports > Barcode Gaps | Report loads |
| 2 | View results | Shows missing barcodes in sequence |
| 3 | Export to Excel | Downloads .xlsx with gap data |

### 8.2 Duplicate Documents Report
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Reports > Duplicates | Report loads |
| 2 | Shows documents with same Type/No/Version/CounterParty | Grouped duplicates |

### 8.3 Unlinked Registrations Report
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Reports > Unlinked | Report loads |
| 2 | Shows documents without PDF | FileId IS NULL |

### 8.4 Scan Copies Report
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Reports > Scan Copies | Fax-only documents listed |

### 8.5 Suppliers Report
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Reports > Suppliers | All CounterParties listed |
| 2 | Export to Excel | Downloads supplier list |

---

## Edge Cases

| Test | Action | Expected Result |
|------|--------|-----------------|
| Large PDF check-in | Check-in 50MB PDF | Uploads successfully or shows size limit |
| Invalid file extension | Place .exe in check-in folder | File not listed or rejected |
| Concurrent edit | Two users edit same document | Last save wins, or conflict warning |
| Network interruption | Lose connection during save | Error message, retry option |
| Special characters in fields | Enter "Test & <Company>" | Saved correctly, no XSS |

---

## Not Permitted (Verify Access Denied)

| Action | Expected Result |
|--------|-----------------|
| Delete document | Delete button not visible |
| Edit User Permissions | Menu not visible |
| Edit reference data | Edit buttons not visible on metadata pages |
| View system configuration | Config menu not visible |

---

**Test Completed By:** _______________
**Date:** _______________
**Issues Found:** _______________
