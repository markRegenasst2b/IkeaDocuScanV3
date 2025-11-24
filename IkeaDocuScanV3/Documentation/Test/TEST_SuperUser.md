# SuperUser Role Test Plan

**Test User:** superuser
**Role Level:** Full Administrative (highest privilege)
**Purpose:** Complete system control - all operations including destructive actions
**Prerequisites:** User has SuperUser flag in database OR SuperUser AD group

---

## Test Cases (SuperUser-Specific)

*Note: All Reader, Publisher, and ADAdmin tests should also pass for SuperUser*

### Document Deletion

#### SU-D01: Delete Document
| Step | Action | Expected |
|------|--------|----------|
| 1 | Select document to delete | Document selected |
| 2 | Click Delete button | Confirmation dialog appears |
| 3 | Confirm deletion | Document deleted, removed from list |
| 4 | Search for deleted barcode | Not found |
| 5 | Check audit trail | Delete action logged |

#### SU-D02: Delete Scanned File
| Step | Action | Expected |
|------|--------|----------|
| 1 | Navigate to Check-in Scanned | File list shown |
| 2 | Select file, click Delete | Confirmation dialog |
| 3 | Confirm deletion | File removed from scan folder |
| 4 | Verify file gone from disk | File no longer exists |

---

### User Management

#### SU-U01: Create New User
| Step | Action | Expected |
|------|--------|----------|
| 1 | Navigate to User Management | User list loads |
| 2 | Click Add User | Create form opens |
| 3 | Enter AccountName (DOMAIN\username) | Field accepts input |
| 4 | Save user | User created |
| 5 | Search for new user | User appears in list |

#### SU-U02: Assign User Permissions
| Step | Action | Expected |
|------|--------|----------|
| 1 | Select user | User details shown |
| 2 | Add DocumentType permission | Permission added |
| 3 | Add CounterParty permission | Permission added |
| 4 | Add Country permission | Permission added |
| 5 | Save | Permissions saved |
| 6 | Reload user | Permissions persisted |

#### SU-U03: Grant SuperUser Flag
| Step | Action | Expected |
|------|--------|----------|
| 1 | Select existing user | User details shown |
| 2 | Enable IsSuperUser checkbox | Checkbox toggles |
| 3 | Save | User updated |
| 4 | Verify in database | IsSuperUser = 1 |

#### SU-U04: Delete User
| Step | Action | Expected |
|------|--------|----------|
| 1 | Select user (not self) | User selected |
| 2 | Click Delete | Confirmation dialog |
| 3 | Confirm | User deleted |
| 4 | Search for user | Not found |

#### SU-U05: Delete User Permission
| Step | Action | Expected |
|------|--------|----------|
| 1 | Select user with permissions | Permissions listed |
| 2 | Delete specific permission | Permission removed |
| 3 | Save | Changes persisted |

---

### Endpoint Authorization Management

#### SU-E01: Modify Endpoint Roles
| Step | Action | Expected |
|------|--------|----------|
| 1 | Navigate to Endpoint Management | Endpoint list loads |
| 2 | Select endpoint (e.g., GET /api/documents/) | Endpoint details shown |
| 3 | Add role (e.g., Publisher) | Role added to list |
| 4 | Remove role | Role removed |
| 5 | Save with reason | Changes saved, audit logged |

#### SU-E02: Invalidate Authorization Cache
| Step | Action | Expected |
|------|--------|----------|
| 1 | Click Invalidate Cache | Confirmation dialog |
| 2 | Confirm | Cache cleared, success message |
| 3 | Verify cache rebuilt | Next request rebuilds cache |

#### SU-E03: Sync Endpoints from Code
| Step | Action | Expected |
|------|--------|----------|
| 1 | Click Sync Endpoints | Sync process runs |
| 2 | Verify new endpoints detected | Any new API routes added |
| 3 | Check for removed endpoints | Obsolete routes flagged |

---

### Reference Data Management

#### SU-R01: Manage Document Types
| Step | Action | Expected |
|------|--------|----------|
| 1 | Navigate to Document Type Admin | Type list loads |
| 2 | Click Add | Create form opens |
| 3 | Enter name, save | Type created |
| 4 | Edit existing type | Changes saved |
| 5 | Delete unused type | Type removed |
| 6 | Try delete type with documents | Error: in use |

#### SU-R02: Manage Counter Parties
| Step | Action | Expected |
|------|--------|----------|
| 1 | Navigate to CounterParty Admin | Party list loads |
| 2 | Create new counter party | Party created |
| 3 | Edit counter party | Changes saved |
| 4 | Delete unused party | Party removed |

