# Step 5: Single Endpoint Dynamic Authorization Test
**Date:** 2025-11-17
**Test Endpoint:** `GET /api/userpermissions/users`
**Goal:** Verify dynamic authorization works correctly before migrating all endpoints

---

## 📋 Test Summary

**What Changed:**
- **Before:** Hard-coded `.RequireAuthorization(policy => policy.RequireRole("SuperUser"))`
- **After:** Dynamic `.RequireAuthorization("Endpoint:GET:/api/userpermissions/users")`

**Expected Behavior:**
- Policy name `"Endpoint:GET:/api/userpermissions/users"` triggers `DynamicAuthorizationPolicyProvider`
- Policy provider queries database via `EndpointAuthorizationService.GetAllowedRolesAsync()`
- Database returns: `["ADAdmin", "SuperUser"]`
- Authorization policy requires user to be in **ADAdmin OR SuperUser** role

---

## ✅ Prerequisites

1. ✅ SQL scripts executed (all 4 seed scripts)
2. ✅ Application builds successfully
3. ✅ Database contains endpoint authorization data
4. ✅ Test identity switcher is working
5. ✅ Application running in DEBUG mode

**Verify Database Data:**
```sql
-- Should return: ADAdmin, SuperUser
SELECT rp.RoleName
FROM EndpointRegistry e
INNER JOIN EndpointRolePermission rp ON e.EndpointId = rp.EndpointId
WHERE e.HttpMethod = 'GET' AND e.Route = '/api/userpermissions/users';
```

---

## 🧪 Test Procedure

### Test 1: Reader Role → Should FAIL (403)

**Setup:**
1. Open application: `https://localhost:44101`
2. Scroll to bottom → "Test Identity Switcher"
3. Select: **👁️ Reader 1**
4. Click "Apply Test Identity"
5. Wait for page reload

**Execute Test:**
```bash
# Method A: Browser
# Navigate to: https://localhost:44101/api/userpermissions/users

# Method B: curl (if auth allows)
curl -X GET https://localhost:44101/api/userpermissions/users \
  -H "Accept: application/json" \
  -v
```

**Expected Result:**
- ❌ **Status Code:** 403 Forbidden
- ❌ **Response:** (empty or error message)
- ✅ **Logs:** "User does not have required role(s): ADAdmin, SuperUser"

**Why it Fails:**
- Reader role is NOT in the allowed roles list (`["ADAdmin", "SuperUser"]`)
- Authorization policy rejects the request

---

### Test 2: Publisher Role → Should FAIL (403)

**Setup:**
1. Switch test identity to: **📝 Publisher 1**
2. Click "Apply Test Identity"
3. Wait for page reload

**Execute Test:**
```bash
curl -X GET https://localhost:44101/api/userpermissions/users \
  -H "Accept: application/json" \
  -v
```

**Expected Result:**
- ❌ **Status Code:** 403 Forbidden
- ❌ **Response:** (empty or error message)

**Why it Fails:**
- Publisher role is NOT in the allowed roles list
- Only ADAdmin and SuperUser are allowed

---

### Test 3: ADAdmin Role → Should SUCCEED (200)

**Setup:**
1. Switch test identity to: **🔧 ADAdmin (Read-Only Admin)**
2. Click "Apply Test Identity"
3. Wait for page reload

**Execute Test:**
```bash
curl -X GET https://localhost:44101/api/userpermissions/users \
  -H "Accept: application/json" \
  -v
```

**Expected Result:**
- ✅ **Status Code:** 200 OK
- ✅ **Response:** JSON array of DocuScanUser objects
- ✅ **Logs:** "Dynamic policy Endpoint:GET:/api/userpermissions/users resolved to roles: ADAdmin, SuperUser"

**Sample Response:**
```json
[
  {
    "userId": 1001,
    "accountName": "DOMAIN\\user1",
    "displayName": "User One",
    "email": "user1@company.com",
    "isSuperUser": false,
    "isActive": true
  },
  // ... more users
]
```

**Why it Succeeds:**
- ADAdmin role IS in the allowed roles list
- Authorization policy grants access

---

### Test 4: SuperUser Role → Should SUCCEED (200)

**Setup:**
1. Switch test identity to: **👑 Super User (DB Flag)**
2. Click "Apply Test Identity"
3. Wait for page reload

**Execute Test:**
```bash
curl -X GET https://localhost:44101/api/userpermissions/users \
  -H "Accept: application/json" \
  -v
```

**Expected Result:**
- ✅ **Status Code:** 200 OK
- ✅ **Response:** JSON array of DocuScanUser objects
- ✅ **Logs:** Same as Test 3

**Why it Succeeds:**
- SuperUser role IS in the allowed roles list
- Authorization policy grants access

---

## 📊 Test Results Matrix

