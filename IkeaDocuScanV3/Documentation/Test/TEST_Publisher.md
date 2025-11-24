# Publisher Role Test Plan

**Test User:** publisher
**Role Level:** Read + Write (inherits all Reader capabilities)
**Prerequisites:** User has Publisher AD group membership, scanned files exist in check-in folder

---

## Test Cases (Publisher-Specific)

*Note: All Reader tests (DOC-R01 through PERM-R01) should also pass for Publisher*

### DOC-P01: Create Document
| Step | Action | Expected |
|------|--------|----------|
| 1 | Click Create/Add Document | Create form opens |
| 2 | Enter barcode (unique) | Field accepts input |
| 3 | Select Document Type | Dropdown works |
| 4 | Select CounterParty | Dropdown works |
| 5 | Select Country | Dropdown works |
| 6 | Enter Date of Contract | Date picker works |
| 7 | Click Save | Document created, success message |
| 8 | Search for new barcode | Document appears in list |

### DOC-P02: Create Document - Validation
| Step | Action | Expected |
|------|--------|----------|
| 1 | Leave barcode empty, click Save | Validation error shown |
| 2 | Enter duplicate barcode | Error: barcode already exists |
| 3 | Leave required fields empty | Validation errors for each field |

### DOC-P03: Update Document
| Step | Action | Expected |
|------|--------|----------|
| 1 | Select existing document | Document details load |
| 2 | Click Edit | Edit mode enabled |
| 3 | Change Document Name | Field editable |
| 4 | Change CounterParty | Dropdown works |
| 5 | Click Save | Changes saved, success message |
| 6 | Reload document | Changes persisted |

### DOC-P04: Update Document - Unsaved Changes Warning
| Step | Action | Expected |
|------|--------|----------|
| 1 | Open document for edit | Edit form displayed |
| 2 | Make a change (do not save) | Form shows unsaved indicator |
| 3 | Try to navigate away | Warning dialog appears |
| 4 | Click Cancel on warning | Stay on page, changes intact |
| 5 | Click Confirm on warning | Navigate away, changes lost |

### DOC-P05: Check-In Scanned File
| Step | Action | Expected |
|------|--------|----------|
| 1 | Navigate to Check-in Scanned | File list with check-in options |
| 2 | Select a scanned file | File details/preview shown |
| 3 | Click Check-in button | Check-in form opens |
| 4 | Enter/select document details | Form populated |
| 5 | Confirm check-in | File checked in, moved from scan folder |
| 6 | Search for new document | Document with file appears |

### DOC-P06: Check-In to Existing Document
| Step | Action | Expected |
|------|--------|----------|
| 1 | Select scanned file | File preview shown |
| 2 | Choose "Add to existing document" | Document search/select appears |
| 3 | Select existing document by barcode | Document linked |
| 4 | Confirm | File attached to existing document |

### DOC-P07: Send Email with Document Link
| Step | Action | Expected |
|------|--------|----------|
| 1 | Select document | Document selected |
| 2 | Click Send Email / Share | Email compose form opens |
| 3 | Enter recipient email | Field accepts valid email |
| 4 | Enter subject and message | Fields editable |
| 5 | Select "Send as Link" | Link option selected |
| 6 | Click Send | Email sent, success message |

### DOC-P08: Send Email with Document Attachment
| Step | Action | Expected |
|------|--------|----------|
| 1 | Select document with PDF | Document selected |
| 2 | Click Send Email | Email compose opens |
| 3 | Select "Send as Attachment" | Attachment option selected |
| 4 | Click Send | Email sent with attachment |

---

## Negative Tests (Must Fail)

### NEG-P01: Cannot Delete Document
| Step | Action | Expected |
|------|--------|----------|
| 1 | View document details | Delete button not visible OR disabled |

### NEG-P02: Cannot Delete Scanned File
| Step | Action | Expected |
|------|--------|----------|
| 1 | Navigate to Check-in Scanned | File list shown |
| 2 | Look for Delete button on file | Not visible OR disabled |

### NEG-P03: Cannot Access Admin Pages
| Step | Action | Expected |
|------|--------|----------|
| 1 | Navigate to User Management | Access denied |
| 2 | Navigate to Endpoint Management | Access denied |
| 3 | Navigate to Document Type Admin | Access denied |
| 4 | Navigate to Configuration | Access denied |

### NEG-P04: Cannot Modify Reference Data
| Step | Action | Expected |
|------|--------|----------|
| 1 | Look for Add CounterParty option | Not available |
| 2 | Look for Edit DocumentType option | Not available |

---

## Edge Cases

### EDGE-P01: Large File Check-In
| Step | Action | Expected |
|------|--------|----------|
| 1 | Place 45MB PDF in scan folder | File appears in list |
| 2 | Attempt check-in | Success (under 50MB limit) |

### EDGE-P02: File Over Size Limit
| Step | Action | Expected |
|------|--------|----------|
| 1 | Place 55MB file in scan folder | File appears in list |
| 2 | Attempt check-in | Error: file exceeds size limit |

### EDGE-P03: Invalid File Extension
| Step | Action | Expected |
|------|--------|----------|
| 1 | Place .exe file in scan folder | File may appear in list |
| 2 | Attempt check-in | Error: file type not allowed |

### EDGE-P04: Special Characters in Document Name
| Step | Action | Expected |
|------|--------|----------|
| 1 | Create document with name: Test & Co. <2024> | Name accepted or sanitized |
| 2 | View document | Name displays correctly |

---

## Sign-Off

| Test ID | Pass/Fail | Tester | Date | Notes |
|---------|-----------|--------|------|-------|
| DOC-P01 | | | | |
| DOC-P02 | | | | |
| DOC-P03 | | | | |
| DOC-P04 | | | | |
| DOC-P05 | | | | |
| DOC-P06 | | | | |
| DOC-P07 | | | | |
| DOC-P08 | | | | |
| NEG-P01 | | | | |
| NEG-P02 | | | | |
| NEG-P03 | | | | |
| NEG-P04 | | | | |
| EDGE-P01 | | | | |
| EDGE-P02 | | | | |
| EDGE-P03 | | | | |
| EDGE-P04 | | | | |
