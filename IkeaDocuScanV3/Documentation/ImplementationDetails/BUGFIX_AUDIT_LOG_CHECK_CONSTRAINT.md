# Audit Log CHECK Constraint Violation Fix

**Date:** 2025-01-20
**Issue:** DbUpdateException - CHECK constraint violation on PermissionChangeAuditLog
**Status:** ✅ FIXED

---

## Problem

When updating endpoint roles, the application crashed with:

```
Microsoft.Data.SqlClient.SqlException: The INSERT statement conflicted with the CHECK constraint "CHK_PermissionChangeAuditLog_ChangeType".
The conflict occurred in database "IkeaDocuScan", table "dbo.PermissionChangeAuditLog", column 'ChangeType'.
```

## Root Cause

The `PermissionChangeAuditLog` table has a CHECK constraint that only allows these `ChangeType` values:
- `'RoleAdded'`
- `'RoleRemoved'`
- `'EndpointCreated'`
- `'EndpointModified'`
- `'EndpointDeactivated'`
- `'EndpointReactivated'`

But the code in `EndpointAuthorizationManagementService.cs` was using **invalid** values:
- ❌ `"RolePermissionUpdate"` (line 113)
- ❌ `"EndpointMetadataUpdate"` (line 218)

These values were not in the allowed list, causing the database constraint violation.

---

## Solution

### File: `EndpointAuthorizationManagementService.cs`

**Change 1 - UpdateEndpointRolesAsync (line 113):**
```csharp
// BEFORE (INVALID):
ChangeType = "RolePermissionUpdate",

// AFTER (VALID):
ChangeType = "EndpointModified",
```

**Change 2 - UpdateEndpointAsync (line 218):**
```csharp
// BEFORE (INVALID):
ChangeType = "EndpointMetadataUpdate",

// AFTER (VALID):
ChangeType = "EndpointModified",
```

---

## Why "EndpointModified"?

The CHECK constraint defines these change types:

| ChangeType | Purpose |
|------------|---------|
| `RoleAdded` | Individual role was added to endpoint |
| `RoleRemoved` | Individual role was removed from endpoint |
| `EndpointCreated` | New endpoint registered in system |
| **`EndpointModified`** | **Endpoint metadata or bulk role changes** |
| `EndpointDeactivated` | Endpoint disabled |
| `EndpointReactivated` | Endpoint re-enabled |

Since both methods were performing bulk updates (not adding/removing individual roles), `"EndpointModified"` is the correct semantic choice.

---

## Files Modified

1. **EndpointAuthorizationManagementService.cs**
   - Line 113: Changed `"RolePermissionUpdate"` → `"EndpointModified"`
   - Line 218: Changed `"EndpointMetadataUpdate"` → `"EndpointModified"`

---

## Testing

### Before Fix:
```
❌ POST /api/endpoint-authorization/endpoints/7/roles
Error: CHECK constraint violation
Status: 500 Internal Server Error
```

### After Fix:
```
✅ POST /api/endpoint-authorization/endpoints/7/roles
Audit log created successfully with ChangeType='EndpointModified'
Status: 200 OK
```

---

## Prevention

To prevent this in the future, consider:

1. **Create an enum** for ChangeType values:
   ```csharp
   public enum PermissionChangeType
   {
       RoleAdded,
       RoleRemoved,
       EndpointCreated,
       EndpointModified,
       EndpointDeactivated,
       EndpointReactivated
   }
   ```

2. **Use the enum** in the entity:
   ```csharp
   [Required]
   public PermissionChangeType ChangeType { get; set; }
   ```

3. **Database migration** to change column from string to int (enum value)

This would catch the error at compile time instead of runtime.

---

## Related

- **CHECK Constraint SQL:** `03_Create_PermissionChangeAuditLog_Table.sql`
- **Service Implementation:** `EndpointAuthorizationManagementService.cs`
- **Entity Definition:** `PermissionChangeAuditLog.cs`

---

## Summary

**The Bug:** Code used `"RolePermissionUpdate"` and `"EndpointMetadataUpdate"` which violated the database CHECK constraint.

**The Fix:** Changed both to `"EndpointModified"` which is the correct semantic value from the allowed list.

**The Result:** ✅ Audit logging now works correctly without constraint violations.