| Test # | Role | Expected | Actual | Pass/Fail | Notes |
|--------|------|----------|--------|-----------|-------|
| 1 | Reader | 403 | ___ | ⬜ | Should be denied |
| 2 | Publisher | 403 | ___ | ⬜ | Should be denied |
| 3 | ADAdmin | 200 | ___ | ⬜ | Should succeed |
| 4 | SuperUser | 200 | ___ | ⬜ | Should succeed |

---

## 🔍 Cache Verification

### Test 5: Verify Caching Behavior

**Goal:** Confirm that database is queried once, then cached for subsequent requests.

**Procedure:**

1. **Enable Debug Logging:**
   Edit `appsettings.json`:
   ```json
   {
     "Logging": {
       "LogLevel": {
         "Default": "Debug",
         "IkeaDocuScan_Web.Services.EndpointAuthorizationService": "Debug",
         "IkeaDocuScan_Web.Authorization.DynamicAuthorizationPolicyProvider": "Debug"
       }
     }
   }
   ```

2. **Restart Application**

3. **Make First Request (ADAdmin):**
   ```bash
   curl https://localhost:44101/api/userpermissions/users
   ```

4. **Check Logs for:**
   ```
   [DEBUG] Endpoint authorization cache miss for GET /api/userpermissions/users
   [INFO] Loaded endpoint authorization for GET /api/userpermissions/users: ADAdmin, SuperUser
   [INFO] Dynamic policy Endpoint:GET:/api/userpermissions/users resolved to roles: ADAdmin, SuperUser
   ```

5. **Make Second Request (ADAdmin):**
   ```bash
   curl https://localhost:44101/api/userpermissions/users
   ```

6. **Check Logs for:**
   ```
   [DEBUG] Endpoint authorization cache hit for GET /api/userpermissions/users
   [INFO] Dynamic policy Endpoint:GET:/api/userpermissions/users resolved to roles: ADAdmin, SuperUser
   ```

**Expected Cache Behavior:**
- ✅ First request: **Cache MISS** → Database query executed
- ✅ Second request: **Cache HIT** → No database query
- ✅ Cache TTL: 30 minutes (configurable in `EndpointAuthorizationService.cs`)

---

## ⚡ Performance Verification

### Test 6: Measure Performance Overhead

**Goal:** Verify dynamic authorization adds <5ms overhead vs hard-coded.

**Baseline (Hard-coded authorization):**
- Typical response time: ~50-100ms (depends on data size)

**Dynamic Authorization:**
- First request (cache miss): +10-20ms overhead (database query)
- Subsequent requests (cache hit): +<1ms overhead (in-memory cache lookup)

**How to Measure:**

1. **Use Browser DevTools:**
   - Open DevTools → Network tab
   - Click endpoint URL
   - Check "Time" column

2. **Use curl with timing:**
   ```bash
   curl -w "\nTime Total: %{time_total}s\n" \
     -o /dev/null -s \
     https://localhost:44101/api/userpermissions/users
   ```

3. **Expected Results:**
   - First request: ~0.07s (70ms) - includes cache miss
   - Second request: ~0.05s (50ms) - cache hit
   - Overhead: ~20ms (first) → <1ms (subsequent)

**Acceptance Criteria:**
- ✅ First request overhead: <50ms
- ✅ Subsequent request overhead: <5ms
- ✅ No noticeable degradation in user experience

---

## 📝 Log Analysis

### What to Look For in Application Logs

**Success Indicators:**
```
[INFO] DynamicAuthorizationPolicyProvider: Resolving dynamic policy for Endpoint:GET:/api/userpermissions/users
[INFO] EndpointAuthorizationService: Loaded endpoint authorization for GET /api/userpermissions/users: ADAdmin, SuperUser
[INFO] DynamicAuthorizationPolicyProvider: Dynamic policy Endpoint:GET:/api/userpermissions/users resolved to roles: ADAdmin, SuperUser
[DEBUG] EndpointAuthorizationService: Endpoint authorization cache hit for GET /api/userpermissions/users
```

**Failure Indicators:**
```
[ERROR] DynamicAuthorizationPolicyProvider: Error resolving dynamic policy Endpoint:GET:/api/userpermissions/users
[WARN] EndpointAuthorizationService: No roles configured for endpoint GET /api/userpermissions/users - access denied by default
[ERROR] EndpointAuthorizationService: Failed to access EndpointRegistry table
```

**Cache Performance:**
```
[DEBUG] Endpoint authorization cache miss for GET /api/userpermissions/users  ← First request
[DEBUG] Endpoint authorization cache hit for GET /api/userpermissions/users   ← Subsequent requests
```

---

## 🐛 Troubleshooting

### Issue 1: All Roles Get 403 (Even SuperUser)

**Cause:** Database seed data missing or incorrect

