# Endpoint Management HTTP Service - Hanging Request Bugfix

**Date:** 2025-01-20
**Issue:** HTTP requests hanging indefinitely on line 96 of `EndpointManagementHttpService.cs`
**Status:** ✅ Fixed

---

## Problem Description

### Symptoms
- HTTP call to `/api/endpoint-authorization/validate` hangs on line 96
- Never returns to line 100 or catch block
- Application appears frozen
- No error message displayed to user
- Debugger shows request never completes

### Root Cause Analysis

The issue was caused by **three missing safeguards**:

#### 1. **No Request Timeout**
```csharp
// BEFORE (BROKEN):
var response = await _http.PostAsJsonAsync(
    "/api/endpoint-authorization/validate",
    dto);
```

Blazor WebAssembly's HttpClient has **no default timeout**, so failed requests hang indefinitely waiting for a response that never comes.

#### 2. **No Authorization Error Handling**
The endpoint requires: `.RequireAuthorization("Endpoint:POST:/api/endpoint-authorization/validate")`

If the endpoint is **not seeded in the database**, the authorization check fails silently and the request hangs because:
- Authorization policy can't find the endpoint in the database
- Server doesn't respond with 403/401
- Client waits forever

#### 3. **Missing Exception Handling for Timeouts**
The generic catch block didn't distinguish between:
- Network timeouts (`TaskCanceledException`)
- Authorization failures (`HttpRequestException` with 403/401)
- Other HTTP errors

---

## Solution Implemented

### Changes Made to `EndpointManagementHttpService.cs`

#### 1. **Added Timeout Protection (30 seconds)**

```csharp
// AFTER (FIXED):
using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

var response = await _http.PostAsJsonAsync(
    "/api/endpoint-authorization/validate",
    dto,
    cts.Token);  // Pass cancellation token
```

**Why 30 seconds?**
- Gives server enough time to respond
- Prevents indefinite hanging
- User gets timely feedback if something is wrong

#### 2. **Added Authorization Error Handling**

```csharp
// Check for authorization failures BEFORE EnsureSuccessStatusCode()
if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
{
    return new ValidatePermissionChangeResult
    {
        IsValid = false,
        ValidationErrors = new List<string> {
            "Permission denied: You do not have access to validate permissions"
        }
    };
}

if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
{
    return new ValidatePermissionChangeResult
    {
        IsValid = false,
        ValidationErrors = new List<string> {
            "Not authenticated: Please log in"
        }
    };
}
```

**Why check status codes manually?**
- `EnsureSuccessStatusCode()` throws generic exceptions
- We want user-friendly error messages
- Allows graceful degradation instead of crashes

#### 3. **Added Specific Exception Handlers**

```csharp
catch (TaskCanceledException ex)
{
    _logger.LogError(ex, "Validation request timed out");
    return new ValidatePermissionChangeResult
    {
        IsValid = false,
        ValidationErrors = new List<string> {
            "Request timed out. Please check your connection and try again."
        }
    };
}
catch (HttpRequestException ex)
{
    _logger.LogError(ex, "HTTP error: {StatusCode}", ex.StatusCode);
    return new ValidatePermissionChangeResult
    {
        IsValid = false,
        ValidationErrors = new List<string> {
            $"Connection error: {ex.Message}"
        }
    };
}
```

---

## Files Modified

### 1. **EndpointManagementHttpService.cs**

**Methods Updated:**
- ✅ `GetAllEndpointsAsync()` - Added timeout and 403 handling
- ✅ `UpdateEndpointRolesAsync()` - Added timeout and 403 handling
- ✅ `ValidatePermissionChangeAsync()` - **Full fix with timeout + 401/403 handling**

**Methods Left Unchanged:**
- `GetAuditLogAsync()` - Read-only, less critical
- `InvalidateCacheAsync()` - Already has non-throwing behavior

---

## Testing the Fix

### Test 1: Normal Operation (Endpoints Seeded)
**Expected:** Requests complete successfully within 1-2 seconds

