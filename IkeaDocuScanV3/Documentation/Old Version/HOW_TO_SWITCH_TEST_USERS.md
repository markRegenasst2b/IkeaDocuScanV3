# How to Switch Test Users in Development

**Date:** 2025-01-20
**Environment:** Development (DEBUG mode only)

---

## Problem Solved

The test authentication handler was not properly assigning role claims to test users, causing "Permission denied" errors even when logged in as SuperUser.

**Fix Applied:**
- Updated `TestAuthenticationHandler.cs` to use `TestIdentityService`
- Handler now properly reads test profile from session
- Fallback to environment-based username detection if no profile selected

---

## Test User Profiles Available

The system has **7 test profiles** with different permission levels:

| Profile ID | Display Name | Roles | SuperUser | HasAccess |
|-----------|--------------|-------|-----------|-----------|
| `superuser` | 👑 Super User | SuperUser | ✅ Yes | ✅ Yes |
| `adadmin` | 🔧 ADAdmin | Reader, ADAdmin | ❌ No | ✅ Yes |
| `publisher` | 📝 Publisher 1 | Reader, Publisher | ❌ No | ✅ Yes |
| `publisher2` | 📝 Publisher 2 | Reader, Publisher | ❌ No | ✅ Yes |
| `reader` | 👁️ Reader 1 | Reader | ❌ No | ✅ Yes |
| `reader2` | 👁️ Reader 2 | Reader | ❌ No | ✅ Yes |
| `no_access` | 🚫 No Access | (none) | ❌ No | ❌ No |
| `no_access2` | 🚫 No Access 2 | (none) | ❌ No | ❌ No |
| `reset` | 🔄 Reset | (uses real identity) | - | - |

---

## How to Switch Test Users

### Method 1: Using API Endpoints (Recommended)

#### **Step 1: Get Available Profiles**
```bash
GET https://localhost:44101/api/test-identity/profiles
```

**Returns:**
```json
[
  {
    "profileId": "superuser",
    "displayName": "👑 Super User (DB Flag)",
    "username": "TEST\\SuperUserTest",
    "email": "superuser@test.local",
    "description": "Full system access via database SuperUser flag",
    "adGroups": [],
    "isSuperUser": true,
    "hasAccess": true,
    "databaseUserId": 1001
  },
  ...
]
```

#### **Step 2: Activate a Profile**
```bash
POST https://localhost:44101/api/test-identity/activate/superuser
```

**Response:**
```json
{
  "success": true,
  "message": "Test identity 'superuser' activated"
}
```

#### **Step 3: Check Current Status**
```bash
GET https://localhost:44101/api/test-identity/status
```

**Returns:**
```json
{
  "isActive": true,
  "currentProfile": {
    "profileId": "superuser",
    "username": "TEST\\SuperUserTest",
    ...
  },
  "activeClaims": [
    "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name: TEST\\SuperUserTest",
    "http://schemas.microsoft.com/ws/2008/06/identity/claims/role: SuperUser",
    "HasAccess: True",
    "IsSuperUser: True",
    ...
  ]
}
```

#### **Step 4: Reset to Real Identity (Optional)**
```bash
POST https://localhost:44101/api/test-identity/reset
```

---

### Method 2: Using Browser Console

Open browser console (F12) and run:

```javascript
// Activate SuperUser profile
fetch('/api/test-identity/activate/superuser', { method: 'POST' })
  .then(r => r.json())
  .then(console.log);

// Check current status
fetch('/api/test-identity/status')
  .then(r => r.json())
  .then(console.log);

// Reset to real identity
fetch('/api/test-identity/reset', { method: 'POST' })
  .then(r => r.json())
  .then(console.log);
```

---

### Method 3: Postman/Insomnia Collection

Create a collection with these requests:

```
Collection: Test Identity Management

1. GET Available Profiles
   GET https://localhost:44101/api/test-identity/profiles

2. Activate SuperUser
   POST https://localhost:44101/api/test-identity/activate/superuser

3. Activate Reader
   POST https://localhost:44101/api/test-identity/activate/reader

4. Activate Publisher
   POST https://localhost:44101/api/test-identity/activate/publisher

5. Activate ADAdmin
   POST https://localhost:44101/api/test-identity/activate/adadmin

6. Check Status
   GET https://localhost:44101/api/test-identity/status

7. Reset Identity
   POST https://localhost:44101/api/test-identity/reset
```

