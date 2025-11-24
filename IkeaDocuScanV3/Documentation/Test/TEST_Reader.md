# Reader Role Test Plan

**Test User:** reader
**Role Level:** Base access (lowest privilege)
**Prerequisites:** User has Reader AD group membership, at least one document exists in system

---

## Test Cases

### DOC-R01: Document List View
| Step | Action | Expected |
|------|--------|----------|
| 1 | Navigate to Documents page | Document list loads with pagination |
| 2 | Verify columns visible | Barcode, Name, Type, CounterParty, Date visible |
| 3 | Click column header | Sorting works |
| 4 | Change page size | Pagination updates correctly |

### DOC-R02: Document Search
| Step | Action | Expected |
|------|--------|----------|
| 1 | Navigate to Search Documents | Search form displays |
| 2 | Enter barcode, click Search | Results filtered by barcode |
| 3 | Select Document Type filter | Results filtered by type |
| 4 | Select CounterParty filter | Results filtered by party |
| 5 | Enter date range | Results filtered by date |
| 6 | Clear filters | All accessible documents shown |

### DOC-R03: Document View/Download
| Step | Action | Expected |
|------|--------|----------|
| 1 | Click document row | Document details/preview opens |
| 2 | Click Download button | File downloads with correct name |
| 3 | Click View/Stream button | PDF opens inline in browser |

### DOC-R04: Excel Export
| Step | Action | Expected |
|------|--------|----------|
| 1 | Select multiple documents | Checkboxes work |
| 2 | Click Export to Excel | Excel file downloads |
| 3 | Open Excel file | Contains selected documents with correct data |

### DOC-R05: Reference Data View (Read-Only)
| Step | Action | Expected |
|------|--------|----------|
| 1 | Open CounterParty dropdown | List of counterparties loads |
| 2 | Open DocumentType dropdown | List of document types loads |
| 3 | Open Country dropdown | List of countries loads |

### DOC-R06: Scanned Files View
| Step | Action | Expected |
|------|--------|----------|
| 1 | Navigate to Check-in Scanned | File list displays |
| 2 | Click file to preview | File preview loads |
| 3 | Verify no Delete button visible | Delete option not available |
| 4 | Verify no Check-in button visible | Check-in option not available |

### DOC-R07: Audit Trail View
| Step | Action | Expected |
|------|--------|----------|
| 1 | Search for document by barcode | Audit entries for document displayed |
| 2 | Verify entries show user, action, timestamp | Audit data visible |

---

## Negative Tests (Must Fail)

### NEG-R01: Cannot Create Document
| Step | Action | Expected |
|------|--------|----------|
| 1 | Look for Create/Add Document button | Button not visible OR disabled |
| 2 | If visible, click it | Access denied / 403 error |

### NEG-R02: Cannot Edit Document
| Step | Action | Expected |
|------|--------|----------|
| 1 | View document details | Edit button not visible OR disabled |
| 2 | If visible, click it | Access denied / 403 error |

### NEG-R03: Cannot Delete Document
| Step | Action | Expected |
|------|--------|----------|
| 1 | View document details | Delete button not visible |

### NEG-R04: Cannot Access Admin Pages
| Step | Action | Expected |
|------|--------|----------|
| 1 | Navigate to /admin/users | Access denied / redirect |
| 2 | Navigate to /admin/endpoints | Access denied / redirect |
| 3 | Navigate to /admin/configuration | Access denied / redirect |

---

## Permission Filtering Test

### PERM-R01: Document Visibility Filter
| Step | Action | Expected |
|------|--------|----------|
| 1 | Note total document count | Count matches user's permission scope |
| 2 | Ask SuperUser for total system count | Reader sees subset if permissions restricted |
| 3 | Search for document outside permission scope | Document not found (not access denied) |

---

## Sign-Off

| Test ID | Pass/Fail | Tester | Date | Notes |
|---------|-----------|--------|------|-------|
| DOC-R01 | | | | |
| DOC-R02 | | | | |
| DOC-R03 | | | | |
| DOC-R04 | | | | |
| DOC-R05 | | | | |
| DOC-R06 | | | | |
| DOC-R07 | | | | |
| NEG-R01 | | | | |
| NEG-R02 | | | | |
| NEG-R03 | | | | |
| NEG-R04 | | | | |
| PERM-R01 | | | | |