**Fix:**
```sql
-- Verify endpoint exists
SELECT * FROM EndpointRegistry
WHERE HttpMethod = 'GET' AND Route = '/api/userpermissions/users';

-- Verify roles assigned
SELECT rp.RoleName
FROM EndpointRegistry e
INNER JOIN EndpointRolePermission rp ON e.EndpointId = rp.EndpointId
WHERE e.HttpMethod = 'GET' AND e.Route = '/api/userpermissions/users';

-- If no results, re-run seed script
```

### Issue 2: Reader/Publisher Get 200 (Should be 403)

**Cause:** Incorrect roles seeded in database

**Fix:**
```sql
-- Delete incorrect permissions
DELETE FROM EndpointRolePermission
WHERE EndpointId = (
  SELECT EndpointId FROM EndpointRegistry
  WHERE HttpMethod = 'GET' AND Route = '/api/userpermissions/users'
)
AND RoleName IN ('Reader', 'Publisher');

-- Verify only ADAdmin and SuperUser remain
SELECT rp.RoleName
FROM EndpointRegistry e
INNER JOIN EndpointRolePermission rp ON e.EndpointId = rp.EndpointId
WHERE e.HttpMethod = 'GET' AND e.Route = '/api/userpermissions/users';
-- Expected: ADAdmin, SuperUser only
```

### Issue 3: Cache Not Working (Database Query on Every Request)

**Cause:** Cache service not registered or disabled

**Fix:**
```csharp
// Verify in Program.cs:
builder.Services.AddMemoryCache();  // Must be registered
builder.Services.AddScoped<IEndpointAuthorizationService, EndpointAuthorizationService>();
```

### Issue 4: Policy Not Resolving (Fallback to Default)

**Cause:** DynamicAuthorizationPolicyProvider not registered as singleton

**Fix:**
```csharp
// Verify in Program.cs (line 82):
builder.Services.AddSingleton<IAuthorizationPolicyProvider, DynamicAuthorizationPolicyProvider>();
```

---

## ✅ Step 5 Success Criteria

Mark Step 5 as **COMPLETE** when:

- ✅ Test 1 (Reader): Returns 403 Forbidden
- ✅ Test 2 (Publisher): Returns 403 Forbidden
- ✅ Test 3 (ADAdmin): Returns 200 OK with data
- ✅ Test 4 (SuperUser): Returns 200 OK with data
- ✅ Test 5 (Cache): First request = cache miss, second = cache hit
- ✅ Test 6 (Performance): Overhead <5ms on cached requests
- ✅ Logs show dynamic policy resolution
- ✅ No exceptions or errors in application logs
- ✅ Authorization behavior matches access matrix

---

## 📊 Expected vs Actual Results

### Database Query Results
```sql
-- Expected roles for GET /api/userpermissions/users
SELECT rp.RoleName
FROM EndpointRegistry e
INNER JOIN EndpointRolePermission rp ON e.EndpointId = rp.EndpointId
WHERE e.HttpMethod = 'GET' AND e.Route = '/api/userpermissions/users';
```

**Expected Output:**
```
RoleName
--------
ADAdmin
SuperUser
```

### Authorization Policy Resolution

**Expected Flow:**
1. Request arrives: `GET /api/userpermissions/users`
2. Authorization attribute: `Endpoint:GET:/api/userpermissions/users`
3. `DynamicAuthorizationPolicyProvider.GetPolicyAsync()` called
4. Parses policy name → method=`GET`, route=`/api/userpermissions/users`
5. `EndpointAuthorizationService.GetAllowedRolesAsync("GET", "/api/userpermissions/users")` called
6. Database query (if cache miss) or cache hit
7. Returns: `["ADAdmin", "SuperUser"]`
8. Policy built: `RequireAuthenticatedUser().RequireRole("ADAdmin", "SuperUser")`
9. User's roles checked against policy
10. Access granted if user has **ADAdmin OR SuperUser** role

---

## 🎯 Next Steps After Step 5

Once Step 5 tests pass:

1. ✅ **Document Results:** Record test outcomes in this file
2. ✅ **Commit Changes:** `git commit -m "Step 5: Enable dynamic authorization for test endpoint"`
3. ➡️ **Proceed to Step 6:** Implement cache management and admin endpoints
4. ➡️ **Proceed to Step 7:** Migrate remaining 85 endpoints to dynamic authorization
5. ➡️ **Proceed to Step 8:** Build admin UI for permission management

---

## 📌 Reference

- **Implementation Plan:** `ROLE_EXTENSION_IMPLEMENTATION_PLAN.md` (Section 5.5)
- **Access Matrix:** `ROLE_EXTENSION_IMPLEMENTATION_PLAN.md` (Section 3.2.1)
- **Code Changes:** `UserPermissionEndpoints.cs` (Line 37)
- **Policy Provider:** `DynamicAuthorizationPolicyProvider.cs`
- **Authorization Service:** `EndpointAuthorizationService.cs`

---

**Test Date:** ___________
**Tested By:** ___________
**Result:** ⬜ PASS / ⬜ FAIL
**Notes:** ___________________________________________________________
