# Authorization Fix Summary

**Date:** 2025-01-07
**Issue:** Reader role could create/edit/delete documents and access administrative functions
**Root Cause:** API endpoints only enforced authentication, not role-based authorization

---

## Fixed Endpoints

### 1. **DocumentEndpoints.cs** ✅
**Authorization Applied:**
- **READ** (GET): All authenticated users with `HasAccess` policy
- **CREATE** (POST): `Publisher` or `SuperUser` role required
- **UPDATE** (PUT): `Publisher` or `SuperUser` role required
- **DELETE** (DELETE): `SuperUser` role only

**Impact:** Reader can now only view documents, cannot create/edit/delete.

---

### 2. **UserPermissionEndpoints.cs** ✅
**Authorization Applied:**
- **READ** (GET `/`, `/users`, `/{id}`, `/user/{userId}`): `SuperUser` role only
- **CREATE** (POST `/`, `/user`): `SuperUser` role only
- **UPDATE** (PUT `/{id}`, `/user/{userId}`): `SuperUser` role only
- **DELETE** (DELETE `/{id}`, `/user/{userId}`): `SuperUser` role only

**Impact:** Only SuperUser can manage user permissions and DocuScanUsers.

---

### 3. **DocumentTypeEndpoints.cs** ✅
**Authorization Applied:**
- **READ** (GET): All authenticated users with `HasAccess` policy
- **CREATE** (POST): `SuperUser` role only (system configuration)
- **UPDATE** (PUT): `SuperUser` role only
- **DELETE** (DELETE): `SuperUser` role only

**Impact:** Only SuperUser can configure document types.

---

### 4. **CounterPartyEndpoints.cs** ✅
**Authorization Applied:**
- **READ** (GET): All authenticated users with `HasAccess` policy
- **CREATE** (POST): `Publisher` or `SuperUser` role required
- **UPDATE** (PUT): `Publisher` or `SuperUser` role required
- **DELETE** (DELETE): `SuperUser` role only

**Impact:** Publisher can create/edit counter parties, only SuperUser can delete.

---

## Authorization Matrix

| **Endpoint**          | **Reader** | **Publisher** | **SuperUser** |
|-----------------------|------------|---------------|---------------|
| **Documents**         |            |               |               |
| - View (GET)          | ✅          | ✅             | ✅             |
| - Create (POST)       | ❌          | ✅             | ✅             |
| - Edit (PUT)          | ❌          | ✅             | ✅             |
| - Delete (DELETE)     | ❌          | ❌             | ✅             |
| **User Permissions**  |            |               |               |
| - View (GET)          | ❌          | ❌             | ✅             |
| - Create (POST)       | ❌          | ❌             | ✅             |
| - Edit (PUT)          | ❌          | ❌             | ✅             |
| - Delete (DELETE)     | ❌          | ❌             | ✅             |
| **Document Types**    |            |               |               |
| - View (GET)          | ✅          | ✅             | ✅             |
| - Create (POST)       | ❌          | ❌             | ✅             |
| - Edit (PUT)          | ❌          | ❌             | ✅             |
| - Delete (DELETE)     | ❌          | ❌             | ✅             |
| **Counter Parties**   |            |               |               |
| - View (GET)          | ✅          | ✅             | ✅             |
| - Create (POST)       | ❌          | ✅             | ✅             |
| - Edit (PUT)          | ❌          | ✅             | ✅             |
| - Delete (DELETE)     | ❌          | ❌             | ✅             |

---

## Role Definitions

### Reader
- **AD Group:** `ADGroup.Builtin.Reader`
- **Permissions:** View documents, view reference data
- **Cannot:** Create, edit, or delete anything

### Publisher
- **AD Groups:** `ADGroup.Builtin.Reader` + `ADGroup.Builtin.Publisher`
- **Permissions:** All Reader permissions + create/edit documents and counter parties
- **Cannot:** Delete documents, manage users/permissions, configure system

### SuperUser
- **AD Group:** `ADGroup.Builtin.SuperUser` OR database `IsSuperUser` flag
- **Permissions:** Full system access, all CRUD operations
- **Can:** Manage users, configure system, delete any data

---

## Endpoints Requiring Review

The following endpoints may also need role-based authorization added:

### Reference Data Endpoints
- **CountryEndpoints.cs** - Consider SuperUser for write operations
- **CurrencyEndpoints.cs** - Consider SuperUser for write operations
- **DocumentNameEndpoints.cs** - Consider Publisher/SuperUser for write operations

### Business Logic Endpoints
- **ActionReminderEndpoints.cs** - Review who can create/manage reminders
- **ScannedFileEndpoints.cs** - Already protected via file system security
- **ConfigurationEndpoints.cs** - Should require SuperUser for all operations
- **ReportEndpoints.cs** - Read-only, current authorization likely sufficient
- **AuditTrailEndpoints.cs** - Read-only, consider SuperUser access only

