# Missing Endpoint Permission Definitions

**Date:** 2025-11-20
**Purpose:** Document endpoints that exist in source code but have no permission definitions in ROLE_EXTENSION_IMPLEMENTATION_PLAN.md

---

## Summary

The following endpoints are **NOT INCLUDED** in the database sync script because they either:
1. Are DEBUG-only endpoints (excluded intentionally)
2. Have no implementation yet (Endpoint Authorization Management)
3. Need permission definitions to be added to the plan

**Total Missing:** 19 endpoints

---

## 1. Endpoint Authorization Management (NOT IMPLEMENTED YET)

**Status:** ❌ **Source code does not exist yet**
**Expected Location:** `IkeaDocuScan-Web/Endpoints/EndpointAuthorizationEndpoints.cs`
**Required for:** Dynamic permission management UI

These 10 endpoints are defined in the implementation plan but have no source code implementation:

| HTTP | Endpoint | Proposed Role(s) | Description |
|------|----------|------------------|-------------|
| GET | `/api/endpoint-authorization/endpoints` | SuperUser | Get all endpoints in registry |
| GET | `/api/endpoint-authorization/endpoints/{id}` | SuperUser | Get endpoint details |
| GET | `/api/endpoint-authorization/endpoints/{id}/roles` | SuperUser | Get roles for endpoint |
| POST | `/api/endpoint-authorization/endpoints/{id}/roles` | SuperUser | Update roles for endpoint |
| GET | `/api/endpoint-authorization/roles` | SuperUser | Get all available roles |
| GET | `/api/endpoint-authorization/audit` | SuperUser | Get permission change audit log |
| POST | `/api/endpoint-authorization/cache/invalidate` | SuperUser | Invalidate authorization cache |
| POST | `/api/endpoint-authorization/sync` | SuperUser | Sync endpoints from code to database |
| GET | `/api/endpoint-authorization/check` | All Roles | Check if user can access specific endpoint |
| POST | `/api/endpoint-authorization/validate` | SuperUser | Validate permission changes before applying |

**Action Required:**
- Create `EndpointAuthorizationEndpoints.cs` file
- Implement all 10 endpoints as per the plan
- Once implemented, add to database sync script

---

## 2. Diagnostic Endpoints (DEBUG-ONLY - EXCLUDED)

**Status:** ✅ **Implemented but intentionally excluded**
**Location:** `IkeaDocuScan-Web/Endpoints/DiagnosticEndpoints.cs`
**Reason:** These are DEBUG-only endpoints wrapped in `#if DEBUG` conditional compilation

These 5 endpoints exist only in DEBUG builds:

| HTTP | Endpoint | Current Auth | Description |
|------|----------|--------------|-------------|
| GET | `/api/diagnostic/db-connection` | SuperUser | Test database connection |
| GET | `/api/diagnostic/endpoint-registry` | SuperUser | Test EndpointRegistry table access |
| GET | `/api/diagnostic/endpoint-role-permission` | SuperUser | Test EndpointRolePermission table access |
| GET | `/api/diagnostic/permission-audit-log` | SuperUser | Test PermissionChangeAuditLog table access |
| GET | `/api/diagnostic/all-tables` | SuperUser | Test all authorization tables |

**Action Required:** None - these are DEBUG-only and should NOT be in production database.

---

## 3. Test Identity Endpoints (DEBUG-ONLY - EXCLUDED)

**Status:** ✅ **Implemented but intentionally excluded**
**Location:** `IkeaDocuScan-Web/Endpoints/TestIdentityEndpoints.cs`
**Reason:** These are DEBUG-only endpoints wrapped in `#if DEBUG` conditional compilation

These 4 endpoints exist only in DEBUG builds:

| HTTP | Endpoint | Current Auth | Description |
|------|----------|--------------|-------------|
| GET | `/api/test-identity/profiles` | SuperUser | Get available test identity profiles |
| GET | `/api/test-identity/status` | SuperUser | Get current test identity status |
| POST | `/api/test-identity/activate/{profileId}` | SuperUser | Activate test identity |
| POST | `/api/test-identity/reset` | SuperUser | Reset to real identity |

