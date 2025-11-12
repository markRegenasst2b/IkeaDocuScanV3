# Test Identity System - Developer Guide

⚠️ **WARNING: FOR DEVELOPMENT/TESTING ENVIRONMENTS ONLY** ⚠️

## Overview

The Test Identity System allows developers to simulate different user identities with various AD group memberships and database permissions without requiring access to multiple Windows accounts or Active Directory changes.

This system is **completely disabled** in production builds and only runs when the application is compiled in DEBUG mode.

## Features

- 🎭 **Multiple Test Personas** - Pre-configured identities covering common scenarios
- 🔄 **Session Persistence** - Test identities persist across browser sessions (7 days)
- 🌐 **URL Activation** - Activate test identities via URL parameters for automation
- 🔍 **Claims Inspection** - View all active claims in real-time
- 🛡️ **Production Safe** - Completely compiled out of release builds

## Setup

### 1. Seed Test Users in Database

Run the SQL script once to create test users:

```bash
# Open SSMS or your SQL client and run:
Database/SeedTestIdentities.sql
```

This creates 6 test users with various permission levels.

### 2. Run Application in DEBUG Mode

The test identity system only works when:
- Application is compiled in DEBUG configuration
- Running in Development environment

```bash
# Build and run in DEBUG mode
dotnet build --configuration Debug
dotnet run --project IkeaDocuScan-Web/IkeaDocuScan-Web
```

## Usage

### Method 1: UI-Based (Recommended for Manual Testing)

1. Navigate to the **Home page** (`/`)
2. Locate the **"⚠️ DEVELOPER TOOLS - Test Identity Switcher"** panel (yellow card with red header)
3. Select a test identity from the dropdown
4. Click **"🎭 Apply Test Identity"**
5. Page will reload with the new identity active
6. Test your application with the selected identity
7. When done, click **"🔄 Reset to Real Identity"**

### Method 2: URL-Based (Recommended for Automation)

Activate a test identity by adding `?testUser=<profileId>` to any URL:

```
Examples:
http://localhost:44100/?testUser=superuser
http://localhost:44100/?testUser=reader
http://localhost:44100/?testUser=no_access
```

The page will redirect and activate the specified identity.

## Test Identity Profiles

### 1. **SuperUser (Database Flag)** - `superuser`
- **Username:** `TEST\SuperUserTest`
- **Database UserId:** 1001
- **IsSuperUser:** True (via database flag)
- **HasAccess:** True
- **AD Groups:** None
- **Use Case:** Test database-based SuperUser access

### 2. **SuperUser (AD Group)** - `superuser_ad`
- **Username:** `TEST\SuperUserAD`
- **Database UserId:** 1002
- **IsSuperUser:** True (via AD group)
- **HasAccess:** True
- **AD Groups:** Reader, Publisher, SuperUser
- **Use Case:** Test AD group-based SuperUser access

### 3. **Publisher** - `publisher`
- **Username:** `TEST\PublisherTest`
- **Database UserId:** 1003
- **IsSuperUser:** False
- **HasAccess:** True
- **AD Groups:** Reader, Publisher
- **Database Permissions:** Multiple document types, counter parties, countries
- **Use Case:** Test document creation/editing permissions

### 4. **Reader** - `reader`
- **Username:** `TEST\ReaderTest`
- **Database UserId:** 1004
- **IsSuperUser:** False
- **HasAccess:** True
- **AD Groups:** Reader
- **Database Permissions:** Limited to specific document types
- **Use Case:** Test read-only access

### 5. **Database Permissions Only** - `db_only`
- **Username:** `TEST\DatabaseOnlyTest`
- **Database UserId:** 1005
- **IsSuperUser:** False
- **HasAccess:** True
- **AD Groups:** None
- **Database Permissions:** Specific counter party and country access
- **Use Case:** Test database permissions without AD groups