#### SU-R03: Manage Countries
| Step | Action | Expected |
|------|--------|----------|
| 1 | Navigate to Country Admin | Country list loads |
| 2 | Add country (code, name) | Country created |
| 3 | Edit country | Changes saved |
| 4 | Delete unused country | Country removed |

#### SU-R04: Manage Currencies
| Step | Action | Expected |
|------|--------|----------|
| 1 | Navigate to Currency Admin | Currency list loads |
| 2 | Add currency | Currency created |
| 3 | Edit currency | Changes saved |
| 4 | Delete unused currency | Currency removed |

---

### Configuration Management

#### SU-C01: Email Template Management
| Step | Action | Expected |
|------|--------|----------|
| 1 | Navigate to Configuration | Config page loads |
| 2 | View email templates | Templates listed |
| 3 | Edit template content | Editor opens |
| 4 | Preview with sample data | Preview renders |
| 5 | Save template | Template saved |

#### SU-C02: Email Recipient Groups
| Step | Action | Expected |
|------|--------|----------|
| 1 | View recipient groups | Groups listed |
| 2 | Add recipient to group | Recipient added |
| 3 | Remove recipient | Recipient removed |
| 4 | Save changes | Changes persisted |

#### SU-C03: SMTP Configuration
| Step | Action | Expected |
|------|--------|----------|
| 1 | View SMTP settings | Current settings shown |
| 2 | Click Test Connection | Test email sent |
| 3 | Verify test received | Email arrives |

---

### System Logs

#### SU-L01: View Application Logs
| Step | Action | Expected |
|------|--------|----------|
| 1 | Navigate to Log Viewer | Log entries displayed |
| 2 | Filter by severity (Error) | Only errors shown |
| 3 | Filter by date range | Filtered results |
| 4 | Search by text | Matching entries |

#### SU-L02: Export Logs
| Step | Action | Expected |
|------|--------|----------|
| 1 | Apply filter criteria | Filtered view |
| 2 | Click Export CSV | CSV downloads |
| 3 | Click Export JSON | JSON downloads |
| 4 | Verify export content | Matches filter |

---

### Permission Bypass Verification

#### SU-B01: See All Documents
| Step | Action | Expected |
|------|--------|----------|
| 1 | Navigate to Documents | Full document list |
| 2 | Compare count with Reader user | SuperUser sees more/all |
| 3 | Access document outside normal permissions | Document accessible |

#### SU-B02: Access All Counter Parties
| Step | Action | Expected |
|------|--------|----------|
| 1 | View documents | All counter parties visible |
| 2 | No permission filtering | Full data access |

---

### Audit Trail Full Access

#### SU-A01: View All Audit Entries
| Step | Action | Expected |
|------|--------|----------|
| 1 | Navigate to Audit Trail | Full audit log |
| 2 | Search by any user | Results shown |
| 3 | View system-wide activity | All actions visible |
| 4 | Export audit data | Export works |

---

## Edge Cases

### EDGE-SU01: Cannot Delete Self
| Step | Action | Expected |
|------|--------|----------|
| 1 | Navigate to User Management | User list shown |
| 2 | Find own user account | Account listed |
| 3 | Attempt to delete self | Error: cannot delete own account |

### EDGE-SU02: Delete Referenced Data
| Step | Action | Expected |
|------|--------|----------|
| 1 | Try delete DocumentType with documents | Error: foreign key constraint |
| 2 | Try delete CounterParty with documents | Error: in use |
| 3 | Try delete Country with documents | Error: in use |

### EDGE-SU03: Restore Accidentally Removed Endpoint Role
| Step | Action | Expected |
|------|--------|----------|
| 1 | Remove all roles from critical endpoint | Warning shown |
| 2 | Confirm removal | Roles removed |
| 3 | Test endpoint access | Access denied for all |
| 4 | Re-add roles | Access restored |

---

## Sign-Off

| Test ID | Pass/Fail | Tester | Date | Notes |
|---------|-----------|--------|------|-------|
| SU-D01 | | | | |
| SU-D02 | | | | |
| SU-U01 | | | | |
| SU-U02 | | | | |
| SU-U03 | | | | |
| SU-U04 | | | | |
| SU-U05 | | | | |
| SU-E01 | | | | |
| SU-E02 | | | | |
| SU-E03 | | | | |
| SU-R01 | | | | |
| SU-R02 | | | | |
| SU-R03 | | | | |
| SU-R04 | | | | |
| SU-C01 | | | | |
| SU-C02 | | | | |
| SU-C03 | | | | |
| SU-L01 | | | | |
| SU-L02 | | | | |
| SU-B01 | | | | |
| SU-B02 | | | | |
| SU-A01 | | | | |
| EDGE-SU01 | | | | |
| EDGE-SU02 | | | | |
| EDGE-SU03 | | | | |