**Action Required:** None - these are DEBUG-only and should NOT be in production database.

---

## 4. Configuration - Missing Permission Definition

**Status:** ⚠️ **Needs permission definition**
**Issue:** This endpoint was in the old database but is NOT in the ROLE_EXTENSION_IMPLEMENTATION_PLAN.md

| HTTP | Endpoint | Old Database Roles | Recommendation |
|------|----------|-------------------|----------------|
| GET | `/api/configuration/all-sections` | SuperUser | Should this be ADAdmin readable? Or SuperUser only? |

**Source Code Status:** Not found in `ConfigurationEndpoints.cs` - may need to be implemented or removed from old database data.

**Action Required:**
1. Determine if this endpoint should exist
2. If yes, implement in `ConfigurationEndpoints.cs`
3. Define permissions (suggest: ADAdmin + SuperUser for read access)
4. Add to implementation plan

---

## 5. Identity Endpoint - Missing from Implementation Plan

**Status:** ⚠️ **Exists in source but not fully documented in plan**
**Issue:** Listed in old database but relationship to plan unclear

| HTTP | Endpoint | Old Database Roles | Implemented In |
|------|----------|-------------------|----------------|
| GET | `/api/identity/current-user` | Reader, Publisher, ADAdmin, SuperUser | **NOT FOUND** - may be same as `/api/user/identity` |

**Current Implementation:**
- `/api/user/identity` endpoint exists in `UserIdentityEndpoints.cs`
- Returns same data (user identity information)
- Accessible to all authenticated users

**Analysis:**
- Old database has `/api/identity/current-user`
- Source code has `/api/user/identity` (EndpointId 114 in sync script)
- These likely serve the same purpose

**Action Required:**
1. Verify if these are the same endpoint (route change)
2. If different, implement missing endpoint
3. If same, use `/api/user/identity` (already in sync script)

---

## 6. Old Audit Trail Endpoints (REPLACED)

**Status:** ✅ **Replaced with new route pattern**
**Issue:** Database had `/audit-trail` routes, source code uses `/audittrail` routes

### Old Database Routes (REMOVED):
| HTTP | Endpoint | EndpointName |
|------|----------|--------------|
| GET | `/api/audit-trail/` | GetAuditTrail |
| POST | `/api/audit-trail/search` | SearchAuditTrail |
| GET | `/api/audit-trail/users` | GetAuditUsers |
| GET | `/api/audit-trail/actions` | GetAuditActions |
| GET | `/api/audit-trail/export` | ExportAuditTrail |
| GET | `/api/audit-trail/{id}` | GetAuditTrailById |
| POST | `/api/audit-trail/` | LogAuditTrail |

### New Source Code Routes (IMPLEMENTED):
| HTTP | Endpoint | EndpointName |
|------|----------|--------------|
| POST | `/api/audittrail/` | LogAuditTrail |
| POST | `/api/audittrail/document/{documentId}` | LogAuditTrailByDocument |
| POST | `/api/audittrail/batch` | LogAuditTrailBatch |
| GET | `/api/audittrail/barcode/{barCode}` | GetAuditTrailByBarCode |
| GET | `/api/audittrail/user/{username}` | GetAuditTrailByUser |
| GET | `/api/audittrail/recent` | GetRecentAuditTrail |
| GET | `/api/audittrail/daterange` | GetAuditTrailByDateRange |

**Action Required:** None - old routes removed, new routes in sync script with proper permissions (Publisher, ADAdmin, SuperUser).

---

## 7. Excel Endpoints - Potential Mismatches

**Status:** ⚠️ **Needs review**
**Issue:** Old database had additional Excel endpoints not in current source

