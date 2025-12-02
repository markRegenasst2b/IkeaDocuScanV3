# IkeaDocuScan Test Cases - ADAdmin Role

**Test Profile:** adadmin
**Prerequisites:** User has ADAdmin role
**Note:** Reader and Publisher tests are not repeated. Run those first.

---

## 1. View User Permissions

### 1.1 View All Users
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to User Permissions | List of all DocuScan users displays |
| 2 | Each user shows permission count | Count column populated |
| 3 | Click on a user | User's permission details expand |
| 4 | View DocumentType permissions | List of allowed document types |

### 1.2 Filter Users
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Enter partial username in filter | Users filtered by name |
| 2 | Clear filter | All users shown |

### 1.3 Cannot Edit (View Only)
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | View user permissions | No Edit button visible |
| 2 | No Add User button | Button not present |
| 3 | No Delete button | Button not present |

---

## 2. View Configuration Data

### 2.1 View Email Templates
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Configuration > Email Templates | List of templates displays |
| 2 | Click on a template | Template content shown (read-only) |
visible |
| 3 | No Edit button | Cannot modify |

### 2.2 View Email Recipients
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Configuration > Email Recipients | Recipient groups display |
| 2 | View Access Request Notification   group | Email addresses listed |
| 3 | No Edit capability | Fields read-only and/or Save button missing/disabled |

### 2.3 SMTP Settings Hidden
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Configuration | SMTP section not visible or shows "Access Denied" |

---

## 3. View System Logs

### 3.1 Browse System Logs
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to System Logs | Log entries display |
| 2 | Filter by Timestamp | Entries within range |
| 3 | Filter by Level (Error, Warning, Info) | Filtered by severity |
| 4 | Filter by Source | Filtered by component |
| 5 | View log details | Full message and stack trace (if error) |

### 3.2 Log Entry Details
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Click on Error entry | Expanded view shows details |
| 2 | Stack trace visible (if applicable) | Technical details shown |
| 3 | No delete/clear option | Logs cannot be modified |

---

## 4. Delete Documents

### 4.1 Delete Single Document
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Search for a test document | Document found |
| 2 | Click Delete button | Confirmation dialog appears |
| 3 | Confirm deletion | Document deleted |
| 4 | Search for same barcode | No results |
| 5 | Check audit trail | Delete action logged |

---

## Edge Cases

| Test | Action | Expected Result |
|------|--------|-----------------|
| Delete then search | Delete, immediately search | Document not in results |
| Log volume | View logs with many entries | Pagination works |
| Log export | Export logs to file | Downloads successfully, csv can be opened in excel with one line per log entry (no missformatting due to newlines in the csv) |

---

## Not Permitted (Verify Access Denied)

| Action | Expected Result |
|--------|-----------------|
| Edit User Permissions | Edit buttons not visible |
| Add new user | Add User button not visible |
| Delete user | Delete User button not visible |
| Edit Email Templates | Edit button not visible |
| Edit Email Recipients | Edit capability disabled |
| View/Edit SMTP Settings | Section hidden or access denied |
| Edit reference data | Edit buttons not visible on metadata pages |

---

**Test Completed By:** _______________
**Date:** _______________
**Issues Found:** _______________
