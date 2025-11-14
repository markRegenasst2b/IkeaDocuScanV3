# SMTP Configuration Architecture Fix

## Issue Identified

**Reporter:** User
**Date:** 2025-11-05
**Severity:** Critical Architecture Flaw

### Problem Statement

The original implementation tested SMTP connection after **EACH individual configuration setting** was saved. This created several serious problems:

1. **Inconsistent Configuration Testing**
   - Updating `SmtpHost` would test with the new host but old port/credentials
   - Updating `SmtpPort` would test with the new port but possibly mismatched host/credentials
   - Each individual update could fail even though the final complete configuration would be valid

2. **Chicken-and-Egg Problem**
   - If current SMTP settings are invalid, you cannot update them because each change would fail the test
   - Example: To fix a wrong SMTP host, you'd need to first update it, but the test would fail with the old (wrong) host

3. **Atomicity Violation**
   - SMTP configuration is not a collection of independent settings
   - It's a single cohesive unit that only makes sense when all parts are configured correctly together

4. **User Experience Issues**
   - Confusing error messages when partial updates fail
   - No clear way to "fix" broken SMTP configuration
   - Risk of leaving system in partially updated state

## Root Cause

In `ConfigurationManagerService.cs` (lines 195-205 of old version):

```csharp
// Validate configuration if it's SMTP-related
if (section == "Email" && (configKey.Contains("Smtp") || configKey == "FromAddress"))
{
    _logger.LogInformation("Testing SMTP configuration after change to {ConfigKey}", configKey);
    var testResult = await TestSmtpConnectionAsync();
    if (!testResult)
    {
        _logger.LogWarning("SMTP test failed. Rolling back configuration change to {ConfigKey}", configKey);
        throw new InvalidOperationException($"SMTP configuration test failed. Configuration not saved.");
    }
}
```

This tested SMTP after every individual setting change, leading to the problems described above.

## Solution Implemented

### 1. Remove Automatic Testing from Individual Updates

**File:** `ConfigurationManagerService.cs`
**Change:** Removed lines 195-205 (SMTP testing logic from `SetConfigurationAsync`)
**Result:** Individual configuration updates no longer test SMTP

### 2. Create Bulk SMTP Update Method

**File:** `ConfigurationManagerService.cs`
**Method:** `SetSmtpConfigurationAsync(SmtpConfigurationDto config, string changedBy, string? reason = null)`
**Lines:** 601-721

**How It Works:**
1. Accepts ALL SMTP settings in a single DTO
2. Opens a database transaction
3. Updates ALL settings (SmtpHost, SmtpPort, UseSsl, Username, Password, FromAddress, FromName)
4. Saves all changes to database
5. **Then** tests SMTP connection with the complete, consistent configuration
6. If test passes: commits transaction
7. If test fails: rolls back ALL changes (no partial updates)

```csharp
public async Task SetSmtpConfigurationAsync(SmtpConfigurationDto config, string changedBy, string? reason = null)
{
    // Use execution strategy for retry support
    var strategy = context.Database.CreateExecutionStrategy();

    await strategy.ExecuteAsync(async () =>
    {
        await using var transaction = await context.Database.BeginTransactionAsync();

        try
        {
            // Update ALL settings
            foreach (var setting in allSmtpSettings)
            {
                // Save setting...
                // Create audit trail...
            }

            await context.SaveChangesAsync();

            // Clear cache to force reading new values
            // Test SMTP with complete configuration
            var testResult = await TestSmtpConnectionAsync();

            if (!testResult)
            {
                throw new InvalidOperationException("SMTP test failed. All changes rolled back.");
            }

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    });
}
```

### 3. Create Bulk SMTP Endpoint

**File:** `ConfigurationEndpoints.cs`
**Endpoint:** `POST /api/configuration/smtp`
**Lines:** 267-305

Accepts `SmtpConfigurationDto` and calls the new bulk update method.

### 4. Create DTO

**File:** `IkeaDocuScan.Shared/DTOs/Configuration/SmtpConfigurationDto.cs`
**Purpose:** Strongly-typed model for all SMTP settings

```csharp
public class SmtpConfigurationDto
{
    public required string SmtpHost { get; set; }
    public int SmtpPort { get; set; } = 587;
    public bool UseSsl { get; set; } = true;
    public string? SmtpUsername { get; set; }
    public string? SmtpPassword { get; set; }
    public required string FromAddress { get; set; }
    public string? FromName { get; set; }
}
```

### 5. Update Interface

**File:** `IConfigurationManager.cs`
**Added:** `Task SetSmtpConfigurationAsync(SmtpConfigurationDto config, string changedBy, string? reason = null);`

### 6. Update UI Component

**File:** `SmtpSettingsEditor.razor`
**Changes:**
- Removed individual setting updates
- Now calls bulk SMTP endpoint with all settings
- Clearer error messages about atomic rollback
- Better user feedback