```csharp
// Should return validation result
var result = await service.ValidatePermissionChangeAsync(new ValidatePermissionChangeDto
{
    EndpointId = 1,
    RoleNames = new List<string> { "SuperUser" }
});

Assert.IsTrue(result.IsValid);
```

### Test 2: Timeout Scenario (Network Failure)
**Expected:** Timeout after 30 seconds with clear error message

**Simulate:** Disconnect network or use invalid server URL

**Result:**
```
ValidationErrors: ["Request timed out. Please check your connection and try again."]
```

### Test 3: Authorization Failure (Endpoints Not Seeded)
**Expected:** 403 Forbidden with clear error message

**Simulate:** Don't run the seeding script, or login as non-SuperUser

**Result:**
```
ValidationErrors: ["Permission denied: You do not have access to validate permissions"]
```

### Test 4: Not Authenticated
**Expected:** 401 Unauthorized with clear error message

**Simulate:** Clear authentication cookies

**Result:**
```
ValidationErrors: ["Not authenticated: Please log in"]
```

---

## How to Verify the Fix

### Before Building/Testing:

1. **Run the database seeding script:**
   ```bash
   sqlcmd -S localhost -d IkeaDocuScan -i 32_Seed_EndpointAuthorization_Endpoints_CORRECTED.sql
   ```

2. **Verify endpoints are seeded:**
   ```sql
   SELECT * FROM EndpointRegistry
   WHERE Category = 'Endpoint Authorization'
   ```
   Should return 10 endpoints.

3. **Verify SuperUser has permissions:**
   ```sql
   SELECT er.Route, erp.RoleName
   FROM EndpointRegistry er
   JOIN EndpointRolePermission erp ON er.EndpointId = erp.EndpointId
   WHERE er.Route = '/api/endpoint-authorization/validate'
   ```
   Should return `SuperUser` role.

### After Building:

4. **Test as SuperUser:**
   - Login as `superuser`
   - Navigate to `/endpoint-management`
   - Toggle a checkbox
   - Should complete within 1-2 seconds
   - Should show success message

5. **Test as Non-SuperUser (optional):**
   - Login as `reader`
   - Try to navigate to `/endpoint-management`
   - Should show 403/Unauthorized (not hang)

---

## Prevention: Best Practices for HTTP Calls in Blazor WASM

### ✅ Always Add Timeouts
```csharp
using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
var response = await _http.GetAsync(url, cts.Token);
```

### ✅ Always Handle Authorization Failures
```csharp
if (response.StatusCode == HttpStatusCode.Forbidden)
{
    // Return user-friendly error
}
```

### ✅ Always Catch Specific Exceptions
```csharp
catch (TaskCanceledException) { /* Timeout */ }
catch (HttpRequestException) { /* Network error */ }
catch (Exception) { /* Other errors */ }
```

### ✅ Always Provide User Feedback
```csharp
// Good: User-friendly message
ValidationErrors = new List<string> {
    "Request timed out. Please check your connection."
};

// Bad: Generic exception message
ValidationErrors = new List<string> { ex.Message };
```

---

## Related Documentation

- **Seeding Scripts:** `DbMigration/db-scripts/32_README_EndpointAuthorization.md`
- **Implementation Guide:** `Documentation/ImplementationDetails/ENDPOINT_MANAGEMENT_IMPLEMENTATION.md`
- **Authorization Policy:** `IkeaDocuScan-Web/Authorization/EndpointAuthorizationHandler.cs`

---

## Summary

### The Bug
HTTP requests hung indefinitely because:
1. No timeout configured
2. Authorization failures caused silent hangs
3. No specific exception handling

### The Fix
Added to all critical HTTP methods:
1. ✅ 30-second timeout with `CancellationTokenSource`
2. ✅ Explicit 403/401 status code checks
3. ✅ Specific exception handlers (`TaskCanceledException`, `HttpRequestException`)
4. ✅ User-friendly error messages

### Result
- No more hanging requests
- Clear error messages for users
- Better debugging information in logs
- Graceful degradation when authorization fails

---

**Status:** ✅ **FIXED** - Ready for testing
