# IkeaDocuScan Test Cases - SuperUser Role

**Test Profile:** superuser
**Prerequisites:** User has SuperUser role (IsSuperUser=true in database)
**Note:** Reader, Publisher, and ADAdmin tests are not repeated. Run those first.

---

## 1. Edit User Permissions

### 1.1 Create New User
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to User Permissions | User list displays |
| 2 | Click "Add User" | User creation form opens |
| 3 | Enter Account Name (DOMAIN\username) | Field accepts value |
| 4 | Set HasAccess = true | Checkbox checked |
| 5 | Leave IsSuperUser = false | Checkbox unchecked |
| 6 | Click Save | User created |
| 7 | User appears in list | New user visible |

### 1.2 Edit User Permissions
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Select existing user | User details load |
| 2 | Click Edit | Edit mode enabled |
| 3 | Add DocumentType permission | Select from dropdown |
| 4 | Save | Permission added |
| 5 | Remove a permission | Click remove/delete |
| 6 | Save | Permission removed |

### 1.3 Batch Update Permissions
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Select user | User selected |
| 2 | Click "Batch Update Document Types" | Multi-select dialog opens |
| 3 | Select multiple document types | Checkboxes selected |
| 4 | Save | All selected types added as permissions |
| 5 | Verify | User has all selected permissions |

### 1.4 Delete User
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Select user with permissions | User has permissions |
| 2 | Click Delete User | Confirmation: "Delete user and all permissions?" |
| 3 | Confirm | User deleted |
| 4 | Verify | User gone, all permissions cascade deleted |

### 1.5 Grant SuperUser
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Edit existing user | Edit form open |
| 2 | Set IsSuperUser = true | Checkbox checked |
| 3 | Save | User is now SuperUser |
| 4 | Verify with that user account | Can access all documents without permission filter |

---

## 2. Edit Metadata Definitions

### 2.1 Currencies - CRUD
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Currencies | List displays |
| 2 | Click Add | Creation form opens |
| 3 | Enter Code (e.g., "TST") and Name | Fields filled |
| 4 | Save | Currency created |
| 5 | Edit currency | Modify name |
| 6 | Save | Changes saved |
| 7 | Delete unused currency | Currency deleted |

### 2.2 Countries - CRUD
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Countries | List displays |
| 2 | Add new country (e.g., "XX", "Test Country") | Country created |
| 3 | Edit country name | Changes saved |
| 4 | Delete unused country | Country deleted |

### 2.3 Document Types - CRUD
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Document Types | List with usage counts |
| 2 | Add new Document Type | Type created |
| 3 | Edit description/name | Changes saved |
| 4 | Toggle Enabled flag | Status changes |
| 5 | Delete unused type | Type deleted |

### 2.4 CounterParties - CRUD
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to CounterParties | List displays |
| 2 | Add new CounterParty | Name, City, Country fields |
| 3 | Save | CounterParty created |
| 4 | Edit details | Changes saved |
| 5 | Delete unused | CounterParty deleted |

### 2.5 Document Names - CRUD
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Document Names | List displays |
| 2 | Add new Document Name | Select DocumentType, enter name |
| 3 | Save | Document Name created |
| 4 | Edit | Changes saved |
| 5 | Delete unused | Document Name deleted |

---

## 3. Referential Integrity

### 3.1 Cannot Delete Referenced Metadata
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Find DocumentType used by documents | Usage count > 0 |
| 2 | Attempt to delete | Error: "Cannot delete, X documents reference this type" |
| 3 | Find Currency used by documents | Usage count > 0 |
| 4 | Attempt to delete | Error: "Cannot delete, referenced by documents" |
| 5 | Find CounterParty used by documents | Has linked documents |
| 6 | Attempt to delete | Error: "Cannot delete, referenced" |

### 3.2 Usage Check Before Delete
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Click Delete on any metadata | System checks usage first |
| 2 | If usage = 0 | Delete proceeds |
| 3 | If usage > 0 | Delete blocked with count |

---

## 4. Edit Endpoint Access Rules

### 4.1 View Endpoint Registry
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Endpoint Authorization | List of endpoints |
| 2 | Each shows Method, Route, Roles | Columns populated |
| 3 | Filter by category | Endpoints filtered |