**Before:**
```csharp
await ConfigService.SetConfigurationAsync("Email", "SmtpHost", smtpHost, reason);
await ConfigService.SetConfigurationAsync("Email", "SmtpPort", smtpPort.ToString(), reason);
await ConfigService.SetConfigurationAsync("Email", "UseSsl", useSsl.ToString(), reason);
// ... etc
```

**After:**
```csharp
var result = await ConfigService.UpdateSmtpConfigurationAsync(new
{
    smtpHost,
    smtpPort,
    useSsl,
    smtpUsername,
    smtpPassword,
    fromAddress,
    fromName = fromDisplayName
});
```

### 7. Update Client Service

**File:** `ConfigurationHttpService.cs`
**Added:** `UpdateSmtpConfigurationAsync(object smtpConfig)` method

### 8. Update PowerShell Test Script

**File:** `Test-ConfigurationManager.ps1`
**Change:** Test 6 now uses bulk SMTP endpoint instead of individual updates

### 9. Update Documentation

**Files Updated:**
- `CONFIGURATION_API_REFERENCE.md` - Added bulk SMTP endpoint docs
- `POWERSHELL_SCRIPT_FIXES.md` - Updated to reflect new endpoint
- `CONFIGURATION_TESTING_GUIDE.md` - Updated SMTP testing scenarios

## Benefits of New Approach

### 1. Atomicity
- All SMTP settings updated in single transaction
- Either all succeed or all fail - no partial updates
- Database always in consistent state

### 2. Correct Testing
- SMTP tested with complete, consistent configuration
- No false failures from incomplete settings
- Test accurately reflects production behavior

### 3. Better User Experience
- Clear success/failure feedback
- If test fails, everything rolls back automatically
- Users understand all settings are tested together

### 4. Fixable Configuration
- Can update broken SMTP config because test happens after all changes
- No chicken-and-egg problem
- Always have a way to fix issues

### 5. Performance
- Single database round-trip instead of 7 separate updates
- Single SMTP test instead of testing after each setting
- Reduced load on SMTP server

### 6. Audit Trail
- All changes timestamped together
- Clear reason in audit log for entire update
- Easy to see complete SMTP configuration changes

## Migration Path

### For Existing Code

No migration needed. Individual configuration endpoints (`POST /api/configuration/{section}/{key}`) still work for non-SMTP settings. SMTP can still be updated individually if needed (without testing), but bulk endpoint is strongly recommended.

### For New Code

**Always use bulk SMTP endpoint:**
```http
POST /api/configuration/smtp
```

**Do not use individual endpoints for SMTP:**
```http
POST /api/configuration/Email/SmtpHost  ❌ (Still works but not recommended)
```

## Testing

### Test Case 1: Valid SMTP Configuration
```powershell
$config = @{
    smtpHost = "smtp.office365.com"
    smtpPort = 587
    useSsl = $true
    smtpUsername = "test@company.com"
    smtpPassword = "ValidPassword"
    fromAddress = "noreply@company.com"
    fromName = "Test System"
} | ConvertTo-Json

Invoke-RestMethod -Uri "https://localhost:44101/api/configuration/smtp" `
    -Method POST -Body $config -ContentType "application/json"

# Expected: 200 OK, all settings saved, SMTP test passes
```

### Test Case 2: Invalid SMTP Configuration
```powershell
$config = @{
    smtpHost = "invalid.smtp.server"
    smtpPort = 587
    useSsl = $true
    smtpUsername = "test@company.com"
    smtpPassword = "WrongPassword"
    fromAddress = "noreply@company.com"
} | ConvertTo-Json

Invoke-RestMethod -Uri "https://localhost:44101/api/configuration/smtp" `
    -Method POST -Body $config -ContentType "application/json"

# Expected: 400 Bad Request, NO settings saved (rollback), error message
```

### Test Case 3: Database Verification
```sql
-- Before update
SELECT ConfigKey, ConfigValue, ModifiedDate
FROM SystemConfigurations
WHERE ConfigSection = 'Email';

-- Run bulk SMTP update that FAILS

-- After failed update (should be identical to before)
SELECT ConfigKey, ConfigValue, ModifiedDate
FROM SystemConfigurations
WHERE ConfigSection = 'Email';

-- Verify: ModifiedDate should NOT change if SMTP test failed
```

## API Comparison

### Old Approach (Individual Updates)

```http
POST /api/configuration/Email/SmtpHost
{ "value": "smtp.office365.com" }
↓ Tests SMTP with new host + old port/credentials
↓ Fails because port 25 != 587
❌ Transaction rolled back

POST /api/configuration/Email/SmtpPort
{ "value": "587" }
↓ Tests SMTP with new port + old host
↓ Might succeed or fail depending on old host
❓ Inconsistent behavior
```