---

## Testing Endpoint Management Page

### Test Scenario 1: SuperUser Access (Should Work)

1. **Activate SuperUser:**
   ```bash
   POST /api/test-identity/activate/superuser
   ```

2. **Navigate to Endpoint Management:**
   ```
   https://localhost:44101/endpoint-management
   ```

3. **Expected Results:**
   - ✅ Page loads successfully
   - ✅ Can see all endpoints
   - ✅ Can toggle checkboxes
   - ✅ Changes save successfully
   - ✅ No "Permission denied" errors

### Test Scenario 2: Non-SuperUser Access (Should Fail)

1. **Activate Reader:**
   ```bash
   POST /api/test-identity/activate/reader
   ```

2. **Try to Navigate to Endpoint Management:**
   ```
   https://localhost:44101/endpoint-management
   ```

3. **Expected Results:**
   - ❌ 403 Forbidden error
   - ❌ Menu item "Endpoint Permissions" not visible
   - ✅ Other menu items visible based on Reader permissions

### Test Scenario 3: Menu Visibility

1. **Activate Different Profiles:**
   - SuperUser → Should see "Endpoint Permissions" menu
   - ADAdmin → Should NOT see "Endpoint Permissions" menu
   - Publisher → Should NOT see "Endpoint Permissions" menu
   - Reader → Should NOT see "Endpoint Permissions" menu

---

## Troubleshooting

### Issue: Still Getting "Permission Denied" Error

**Check 1: Is Test Profile Active?**
```bash
GET /api/test-identity/status
```
Should return `"isActive": true`

**Check 2: Does Profile Have SuperUser Role?**
```bash
GET /api/test-identity/status
```
Check `activeClaims` for `"role: SuperUser"`

**Check 3: Are Endpoints Seeded in Database?**
```sql
SELECT * FROM EndpointRegistry WHERE Category = 'Endpoint Authorization'
```
Should return 10 endpoints.

**Check 4: Does SuperUser Role Have Permissions?**
```sql
SELECT er.Route, erp.RoleName
FROM EndpointRegistry er
JOIN EndpointRolePermission erp ON er.EndpointId = erp.EndpointId
WHERE er.Route = '/api/endpoint-authorization/endpoints/{id}/roles'
  AND erp.RoleName = 'SuperUser'
```
Should return 1 row.

### Issue: Session Lost After Page Refresh

This is **expected behavior**. Test identity is stored in session which may reset on app restart.

**Solution:** Re-activate the test profile after each application restart:
```bash
POST /api/test-identity/activate/superuser
```

### Issue: Fallback Username Detection Not Working

The handler has a **fallback** that detects roles from your OS username if no test profile is active.

**For it to work, your OS username must contain:**
- `"superuser"` → Gets SuperUser role
- `"adadmin"` → Gets ADAdmin role
- `"publisher"` → Gets Publisher role
- `"reader"` → Gets Reader role

**Example:**
- Username: `john_superuser` → Gets SuperUser role ✅
- Username: `john_admin` → No role ❌ (must contain exact keyword)

---

## For Production / Windows Authentication

On Windows with Active Directory:
- Test identity system is **disabled** (DEBUG mode only)
- `WindowsIdentityMiddleware` takes over
- Roles assigned from:
  1. Database `IsSuperUser` flag
  2. AD group membership
- No manual profile switching needed

---

## Related Files

- **Handler:** `TestAuthenticationHandler.cs` (line 38-57)
- **Service:** `Services/TestIdentityService.cs` (line 249-287)
- **Endpoints:** `Endpoints/TestIdentityEndpoints.cs`
- **Client Service:** `IkeaDocuScan-Web.Client/Services/TestIdentityHttpService.cs`

---

## Quick Reference Card

```bash
# === QUICK COMMANDS ===

# 1. Switch to SuperUser
curl -X POST https://localhost:44101/api/test-identity/activate/superuser

# 2. Check who you are
curl https://localhost:44101/api/test-identity/status

# 3. Test endpoint management access
curl https://localhost:44101/api/endpoint-authorization/endpoints

# 4. Reset to real identity
curl -X POST https://localhost:44101/api/test-identity/reset
```

---

**Last Updated:** 2025-01-20
**Author:** Endpoint Management Bugfix