### 4.2 Add Role to Endpoint
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Select endpoint (e.g., GET /api/documents/) | Endpoint selected |
| 2 | View current roles | Shows Reader, Publisher, etc. |
| 3 | Add new role | Role added to list |
| 4 | Save | Permission saved |
| 5 | Test with user having that role | Access granted |

### 4.3 Remove Role from Endpoint
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Select endpoint | Endpoint selected |
| 2 | Remove a role | Role removed |
| 3 | Save | Permission removed |
| 4 | Test with user having only that role | Access denied |

### 4.4 Available Roles
| Role | Description |
|------|-------------|
| Reader | Read-only access |
| Publisher | Create/Edit access |
| ADAdmin | Administrative view access |
| SuperUser | Full access |

---

## 5. Edit Configuration Data

### 5.1 Edit Email Templates
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Email Templates | Template list |
| 2 | Select template | Content loads |
| 3 | Modify Subject | Text updated |
| 4 | Modify HtmlBody | HTML updated |
| 5 | Use placeholder (e.g., {BarCode}) | Placeholder in content |
| 6 | Preview | Renders with sample data |
| 7 | Save | Template saved |
| 8 | Test by triggering email | Uses updated template |

### 5.2 Edit Email Recipients
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Email Recipients | Recipient groups |
| 2 | Edit Admin group | Add/remove emails |
| 3 | Save | Changes saved |
| 4 | Trigger admin notification | Emails sent to updated list |

### 5.3 Edit SMTP Settings
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to SMTP Configuration | Settings form |
| 2 | View current Host/Port | Values displayed |
| 3 | Modify Host | Field editable |
| 4 | Modify Port | Field editable |
| 5 | Update Username/Password | Credentials updated |
| 6 | Click Test Connection | Test email sent |
| 7 | Success message | "Connection successful" |
| 8 | Save | Settings persisted |

### 5.4 Test SMTP
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Click "Test SMTP" button | Test initiated |
| 2 | Enter test recipient | Email field |
| 3 | Send test | Test email sent |
| 4 | Verify receipt | Email arrives |

---

## 6. View Audit Trail

### 6.1 Comprehensive Audit View
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Audit Trail | Full audit history |
| 2 | Filter by Barcode | Document history |
| 3 | Filter by User | User activity |
| 4 | Filter by Action Type | Specific actions |
| 5 | Filter by Date Range | Time-bounded |
| 6 | Export | Download audit data |

### 6.2 Audit Entry Types
| Action | Logged When |
|--------|-------------|
| Register | New document created |
| Edit | Document modified |
| CheckIn | PDF attached |
| Delete | Document deleted |
| SendLink | Email link sent |
| SendAttachment | Email attachment sent |

---

## 7. View System Log

### 7.1 Full Log Access
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to System Logs | All log entries |
| 2 | Filter by Level (Error) | Only errors shown |
| 3 | Filter by Level (Warning) | Only warnings |
| 4 | Filter by Source | Component-specific |
| 5 | View stack traces | Full error details |

---

## 8. View Access Audit

### 8.1 User-to-DocumentType Matrix
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Access Audit | Matrix view |
| 2 | Select a User | Shows DocumentTypes user can access |
| 3 | Select a DocumentType | Shows Users who can access it |
| 4 | Export matrix | Download access report |

---

## Edge Cases

| Test | Action | Expected Result |
|------|--------|-----------------|
| Delete yourself | SuperUser tries to delete own account | Prevented or warning |
| Remove own SuperUser | SuperUser removes own IsSuperUser flag | Warning: "You will lose admin access" |
| Invalid email template | Save template with broken placeholder | Validation error |
| SMTP test fails | Enter invalid SMTP settings | Error with details, rollback |
| Circular permission | Grant permission that creates loop | Handled gracefully |
| Empty endpoint roles | Remove all roles from endpoint | Warning or prevented |
| Mass permission update | Update 100+ permissions at once | Completes successfully |

---

## Security Tests (SuperUser Only)

| Test | Action | Expected Result |
|------|--------|-----------------|
| SQL Injection in user search | Enter "'; DROP TABLE--" | No SQL injection |
| XSS in template | Enter "<script>alert(1)</script>" | Script escaped |
| Path traversal in config | Enter "../../../etc/passwd" | Path blocked |
| Privilege escalation | Non-superuser tries /admin endpoints | 403 Forbidden |

---

**Test Completed By:** _______________
**Date:** _______________
**Issues Found:** _______________
