# Role Extension & Configurable Endpoint Authorization Implementation Plan

**Date:** 2025-11-17
**Version:** 1.0
**Project:** IkeaDocuScan-V3 Blazor Application
**Status:** Ready for Review and Approval

---

## Table of Contents

1. [Executive Summary](#1-executive-summary)
2. [Role Definitions](#2-role-definitions)
3. [Role/Permission Matrix](#3-rolepermission-matrix)
4. [Database Schema Changes](#4-database-schema-changes)
5. [8-Step Implementation Plan](#5-8-step-implementation-plan)
6. [Test Procedures](#6-test-procedures)
7. [Implementation Prompt](#7-implementation-prompt)

---

## 1. Executive Summary

### 1.1 Project Overview

This plan extends the IkeaDocuScan authorization system to:
1. **Add a new ADAdmin role** for user and permission management without full system access
2. **Make endpoint authorization configurable** via database instead of hard-coded in source code
3. **Provide audit trail** for all permission changes
4. **Enable dynamic authorization** without requiring code changes or redeployment

### 1.2 Current State

**Current Roles:**
- Reader: Read-only access to documents
- Publisher: Create, edit, and send documents
- SuperUser: Full administrative access

**Current Authorization:**
- Hard-coded in endpoint definitions using `.RequireAuthorization()` and `.RequireRole()`
- 126 API endpoints across 17 endpoint files
- Authorization changes require code modifications and redeployment

### 1.3 Proposed Changes

**New Role:**
- **ADAdmin**: User and permission management without system configuration access

**New System:**
- Database-driven endpoint authorization (EndpointRegistry, EndpointRolePermission tables)
- Dynamic policy provider that loads permissions from database
- Cached authorization decisions for performance
- Audit log for all permission changes
- Admin UI for managing endpoint permissions

### 1.4 Key Changes Summary

- **1 new AD role mapping**: ADAdmin (mapped to existing ADGroup.Builtin.SuperUser)
- **SuperUser becomes database-only**: No AD group, only database flag IsSuperUser=true
- **3 new database tables**: EndpointRegistry, EndpointRolePermission, PermissionChangeAuditLog
- **35 endpoints** require authorization updates (user permissions, logs, config, action reminders, navigation)
- **Dynamic authorization policy provider** replaces hard-coded authorization
- **Cache management service** for performance
- **Admin UI** for permission management
- **TestIdentityService** updates for ADAdmin role testing
- **NavMenu** updates for role-based visibility using endpoint permissions

---

## 2. Role Definitions

### 2.1 Complete Role Hierarchy

| Role | Purpose | Key Permissions | AD Group |
|------|---------|-----------------|----------|
| **Reader** | View-only access | View documents, search, export (filtered results) | ADGroup.Builtin.Reader |
| **Publisher** | Document management | All Reader + Create/Edit documents, Send emails | ADGroup.Builtin.Publisher |
| **ADAdmin** | User management | All Reader + Manage users, permissions, view logs | ADGroup.Builtin.SuperUser |
| **SuperUser** | System administration | All permissions + Delete, System configuration | NO AD GROUP, only database flag |

### 2.2 ADAdmin Role Details

**Purpose:** Delegate user and permission management without granting full system administrative access.

**Permissions:**
- ✅ View all users and permissions
- ✅ View audit logs
- ✅ View system logs (read-only)
- ❌ **Cannot** Manage user permissions
- ❌ **Cannot** Create, edit, delete users
- ❌ **Cannot** modify system configuration (SMTP, email templates, etc.)
- ❌ **Cannot** delete documents
- ❌ **Cannot** modify reference data (countries, currencies, document types)

**Use Case:** Department managers or HR personnel who need to VIEW user access and logs without full system control.

**IMPORTANT:** ADAdmin is **read-only** for user permissions. Only SuperUser can create/edit/delete users and permissions.

### 2.3 Configuration Addition

Add to `IkeaDocuScanOptions.cs`:
```csharp
/// <summary>
/// Active Directory group for ADAdmin role (maps to existing SuperUser AD group)
/// </summary>
public string? ADGroupADAdmin { get; set; } = "ADGroup.Builtin.SuperUser";
```

Update `appsettings.json` to **reuse** existing SuperUser AD group:
```json
{
  "IkeaDocuScan": {
    "ADGroupReader": "ADGroup.Builtin.Reader",
    "ADGroupPublisher": "ADGroup.Builtin.Publisher",
    "ADGroupADAdmin": "ADGroup.Builtin.SuperUser"
  }
}
```

**Key Design Decision:**
- **ADAdmin** role is assigned via AD group membership (ADGroup.Builtin.SuperUser)
- **SuperUser** role is assigned via database flag (DocuScanUser.IsSuperUser = true)
- Users in the SuperUser AD group automatically get **ADAdmin** role claim
- Only users with database flag IsSuperUser=true get **SuperUser** role claim
- This allows IT administrators to delegate read-only administrative tasks without granting full system control

---

## 3. Role/Permission Matrix

### 3.1 Matrix Overview

**Total Endpoints:** 126 (excluding 4 DEBUG-only test identity endpoints)
**Endpoints Requiring Changes:** 86 (68.3%)
- 11 User Permission endpoints (2 GET self-access for all roles, 2 GET ADAdmin read-only added, POST/PUT/DELETE SuperUser only)
- 3 Action Reminder endpoints (Reader access removed - Publisher+ only)
- 5 Log Viewer endpoints (ADAdmin read-only added)
- 5 Configuration GET endpoints (ADAdmin read-only added)
- 7 Counter Party endpoints (Reader access removed from 4 GET endpoints)
- 6 Scanned Files endpoints (Reader access removed from 5 GET endpoints)
- 14 Report endpoints (Reader access removed from all)
- 6 Country endpoints (Reader access removed from 3 GET endpoints)
- 6 Currency endpoints (Reader access removed from 3 GET endpoints)
- 7 Document Type endpoints (Reader access removed from 4 GET endpoints)
- 6 Document Name endpoints (Reader access removed from 3 GET endpoints)
- 10 new Endpoint Authorization endpoints (SuperUser only, 1 check endpoint for all roles)

**Endpoints Unchanged:** 40 (31.7%)
- Documents: 10 endpoints (unchanged authorization)
- Audit Trail: 7 endpoints (unchanged)
- Excel Export: 4 endpoints (unchanged)
- Email: 3 endpoints (unchanged)
- User Identity: 1 endpoint (unchanged)
- Configuration (Write): 15 endpoints (SuperUser only - POST/PUT/DELETE and 3 SuperUser-only GET endpoints unchanged)

### 3.2 Endpoints Requiring Authorization Updates

#### 3.2.1 User Permission Management (11 endpoints)

**Current:** SuperUser only for all operations
**Proposed:** Differentiated by operation type

| HTTP | Endpoint | Current Auth | Proposed Auth | Change Type | Reason |
|------|----------|--------------|---------------|-------------|--------|
| GET | `/api/userpermissions/` | SuperUser | ADAdmin, SuperUser | Add ADAdmin (read-only) | View all users |
| GET | `/api/userpermissions/users` | SuperUser | ADAdmin, SuperUser | Add ADAdmin (read-only) | View all users |
| GET | `/api/userpermissions/{id}` | SuperUser | **All Roles** | Add Reader, Publisher, ADAdmin | Users need to see their own permissions for document filtering |
| GET | `/api/userpermissions/user/{userId}` | SuperUser | **All Roles** | Add Reader, Publisher, ADAdmin | Users need to see their own permissions for document filtering |
| GET | `/api/userpermissions/me` | HasAccess | **All Roles** (unchanged) | No change | Already accessible to all |
| POST | `/api/userpermissions/` | SuperUser | SuperUser only | No change | Create permission |
| PUT | `/api/userpermissions/{id}` | SuperUser | SuperUser only | No change | Update permission |
| DELETE | `/api/userpermissions/{id}` | SuperUser | SuperUser only | No change | Delete permission |
| DELETE | `/api/userpermissions/user/{userId}` | SuperUser | SuperUser only | No change | Delete user |
| POST | `/api/userpermissions/user` | SuperUser | SuperUser only | No change | Create user |
| PUT | `/api/userpermissions/user/{userId}` | SuperUser | SuperUser only | No change | Update user |

**Key Changes:**
- **GET `/api/userpermissions/{id}`** and **GET `/api/userpermissions/user/{userId}`**: Now accessible to **ALL roles** (Reader, Publisher, ADAdmin, SuperUser)
  - **Reason:** All users need to query their own permissions to understand document filtering
  - **Implementation:** Add service-layer check to ensure users can only access their own permissions (userId matches current user), unless they are ADAdmin/SuperUser
- **GET `/api/userpermissions/`** and **GET `/api/userpermissions/users`**: ADAdmin + SuperUser (read-only)
- **POST/PUT/DELETE operations**: SuperUser only (no changes)

**CRITICAL SERVICE-LAYER SECURITY REQUIREMENT:**

For `GET /api/userpermissions/{id}` and `GET /api/userpermissions/user/{userId}`, the service layer MUST enforce:

```csharp
public async Task<UserPermissionDto> GetUserPermissionByIdAsync(int id)
{
    var currentUser = await _currentUserService.GetCurrentUserAsync();
    var permission = await _dbContext.UserPermissions.FindAsync(id);

    if (permission == null)
        throw new NotFoundException("Permission not found");

    // Security check: Users can only view their own permissions
    // ADAdmin and SuperUser can view all permissions
    if (!currentUser.IsSuperUser &&
        !currentUser.IsInRole("ADAdmin") &&
        permission.UserId != currentUser.UserId)
    {
        throw new UnauthorizedAccessException("You can only access your own permissions");
    }

    return MapToDto(permission);
}
```

**This prevents users from accessing other users' permissions even though the endpoint is accessible to all roles.**

#### 3.2.2 Action Reminder Endpoints (3 endpoints)

**Current:** HasAccess (all roles)
**Proposed:** Publisher + ADAdmin + SuperUser only (remove Reader access)

| HTTP | Endpoint | Current Auth | Proposed Auth | Change Type | Reason |
|------|----------|--------------|---------------|-------------|--------|
| GET | `/api/action-reminders/` | HasAccess | Publisher, ADAdmin, SuperUser | Remove Reader | Action reminders are for active document management |
| GET | `/api/action-reminders/count` | HasAccess | Publisher, ADAdmin, SuperUser | Remove Reader | Action reminders are for active document management |
| GET | `/api/action-reminders/date/{date}` | HasAccess | Publisher, ADAdmin, SuperUser | Remove Reader | Action reminders are for active document management |

**Rationale:** Reader role is view-only and should not have access to action reminders, which are intended for users who actively manage and process documents.

#### 3.2.3 Log Viewer Endpoints (5 endpoints)

**Current:** SuperUser only
**Proposed:** ADAdmin + SuperUser (read-only access)

| HTTP | Endpoint | Current Auth | Proposed Auth | Change Type |
|------|----------|--------------|---------------|-------------|
| POST | `/api/logs/search` | SuperUser | ADAdmin, SuperUser | Add ADAdmin |
| GET | `/api/logs/export` | SuperUser | ADAdmin, SuperUser | Add ADAdmin |
| GET | `/api/logs/dates` | SuperUser | ADAdmin, SuperUser | Add ADAdmin |
| GET | `/api/logs/sources` | SuperUser | ADAdmin, SuperUser | Add ADAdmin |
| GET | `/api/logs/statistics` | SuperUser | ADAdmin, SuperUser | Add ADAdmin |

#### 3.2.4 Configuration Endpoints - Read-Only (5 endpoints)

**Current:** SuperUser only
**Proposed:** ADAdmin + SuperUser (read access only)

| HTTP | Endpoint | Current Auth | Proposed Auth | Change Type |
|------|----------|--------------|---------------|-------------|
| GET | `/api/configuration/email-recipients` | SuperUser | ADAdmin, SuperUser | Add ADAdmin (read) |
| GET | `/api/configuration/email-recipients/{groupKey}` | SuperUser | ADAdmin, SuperUser | Add ADAdmin (read) |
| GET | `/api/configuration/email-templates` | SuperUser | ADAdmin, SuperUser | Add ADAdmin (read) |
| GET | `/api/configuration/email-templates/{key}` | SuperUser | ADAdmin, SuperUser | Add ADAdmin (read) |
| GET | `/api/configuration/sections` | SuperUser | ADAdmin, SuperUser | Add ADAdmin (read) |

**Note:** POST/PUT/DELETE configuration endpoints remain **SuperUser only**.

#### 3.2.5 New Endpoint Authorization Management (10 endpoints - NEW)

**Purpose:** Admin UI for managing endpoint permissions dynamically

| HTTP | Endpoint | Proposed Auth | Description |
|------|----------|---------------|-------------|
| GET | `/api/endpoint-authorization/endpoints` | SuperUser | Get all endpoints in registry |
| GET | `/api/endpoint-authorization/endpoints/{id}` | SuperUser | Get endpoint details |
| GET | `/api/endpoint-authorization/endpoints/{id}/roles` | SuperUser | Get roles for endpoint |
| POST | `/api/endpoint-authorization/endpoints/{id}/roles` | SuperUser | Update roles for endpoint |
| GET | `/api/endpoint-authorization/roles` | SuperUser | Get all available roles |
| GET | `/api/endpoint-authorization/audit` | SuperUser | Get permission change audit log |
| POST | `/api/endpoint-authorization/cache/invalidate` | SuperUser | Invalidate authorization cache |
| POST | `/api/endpoint-authorization/sync` | SuperUser | Sync endpoints from code to database |
| GET | `/api/endpoint-authorization/check` | **All Roles** | Check if user can access specific endpoint (for NavMenu visibility) |
| POST | `/api/endpoint-authorization/validate` | SuperUser | Validate permission changes before applying |

**Note:** The `/check` endpoint is accessible to all authenticated users and is used by NavMenu.razor to determine menu item visibility based on role permissions.

#### 3.2.6 Navigation Menu Updates (Component Changes - NEW)

**Purpose:** Show/hide menu items based on role permissions using endpoint authorization

**Component:** `IkeaDocuScan-Web.Client/Layout/NavMenu.razor`

**Changes Required:**
- Each menu item must be assigned a **representative endpoint**
- Menu items call `/api/endpoint-authorization/check?endpoint={endpointRoute}&method={httpMethod}` to determine visibility
- Response indicates whether current user's roles can access that endpoint
- Menu items are rendered conditionally based on response

**Menu Item to Endpoint Mapping:**

| Menu Item | Representative Endpoint | Visible To |
|-----------|------------------------|------------|
| Home | N/A | All authenticated users |
| Documents | `GET /api/documents/` | Reader, Publisher, ADAdmin, SuperUser |
| Scanned Files | `GET /api/scannedfiles/` | **Publisher, ADAdmin, SuperUser** (NOT Reader) |
| Action Reminders | `GET /api/action-reminders/` | **Publisher, ADAdmin, SuperUser** (NOT Reader) |
| Reports | `GET /api/reports/barcode-gaps` | **Publisher, ADAdmin, SuperUser** (NOT Reader) |
| Counter Parties | `GET /api/counterparties/` | **Publisher, ADAdmin, SuperUser** (NOT Reader) |
| Reference Data | `GET /api/countries/` | **Publisher, ADAdmin, SuperUser** (NOT Reader) |
| User Permissions | `GET /api/userpermissions/users` | ADAdmin, SuperUser |
| Logs | `GET /api/logs/search` | ADAdmin, SuperUser |
| Configuration | `GET /api/configuration/sections` | ADAdmin (read), SuperUser |
| Endpoint Authorization | `GET /api/endpoint-authorization/endpoints` | SuperUser only |

**Implementation Details:** See Step 8 for detailed NavMenu.razor modifications.

### 3.3 Endpoints Unchanged (40 endpoints)

The following endpoint categories remain unchanged with their current authorization policies:

| Category | Count | Authorization Pattern |
|----------|-------|----------------------|
| **Documents** | 10 | GET (all roles), POST/PUT (Publisher+), DELETE (SuperUser) |
| **Audit Trail** | 7 | Publisher, ADAdmin, SuperUser |
| **Excel Export** | 4 | All roles (HasAccess) |
| **Email Operations** | 3 | Publisher, ADAdmin, SuperUser |
| **User Identity** | 1 | All authenticated users |
| **Configuration (Write)** | 15 | SuperUser only (POST/PUT/DELETE and 3 SuperUser-only GET endpoints) |

**Total Unchanged:** 40 endpoints (31.7%)

**Rationale for "Unchanged":**
- These endpoints already have the correct authorization for the new role structure
- Document operations already properly restrict write/delete operations to Publisher+ and SuperUser
- Audit Trail, Excel Export, and Email already exclude Reader role appropriately
- Configuration write operations (POST/PUT/DELETE) remain SuperUser-only
- User Identity endpoint remains accessible to all authenticated users

**Note:** The following categories are CHANGED (see Section 3.2 and 3.4 for details):
- Counter Party Operations (7 endpoints) - Reader access removed
- Scanned Files (6 endpoints) - Reader access removed
- Reports (14 endpoints) - Reader access removed
- Reference Data: Countries (6), Currencies (6), Document Types (7), Document Names (6) - Reader access removed
- User Permissions (11 endpoints) - ADAdmin read access added, self-access added for all roles
- Configuration (5 GET endpoints) - ADAdmin read access added
- Log Viewer (5 endpoints) - ADAdmin access added
- Action Reminders (3 endpoints) - Reader access removed

### 3.4 Full Endpoint Matrix by Category

<details>
<summary><b>Click to expand: Complete 126-endpoint authorization matrix</b></summary>

#### Documents (10 endpoints)
| HTTP | Endpoint | Reader | Publisher | ADAdmin | SuperUser |
|------|----------|--------|-----------|---------|-----------|
| GET | `/api/documents/` | ✅ | ✅ | ✅ | ✅ |
| GET | `/api/documents/{id}` | ✅ | ✅ | ✅ | ✅ |
| GET | `/api/documents/barcode/{barCode}` | ✅ | ✅ | ✅ | ✅ |
| POST | `/api/documents/by-ids` | ✅ | ✅ | ✅ | ✅ |
| POST | `/api/documents/` | ❌ | ✅ | ✅ | ✅ |
| PUT | `/api/documents/{id}` | ❌ | ✅ | ✅ | ✅ |
| DELETE | `/api/documents/{id}` | ❌ | ❌ | ❌ | ✅ |
| POST | `/api/documents/search` | ✅ | ✅ | ✅ | ✅ |
| GET | `/api/documents/{id}/stream` | ✅ | ✅ | ✅ | ✅ |
| GET | `/api/documents/{id}/download` | ✅ | ✅ | ✅ | ✅ |

#### Counter Parties (7 endpoints) - **CHANGES REQUIRED**
| HTTP | Endpoint | Reader | Publisher | ADAdmin | SuperUser |
|------|----------|--------|-----------|---------|-----------|
| GET | `/api/counterparties/` | ❌ **REMOVED** | ✅ | ✅ | ✅ |
| GET | `/api/counterparties/search` | ❌ **REMOVED** | ✅ | ✅ | ✅ |
| GET | `/api/counterparties/{id}` | ❌ **REMOVED** | ✅ | ✅ | ✅ |
| POST | `/api/counterparties/` | ❌ | ❌ | ❌ | ✅ |
| PUT | `/api/counterparties/{id}` | ❌ | ❌ | ❌ | ✅ |
| DELETE | `/api/counterparties/{id}` | ❌ | ❌ | ❌ | ✅ |
| GET | `/api/counterparties/{id}/usage` | ❌ **REMOVED** | ✅ | ✅ | ✅ |

**Note:** Reader access removed from all GET endpoints - now Publisher+ only.

#### User Permissions (11 endpoints) - **CHANGES REQUIRED**
| HTTP | Endpoint | Reader | Publisher | ADAdmin | SuperUser |
|------|----------|--------|-----------|---------|-----------|
| GET | `/api/userpermissions/` | ❌ | ❌ | ✅ **NEW** | ✅ |
| GET | `/api/userpermissions/users` | ❌ | ❌ | ✅ **NEW** | ✅ |
| GET | `/api/userpermissions/{id}` | ✅ **NEW** | ✅ **NEW** | ✅ **NEW** | ✅ |
| GET | `/api/userpermissions/user/{userId}` | ✅ **NEW** | ✅ **NEW** | ✅ **NEW** | ✅ |
| GET | `/api/userpermissions/me` | ✅ | ✅ | ✅ | ✅ |
| POST | `/api/userpermissions/` | ❌ | ❌ | ❌ | ✅ |
| PUT | `/api/userpermissions/{id}` | ❌ | ❌ | ❌ | ✅ |
| DELETE | `/api/userpermissions/{id}` | ❌ | ❌ | ❌ | ✅ |
| DELETE | `/api/userpermissions/user/{userId}` | ❌ | ❌ | ❌ | ✅ |
| POST | `/api/userpermissions/user` | ❌ | ❌ | ❌ | ✅ |
| PUT | `/api/userpermissions/user/{userId}` | ❌ | ❌ | ❌ | ✅ |

**Note:**
- **Self-access GET endpoints** (lines 3-4): All roles can access, but service layer enforces users can only view their own permissions unless ADAdmin/SuperUser
- **View all endpoints** (lines 1-2): ADAdmin and SuperUser can view all users/permissions (read-only for ADAdmin)
- **Write operations** (lines 6-11): SuperUser only - ADAdmin is read-only

#### Configuration (19 endpoints) - **5 CHANGES FOR READ ACCESS**
| HTTP | Endpoint | Reader | Publisher | ADAdmin | SuperUser |
|------|----------|--------|-----------|---------|-----------|
| GET | `/api/configuration/email-recipients` | ❌ | ❌ | ✅ **NEW** | ✅ |
| GET | `/api/configuration/email-recipients/{groupKey}` | ❌ | ❌ | ✅ **NEW** | ✅ |
| POST | `/api/configuration/email-recipients/{groupKey}` | ❌ | ❌ | ❌ | ✅ |
| GET | `/api/configuration/email-templates` | ❌ | ❌ | ✅ **NEW** | ✅ |
| GET | `/api/configuration/email-templates/{key}` | ❌ | ❌ | ✅ **NEW** | ✅ |
| POST | `/api/configuration/email-templates` | ❌ | ❌ | ❌ | ✅ |
| PUT | `/api/configuration/email-templates/{id}` | ❌ | ❌ | ❌ | ✅ |
| DELETE | `/api/configuration/email-templates/{id}` | ❌ | ❌ | ❌ | ✅ |
| GET | `/api/configuration/sections` | ❌ | ❌ | ✅ **NEW** | ✅ |
| GET | `/api/configuration/{section}/{key}` | ❌ | ❌ | ❌ | ✅ |
| POST | `/api/configuration/{section}/{key}` | ❌ | ❌ | ❌ | ✅ |
| POST | `/api/configuration/smtp` | ❌ | ❌ | ❌ | ✅ |
| POST | `/api/configuration/test-smtp` | ❌ | ❌ | ❌ | ✅ |
| POST | `/api/configuration/reload` | ❌ | ❌ | ❌ | ✅ |
| POST | `/api/configuration/migrate` | ❌ | ❌ | ❌ | ✅ |
| POST | `/api/configuration/email-templates/preview` | ❌ | ❌ | ❌ | ✅ |
| GET | `/api/configuration/email-templates/placeholders` | ❌ | ❌ | ❌ | ✅ |
| GET | `/api/configuration/email-templates/diagnostic/DocumentAttachment` | ❌ | ❌ | ❌ | ✅ |

#### Log Viewer (5 endpoints) - **CHANGES REQUIRED**
| HTTP | Endpoint | Reader | Publisher | ADAdmin | SuperUser |
|------|----------|--------|-----------|---------|-----------|
| POST | `/api/logs/search` | ❌ | ❌ | ✅ **NEW** | ✅ |
| GET | `/api/logs/export` | ❌ | ❌ | ✅ **NEW** | ✅ |
| GET | `/api/logs/dates` | ❌ | ❌ | ✅ **NEW** | ✅ |
| GET | `/api/logs/sources` | ❌ | ❌ | ✅ **NEW** | ✅ |
| GET | `/api/logs/statistics` | ❌ | ❌ | ✅ **NEW** | ✅ |

#### Scanned Files (6 endpoints) - **CHANGES REQUIRED**
| HTTP | Endpoint | Reader | Publisher | ADAdmin | SuperUser |
|------|----------|--------|-----------|---------|-----------|
| GET | `/api/scannedfiles/` | ❌ **REMOVED** | ✅ | ✅ | ✅ |
| GET | `/api/scannedfiles/{fileName}` | ❌ **REMOVED** | ✅ | ✅ | ✅ |
| GET | `/api/scannedfiles/{fileName}/content` | ❌ **REMOVED** | ✅ | ✅ | ✅ |
| GET | `/api/scannedfiles/{fileName}/exists` | ❌ **REMOVED** | ✅ | ✅ | ✅ |
| GET | `/api/scannedfiles/{fileName}/stream` | ❌ **REMOVED** | ✅ | ✅ | ✅ |
| DELETE | `/api/scannedfiles/{fileName}` | ❌ | ❌ | ❌ | ✅ |

**Note:** Reader access removed from all GET endpoints - now Publisher+ only.

#### Reports (14 endpoints) - **CHANGES REQUIRED**
| HTTP | Endpoint | Reader | Publisher | ADAdmin | SuperUser |
|------|----------|--------|-----------|---------|-----------|
| GET | `/api/reports/barcode-gaps` | ❌ **REMOVED** | ✅ | ✅ | ✅ |
| GET | `/api/reports/duplicate-documents` | ❌ **REMOVED** | ✅ | ✅ | ✅ |
| GET | `/api/reports/unlinked-registrations` | ❌ **REMOVED** | ✅ | ✅ | ✅ |
| GET | `/api/reports/scan-copies` | ❌ **REMOVED** | ✅ | ✅ | ✅ |
| GET | `/api/reports/suppliers` | ❌ **REMOVED** | ✅ | ✅ | ✅ |
| GET | `/api/reports/all-documents` | ❌ **REMOVED** | ✅ | ✅ | ✅ |
| GET | `/api/reports/barcode-gaps/excel` | ❌ **REMOVED** | ✅ | ✅ | ✅ |
| GET | `/api/reports/duplicate-documents/excel` | ❌ **REMOVED** | ✅ | ✅ | ✅ |
| GET | `/api/reports/unlinked-registrations/excel` | ❌ **REMOVED** | ✅ | ✅ | ✅ |
| GET | `/api/reports/scan-copies/excel` | ❌ **REMOVED** | ✅ | ✅ | ✅ |
| GET | `/api/reports/suppliers/excel` | ❌ **REMOVED** | ✅ | ✅ | ✅ |
| GET | `/api/reports/all-documents/excel` | ❌ **REMOVED** | ✅ | ✅ | ✅ |
| POST | `/api/reports/documents/search/excel` | ❌ **REMOVED** | ✅ | ✅ | ✅ |
| POST | `/api/reports/documents/selected/excel` | ❌ **REMOVED** | ✅ | ✅ | ✅ |

**Note:** Reader access removed from all endpoints - now Publisher+ only.

#### Countries (6 endpoints) - **CHANGES REQUIRED**
| HTTP | Endpoint | Reader | Publisher | ADAdmin | SuperUser |
|------|----------|--------|-----------|---------|-----------|
| GET | `/api/countries/` | ❌ **REMOVED** | ✅ | ✅ | ✅ |
| GET | `/api/countries/{code}` | ❌ **REMOVED** | ✅ | ✅ | ✅ |
| POST | `/api/countries/` | ❌ | ❌ | ❌ | ✅ |
| PUT | `/api/countries/{code}` | ❌ | ❌ | ❌ | ✅ |
| DELETE | `/api/countries/{code}` | ❌ | ❌ | ❌ | ✅ |
| GET | `/api/countries/{code}/usage` | ❌ **REMOVED** | ✅ | ✅ | ✅ |

**Note:** Reader access removed from all GET endpoints - now Publisher+ only.

#### Currencies (6 endpoints) - **CHANGES REQUIRED**
| HTTP | Endpoint | Reader | Publisher | ADAdmin | SuperUser |
|------|----------|--------|-----------|---------|-----------|
| GET | `/api/currencies/` | ❌ **REMOVED** | ✅ | ✅ | ✅ |
| GET | `/api/currencies/{code}` | ❌ **REMOVED** | ✅ | ✅ | ✅ |
| POST | `/api/currencies/` | ❌ | ❌ | ❌ | ✅ |
| PUT | `/api/currencies/{code}` | ❌ | ❌ | ❌ | ✅ |
| DELETE | `/api/currencies/{code}` | ❌ | ❌ | ❌ | ✅ |
| GET | `/api/currencies/{code}/usage` | ❌ **REMOVED** | ✅ | ✅ | ✅ |

**Note:** Reader access removed from all GET endpoints - now Publisher+ only.

#### Document Types (7 endpoints) - **CHANGES REQUIRED**
| HTTP | Endpoint | Reader | Publisher | ADAdmin | SuperUser |
|------|----------|--------|-----------|---------|-----------|
| GET | `/api/documenttypes/` | ❌ **REMOVED** | ✅ | ✅ | ✅ |
| GET | `/api/documenttypes/all` | ❌ **REMOVED** | ✅ | ✅ | ✅ |
| GET | `/api/documenttypes/{id}` | ❌ **REMOVED** | ✅ | ✅ | ✅ |
| POST | `/api/documenttypes/` | ❌ | ❌ | ❌ | ✅ |
| PUT | `/api/documenttypes/{id}` | ❌ | ❌ | ❌ | ✅ |
| DELETE | `/api/documenttypes/{id}` | ❌ | ❌ | ❌ | ✅ |
| GET | `/api/documenttypes/{id}/usage` | ❌ **REMOVED** | ✅ | ✅ | ✅ |

**Note:** Reader access removed from all GET endpoints - now Publisher+ only.

#### Document Names (6 endpoints) - **CHANGES REQUIRED**
| HTTP | Endpoint | Reader | Publisher | ADAdmin | SuperUser |
|------|----------|--------|-----------|---------|-----------|
| GET | `/api/documentnames/` | ❌ **REMOVED** | ✅ | ✅ | ✅ |
| GET | `/api/documentnames/bytype/{documentTypeId}` | ❌ **REMOVED** | ✅ | ✅ | ✅ |
| GET | `/api/documentnames/{id}` | ❌ **REMOVED** | ✅ | ✅ | ✅ |
| POST | `/api/documentnames/` | ❌ | ❌ | ❌ | ✅ |
| PUT | `/api/documentnames/{id}` | ❌ | ❌ | ❌ | ✅ |
| DELETE | `/api/documentnames/{id}` | ❌ | ❌ | ❌ | ✅ |

**Note:** Reader access removed from all GET endpoints - now Publisher+ only.

#### Endpoint Authorization (10 endpoints) - **NEW**
| HTTP | Endpoint | Reader | Publisher | ADAdmin | SuperUser |
|------|----------|--------|-----------|---------|-----------|
| GET | `/api/endpoint-authorization/endpoints` | ❌ | ❌ | ❌ | ✅ |
| GET | `/api/endpoint-authorization/endpoints/{id}` | ❌ | ❌ | ❌ | ✅ |
| GET | `/api/endpoint-authorization/endpoints/{id}/roles` | ❌ | ❌ | ❌ | ✅ |
| POST | `/api/endpoint-authorization/endpoints/{id}/roles` | ❌ | ❌ | ❌ | ✅ |
| GET | `/api/endpoint-authorization/roles` | ❌ | ❌ | ❌ | ✅ |
| GET | `/api/endpoint-authorization/audit` | ❌ | ❌ | ❌ | ✅ |
| POST | `/api/endpoint-authorization/cache/invalidate` | ❌ | ❌ | ❌ | ✅ |
| POST | `/api/endpoint-authorization/sync` | ❌ | ❌ | ❌ | ✅ |
| GET | `/api/endpoint-authorization/check` | ✅ | ✅ | ✅ | ✅ |
| POST | `/api/endpoint-authorization/validate` | ❌ | ❌ | ❌ | ✅ |

*Other categories (Audit Trail, Excel Export, Email, User Identity) remain unchanged with HasAccess for all roles.*

</details>

### 3.5 Summary Statistics

| Metric | Value |
|--------|-------|
| Total Endpoints (Production) | 126 |
| Endpoints Requiring Changes | 86 (68.3%) |
| Endpoints Unchanged | 40 (31.7%) |
| New Endpoints to Create | 10 |
| **Reader Access** | 22 endpoints |
| **Publisher Access** | 66 endpoints |
| **ADAdmin Access** | 78 endpoints |
| **SuperUser Access** | 126 endpoints (all) |

**Reader Endpoint Breakdown (22 total):**
- Documents: 6 GET endpoints
- Audit Trail: 7 endpoints
- Excel Export: 4 endpoints
- User Permissions: 3 endpoints (GET /me + 2 self-access GET endpoints)
- User Identity: 1 endpoint
- Endpoint Authorization: 1 endpoint (GET /check)

**Publisher Endpoint Breakdown (66 total):**
- All Reader endpoints: 22
- Document write operations: 2 (POST, PUT)
- Action Reminders: 3
- Email: 3
- Scanned Files: 5 GET endpoints
- Reports: 14 endpoints
- Counter Parties: 4 GET endpoints
- Countries: 3 GET endpoints
- Currencies: 3 GET endpoints
- Document Types: 4 GET endpoints
- Document Names: 3 GET endpoints

**ADAdmin Endpoint Breakdown (78 total):**
- All Publisher endpoints: 66
- User Permissions (view all): 2 GET endpoints
- Log Viewer: 5 endpoints
- Configuration (read-only): 5 GET endpoints

**Key Authorization Changes:**
- **Reader role significantly restricted** - lost access to 40+ endpoints (Scanned Files, Reports, Counter Parties, Reference Data categories)
- **Reader retains document viewing only** - primary use case is viewing and searching documents with Excel export
- **2 User Permission endpoints** now accessible to ALL roles (for self-access with service-layer security): `GET /api/userpermissions/{id}` and `GET /api/userpermissions/user/{userId}`
- **Service-layer security** enforces users can only access their own permissions unless ADAdmin/SuperUser
- **Action Reminders** removed from Reader role (now Publisher+ only)
- **ADAdmin** is read-only for user permissions, logs, and configuration - no write access
- **SuperUser** becomes database-only flag (no longer assigned via AD group)

---

## 4. Database Schema Changes

### 4.1 Overview

Three new tables will be added to support dynamic endpoint authorization:

1. **EndpointRegistry**: Catalog of all API endpoints
2. **EndpointRolePermission**: Role-to-endpoint mappings
3. **PermissionChangeAuditLog**: Audit trail for permission changes

**IMPORTANT:** These changes will be applied via **manual SQL scripts**, NOT EF Core migrations, to avoid disrupting the existing migration history.

### 4.2 SQL Scripts

#### 4.2.1 Create EndpointRegistry Table

```sql
-- =============================================
-- Script: 01_Create_EndpointRegistry_Table.sql
-- Description: Creates the EndpointRegistry table for cataloging all API endpoints
-- Date: 2025-11-17
-- =============================================

USE [IkeaDocuScan]
GO

-- Drop table if exists (for development/testing only)
-- IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[EndpointRegistry]') AND type in (N'U'))
-- DROP TABLE [dbo].[EndpointRegistry]
-- GO

CREATE TABLE [dbo].[EndpointRegistry](
    [EndpointId] INT IDENTITY(1,1) NOT NULL,
    [HttpMethod] VARCHAR(10) NOT NULL,
    [Route] VARCHAR(500) NOT NULL,
    [EndpointName] VARCHAR(200) NOT NULL,
    [Description] NVARCHAR(500) NULL,
    [Category] VARCHAR(100) NULL,
    [IsActive] BIT NOT NULL DEFAULT 1,
    [CreatedOn] DATETIME2(7) NOT NULL DEFAULT GETDATE(),
    [ModifiedOn] DATETIME2(7) NULL,
    CONSTRAINT [PK_EndpointRegistry] PRIMARY KEY CLUSTERED ([EndpointId] ASC),
    CONSTRAINT [UK_EndpointRegistry_Method_Route] UNIQUE NONCLUSTERED ([HttpMethod] ASC, [Route] ASC)
) ON [PRIMARY]
GO

-- Create indexes for performance
CREATE NONCLUSTERED INDEX [IX_EndpointRegistry_Category]
ON [dbo].[EndpointRegistry] ([Category] ASC)
WHERE [IsActive] = 1
GO

CREATE NONCLUSTERED INDEX [IX_EndpointRegistry_IsActive]
ON [dbo].[EndpointRegistry] ([IsActive] ASC)
GO

PRINT 'EndpointRegistry table created successfully'
GO
```

#### 4.2.2 Create EndpointRolePermission Table

```sql
-- =============================================
-- Script: 02_Create_EndpointRolePermission_Table.sql
-- Description: Creates the EndpointRolePermission table for role-to-endpoint mappings
-- Date: 2025-11-17
-- =============================================

USE [IkeaDocuScan]
GO

-- Drop table if exists (for development/testing only)
-- IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[EndpointRolePermission]') AND type in (N'U'))
-- DROP TABLE [dbo].[EndpointRolePermission]
-- GO

CREATE TABLE [dbo].[EndpointRolePermission](
    [PermissionId] INT IDENTITY(1,1) NOT NULL,
    [EndpointId] INT NOT NULL,
    [RoleName] VARCHAR(50) NOT NULL,
    [CreatedOn] DATETIME2(7) NOT NULL DEFAULT GETDATE(),
    [CreatedBy] VARCHAR(255) NULL,
    CONSTRAINT [PK_EndpointRolePermission] PRIMARY KEY CLUSTERED ([PermissionId] ASC),
    CONSTRAINT [UK_EndpointRolePermission_EndpointRole] UNIQUE NONCLUSTERED ([EndpointId] ASC, [RoleName] ASC),
    CONSTRAINT [FK_EndpointRolePermission_Endpoint] FOREIGN KEY([EndpointId])
        REFERENCES [dbo].[EndpointRegistry] ([EndpointId])
        ON DELETE CASCADE,
    CONSTRAINT [CHK_EndpointRolePermission_RoleName] CHECK ([RoleName] IN ('Reader', 'Publisher', 'ADAdmin', 'SuperUser'))
) ON [PRIMARY]
GO

-- Create indexes for performance
CREATE NONCLUSTERED INDEX [IX_EndpointRolePermission_EndpointId]
ON [dbo].[EndpointRolePermission] ([EndpointId] ASC)
INCLUDE ([RoleName])
GO

CREATE NONCLUSTERED INDEX [IX_EndpointRolePermission_RoleName]
ON [dbo].[EndpointRolePermission] ([RoleName] ASC)
GO

PRINT 'EndpointRolePermission table created successfully'
GO
```

#### 4.2.3 Create PermissionChangeAuditLog Table

```sql
-- =============================================
-- Script: 03_Create_PermissionChangeAuditLog_Table.sql
-- Description: Creates the PermissionChangeAuditLog table for auditing permission changes
-- Date: 2025-11-17
-- =============================================

USE [IkeaDocuScan]
GO

-- Drop table if exists (for development/testing only)
-- IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PermissionChangeAuditLog]') AND type in (N'U'))
-- DROP TABLE [dbo].[PermissionChangeAuditLog]
-- GO

CREATE TABLE [dbo].[PermissionChangeAuditLog](
    [AuditId] INT IDENTITY(1,1) NOT NULL,
    [EndpointId] INT NOT NULL,
    [ChangedBy] VARCHAR(255) NOT NULL,
    [ChangeType] VARCHAR(50) NOT NULL,
    [OldValue] NVARCHAR(MAX) NULL,
    [NewValue] NVARCHAR(MAX) NULL,
    [ChangeReason] NVARCHAR(500) NULL,
    [ChangedOn] DATETIME2(7) NOT NULL DEFAULT GETDATE(),
    CONSTRAINT [PK_PermissionChangeAuditLog] PRIMARY KEY CLUSTERED ([AuditId] ASC),
    CONSTRAINT [FK_PermissionChangeAudit_Endpoint] FOREIGN KEY([EndpointId])
        REFERENCES [dbo].[EndpointRegistry] ([EndpointId]),
    CONSTRAINT [CHK_PermissionChangeAuditLog_ChangeType] CHECK ([ChangeType] IN ('RoleAdded', 'RoleRemoved', 'EndpointCreated', 'EndpointModified', 'EndpointDeactivated', 'EndpointReactivated'))
) ON [PRIMARY]
GO

-- Create indexes for performance
CREATE NONCLUSTERED INDEX [IX_PermissionChangeAuditLog_EndpointId]
ON [dbo].[PermissionChangeAuditLog] ([EndpointId] ASC)
GO

CREATE NONCLUSTERED INDEX [IX_PermissionChangeAuditLog_ChangedOn]
ON [dbo].[PermissionChangeAuditLog] ([ChangedOn] DESC)
GO

CREATE NONCLUSTERED INDEX [IX_PermissionChangeAuditLog_ChangedBy]
ON [dbo].[PermissionChangeAuditLog] ([ChangedBy] ASC)
GO

PRINT 'PermissionChangeAuditLog table created successfully'
GO
```

#### 4.2.4 Seed EndpointRegistry Data

**Note:** This is a large script. For brevity, showing structure and first 20 endpoints. Full script will seed all 126 endpoints.

```sql
-- =============================================
-- Script: 04_Seed_EndpointRegistry_Data.sql
-- Description: Seeds the EndpointRegistry table with all 126 API endpoints
-- Date: 2025-11-17
-- =============================================

USE [IkeaDocuScan]
GO

SET IDENTITY_INSERT [dbo].[EndpointRegistry] ON
GO

-- Documents (10 endpoints)
INSERT INTO [dbo].[EndpointRegistry] ([EndpointId], [HttpMethod], [Route], [EndpointName], [Description], [Category], [IsActive])
VALUES
(1, 'GET', '/api/documents/', 'GetAllDocuments', 'Get all documents (filtered by permissions)', 'Documents', 1),
(2, 'GET', '/api/documents/{id}', 'GetDocumentById', 'Get document by ID', 'Documents', 1),
(3, 'GET', '/api/documents/barcode/{barCode}', 'GetDocumentByBarcode', 'Get document by barcode', 'Documents', 1),
(4, 'POST', '/api/documents/by-ids', 'GetDocumentsByIds', 'Get documents by list of IDs', 'Documents', 1),
(5, 'POST', '/api/documents/', 'CreateDocument', 'Create new document', 'Documents', 1),
(6, 'PUT', '/api/documents/{id}', 'UpdateDocument', 'Update document', 'Documents', 1),
(7, 'DELETE', '/api/documents/{id}', 'DeleteDocument', 'Delete document', 'Documents', 1),
(8, 'POST', '/api/documents/search', 'SearchDocuments', 'Search documents', 'Documents', 1),
(9, 'GET', '/api/documents/{id}/stream', 'StreamDocumentFile', 'Stream document file (inline)', 'Documents', 1),
(10, 'GET', '/api/documents/{id}/download', 'DownloadDocumentFile', 'Download document file', 'Documents', 1),

-- Counter Parties (7 endpoints)
(11, 'GET', '/api/counterparties/', 'GetAllCounterParties', 'Get all counter parties', 'CounterParties', 1),
(12, 'GET', '/api/counterparties/search', 'SearchCounterParties', 'Search counter parties', 'CounterParties', 1),
(13, 'GET', '/api/counterparties/{id}', 'GetCounterPartyById', 'Get counter party by ID', 'CounterParties', 1),
(14, 'POST', '/api/counterparties/', 'CreateCounterParty', 'Create counter party', 'CounterParties', 1),
(15, 'PUT', '/api/counterparties/{id}', 'UpdateCounterParty', 'Update counter party', 'CounterParties', 1),
(16, 'DELETE', '/api/counterparties/{id}', 'DeleteCounterParty', 'Delete counter party', 'CounterParties', 1),
(17, 'GET', '/api/counterparties/{id}/usage', 'GetCounterPartyUsage', 'Get counter party usage count', 'CounterParties', 1),

-- Countries (6 endpoints)
(18, 'GET', '/api/countries/', 'GetAllCountries', 'Get all countries', 'Countries', 1),
(19, 'GET', '/api/countries/{code}', 'GetCountryByCode', 'Get country by code', 'Countries', 1),
(20, 'POST', '/api/countries/', 'CreateCountry', 'Create country', 'Countries', 1);
-- ... (Continue for all 126 endpoints - truncated for brevity)

SET IDENTITY_INSERT [dbo].[EndpointRegistry] OFF
GO

PRINT '126 endpoints seeded successfully'
GO
```

**Full seed script:** See `ROLE_EXTENSION_SEED_DATA_COMPLETE.sql` (will be provided in separate file due to size).

#### 4.2.5 Seed EndpointRolePermission Data

```sql
-- =============================================
-- Script: 05_Seed_EndpointRolePermission_Data.sql
-- Description: Seeds the EndpointRolePermission table with role mappings
-- Date: 2025-11-17
-- =============================================

USE [IkeaDocuScan]
GO

-- Helper function to add role permission
-- Usage: EXEC AddRolePermission @EndpointId, @RoleName

-- Documents - Read operations (All roles)
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName], [CreatedBy])
SELECT EndpointId, 'Reader', 'SYSTEM' FROM [dbo].[EndpointRegistry] WHERE EndpointId IN (1,2,3,4,8,9,10);

INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName], [CreatedBy])
SELECT EndpointId, 'Publisher', 'SYSTEM' FROM [dbo].[EndpointRegistry] WHERE EndpointId IN (1,2,3,4,8,9,10);

INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName], [CreatedBy])
SELECT EndpointId, 'ADAdmin', 'SYSTEM' FROM [dbo].[EndpointRegistry] WHERE EndpointId IN (1,2,3,4,8,9,10);

INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName], [CreatedBy])
SELECT EndpointId, 'SuperUser', 'SYSTEM' FROM [dbo].[EndpointRegistry] WHERE EndpointId IN (1,2,3,4,8,9,10);

-- Documents - Create/Update (Publisher, SuperUser)
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName], [CreatedBy])
SELECT EndpointId, 'Publisher', 'SYSTEM' FROM [dbo].[EndpointRegistry] WHERE EndpointId IN (5,6);

INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName], [CreatedBy])
SELECT EndpointId, 'SuperUser', 'SYSTEM' FROM [dbo].[EndpointRegistry] WHERE EndpointId IN (5,6);

-- Documents - Delete (SuperUser only)
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName], [CreatedBy])
SELECT EndpointId, 'SuperUser', 'SYSTEM' FROM [dbo].[EndpointRegistry] WHERE EndpointId = 7;

-- User Permissions - ADAdmin and SuperUser (NEW - 11 endpoints)
-- EndpointIds 40-50 (assumed based on seeding order)
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName], [CreatedBy])
SELECT e.EndpointId, r.RoleName, 'SYSTEM'
FROM [dbo].[EndpointRegistry] e
CROSS JOIN (VALUES ('ADAdmin'), ('SuperUser')) AS r(RoleName)
WHERE e.Route LIKE '/api/userpermissions%'
  AND e.Route <> '/api/userpermissions/me'
  AND e.IsActive = 1;

-- User Permissions - /me endpoint (All authenticated users)
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName], [CreatedBy])
SELECT e.EndpointId, r.RoleName, 'SYSTEM'
FROM [dbo].[EndpointRegistry] e
CROSS JOIN (VALUES ('Reader'), ('Publisher'), ('ADAdmin'), ('SuperUser')) AS r(RoleName)
WHERE e.Route = '/api/userpermissions/me'
  AND e.IsActive = 1;

-- Log Viewer - ADAdmin and SuperUser (NEW - 5 endpoints)
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName], [CreatedBy])
SELECT e.EndpointId, r.RoleName, 'SYSTEM'
FROM [dbo].[EndpointRegistry] e
CROSS JOIN (VALUES ('ADAdmin'), ('SuperUser')) AS r(RoleName)
WHERE e.Route LIKE '/api/logs%'
  AND e.IsActive = 1;

-- Configuration Read-Only - ADAdmin and SuperUser (NEW - 5 endpoints)
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName], [CreatedBy])
SELECT e.EndpointId, r.RoleName, 'SYSTEM'
FROM [dbo].[EndpointRegistry] e
CROSS JOIN (VALUES ('ADAdmin'), ('SuperUser')) AS r(RoleName)
WHERE e.Route LIKE '/api/configuration%'
  AND e.HttpMethod = 'GET'
  AND e.Route IN (
    '/api/configuration/email-recipients',
    '/api/configuration/email-recipients/{groupKey}',
    '/api/configuration/email-templates',
    '/api/configuration/email-templates/{key}',
    '/api/configuration/sections'
  )
  AND e.IsActive = 1;

-- Configuration Write - SuperUser only (unchanged)
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName], [CreatedBy])
SELECT e.EndpointId, 'SuperUser', 'SYSTEM'
FROM [dbo].[EndpointRegistry] e
WHERE e.Route LIKE '/api/configuration%'
  AND (e.HttpMethod IN ('POST', 'PUT', 'DELETE')
       OR e.Route NOT IN (
         '/api/configuration/email-recipients',
         '/api/configuration/email-recipients/{groupKey}',
         '/api/configuration/email-templates',
         '/api/configuration/email-templates/{key}',
         '/api/configuration/sections'
       ))
  AND e.IsActive = 1;

PRINT 'Endpoint role permissions seeded successfully'
GO

-- Verification query
SELECT
    er.EndpointName,
    er.HttpMethod,
    er.Route,
    STRING_AGG(erp.RoleName, ', ') AS AllowedRoles
FROM [dbo].[EndpointRegistry] er
INNER JOIN [dbo].[EndpointRolePermission] erp ON er.EndpointId = erp.EndpointId
WHERE er.IsActive = 1
GROUP BY er.EndpointId, er.EndpointName, er.HttpMethod, er.Route
ORDER BY er.Category, er.EndpointId;
GO
```

#### 4.2.6 Rollback Script

```sql
-- =============================================
-- Script: 99_Rollback_Authorization_Changes.sql
-- Description: Rolls back all authorization schema changes
-- Date: 2025-11-17
-- WARNING: This will delete all authorization data!
-- =============================================

USE [IkeaDocuScan]
GO

PRINT 'Starting rollback of authorization schema changes...'
GO

-- Drop tables in reverse dependency order
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PermissionChangeAuditLog]') AND type in (N'U'))
BEGIN
    DROP TABLE [dbo].[PermissionChangeAuditLog]
    PRINT 'PermissionChangeAuditLog table dropped'
END
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[EndpointRolePermission]') AND type in (N'U'))
BEGIN
    DROP TABLE [dbo].[EndpointRolePermission]
    PRINT 'EndpointRolePermission table dropped'
END
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[EndpointRegistry]') AND type in (N'U'))
BEGIN
    DROP TABLE [dbo].[EndpointRegistry]
    PRINT 'EndpointRegistry table dropped'
END
GO

PRINT 'Rollback completed successfully'
GO
```

### 4.3 Database Schema Diagram

```
┌─────────────────────────────────┐
│    EndpointRegistry             │
├─────────────────────────────────┤
│ PK  EndpointId (INT)            │
│     HttpMethod (VARCHAR(10))    │
│     Route (VARCHAR(500))        │
│     EndpointName (VARCHAR(200)) │
│     Description (NVARCHAR(500)) │
│     Category (VARCHAR(100))     │
│     IsActive (BIT)              │
│     CreatedOn (DATETIME2)       │
│     ModifiedOn (DATETIME2)      │
└─────────────────────────────────┘
          │ 1
          │
          │ N
┌─────────────────────────────────┐
│  EndpointRolePermission         │
├─────────────────────────────────┤
│ PK  PermissionId (INT)          │
│ FK  EndpointId (INT)            │
│     RoleName (VARCHAR(50))      │
│     CreatedOn (DATETIME2)       │
│     CreatedBy (VARCHAR(255))    │
└─────────────────────────────────┘
          │
          │
          │ Referenced by
          │
┌─────────────────────────────────┐
│  PermissionChangeAuditLog       │
├─────────────────────────────────┤
│ PK  AuditId (INT)               │
│ FK  EndpointId (INT)            │
│     ChangedBy (VARCHAR(255))    │
│     ChangeType (VARCHAR(50))    │
│     OldValue (NVARCHAR(MAX))    │
│     NewValue (NVARCHAR(MAX))    │
│     ChangeReason (NVARCHAR(500))│
│     ChangedOn (DATETIME2)       │
└─────────────────────────────────┘
```

---

## 5. 8-Step Implementation Plan

### Overview

The implementation is divided into 8 incremental steps, each with clear deliverables and test procedures. This allows for progressive rollout with validation at each stage.

### Step 1: Database Schema + Manual Seed Data

**Goal:** Create database tables and seed with current endpoint authorization.

**Tasks:**
1. Run SQL script `01_Create_EndpointRegistry_Table.sql`
2. Run SQL script `02_Create_EndpointRolePermission_Table.sql`
3. Run SQL script `03_Create_PermissionChangeAuditLog_Table.sql`
4. Run SQL script `04_Seed_EndpointRegistry_Data.sql` (all 126 endpoints)
5. Run SQL script `05_Seed_EndpointRolePermission_Data.sql`
6. Verify data integrity

**Deliverables:**
- 3 new database tables created
- 126 endpoints registered
- ~500 role permission records inserted
- Verified with SQL queries

**Test Procedures:** See [Section 6.1](#61-step-1-database-schema-tests)

**Rollback:** Run `99_Rollback_Authorization_Changes.sql`

---

### Step 2: Entity Classes + DbContext Updates

**Goal:** Add EF Core entity classes for new tables.

**Tasks:**
1. Create `EndpointRegistry.cs` entity in `IkeaDocuScan.Infrastructure/Entities/`
2. Create `EndpointRolePermission.cs` entity
3. Create `PermissionChangeAuditLog.cs` entity
4. Update `AppDbContext.cs` to include new DbSets
5. Configure entity relationships in `OnModelCreating()`
6. Do NOT create EF migrations (tables already exist)

**File Changes:**
- `IkeaDocuScan.Infrastructure/Entities/EndpointRegistry.cs` (new)
- `IkeaDocuScan.Infrastructure/Entities/EndpointRolePermission.cs` (new)
- `IkeaDocuScan.Infrastructure/Entities/PermissionChangeAuditLog.cs` (new)
- `IkeaDocuScan.Infrastructure/Data/AppDbContext.cs` (modify)

**Deliverables:**
- 3 new entity classes
- DbContext updated with DbSets
- Entity relationships configured
- Build succeeds

**Test Procedures:** See [Section 6.2](#62-step-2-entity-classes-tests)

**Rollback:** Revert code changes

---

### Step 3: Add ADAdmin Role to Middleware + Test Identity Updates

**Goal:** Add ADAdmin role claim support to WindowsIdentityMiddleware and update test identity components for ADAdmin role testing.

**Tasks:**
1. Add `ADGroupADAdmin` property to `IkeaDocuScanOptions.cs`
2. Update `appsettings.json` with ADAdmin group configuration (reuse existing SuperUser AD group)
3. Update `WindowsIdentityMiddleware.cs` to check ADAdmin AD group
4. **CRITICAL:** Modify SuperUser role claim logic to check database flag instead of AD group
5. Add ADAdmin role claim when user is in AD group
6. Update `TestIdentityService.cs` to include ADAdmin test profile
7. Update `DevIdentitySwitcher.razor` to display ADAdmin option
8. Test with users in ADAdmin AD group

**File Changes:**
- `IkeaDocuScan.Shared/Configuration/IkeaDocuScanOptions.cs` (modify)
- `IkeaDocuScan-Web/appsettings.json` (modify)
- `IkeaDocuScan-Web/Middleware/WindowsIdentityMiddleware.cs` (modify)
- `IkeaDocuScan-Web/Services/TestIdentityService.cs` (modify - DEBUG only)
- `IkeaDocuScan-Web.Client/Components/DevIdentitySwitcher.razor` (modify - DEBUG only)

**Code Changes:**

**IkeaDocuScanOptions.cs:**
```csharp
/// <summary>
/// Active Directory group for ADAdmin role (maps to existing SuperUser AD group)
/// </summary>
public string? ADGroupADAdmin { get; set; } = "ADGroup.Builtin.SuperUser";
```

**appsettings.json:**
```json
{
  "IkeaDocuScan": {
    "ADGroupReader": "ADGroup.Builtin.Reader",
    "ADGroupPublisher": "ADGroup.Builtin.Publisher",
    "ADGroupADAdmin": "ADGroup.Builtin.SuperUser",
    "ADGroupSuperUser": "ADGroup.Builtin.SuperUser"
  }
}
```

**WindowsIdentityMiddleware.cs** (in `AddADGroupClaims` method):
```csharp
// Check ADAdmin group (replaces old SuperUser AD group check)
if (!string.IsNullOrWhiteSpace(_options.ADGroupADAdmin))
{
    if (principal.IsInRole(_options.ADGroupADAdmin))
    {
        claims.Add(new Claim(ClaimTypes.Role, "ADAdmin"));
        _logger.LogInformation("User {Username} is in AD group {GroupName} - assigned ADAdmin role",
            windowsIdentity.Name, _options.ADGroupADAdmin);
    }
}

// SuperUser role is now ONLY assigned via database flag (not AD group)
// This logic should already exist from database permission loading
// Ensure the "IsSuperUser" claim check is based on database flag only
```

**IMPORTANT:** Remove or comment out the old SuperUser AD group check if it exists. SuperUser role should ONLY come from database flag `IsSuperUser = true`.

**TestIdentityService.cs** (add ADAdmin profile):
```csharp
public static TestIdentityProfile ADAdminProfile => new TestIdentityProfile
{
    ProfileId = "adadmin",
    DisplayName = "Test ADAdmin",
    AccountName = "TEST\\adadmin",
    Roles = new[] { "Reader", "ADAdmin" },
    IsSuperUser = false,
    HasAccess = true,
    Permissions = null // ADAdmin sees all (read-only)
};

// Add to GetAllProfiles() method
public static List<TestIdentityProfile> GetAllProfiles()
{
    return new List<TestIdentityProfile>
    {
        ReaderProfile,
        PublisherProfile,
        ADAdminProfile,  // NEW
        SuperUserProfile
    };
}
```

**DevIdentitySwitcher.razor** (add ADAdmin option):
```razor
<option value="adadmin">Test ADAdmin (View Users & Logs)</option>
```

**Deliverables:**
- ADAdmin role configuration added (reusing existing SuperUser AD group)
- SuperUser role now database-only (removed from AD group check)
- Middleware updated to check ADAdmin AD group
- ADAdmin role claim added for users in group
- TestIdentityService includes ADAdmin profile for testing
- DevIdentitySwitcher displays ADAdmin option
- Build succeeds

**Test Procedures:** See [Section 6.3](#63-step-3-adamin-role-tests)

**Rollback:** Revert code changes, remove AD group configuration

---

### Step 4: Authorization Policy Provider (Dynamic)

**Goal:** Create dynamic authorization policy provider that reads from database.

**Tasks:**
1. Create `DynamicAuthorizationPolicyProvider.cs`
2. Create `DynamicAuthorizationHandler.cs`
3. Create `IEndpointAuthorizationService.cs` interface
4. Create `EndpointAuthorizationService.cs` implementation with caching
5. Register services in `Program.cs`
6. Configure authorization to use dynamic provider

**File Changes:**
- `IkeaDocuScan-Web/Authorization/DynamicAuthorizationPolicyProvider.cs` (new)
- `IkeaDocuScan-Web/Authorization/DynamicAuthorizationHandler.cs` (new)
- `IkeaDocuScan.Shared/Interfaces/IEndpointAuthorizationService.cs` (new)
- `IkeaDocuScan-Web/Services/EndpointAuthorizationService.cs` (new)
- `IkeaDocuScan-Web/Program.cs` (modify)

**Key Components:**

**DynamicAuthorizationPolicyProvider.cs:**
```csharp
public class DynamicAuthorizationPolicyProvider : IAuthorizationPolicyProvider
{
    private readonly DefaultAuthorizationPolicyProvider _fallbackProvider;
    private readonly IEndpointAuthorizationService _authService;

    public async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        // Check if policy is for endpoint authorization
        if (policyName.StartsWith("Endpoint:"))
        {
            var parts = policyName.Split(':');
            if (parts.Length == 3)
            {
                var method = parts[1];
                var route = parts[2];
                var roles = await _authService.GetAllowedRolesAsync(method, route);

                if (roles.Any())
                {
                    return new AuthorizationPolicyBuilder()
                        .RequireRole(roles.ToArray())
                        .Build();
                }
            }
        }

        // Fall back to default provider
        return await _fallbackProvider.GetPolicyAsync(policyName);
    }
}
```

**EndpointAuthorizationService.cs:**
```csharp
public class EndpointAuthorizationService : IEndpointAuthorizationService
{
    private readonly AppDbContext _dbContext;
    private readonly IMemoryCache _cache;
    private const string CacheKeyPrefix = "EndpointAuth:";
    private const int CacheMinutes = 30;

    public async Task<List<string>> GetAllowedRolesAsync(string httpMethod, string route)
    {
        var cacheKey = $"{CacheKeyPrefix}{httpMethod}:{route}";

        if (!_cache.TryGetValue(cacheKey, out List<string> roles))
        {
            roles = await _dbContext.EndpointRegistry
                .Where(e => e.HttpMethod == httpMethod && e.Route == route && e.IsActive)
                .SelectMany(e => e.EndpointRolePermissions.Select(p => p.RoleName))
                .ToListAsync();

            _cache.Set(cacheKey, roles, TimeSpan.FromMinutes(CacheMinutes));
        }

        return roles;
    }

    public void InvalidateCache()
    {
        // Clear all endpoint authorization cache entries
    }
}
```

**Deliverables:**
- Dynamic policy provider created
- Service layer for endpoint authorization
- In-memory caching implemented
- Services registered in DI container
- Build succeeds

**Test Procedures:** See [Section 6.4](#64-step-4-policy-provider-tests)

**Rollback:** Revert code changes, remove service registrations

---

### Step 5: Activate Dynamic Authorization (Single Endpoint Test)

**Goal:** Test dynamic authorization with one endpoint before migrating all.

**Tasks:**
1. Choose test endpoint: `GET /api/userpermissions/users`
2. Update endpoint to use dynamic authorization
3. Test with Reader (should fail), ADAdmin (should succeed), SuperUser (should succeed)
4. Verify database lookup and caching
5. Confirm no performance degradation

**File Changes:**
- `IkeaDocuScan-Web/Endpoints/UserPermissionEndpoints.cs` (modify one endpoint only)

**Code Change:**
```csharp
// BEFORE (hard-coded):
var group = routes.MapGroup("/api/userpermissions")
    .RequireAuthorization("HasAccess");

group.MapGet("/users", GetAllDocuScanUsers)
    .RequireAuthorization("SuperUser"); // OLD

// AFTER (dynamic):
group.MapGet("/users", GetAllDocuScanUsers)
    .RequireAuthorization("Endpoint:GET:/api/userpermissions/users"); // NEW - reads from database
```

**Deliverables:**
- Single endpoint migrated to dynamic authorization
- Authorization works correctly for all roles
- Cache is populated on first request
- Subsequent requests use cached data
- Build and tests pass

**Test Procedures:** See [Section 6.5](#65-step-5-single-endpoint-tests)

**Rollback:** Revert endpoint authorization to hard-coded

---

### Step 6: Cache Management + Service Layer

**Goal:** Add cache invalidation endpoints and complete service layer.

**Tasks:**
1. Create `EndpointAuthorizationManagementService.cs`
2. Add methods for updating permissions
3. Add cache invalidation logic
4. Create audit logging for permission changes
5. Add validation logic

**File Changes:**
- `IkeaDocuScan.Shared/Interfaces/IEndpointAuthorizationManagementService.cs` (new)
- `IkeaDocuScan-Web/Services/EndpointAuthorizationManagementService.cs` (new)
- `IkeaDocuScan-Web/Program.cs` (modify - register service)

**Key Methods:**
```csharp
public interface IEndpointAuthorizationManagementService
{
    Task<EndpointRegistry> GetEndpointAsync(int endpointId);
    Task<List<EndpointRegistry>> GetAllEndpointsAsync();
    Task<List<string>> GetEndpointRolesAsync(int endpointId);
    Task UpdateEndpointRolesAsync(int endpointId, List<string> roles, string changedBy, string reason);
    Task<List<PermissionChangeAuditLog>> GetAuditLogAsync(int? endpointId = null, DateTime? fromDate = null);
    Task SyncEndpointsFromCodeAsync(); // Sync code-defined endpoints to database
    Task InvalidateCacheAsync();
    Task<bool> ValidatePermissionChangeAsync(int endpointId, List<string> newRoles);
}
```

**Deliverables:**
- Management service implemented
- Cache invalidation working
- Audit logging functional
- Validation logic in place
- Service registered in DI
- Build succeeds

**Test Procedures:** See [Section 6.6](#66-step-6-service-layer-tests)

**Rollback:** Revert code changes

---

### Step 7: Migrate Existing Endpoints (31 endpoints)

**Goal:** Migrate all 31 endpoints requiring authorization changes to dynamic authorization.

**Tasks:**
1. Update `UserPermissionEndpoints.cs` (10 remaining endpoints)
2. Update `LogViewerEndpoints.cs` (5 endpoints)
3. Update `ConfigurationEndpoints.cs` (5 GET endpoints)
4. Test each category thoroughly
5. Verify no regressions

**File Changes:**
- `IkeaDocuScan-Web/Endpoints/UserPermissionEndpoints.cs` (modify)
- `IkeaDocuScan-Web/Endpoints/LogViewerEndpoints.cs` (modify)
- `IkeaDocuScan-Web/Endpoints/ConfigurationEndpoints.cs` (modify)

**Migration Pattern:**

**Before:**
```csharp
group.MapGet("/", GetAllUserPermissions)
    .RequireAuthorization("SuperUser");
```

**After:**
```csharp
group.MapGet("/", GetAllUserPermissions)
    .RequireAuthorization("Endpoint:GET:/api/userpermissions/");
```

**Deliverables:**
- All 31 endpoints migrated to dynamic authorization
- ADAdmin users can access user permission and log endpoints
- SuperUser retains all access
- Reader/Publisher access unchanged
- All tests pass

**Test Procedures:** See [Section 6.7](#67-step-7-endpoint-migration-tests)

**Rollback:** Revert endpoint changes

---

### Step 8: UI for Permission Management + NavMenu Role-Based Visibility

**Goal:** Create admin UI for managing endpoint permissions dynamically and implement role-based menu visibility.

**Tasks:**
1. Create `EndpointAuthorizationEndpoints.cs` (10 new API endpoints)
2. **Update `NavMenu.razor` for role-based visibility** (PRIORITY)
3. Create `EndpointAuthorizationManagement.razor` page (Blazor component)
4. Update `QueryableExtensions.cs` to handle dynamic permissions in queries (if needed)
5. Add endpoint authorization management menu item (SuperUser only)
6. Implement permission change workflow with confirmation
7. Add bulk operations (select multiple endpoints)
8. Test full workflow including menu visibility for all roles

**File Changes:**
- `IkeaDocuScan-Web/Endpoints/EndpointAuthorizationEndpoints.cs` (new)
- **`IkeaDocuScan-Web.Client/Layout/NavMenu.razor` (modify - CRITICAL)**
- `IkeaDocuScan-Web.Client/Pages/EndpointAuthorizationManagement.razor` (new)
- `IkeaDocuScan.Shared/Extensions/QueryableExtensions.cs` (modify if needed)
- `IkeaDocuScan-Web.Client/Services/EndpointAuthorizationHttpService.cs` (new - client-side service)

**NavMenu.razor Modifications (CRITICAL):**

Each menu item must:
1. Be assigned a representative endpoint
2. Call `/api/endpoint-authorization/check` to verify user access
3. Conditionally render based on response

**Implementation Pattern:**
```razor
@code {
    private Dictionary<string, bool> menuItemVisibility = new();

    protected override async Task OnInitializedAsync()
    {
        // Check visibility for each menu item
        menuItemVisibility["Documents"] = await CheckEndpointAccessAsync("GET", "/api/documents/");
        menuItemVisibility["ScannedFiles"] = await CheckEndpointAccessAsync("GET", "/api/scannedfiles/");
        menuItemVisibility["ActionReminders"] = await CheckEndpointAccessAsync("GET", "/api/action-reminders/");
        menuItemVisibility["Reports"] = await CheckEndpointAccessAsync("GET", "/api/reports/barcode-gaps");
        menuItemVisibility["CounterParties"] = await CheckEndpointAccessAsync("GET", "/api/counterparties/");
        menuItemVisibility["ReferenceData"] = await CheckEndpointAccessAsync("GET", "/api/countries/");
        menuItemVisibility["UserPermissions"] = await CheckEndpointAccessAsync("GET", "/api/userpermissions/users");
        menuItemVisibility["Logs"] = await CheckEndpointAccessAsync("POST", "/api/logs/search");
        menuItemVisibility["Configuration"] = await CheckEndpointAccessAsync("GET", "/api/configuration/sections");
        menuItemVisibility["EndpointAuthorization"] = await CheckEndpointAccessAsync("GET", "/api/endpoint-authorization/endpoints");
    }

    private async Task<bool> CheckEndpointAccessAsync(string method, string route)
    {
        try
        {
            var result = await Http.GetFromJsonAsync<EndpointAccessCheckResult>(
                $"/api/endpoint-authorization/check?method={method}&route={Uri.EscapeDataString(route)}"
            );
            return result?.HasAccess ?? false;
        }
        catch
        {
            return false; // Default to hidden on error
        }
    }
}

<!-- Menu items with conditional rendering -->
@if (menuItemVisibility.GetValueOrDefault("Documents", false))
{
    <div class="nav-item px-3">
        <NavLink class="nav-link" href="documents">
            <span class="bi bi-file-earmark-text-nav-menu" aria-hidden="true"></span> Documents
        </NavLink>
    </div>
}

@if (menuItemVisibility.GetValueOrDefault("ActionReminders", false))
{
    <div class="nav-item px-3">
        <NavLink class="nav-link" href="action-reminders">
            <span class="bi bi-bell-nav-menu" aria-hidden="true"></span> Action Reminders
        </NavLink>
    </div>
}

@if (menuItemVisibility.GetValueOrDefault("UserPermissions", false))
{
    <div class="nav-item px-3">
        <NavLink class="nav-link" href="user-permissions">
            <span class="bi bi-people-nav-menu" aria-hidden="true"></span> User Permissions
        </NavLink>
    </div>
}

@if (menuItemVisibility.GetValueOrDefault("EndpointAuthorization", false))
{
    <div class="nav-item px-3">
        <NavLink class="nav-link" href="endpoint-authorization">
            <span class="bi bi-shield-lock-nav-menu" aria-hidden="true"></span> Endpoint Authorization
        </NavLink>
    </div>
}

<!-- ... repeat for all menu items -->
```

**Expected Menu Visibility by Role:**

| Menu Item | Reader | Publisher | ADAdmin | SuperUser |
|-----------|--------|-----------|---------|-----------|
| Home | ✅ | ✅ | ✅ | ✅ |
| Documents | ✅ | ✅ | ✅ | ✅ |
| Scanned Files | ❌ | ✅ | ✅ | ✅ |
| Action Reminders | ❌ | ✅ | ✅ | ✅ |
| Reports | ❌ | ✅ | ✅ | ✅ |
| Counter Parties | ❌ | ✅ | ✅ | ✅ |
| Reference Data | ❌  | ✅ | ✅ | ✅ |
| User Permissions | ❌ | ❌ | ✅ | ✅ |
| Logs | ❌ | ❌ | ✅ | ✅ |
| Configuration | ❌ | ❌ | ✅ | ✅ |
| Endpoint Authorization | ❌ | ❌ | ❌ | ✅ |

**Endpoint Authorization Admin UI Features:**
- List all endpoints with current permissions
- Filter by category, HTTP method, role
- Search by route or endpoint name
- Edit permissions (add/remove roles)
- View audit log
- Cache invalidation button
- Sync endpoints from code button
- Validation warnings (e.g., "No roles assigned - endpoint will be inaccessible")

**Deliverables:**
- 10 new API endpoints functional
- **NavMenu.razor updated with role-based visibility**
- Menu items show/hide correctly for all roles
- Admin UI page created
- Permission management workflow working
- Audit log visible
- Cache management functional
- All features tested

**Test Procedures:** See [Section 6.8](#68-step-8-admin-ui-tests)

**Rollback:** Remove UI page, delete endpoint file, revert NavMenu changes

---

## 6. Test Procedures

### 6.1 Step 1: Database Schema Tests

**Objective:** Verify database tables created correctly and seeded with data.

#### Test 1.1: Verify Table Creation

```sql
-- Expected: All 3 tables exist
SELECT
    TABLE_NAME,
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = t.TABLE_NAME) AS ColumnCount
FROM INFORMATION_SCHEMA.TABLES t
WHERE TABLE_NAME IN ('EndpointRegistry', 'EndpointRolePermission', 'PermissionChangeAuditLog')
ORDER BY TABLE_NAME;
```

**Expected Result:**
```
TABLE_NAME                    ColumnCount
EndpointRegistry              9
EndpointRolePermission        4
PermissionChangeAuditLog      8
```

#### Test 1.2: Verify Constraints and Indexes

```sql
-- Check unique constraints
SELECT
    CONSTRAINT_NAME,
    TABLE_NAME
FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
WHERE TABLE_NAME IN ('EndpointRegistry', 'EndpointRolePermission')
  AND CONSTRAINT_TYPE = 'UNIQUE';

-- Check foreign keys
SELECT
    CONSTRAINT_NAME,
    TABLE_NAME
FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
WHERE TABLE_NAME IN ('EndpointRolePermission', 'PermissionChangeAuditLog')
  AND CONSTRAINT_TYPE = 'FOREIGN KEY';
```

**Expected Result:**
- UK_EndpointRegistry_Method_Route
- UK_EndpointRolePermission_EndpointRole
- FK_EndpointRolePermission_Endpoint
- FK_PermissionChangeAudit_Endpoint

#### Test 1.3: Verify Seed Data Count

```sql
-- Expected: 126 endpoints
SELECT COUNT(*) AS EndpointCount FROM EndpointRegistry WHERE IsActive = 1;

-- Expected: ~500 role permissions (varies based on matrix)
SELECT COUNT(*) AS PermissionCount FROM EndpointRolePermission;

-- Expected: 0 audit logs (nothing changed yet)
SELECT COUNT(*) AS AuditCount FROM PermissionChangeAuditLog;
```

**Expected Result:**
```
EndpointCount: 126
PermissionCount: 400-500 (depends on final matrix)
AuditCount: 0
```

#### Test 1.4: Verify Role Distribution

```sql
-- Check role distribution
SELECT
    RoleName,
    COUNT(*) AS EndpointCount
FROM EndpointRolePermission
GROUP BY RoleName
ORDER BY RoleName;
```

**Expected Result:**
```
RoleName      EndpointCount
Reader        60
Publisher     69
ADAdmin       21
SuperUser     126
```

#### Test 1.5: Verify Specific Endpoint Permissions

```sql
-- Test: User permission endpoints should have ADAdmin and SuperUser
SELECT
    er.HttpMethod,
    er.Route,
    STRING_AGG(erp.RoleName, ', ') AS Roles
FROM EndpointRegistry er
JOIN EndpointRolePermission erp ON er.EndpointId = erp.EndpointId
WHERE er.Route LIKE '/api/userpermissions%'
  AND er.Route <> '/api/userpermissions/me'
GROUP BY er.EndpointId, er.HttpMethod, er.Route
ORDER BY er.Route;
```

**Expected Result:** All `/api/userpermissions/*` endpoints (except `/me`) should have "ADAdmin, SuperUser".

#### Test 1.6: Rollback Test

```sql
-- Run rollback script
EXEC sp_executesql N'$(99_Rollback_Authorization_Changes.sql)';

-- Verify tables dropped
SELECT TABLE_NAME
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_NAME IN ('EndpointRegistry', 'EndpointRolePermission', 'PermissionChangeAuditLog');
```

**Expected Result:** No tables found (0 rows).

**Re-run setup scripts after rollback test.**

---

### 6.2 Step 2: Entity Classes Tests

**Objective:** Verify EF Core entity classes work correctly with database.

#### Test 2.1: Build Success

```bash
cd IkeaDocuScanV3/
dotnet build
```

**Expected Result:** Build succeeded with 0 errors, 0 warnings.

#### Test 2.2: DbContext Query Test

Create a simple test endpoint or use SQL Management Studio with EF Core:

```csharp
// In a test controller or endpoint
[HttpGet("test/endpoints")]
public async Task<IActionResult> TestEndpoints([FromServices] AppDbContext dbContext)
{
    var endpoints = await dbContext.EndpointRegistry
        .Include(e => e.EndpointRolePermissions)
        .Take(5)
        .ToListAsync();

    return Ok(endpoints);
}
```

**Expected Result:** Returns 5 endpoints with their role permissions (navigation property loaded).

#### Test 2.3: Verify Entity Relationships

```csharp
// Test cascade delete
var endpoint = await dbContext.EndpointRegistry
    .Include(e => e.EndpointRolePermissions)
    .FirstOrDefaultAsync();

var permissionCount = endpoint.EndpointRolePermissions.Count;

// Verify navigation properties
Assert.NotNull(endpoint);
Assert.True(permissionCount > 0);
Assert.NotNull(endpoint.EndpointRolePermissions.First().Endpoint); // Reverse navigation
```

**Expected Result:** All assertions pass.

---

### 6.3 Step 3: ADAdmin Role Tests

**Objective:** Verify ADAdmin role claim is added for users in AD group.

#### Test 3.1: Configuration Verification

```bash
# Check appsettings.json
cat appsettings.json | grep ADGroupADAdmin
```

**Expected Result:** `"ADGroupADAdmin": "ADGroup.Builtin.ADAdmin"`

#### Test 3.2: Add User to ADAdmin AD Group

```powershell
# PowerShell (requires AD admin rights)
Add-ADGroupMember -Identity "ADGroup.Builtin.ADAdmin" -Members "testuser"
```

#### Test 3.3: Verify Role Claim

**Option A: Via API**
```bash
curl -X GET https://localhost:44101/api/user/identity \
  -u "DOMAIN\testuser" \
  --negotiate
```

**Expected Result:** Response includes `"type": "http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "value": "ADAdmin"`.

**Option B: Via Application Logs**

Log in as test user and check logs:
```
[Information] User DOMAIN\testuser is in AD group ADGroup.Builtin.ADAdmin
```

#### Test 3.4: Verify Multiple Roles

Add user to both Publisher and ADAdmin groups:

```powershell
Add-ADGroupMember -Identity "ADGroup.Builtin.Publisher" -Members "testuser"
Add-ADGroupMember -Identity "ADGroup.Builtin.ADAdmin" -Members "testuser"
```

**Expected Result:** User gets both "Publisher" and "ADAdmin" role claims.

---

### 6.4 Step 4: Policy Provider Tests

**Objective:** Verify dynamic policy provider loads permissions from database.

#### Test 4.1: Service Registration

```csharp
// In Program.cs, verify services registered
builder.Services.AddScoped<IEndpointAuthorizationService, EndpointAuthorizationService>();
builder.Services.AddSingleton<IAuthorizationPolicyProvider, DynamicAuthorizationPolicyProvider>();
```

#### Test 4.2: Policy Resolution Test

Create a test endpoint:

```csharp
[HttpGet("test/policy")]
[Authorize(Policy = "Endpoint:GET:/api/userpermissions/users")]
public IActionResult TestPolicy()
{
    return Ok("Policy resolved successfully");
}
```

**Test Steps:**
1. Login as Reader → Expect 403 Forbidden
2. Login as ADAdmin → Expect 200 OK
3. Login as SuperUser → Expect 200 OK

#### Test 4.3: Cache Performance Test

```csharp
// Measure first request (database hit)
var stopwatch = Stopwatch.StartNew();
var roles1 = await _authService.GetAllowedRolesAsync("GET", "/api/userpermissions/users");
stopwatch.Stop();
var firstRequestMs = stopwatch.ElapsedMilliseconds;

// Measure second request (cache hit)
stopwatch.Restart();
var roles2 = await _authService.GetAllowedRolesAsync("GET", "/api/userpermissions/users");
stopwatch.Stop();
var secondRequestMs = stopwatch.ElapsedMilliseconds;

Assert.True(secondRequestMs < firstRequestMs); // Cache should be faster
Assert.Equal(roles1, roles2); // Results should be identical
```

**Expected Result:** Second request is significantly faster (< 1ms vs. 10-50ms).

#### Test 4.4: Fallback Provider Test

Test that existing hard-coded policies still work:

```bash
# Test HasAccess policy (not dynamic)
curl -X GET https://localhost:44101/api/documents/ \
  -u "DOMAIN\reader" \
  --negotiate
```

**Expected Result:** 200 OK (falls back to original HasAccess policy).

---

### 6.5 Step 5: Single Endpoint Tests

**Objective:** Verify single endpoint works with dynamic authorization.

#### Test 5.1: Reader Access (Should Fail)

```bash
curl -X GET https://localhost:44101/api/userpermissions/users \
  -u "DOMAIN\reader" \
  --negotiate \
  -i
```

**Expected Result:** HTTP 403 Forbidden

#### Test 5.2: ADAdmin Access (Should Succeed)

```bash
curl -X GET https://localhost:44101/api/userpermissions/users \
  -u "DOMAIN\adminuser" \
  --negotiate \
  -i
```

**Expected Result:** HTTP 200 OK with user list

#### Test 5.3: SuperUser Access (Should Succeed)

```bash
curl -X GET https://localhost:44101/api/userpermissions/users \
  -u "DOMAIN\superuser" \
  --negotiate \
  -i
```

**Expected Result:** HTTP 200 OK with user list

#### Test 5.4: Cache Verification

```sql
-- Check application logs for cache activity
-- First request should show: "Loading endpoint authorization from database"
-- Subsequent requests should show: "Using cached endpoint authorization"
```

#### Test 5.5: Database Modification Test

```sql
-- Remove ADAdmin role from endpoint
DELETE FROM EndpointRolePermission
WHERE EndpointId = (SELECT EndpointId FROM EndpointRegistry WHERE Route = '/api/userpermissions/users')
  AND RoleName = 'ADAdmin';
```

**Test:** Login as ADAdmin → Expect 403 Forbidden (still cached)

**Invalidate Cache:** Call cache invalidation endpoint or restart application

**Test Again:** Login as ADAdmin → Expect 403 Forbidden (cache refreshed)

**Restore Permission:**
```sql
INSERT INTO EndpointRolePermission (EndpointId, RoleName, CreatedBy)
SELECT EndpointId, 'ADAdmin', 'TEST' FROM EndpointRegistry WHERE Route = '/api/userpermissions/users';
```

---

### 6.6 Step 6: Service Layer Tests

**Objective:** Verify management service and cache invalidation.

#### Test 6.1: Get Endpoint Roles

```csharp
var roles = await _managementService.GetEndpointRolesAsync(endpointId);
Assert.Contains("ADAdmin", roles);
Assert.Contains("SuperUser", roles);
```

**Expected Result:** Returns ["ADAdmin", "SuperUser"] for user permission endpoints.

#### Test 6.2: Update Endpoint Roles

```csharp
var endpointId = 40; // Example: /api/userpermissions/users
var newRoles = new List<string> { "Publisher", "ADAdmin", "SuperUser" };

await _managementService.UpdateEndpointRolesAsync(
    endpointId,
    newRoles,
    "DOMAIN\\testadmin",
    "Adding Publisher access for testing"
);
```

**Verify:**
```sql
SELECT RoleName FROM EndpointRolePermission WHERE EndpointId = 40;
```

**Expected Result:** Returns "Publisher", "ADAdmin", "SuperUser".

#### Test 6.3: Audit Log Created

```sql
SELECT * FROM PermissionChangeAuditLog
WHERE EndpointId = 40
ORDER BY ChangedOn DESC;
```

**Expected Result:**
- 1 audit record
- ChangeType: "RoleAdded"
- NewValue: "Publisher"
- ChangedBy: "DOMAIN\\testadmin"
- ChangeReason: "Adding Publisher access for testing"

#### Test 6.4: Cache Invalidation

```csharp
// Before invalidation - cache hit
var roles1 = await _authService.GetAllowedRolesAsync("GET", "/api/userpermissions/users");

// Invalidate cache
await _managementService.InvalidateCacheAsync();

// After invalidation - database hit (should reload)
var roles2 = await _authService.GetAllowedRolesAsync("GET", "/api/userpermissions/users");

Assert.Equal(roles1, roles2); // Should still match after reload
```

#### Test 6.5: Validation Logic Test

```csharp
// Test: Removing all roles should fail validation
var isValid = await _managementService.ValidatePermissionChangeAsync(
    endpointId,
    new List<string>() // Empty role list
);

Assert.False(isValid); // Should fail - endpoint would be inaccessible
```

**Expected Result:** Validation fails with warning message.

---

### 6.7 Step 7: Endpoint Migration Tests

**Objective:** Verify all 31 endpoints migrated correctly.

#### Test 7.1: User Permission Endpoints (11 endpoints)

**Test Matrix:**

| Endpoint | Reader | Publisher | ADAdmin | SuperUser |
|----------|--------|-----------|---------|-----------|
| GET /api/userpermissions/ | 403 | 403 | 200 | 200 |
| POST /api/userpermissions/ | 403 | 403 | 200 | 200 |
| DELETE /api/userpermissions/{id} | 403 | 403 | 200 | 200 |
| GET /api/userpermissions/me | 200 | 200 | 200 | 200 |

**Test Script:**
```bash
# Test as Reader
for endpoint in "users" "1" "user/1"; do
  curl -X GET https://localhost:44101/api/userpermissions/$endpoint \
    -u "DOMAIN\reader" --negotiate -w "\n%{http_code}\n"
done

# Test as ADAdmin
for endpoint in "users" "1" "user/1"; do
  curl -X GET https://localhost:44101/api/userpermissions/$endpoint \
    -u "DOMAIN\adminuser" --negotiate -w "\n%{http_code}\n"
done
```

**Expected Results:** All tests pass according to matrix.

#### Test 7.2: Log Viewer Endpoints (5 endpoints)

**Test:**
```bash
# Reader - should fail
curl -X POST https://localhost:44101/api/logs/search \
  -u "DOMAIN\reader" --negotiate \
  -H "Content-Type: application/json" \
  -d '{"fromDate":"2025-01-01"}' \
  -w "\n%{http_code}\n"

# ADAdmin - should succeed
curl -X POST https://localhost:44101/api/logs/search \
  -u "DOMAIN\adminuser" --negotiate \
  -H "Content-Type: application/json" \
  -d '{"fromDate":"2025-01-01"}' \
  -w "\n%{http_code}\n"
```

**Expected Results:**
- Reader: 403
- ADAdmin: 200
- SuperUser: 200

#### Test 7.3: Configuration Read-Only Endpoints (5 endpoints)

**Test:**
```bash
# Reader - should fail
curl -X GET https://localhost:44101/api/configuration/email-recipients \
  -u "DOMAIN\reader" --negotiate -w "\n%{http_code}\n"

# ADAdmin - should succeed (read-only)
curl -X GET https://localhost:44101/api/configuration/email-recipients \
  -u "DOMAIN\adminuser" --negotiate -w "\n%{http_code}\n"

# ADAdmin - should fail (write operation)
curl -X POST https://localhost:44101/api/configuration/email-recipients/TestGroup \
  -u "DOMAIN\adminuser" --negotiate \
  -H "Content-Type: application/json" \
  -d '{"recipients":["test@example.com"]}' \
  -w "\n%{http_code}\n"
```

**Expected Results:**
- Reader GET: 403
- ADAdmin GET: 200
- ADAdmin POST: 403 (write operations still SuperUser only)

#### Test 7.4: Regression Test - Unchanged Endpoints

**Test:** Verify document endpoints still work as before.

```bash
# Reader - can view documents
curl -X GET https://localhost:44101/api/documents/ \
  -u "DOMAIN\reader" --negotiate -w "\n%{http_code}\n"

# Reader - cannot create documents
curl -X POST https://localhost:44101/api/documents/ \
  -u "DOMAIN\reader" --negotiate \
  -H "Content-Type: application/json" \
  -d '{"name":"Test"}' \
  -w "\n%{http_code}\n"

# Publisher - can create documents
curl -X POST https://localhost:44101/api/documents/ \
  -u "DOMAIN\publisher" --negotiate \
  -H "Content-Type: application/json" \
  -d '{"name":"Test"}' \
  -w "\n%{http_code}\n"
```

**Expected Results:**
- Reader GET: 200
- Reader POST: 403
- Publisher POST: 201

---

### 6.8 Step 8: Admin UI Tests

**Objective:** Verify admin UI for managing endpoint permissions.

#### Test 8.1: Access Control

**Test:** Navigate to `/endpoint-authorization-management`

- As Reader → 404 or Access Denied
- As Publisher → 404 or Access Denied
- As ADAdmin → 404 or Access Denied
- As SuperUser → Page loads successfully

**Expected Result:** Only SuperUser can access the page.

#### Test 8.2: Endpoint List Display

**Navigate to management page as SuperUser.**

**Verify:**
- List shows all 126 endpoints
- Columns: HTTP Method, Route, Endpoint Name, Category, Allowed Roles
- Search box functional
- Filter by category dropdown functional
- Filter by role dropdown functional

#### Test 8.3: Edit Permissions Workflow

**Test Steps:**
1. Select endpoint: `GET /api/userpermissions/users`
2. Click "Edit Permissions"
3. Current roles displayed: ADAdmin, SuperUser
4. Add role: Reader
5. Enter reason: "Testing permission change"
6. Click "Save"
7. Confirm dialog appears
8. Click "Confirm"

**Verify:**
```sql
SELECT RoleName FROM EndpointRolePermission
WHERE EndpointId = (SELECT EndpointId FROM EndpointRegistry WHERE Route = '/api/userpermissions/users');
```

**Expected Result:** Returns "Reader", "ADAdmin", "SuperUser".

**Verify Audit Log:**
```sql
SELECT * FROM PermissionChangeAuditLog WHERE EndpointId = 40 ORDER BY ChangedOn DESC;
```

**Expected Result:** New audit record with ChangeType = "RoleAdded", NewValue = "Reader".

#### Test 8.4: Cache Invalidation

**Test Steps:**
1. Click "Invalidate Cache" button
2. Success message appears
3. Make API call to test endpoint
4. Verify authorization reflects new permissions

**Test:**
```bash
# Reader should now have access (after cache invalidation)
curl -X GET https://localhost:44101/api/userpermissions/users \
  -u "DOMAIN\reader" --negotiate -w "\n%{http_code}\n"
```

**Expected Result:** 200 OK (permission change effective immediately after cache clear).

#### Test 8.5: Validation Warnings

**Test Steps:**
1. Select endpoint
2. Click "Edit Permissions"
3. Remove all roles
4. Click "Save"

**Expected Result:** Warning message: "Endpoint will be inaccessible to all users. Are you sure?"

**Click "Confirm"** → Saves (for testing purposes)

**Verify:**
```sql
SELECT COUNT(*) FROM EndpointRolePermission WHERE EndpointId = 40;
```

**Expected Result:** 0 (no roles assigned).

**Test access:**
```bash
curl -X GET https://localhost:44101/api/userpermissions/users \
  -u "DOMAIN\superuser" --negotiate -w "\n%{http_code}\n"
```

**Expected Result:** 403 Forbidden (even SuperUser cannot access - no roles assigned).

**Restore permissions after test.**

#### Test 8.6: Sync Endpoints from Code

**Test Steps:**
1. Add a new test endpoint in code:
```csharp
group.MapGet("/test", () => "Test endpoint")
    .RequireAuthorization("HasAccess");
```
2. Rebuild and run application
3. Navigate to admin UI
4. Click "Sync Endpoints from Code"
5. Success message appears

**Verify:**
```sql
SELECT * FROM EndpointRegistry WHERE Route = '/api/test-endpoint';
```

**Expected Result:** New endpoint registered in database with default permissions.

#### Test 8.7: Bulk Operations

**Test Steps:**
1. Select multiple endpoints using checkboxes
2. Click "Bulk Edit"
3. Select action: "Add Role"
4. Choose role: ADAdmin
5. Click "Apply"
6. Confirm dialog appears
7. Click "Confirm"

**Verify:** All selected endpoints now have ADAdmin role assigned.

---

## 7. Implementation Prompt

### 7.1 Usage Instructions

When you're ready to implement this plan in a future session, use the following prompt to guide the implementation. This prompt references this document and provides step-by-step instructions.

### 7.2 Implementation Prompt Template

```
I'm ready to implement the Role Extension & Configurable Endpoint Authorization system for IkeaDocuScan.

Reference document: /app/data/IkeaDocuScan-V3/IkeaDocuScanV3/Documentation/ImplementationDetails/ROLE_EXTENSION_IMPLEMENTATION_PLAN.md

Please proceed with the 8-step implementation plan:

CURRENT STEP: [Specify step number 1-8]

For Step [X], please:
1. Read the implementation plan details from Section 5.X
2. Follow the file changes specified
3. Execute the tasks listed
4. Run the test procedures from Section 6.X
5. Confirm all tests pass before proceeding

After completing Step [X], I will review the changes and then ask you to proceed with Step [X+1].

IMPORTANT NOTES:
- Follow the plan exactly as specified
- Run ALL test procedures for each step
- Do NOT skip steps or combine steps
- Confirm test results before proceeding
- If any test fails, stop and report the issue
- Database changes are manual SQL scripts, NOT EF migrations

Please begin with Step [X].
```

### 7.3 Step-by-Step Prompts

#### Prompt for Step 1:
```
Implement Step 1: Database Schema + Manual Seed Data

Reference: ROLE_EXTENSION_IMPLEMENTATION_PLAN.md Section 5, Step 1

Tasks:
1. Create SQL scripts (01-05) from Section 4.2
2. Execute scripts in order against IkeaDocuScan database
3. Run all test procedures from Section 6.1
4. Verify:
   - 3 tables created
   - 126 endpoints seeded
   - ~500 role permissions inserted
   - All constraints and indexes created

Report test results when complete.
```

#### Prompt for Step 2:
```
Implement Step 2: Entity Classes + DbContext Updates

Reference: ROLE_EXTENSION_IMPLEMENTATION_PLAN.md Section 5, Step 2

Tasks:
1. Create entity classes: EndpointRegistry.cs, EndpointRolePermission.cs, PermissionChangeAuditLog.cs
2. Update AppDbContext.cs with DbSets and relationships
3. Do NOT create EF migrations (tables already exist)
4. Run all test procedures from Section 6.2
5. Verify build succeeds and entity queries work

Report test results when complete.
```

#### Prompt for Step 3:
```
Implement Step 3: Add ADAdmin Role to Middleware

Reference: ROLE_EXTENSION_IMPLEMENTATION_PLAN.md Section 5, Step 3

Tasks:
1. Update IkeaDocuScanOptions.cs with ADGroupADAdmin property
2. Update appsettings.json with ADAdmin group configuration
3. Update WindowsIdentityMiddleware.cs to check ADAdmin AD group
4. Run all test procedures from Section 6.3
5. Verify ADAdmin role claim added for users in AD group

Report test results when complete.
```

#### Prompt for Step 4:
```
Implement Step 4: Authorization Policy Provider (Dynamic)

Reference: ROLE_EXTENSION_IMPLEMENTATION_PLAN.md Section 5, Step 4

Tasks:
1. Create DynamicAuthorizationPolicyProvider.cs
2. Create DynamicAuthorizationHandler.cs (if needed)
3. Create IEndpointAuthorizationService.cs interface
4. Create EndpointAuthorizationService.cs with caching
5. Register services in Program.cs
6. Run all test procedures from Section 6.4
7. Verify dynamic policy resolution and caching work

Report test results when complete.
```

#### Prompt for Step 5:
```
Implement Step 5: Activate Dynamic Authorization (Single Endpoint Test)

Reference: ROLE_EXTENSION_IMPLEMENTATION_PLAN.md Section 5, Step 5

Tasks:
1. Update single endpoint: GET /api/userpermissions/users
2. Change authorization from "SuperUser" to "Endpoint:GET:/api/userpermissions/users"
3. Run all test procedures from Section 6.5
4. Verify Reader fails, ADAdmin succeeds, SuperUser succeeds
5. Verify caching works correctly

Report test results when complete.
```

#### Prompt for Step 6:
```
Implement Step 6: Cache Management + Service Layer

Reference: ROLE_EXTENSION_IMPLEMENTATION_PLAN.md Section 5, Step 6

Tasks:
1. Create IEndpointAuthorizationManagementService.cs interface
2. Create EndpointAuthorizationManagementService.cs
3. Implement all service methods (update, audit, validation, cache invalidation)
4. Register service in Program.cs
5. Run all test procedures from Section 6.6
6. Verify management operations and audit logging work

Report test results when complete.
```

#### Prompt for Step 7:
```
Implement Step 7: Migrate Existing Endpoints (31 endpoints)

Reference: ROLE_EXTENSION_IMPLEMENTATION_PLAN.md Section 5, Step 7

Tasks:
1. Update UserPermissionEndpoints.cs (10 remaining endpoints)
2. Update LogViewerEndpoints.cs (5 endpoints)
3. Update ConfigurationEndpoints.cs (5 GET endpoints)
4. Run all test procedures from Section 6.7
5. Verify ADAdmin access works for all migrated endpoints
6. Verify no regressions in unchanged endpoints

Report test results when complete.
```

#### Prompt for Step 8:
```
Implement Step 8: UI for Permission Management

Reference: ROLE_EXTENSION_IMPLEMENTATION_PLAN.md Section 5, Step 8

Tasks:
1. Create EndpointAuthorizationEndpoints.cs (10 new API endpoints)
2. Create EndpointAuthorizationManagement.razor (admin UI page)
3. Update navigation menu (SuperUser only)
4. Implement full permission management workflow
5. Run all test procedures from Section 6.8
6. Verify admin UI functional end-to-end

Report test results when complete.
```

### 7.4 Rollback Prompt

```
ROLLBACK: Role Extension Implementation

If any step fails or needs to be rolled back:

1. Revert code changes using git:
   git reset --hard HEAD

2. Rollback database changes:
   Execute: 99_Rollback_Authorization_Changes.sql

3. Verify rollback:
   - Check tables dropped: SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME LIKE 'Endpoint%'
   - Build application: dotnet build
   - Test existing functionality works

Report rollback status when complete.
```

---

## 8. Appendix

### 8.1 Key Decisions and Rationale

#### Decision 1: Manual SQL Scripts vs. EF Migrations
**Decision:** Use manual SQL scripts instead of EF Core migrations.

**Rationale:**
- Existing migration history should not be disrupted
- SQL scripts provide better control over database changes
- Easier to review and audit before applying
- Can be executed by DBA without deploying code
- Avoids potential conflicts with existing migrations

#### Decision 2: ADAdmin Role Instead of Multiple Roles
**Decision:** Create single ADAdmin role instead of separate roles for user management, log viewing, etc.

**Rationale:**
- Simpler role hierarchy
- Easier to understand and manage
- Aligns with common organizational structures (IT admins who manage users but not system config)
- Can be extended later if needed
- Reduces complexity in authorization checks

#### Decision 3: Database-Driven Authorization
**Decision:** Move authorization from hard-coded to database-driven.

**Rationale:**
- Enables changing permissions without code changes
- Allows quick response to security requirements
- Provides audit trail of permission changes
- Supports dynamic authorization scenarios
- Reduces deployment risk (no code changes for permission updates)

#### Decision 4: In-Memory Caching with 30-Minute TTL
**Decision:** Cache endpoint permissions in memory with 30-minute expiration.

**Rationale:**
- Performance: Authorization checks are frequent and should be fast
- Balance: 30 minutes is long enough for performance benefit but short enough to pick up changes
- Explicit invalidation available for immediate updates
- Scoped to application instance (doesn't require distributed cache)

### 8.2 Security Considerations

1. **SuperUser Privilege:** SuperUser retains access to all operations, including modifying their own permissions. This is intentional to prevent lockout.

2. **Permission Change Audit:** All permission changes are logged with who, what, when, and why. This supports compliance and security investigations.

3. **Validation Before Apply:** Removing all roles from an endpoint is allowed but requires explicit confirmation (prevents accidental lockout).

4. **Cache Invalidation:** Cache can be invalidated immediately via API endpoint (SuperUser only) for emergency changes.

5. **AD Group Priority:** AD group membership for SuperUser overrides database settings to prevent lockout scenarios.

### 8.3 Performance Considerations

1. **Cache Hit Rate:** Expected 99%+ cache hit rate after warm-up, significantly reducing database queries.

2. **Database Indexes:** All foreign keys and frequently queried columns have indexes for optimal performance.

3. **Query Optimization:** Use `AsNoTracking()` for read-only authorization queries.

4. **Lazy Loading:** Authorization data loaded on-demand, not eagerly.

### 8.4 Future Enhancements

Potential future improvements:

1. **Role Hierarchies:** Implement automatic role inheritance (e.g., SuperUser automatically gets all lower role permissions).

2. **Time-Based Permissions:** Allow temporary permission grants that expire automatically.

3. **Permission Groups:** Create named permission groups for easier management (e.g., "Finance Team" gets access to specific endpoints).

4. **Distributed Cache:** For multi-server deployments, use Redis or similar for shared cache.

5. **GraphQL Support:** Extend dynamic authorization to GraphQL endpoints if added.

6. **Permission Request Workflow:** Allow users to request access to specific endpoints with approval workflow.

---

**END OF IMPLEMENTATION PLAN**

---

## Approval Checklist

Before proceeding with implementation, please review and approve:

- [ ] Role definitions are appropriate for organization structure
- [ ] 86 endpoints requiring authorization changes are correct (68.3% of total)
- [ ] ADAdmin read-only access to user permissions, logs, and configuration is acceptable
- [ ] Reader role restrictions (no access to Scanned Files, Reports, Counter Parties, Reference Data) are appropriate
- [ ] Database schema design is appropriate
- [ ] Implementation steps are clear and actionable
- [ ] Test procedures are comprehensive
- [ ] Security considerations are addressed
- [ ] Rollback procedures are acceptable

**Approved by:** ________________
**Date:** ________________
**Notes:** ________________