### Recommendations:
1. **Configuration** endpoints should be SuperUser only
2. **Reference data** (Country, Currency) should be SuperUser for write operations
3. **Audit logs** should be SuperUser only (sensitive data)

---

## Testing the Fixes

### Test with Reader Identity
```bash
# Navigate to /?testUser=reader
```

**Expected Behavior:**
- ✅ Can view documents
- ✅ Can search documents
- ❌ Cannot create document → **403 Forbidden**
- ❌ Cannot edit document → **403 Forbidden**
- ❌ Cannot delete document → **403 Forbidden**
- ❌ Cannot access user permissions page → **403 Forbidden**

### Test with Publisher Identity
```bash
# Navigate to /?testUser=publisher
```

**Expected Behavior:**
- ✅ Can view documents
- ✅ Can create documents
- ✅ Can edit documents
- ❌ Cannot delete documents → **403 Forbidden**
- ❌ Cannot manage user permissions → **403 Forbidden**
- ❌ Cannot configure document types → **403 Forbidden**

### Test with SuperUser Identity
```bash
# Navigate to /?testUser=superuser
```

**Expected Behavior:**
- ✅ Can perform ALL operations
- ✅ Full access to all endpoints

---

## Security Improvements

### Before Fix
```csharp
var group = routes.MapGroup("/api/documents")
    .RequireAuthorization()  // ⚠️ Only checks authentication!
    .WithTags("Documents");

group.MapPost("/", async (CreateDocumentDto dto, ...) => { ... })
    .WithName("CreateDocument");
```

**Problem:** ANY authenticated user could create documents.

### After Fix
```csharp
var group = routes.MapGroup("/api/documents")
    .RequireAuthorization("HasAccess")  // ✅ Check HasAccess policy
    .WithTags("Documents");

group.MapPost("/", async (CreateDocumentDto dto, ...) => { ... })
    .WithName("CreateDocument")
    .RequireAuthorization(policy => policy.RequireRole("Publisher", "SuperUser"))  // ✅ Role check!
    .Produces(403);  // ✅ Document 403 response
```

**Solution:** Only Publisher or SuperUser can create documents.

---

## HTTP Status Codes

All protected endpoints now properly return:
- **200/201/204**: Success (authorized)
- **400**: Bad Request (validation error)
- **401**: Unauthorized (not authenticated)
- **403**: Forbidden (authenticated but insufficient role)
- **404**: Not Found

**Important:** Clients must handle **403 Forbidden** responses for unauthorized role access.

---

## Client-Side Changes Recommended

### Update Error Handling
Client-side services should distinguish between 401 and 403:

```csharp
try
{
    await documentService.CreateAsync(dto);
}
catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Forbidden)
{
    // User is authenticated but doesn't have permission
    ShowError("You don't have permission to create documents. Publisher role required.");
}
catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
{
    // User is not authenticated
    RedirectToLogin();
}
```

### UI Button Visibility
Consider hiding buttons based on roles:

```razor
<AuthorizeView Roles="Publisher,SuperUser">
    <Authorized>
        <button @onclick="CreateDocument">Create Document</button>
    </Authorized>
    <NotAuthorized>
        <!-- Button hidden for Reader -->
    </NotAuthorized>
</AuthorizeView>
```

---

## Migration Notes

### Production Deployment
1. ✅ **No database changes required**
2. ✅ **No configuration changes required**
3. ⚠️ **Users with Reader role will lose write access** (intentional security fix)
4. ⚠️ **Ensure all users have correct AD group memberships before deployment**

### Rollback Plan
If issues arise, revert the following files:
- `DocumentEndpoints.cs`
- `UserPermissionEndpoints.cs`
- `DocumentTypeEndpoints.cs`
- `CounterPartyEndpoints.cs`

---

## Additional Security Recommendations

1. **Audit Logging:** Log all 403 Forbidden responses to track unauthorized access attempts
2. **Role Review:** Periodically review AD group memberships
3. **Least Privilege:** Start users as Reader, promote to Publisher only as needed
4. **SuperUser Limitation:** Minimize number of SuperUser accounts
5. **Regular Testing:** Use test identity system to verify role enforcement

---

## References

- Test Identity System: `Documentation/TEST_IDENTITY_SYSTEM.md`
- Authorization Guide: `Documentation/AUTHORIZATION_GUIDE.md`
- AD Groups Reference: `Documentation/AD_GROUPS_QUICK_REFERENCE.md`

---

**Status:** ✅ Authorization fixes implemented and ready for testing
