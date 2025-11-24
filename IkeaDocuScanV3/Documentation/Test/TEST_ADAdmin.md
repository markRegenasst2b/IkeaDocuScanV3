# ADAdmin Role Test Plan

**Test User:** adadmin
**Role Level:** Read-Only Administrative (inherits Reader capabilities)
**Purpose:** Audit and oversight role - can view admin configurations but cannot modify
**Prerequisites:** User has ADAdmin AD group membership

---

## Test Cases (ADAdmin-Specific)

*Note: All Reader tests (DOC-R01 through PERM-R01) should also pass for ADAdmin*

### ADM-A01: View Endpoint Authorization List
| Step | Action | Expected |
|------|--------|----------|
| 1 | Navigate to Endpoint Management | Page loads (no access denied) |
| 2 | Verify endpoint list displays | All API endpoints listed |
| 3 | Filter by HTTP method (GET) | Filtered list shown |
| 4 | Filter by route pattern | Filtered results |
| 5 | Clear filters | Full list restored |

### ADM-A02: View Endpoint Role Assignments
| Step | Action | Expected |
|------|--------|----------|
| 1 | Select an endpoint from list | Endpoint details shown |
| 2 | View assigned roles | Roles displayed (Reader, Publisher, etc.) |
| 3 | Verify role badges visible | Visual indication of role assignments |

### ADM-A03: View Endpoint Authorization Audit Log
| Step | Action | Expected |
|------|--------|----------|
| 1 | Navigate to Endpoint Audit section | Audit log displayed |
| 2 | View recent permission changes | Timestamp, user, action, endpoint shown |
| 3 | Filter by date range | Filtered audit entries |

### ADM-A04: View User Permissions Overview
| Step | Action | Expected |
|------|--------|----------|
| 1 | Navigate to User Permissions page | User list loads |
| 2 | View user permission details | CounterParty, Country, DocType restrictions visible |
| 3 | Filter users by permission status | Filtered results |

### ADM-A05: View Available Roles
| Step | Action | Expected |
|------|--------|----------|
| 1 | Access roles reference | Role list displayed |
| 2 | Verify roles shown | Reader, Publisher, ADAdmin, SuperUser listed |

---

## Negative Tests (Must Fail)

### NEG-A01: Cannot Modify Endpoint Roles
| Step | Action | Expected |
|------|--------|----------|
| 1 | View endpoint details | Edit controls not visible OR disabled |
| 2 | If edit button visible, click it | Access denied / 403 |
| 3 | Look for "Save" or "Update" button | Not available |

### NEG-A02: Cannot Invalidate Authorization Cache
| Step | Action | Expected |
|------|--------|----------|
| 1 | Look for "Invalidate Cache" button | Not visible OR disabled |
| 2 | If visible, click it | Access denied / 403 |

### NEG-A03: Cannot Sync Endpoints
| Step | Action | Expected |
|------|--------|----------|
| 1 | Look for "Sync Endpoints" button | Not visible OR disabled |

### NEG-A04: Cannot Create/Edit Users
| Step | Action | Expected |
|------|--------|----------|
| 1 | Look for "Add User" button | Not visible OR disabled |
| 2 | View user, look for Edit | Not available |
| 3 | Look for Delete user option | Not available |

### NEG-A05: Cannot Create/Edit Documents
| Step | Action | Expected |
|------|--------|----------|
| 1 | Navigate to Documents | List view only |
| 2 | Look for Create button | Not visible OR disabled |
| 3 | View document, look for Edit | Not available |

### NEG-A06: Cannot Access Reference Data Admin
| Step | Action | Expected |
|------|--------|----------|
| 1 | Navigate to Document Type Admin | Access denied |
| 2 | Navigate to CounterParty Admin | Access denied |
| 3 | Navigate to Configuration Management | Access denied |

### NEG-A07: Cannot Access System Logs
| Step | Action | Expected |
|------|--------|----------|
| 1 | Navigate to Log Viewer | Access denied (SuperUser only) |

---

## Verification Tests

### VER-A01: Read-Only Confirmation
| Step | Action | Expected |
|------|--------|----------|
| 1 | Review all visible admin pages | No editable fields enabled |
| 2 | Check for any POST/PUT/DELETE actions | None available |
| 3 | Verify all data is display-only | Confirmed read-only |

### VER-A02: Audit Trail Access
| Step | Action | Expected |
|------|--------|----------|
| 1 | Search audit trail by barcode | Results displayed |
| 2 | Search by username | Results displayed |
| 3 | ADAdmin actions logged | Own access recorded in audit |

---

## Sign-Off

| Test ID | Pass/Fail | Tester | Date | Notes |
|---------|-----------|--------|------|-------|
| ADM-A01 | | | | |
| ADM-A02 | | | | |
| ADM-A03 | | | | |
| ADM-A04 | | | | |
| ADM-A05 | | | | |
| NEG-A01 | | | | |
| NEG-A02 | | | | |
| NEG-A03 | | | | |
| NEG-A04 | | | | |
| NEG-A05 | | | | |
| NEG-A06 | | | | |
| NEG-A07 | | | | |
| VER-A01 | | | | |
| VER-A02 | | | | |
