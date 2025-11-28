# IkeaDocuScan Test Cases - Reader Role

**Test Profile:** reader
**Prerequisites:** User has Reader role, has UserPermissions for at least one DocumentType

---

## 1. Document Search

### 1.1 Basic Search
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Documents page | Document list loads |
| 2 | Enter barcode in search field | Results filtered to matching barcode |
| 3 | Clear search, select Document Type filter | Results show only selected type |
| 4 | Apply date range filter (Receiving Date) | Results within date range |
| 5 | Combine multiple filters | Results match ALL criteria |

### 1.2 Permission Filtering
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Search without filters | Only documents matching your UserPermissions appear |
| 2 | Note a barcode you CAN see | Document displays |
| 3 | Ask another user for a barcode outside your permissions | Search returns no results for that barcode |

### 1.3 Pagination and Sorting
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Search returns >25 results | Pagination controls appear |
| 2 | Click page 2 | Next page of results loads |
| 3 | Click column header (e.g., BarCode) | Results sort ascending |
| 4 | Click same header again | Results sort descending |

---

## 2. View Document

### 2.1 Open PDF
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Click PDF icon on document row | PDF opens in new tab/viewer |
| 2 | Find document WITHOUT PDF attached | PDF icon disabled or hidden |

### 2.2 View Properties
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Click document row or View button | Properties page opens in read-only mode |
| 2 | All fields are displayed but NOT editable | No input fields, only display values |
| 3 | Click Back/Cancel | Returns to search results |

---

## 3. Export to Excel

### 3.1 Export Search Results
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Perform a search with results | Export button enabled |
| 2 | Click Export to Excel | .xlsx file downloads |
| 3 | Open file | Contains search results columns, only YOUR permitted documents |

### 3.2 Export Empty Results
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Search with no results | Export button disabled or shows warning |

---

## 4. Email Link Reception

### 4.1 Valid Link with Permission
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Receive email with document link | Link format: /documents/preview/{id} or /documents/{barcode} |
| 2 | Click link | Document properties page opens |
| 3 | Click Download PDF | PDF downloads to your computer |

### 4.2 Valid Link without Permission
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Receive email with link to document outside your permissions | N/A |
| 2 | Click link | Access denied message displayed |
| 3 | Page shows option to request access | Access request form visible |

---

## 5. Access Denied Scenarios

### 5.1 No User Record
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | User not in DocuScanUser table attempts login | Access denied page |
| 2 | Access request form available | Can submit request with reason |

### 5.2 Unauthorized Endpoint
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Manually navigate to /checkin-scanned | Access denied or redirect |
| 2 | Manually navigate to /admin/* pages | Access denied or redirect |

---

## Edge Cases

| Test | Action | Expected Result |
|------|--------|-----------------|
| Empty search | Click Search with no filters | Returns permitted documents (paginated) |
| Special characters | Search with barcode "12345" then "12345'" | No SQL injection, appropriate results |
| Large export | Export >1000 rows | Export completes or shows limit warning |
| Session timeout | Leave page idle, then search | Redirect to login or auto-refresh |
| Concurrent access | Two users search same document | Both see results independently |

---

## Not Permitted (Verify Access Denied)

| Action | Expected Result |
|--------|-----------------|
| Edit document properties | Edit button not visible or disabled |
| Delete document | Delete option not available |
| Register new document | Register menu/button not visible |
| Check-in files | Check-in menu not visible |
| View audit trail | Audit Trail menu not visible |
| Manage users | User Management not visible |

---

**Test Completed By:** _______________
**Date:** _______________
**Issues Found:** _______________