### 6. **AD Groups Only** - `ad_only`
- **Username:** `TEST\ADOnlyTest`
- **Database UserId:** None (not in database)
- **IsSuperUser:** False
- **HasAccess:** False
- **AD Groups:** Reader, Publisher
- **Use Case:** Test user with AD groups but no database record

### 7. **No Access** - `no_access`
- **Username:** `TEST\NoAccessTest`
- **Database UserId:** 1006
- **IsSuperUser:** False
- **HasAccess:** False
- **AD Groups:** None
- **Database Permissions:** None
- **Use Case:** Test access denial and permission request flow

## Claims Generated

Each test identity generates the following claims:

- `http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name` - Username
- `http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier` - User identifier
- `http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress` - Email (if set)
- `HasAccess` - Boolean string ("True"/"False")
- `IsSuperUser` - Boolean string ("True"/"False")
- `UserId` - Database user ID (if exists)
- `http://schemas.microsoft.com/ws/2008/06/identity/claims/role` - Role claims (Reader, Publisher, SuperUser)

## Testing Scenarios

### Scenario 1: Verify Navigation Menu Authorization

1. Activate **No Access** identity
2. Verify protected menu items are hidden
3. Activate **Reader** identity
4. Verify appropriate menu items appear
5. Activate **SuperUser** identity
6. Verify all menu items are visible

### Scenario 2: Test Document Permissions

1. Activate **Reader** identity
2. Try to create/edit a document → Should be blocked
3. Activate **Publisher** identity
4. Create/edit a document → Should succeed
5. Verify permissions are enforced based on document type/counter party

### Scenario 3: Test Database Permission Filtering

1. Activate **Database Only** identity
2. View documents list
3. Verify only documents matching user's permissions are visible
4. Try accessing documents outside permissions → Should be blocked

### Scenario 4: Test Access Request Flow

1. Activate **No Access** identity
2. Navigate to restricted page
3. Verify "Request Access" flow appears
4. Test access request submission

## Technical Details

### Architecture

```
User Interface (Home.razor)
    ↓
DevIdentitySwitcher.razor (UI Component)
    ↓
TestIdentityHttpService (Client Service)
    ↓
TestIdentityEndpoints (Server API)
    ↓
TestIdentityService (Server Service)
    ↓
Session Storage (7-day persistence)
    ↓
TestIdentityMiddleware (Intercepts requests)
    ↓
ClaimsPrincipal Injection
    ↓
Normal Authorization Pipeline
```

### Middleware Order

Critical middleware order for test identities:

1. `UseSession()` - Required for persistence
2. `UseAuthentication()` - Standard auth
3. **`UseTestIdentity()`** - Injects test identity (DEBUG only)
4. `WindowsIdentityMiddleware` - Processes Windows identity
5. `UseAuthorization()` - Checks policies

### Session Storage

Test identities are stored in ASP.NET Core Session with:
- **Key:** `TestIdentity_Profile`
- **Lifetime:** 7 days (configurable)
- **Storage:** In-memory distributed cache
- **Cookies:** Secure, HttpOnly, Essential

## Security Guarantees

### Compile-Time Protection

All test identity code is wrapped in `#if DEBUG` preprocessor directives:

```csharp
#if DEBUG
// Test identity code here
#endif
```

This ensures:
- ✅ Code is completely excluded from Release builds
- ✅ No runtime checks needed
- ✅ Zero performance impact in production
- ✅ No security risk in deployed environments

### Runtime Protection

Additional safeguards:
- Endpoints only registered in DEBUG mode
- Middleware extension returns no-op in non-DEBUG builds
- Service registration conditional on DEBUG flag
- UI components conditionally rendered

### Visual Warnings

When active, test identities display:
- 🚨 Large red/yellow warning banner on Home page
- ⚠️ Warning icons throughout UI
- 🎭 Test identity indicator in current status
- Console log warnings on activation

## Troubleshooting

### Problem: Test Identity Panel Not Visible