### New Approach (Bulk Update)

```http
POST /api/configuration/smtp
{
  "smtpHost": "smtp.office365.com",
  "smtpPort": 587,
  "useSsl": true,
  "smtpUsername": "user@company.com",
  "smtpPassword": "password",
  "fromAddress": "noreply@company.com"
}
↓ Saves all settings
↓ Tests SMTP with complete configuration
↓ Either all succeed or all roll back
✅ Consistent, predictable behavior
```

## Backwards Compatibility

### Breaking Changes
**None.** Individual configuration endpoints still work.

### Deprecated Patterns
Using individual endpoints for SMTP configuration is **discouraged** but not removed.

### Recommended Migration
Update all SMTP configuration code to use new bulk endpoint:
- UI components: ✅ Already updated
- PowerShell scripts: ✅ Already updated
- API consumers: Should migrate to bulk endpoint

## Future Improvements

1. **Validation Before Save**
   - Validate email format for FromAddress
   - Validate port range (1-65535)
   - Validate SmtpHost is not empty

2. **Dry Run Mode**
   - Allow testing SMTP without saving
   - Return what would be changed

3. **Configuration Diff**
   - Show user what will change before saving
   - Confirm dialog in UI

4. **Timeout Configuration**
   - Allow configuring SMTP test timeout
   - Currently hardcoded to 10 seconds

## Related Files

### Modified
- `ConfigurationManagerService.cs` - Removed auto-test, added bulk method
- `IConfigurationManager.cs` - Added bulk method signature
- `ConfigurationEndpoints.cs` - Added bulk SMTP endpoint
- `SmtpSettingsEditor.razor` - Updated to use bulk endpoint
- `ConfigurationHttpService.cs` - Added bulk update method
- `Test-ConfigurationManager.ps1` - Updated Test 6
- `CONFIGURATION_API_REFERENCE.md` - Documented new endpoint

### Created
- `SmtpConfigurationDto.cs` - DTO for bulk SMTP update
- `SMTP_CONFIGURATION_FIX.md` - This document

### Unchanged
- `EmailTemplateEditor.razor` - No changes needed
- `EmailRecipientsEditor.razor` - No changes needed
- Individual configuration endpoints - Still available

## Known Issues and Solutions

### Issue 1: Foreign Key Constraint Violation (FIXED)

**Error:** "The MERGE statement conflicted with the FOREIGN KEY constraint FK_ConfigAudit_Config"

**Root Cause:** Audit entries were created with `ConfigurationId = 0` for new configurations.

**Fix Applied:**
- Save configuration entries first to get `ConfigurationId`
- Then create audit entries with correct foreign key reference
- Method now works in 4 steps: create configs → save → create audits → save

### Issue 2: HTTP Timeout After 100 Seconds

**Error:** "The request was canceled due to the configured HttpClient.Timeout of 100 seconds elapsing"

**Root Causes:**
1. SMTP server unreachable, causing long connection attempts
2. Firewall blocking SMTP ports
3. Database performance issues

**Fixes Applied:**
1. Added `CancellationTokenSource` with 15-second timeout to SMTP test
2. All SMTP operations (Connect, Authenticate, Disconnect) now respect cancellation
3. Added validation to skip test if SmtpHost is empty
4. Better error logging with specific timeout messages

**Test Timeout Hierarchy:**
```
HTTP Request (100s max)
  └─ Database Transaction
      └─ Save Settings (< 5s typically)
      └─ SMTP Test (15s max with cancellation)
          └─ Connect (cancellable)
          └─ Authenticate (cancellable)
          └─ Disconnect (cancellable)
      └─ Commit/Rollback (< 1s)
```

**User Actions:**
- Test SMTP connectivity first: `POST /api/configuration/test-smtp`
- Verify firewall allows outbound SMTP (ports 25, 587, 465)
- Use correct SMTP settings for your mail server
- See `TROUBLESHOOTING_SMTP.md` for detailed debugging steps

## Conclusion

This fix addresses a critical architectural flaw where SMTP configuration was tested incrementally rather than atomically. The new bulk update approach ensures:

1. **Atomicity** - All or nothing
2. **Consistency** - Always testable configuration
3. **Reliability** - Predictable behavior
4. **Usability** - Clear success/failure
5. **Performance** - Fewer round-trips
6. **Fast Failure** - 15-second max for SMTP test

The fix is backwards compatible and improves both the technical architecture and user experience.

## Additional Documentation

- **TROUBLESHOOTING_SMTP.md** - Comprehensive guide for timeout and connectivity issues
- **CONFIGURATION_API_REFERENCE.md** - Complete API documentation
- **CONFIGURATION_TESTING_GUIDE.md** - Testing scenarios and examples