### Missing from Source Code:
| HTTP | Endpoint | Old Database Name | Status |
|------|----------|-------------------|--------|
| GET | `/api/excel/preview` | PreviewExcel | ❌ Not implemented |
| POST | `/api/excel/export` | ExportExcel | ❌ Duplicate/old version? |
| GET | `/api/excel/template/{templateName}` | GetExcelTemplate | ❌ Not implemented |
| POST | `/api/excel/import` | ImportExcel | ❌ Not implemented |

### Implemented in Source Code:
| HTTP | Endpoint | Source Code Name | Status |
|------|----------|------------------|--------|
| POST | `/api/excel/export/documents` | ExportDocumentsToExcel | ✅ Included (ID 107) |
| POST | `/api/excel/validate/documents` | ValidateExportSize | ✅ Included (ID 108) |
| GET | `/api/excel/metadata/documents` | GetDocumentExportMetadata | ✅ Included (ID 109) |
| POST | `/api/excel/export/by-ids` | ExportDocumentsByIdsToExcel | ✅ Included (ID 110) |

**Action Required:**
1. Determine if preview/template/import endpoints are needed
2. If yes, implement them in `ExcelExportEndpoints.cs`
3. Define permissions (suggest same as export: All roles)
4. Add to sync script

---

## 8. Email - Minor Discrepancy

**Status:** ⚠️ **Needs verification**
**Issue:** Old database had singular vs plural endpoint

### Old Database:
| HTTP | Endpoint | Old Database Name |
|------|----------|-------------------|
| POST | `/api/email/send-with-attachment` | SendEmailWithAttachment (SINGULAR) |
| GET | `/api/email/test` | TestEmail |

### Source Code Implementation:
| HTTP | Endpoint | Source Code Name |
|------|----------|------------------|
| POST | `/api/email/send-with-attachments` | SendEmailWithAttachments (PLURAL) - ✅ Included (ID 112) |
| GET | `/api/email/test` | TestEmail - ❌ **Not found in source** |

**Action Required:**
1. Verify `/api/email/test` endpoint - implement if needed
2. If implemented, add permission definition (suggest: Publisher, ADAdmin, SuperUser)
3. Add to sync script

---

## Summary of Actions Required

| Priority | Action | Endpoint Count | Status |
|----------|--------|----------------|--------|
| 🔴 **HIGH** | Implement Endpoint Authorization Management | 10 | Blocked - needed for permission UI |
| 🟡 **MEDIUM** | Clarify `/api/configuration/all-sections` | 1 | Needs decision |
| 🟡 **MEDIUM** | Clarify `/api/identity/current-user` vs `/api/user/identity` | 1 | Needs verification |
| 🟡 **MEDIUM** | Review Excel preview/template/import endpoints | 3 | Needs decision |
| 🟡 **MEDIUM** | Implement `/api/email/test` endpoint | 1 | Needs decision |
| 🟢 **LOW** | Document DEBUG-only exclusions | 9 | ✅ Documented here |
| 🟢 **LOW** | Document audit trail route changes | 7 | ✅ Documented here |

**Total Endpoints Needing Attention:** 16 endpoints
**Total DEBUG-only (Excluded):** 9 endpoints
**Total Already Handled:** 7 endpoints (audit trail route changes)

---

## Recommendations

### Immediate Actions:
1. **Implement Endpoint Authorization Management** - This is critical for the dynamic permission system
2. **Clarify identity endpoint** - Determine if `/api/identity/current-user` is needed or if `/api/user/identity` replaces it
3. **Review Excel endpoints** - Decide if preview/template/import functionality is needed

### Future Considerations:
1. Add `all-sections` configuration endpoint if ADAdmin role needs to see all config sections
2. Implement email test endpoint if SMTP testing is needed from API
3. Consider adding a sync endpoint to automatically detect source code endpoints and update database

---

## Notes for Implementation Plan Update

When the missing endpoints are implemented, update `ROLE_EXTENSION_IMPLEMENTATION_PLAN.md` with:
1. Complete endpoint definitions
2. Permission matrix entries
3. Implementation step details
4. Test procedures

Then re-run the sync script to include the new endpoints.