**Solutions:**
- Ensure application is compiled in DEBUG configuration
- Check that you're running in Development environment
- Verify Home.razor includes `<DevIdentitySwitcher />` with `#if DEBUG`

### Problem: Test Identity Not Persisting

**Solutions:**
- Ensure session middleware is enabled in Program.cs
- Check browser allows cookies
- Verify session timeout hasn't expired (7 days default)

### Problem: Claims Not Showing Expected Values

**Solutions:**
- Use "Show Current Claims" button to inspect actual claims
- Verify database has test users (run SeedTestIdentities.sql)
- Check WindowsIdentityMiddleware isn't overriding test claims

### Problem: API Calls Failing

**Solutions:**
- Check Network tab for 404 errors on `/api/test-identity/*`
- Verify TestIdentityEndpoints are registered in Program.cs
- Ensure DEBUG flag is set during compilation

## Best Practices

1. **Always Reset After Testing**
   - Click "Reset to Real Identity" when done
   - Prevents confusion when switching to real testing

2. **Document Test Scenarios**
   - Keep track of which identities test which features
   - Create test checklists for regression testing

3. **Use URL Parameters for Automation**
   - Integrate with automated UI tests
   - Example: `page.goto('http://localhost:44100/?testUser=reader')`

4. **Verify in Multiple Browsers**
   - Test identities are session-based
   - Each browser/incognito window has independent session

5. **Check Claims When Debugging**
   - Use "Show Current Claims" to verify expected claims
   - Compare against what authorization policies expect

## API Reference

### Endpoints (DEBUG Only)

#### GET /api/test-identity/profiles
Returns all available test identity profiles.

**Response:**
```json
[
  {
    "profileId": "superuser",
    "displayName": "👑 Super User (Full Access)",
    "username": "TEST\\SuperUserTest",
    "email": "superuser@test.local",
    "adGroups": [],
    "isSuperUser": true,
    "hasAccess": true,
    "databaseUserId": 1001,
    "description": "Full system access via database SuperUser flag"
  }
]
```

#### GET /api/test-identity/status
Returns current test identity status.

**Response:**
```json
{
  "isActive": true,
  "currentProfile": { /* TestIdentityProfile */ },
  "activeClaims": [
    "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name: TEST\\SuperUserTest",
    "HasAccess: True",
    "IsSuperUser: True"
  ]
}
```

#### POST /api/test-identity/activate/{profileId}
Activates a test identity profile.

**Parameters:**
- `profileId` (path): Profile ID to activate (e.g., "superuser", "reader")

**Response:**
```json
{
  "success": true,
  "message": "Test identity 'superuser' activated"
}
```

#### POST /api/test-identity/reset
Removes active test identity, returning to real Windows identity.

**Response:**
```json
{
  "success": true,
  "message": "Test identity removed"
}
```

## Contributing Test Identities

To add new test identity profiles:

1. **Update SQL Script**
   - Add new user in `SeedTestIdentities.sql`
   - Choose unique UserId (suggest 1100+)
   - Add appropriate permissions

2. **Update TestIdentityService.cs**
   - Add new profile in `GetAvailableProfiles()`
   - Set appropriate claims and AD groups
   - Provide clear description

3. **Test New Profile**
   - Run SQL script to seed database
   - Activate new profile in UI
   - Verify claims and permissions work as expected

4. **Document**
   - Add to this file's profile list
   - Document intended use case
   - Note any special behaviors

## Removal for Production

Test identity code is automatically excluded from production builds. To verify:

```bash
# Build in Release configuration
dotnet build --configuration Release

# Verify test identity code is excluded
# Search compiled assemblies - should find no test identity references
```

## Support

For issues or questions:
1. Check console logs for warnings/errors
2. Use "Show Current Claims" button to debug
3. Verify database seeding was successful
4. Check middleware order in Program.cs

---

**Last Updated:** 2025-01-07
**Compatible With:** .NET 9.0, IkeaDocuScan V3
