# SMTP Timeout Fix Summary

## Problem

You were experiencing: **"The request was canceled due to the configured HttpClient.Timeout of 100 seconds elapsing"**

## Root Causes Identified

1. **Execution Strategy Retries**: The SQL Server execution strategy was retrying the entire operation multiple times. Each retry included a 15-second SMTP test, causing the total time to exceed 100 seconds.

2. **SMTP Server Unreachable**: If your SMTP server is unreachable (firewall, wrong host, etc.), the connection attempts would hang even with timeouts.

## Fixes Applied

### Fix 1: Removed Execution Strategy for SMTP Updates

**File:** `ConfigurationManagerService.cs:625-768`

**Before:**
```csharp
var strategy = context.Database.CreateExecutionStrategy();
await strategy.ExecuteAsync(async () => {
    // ... transaction with SMTP test ...
    // If SMTP test fails, strategy retries multiple times!
});
```

**After:**
```csharp
// Direct transaction - no retries
await using var transaction = await context.Database.BeginTransactionAsync();
try {
    // ... save settings ...
    // ... test SMTP once ...
    await transaction.CommitAsync();
}
catch {
    await transaction.RollbackAsync();
    throw;
}
```

**Result:** SMTP update now completes in ~20 seconds max (15s test + 5s database).

### Fix 2: Added `skipTest` Parameter

**Signature:**
```csharp
Task SetSmtpConfigurationAsync(
    SmtpConfigurationDto config,
    string changedBy,
    string? reason = null,
    bool skipTest = false  // NEW PARAMETER
);
```

**Usage:**
- `skipTest = false` (default): Tests SMTP connection, rolls back if test fails
- `skipTest = true`: Saves settings immediately without testing (⚠️ use with caution)

### Fix 3: Updated API Endpoint

**Endpoint:** `POST /api/configuration/smtp?skipTest=true`

**Request Body:**
```json
{
  "smtpHost": "smtp.office365.com",
  "smtpPort": 587,
  "useSsl": true,
  "smtpUsername": "user@company.com",
  "smtpPassword": "password",
  "fromAddress": "noreply@company.com",
  "fromName": "System"
}
```

**Response (skipTest = true):**
```json
{
  "success": true,
  "message": "SMTP configuration saved without testing (validation skipped)",
  "tested": false
}
```

## Immediate Workaround - Use This Now!

### Option 1: Quick PowerShell Script (Recommended)

Run the emergency update script:

```powershell
.\Update-SMTP-SkipTest.ps1 -SkipCertificateCheck
```

This script will:
1. Prompt you for SMTP settings
2. Save them WITHOUT testing
3. Complete in < 5 seconds

### Option 2: Manual API Call

```powershell
$config = @{
    smtpHost = "smtp.office365.com"
    smtpPort = 587
    useSsl = $true
    smtpUsername = "your-email@company.com"
    smtpPassword = "your-password"
    fromAddress = "noreply@company.com"
    fromName = "DocuScan System"
} | ConvertTo-Json

# Add ?skipTest=true to query string to skip SMTP testing
Invoke-RestMethod -Uri "https://localhost:44101/api/configuration/smtp?skipTest=true" `
    -Method POST `
    -Body $config `
    -ContentType "application/json" `
    -UseDefaultCredentials `
    -SkipCertificateCheck
```

### Option 3: Direct Database Update (Emergency)

```sql
-- Insert/Update SMTP settings directly
MERGE SystemConfigurations AS target
USING (VALUES
    ('Email', 'SmtpHost', 'smtp.office365.com'),
    ('Email', 'SmtpPort', '587'),
    ('Email', 'UseSsl', 'True'),
    ('Email', 'SmtpUsername', 'user@company.com'),
    ('Email', 'SmtpPassword', 'password'),
    ('Email', 'FromAddress', 'noreply@company.com'),
    ('Email', 'FromName', 'DocuScan System')
) AS source (Section, [Key], Value)
ON target.ConfigSection = source.Section AND target.ConfigKey = source.[Key]
WHEN MATCHED THEN
    UPDATE SET ConfigValue = source.Value, ModifiedDate = GETUTCDATE(), ModifiedBy = 'ADMIN'
WHEN NOT MATCHED THEN
    INSERT (ConfigSection, ConfigKey, ConfigValue, ValueType, IsActive, IsOverride, CreatedBy, CreatedDate)
    VALUES (source.Section, source.[Key], source.Value, 'String', 1, 1, 'ADMIN', GETUTCDATE());

-- Reload cache via API
-- POST https://localhost:44101/api/configuration/reload
```

## After Saving Settings

### Test SMTP Connection Separately

Once settings are saved (with `skipTest = true`), test the connection:

```powershell
# Test SMTP with saved configuration
Invoke-RestMethod -Uri "https://localhost:44101/api/configuration/test-smtp" `
    -Method POST `
    -UseDefaultCredentials `
    -SkipCertificateCheck
```

**Expected Response (Success):**
```json
{
  "success": true,
  "message": "SMTP connection successful"
}
```

**Expected Response (Failure):**
```json
{
  "success": false,
  "error": "SMTP connection test timed out after 15 seconds"
}
```

## Files Modified

| File | Change | Purpose |
|------|--------|---------|
| `ConfigurationManagerService.cs` | Removed execution strategy, added skipTest | No retries, faster execution |
| `IConfigurationManager.cs` | Added skipTest parameter | Interface signature update |
| `ConfigurationEndpoints.cs` | Accept SmtpConfigurationRequest | Support skipTest in API |
| `Update-SMTP-SkipTest.ps1` | New file | Emergency script to save without testing |
| `TIMEOUT_FIX_SUMMARY.md` | New file | This document |

## Performance Comparison

| Scenario | Old Behavior | New Behavior |
|----------|--------------|--------------|
| **Valid SMTP** | ~20s (test once) | ~20s (test once) |
| **Invalid SMTP** | 100s+ timeout (retries) | **15s (fail fast)** |
| **Skip Test** | Not available | **< 5s (save only)** |

## Recommendations

### For Development/Testing
✅ **Use `skipTest = true`** to quickly iterate on SMTP settings

### For Production
❌ **Do NOT use `skipTest = true`** - Always validate SMTP settings before saving

### When SMTP Server is Unreachable
1. Save with `skipTest = true` using emergency script
2. Fix firewall/network issues
3. Test separately: `POST /api/configuration/test-smtp`
4. Update settings with `skipTest = false` once connectivity works

## Troubleshooting

### Still Getting Timeouts?

**Check Database Performance:**
```sql
-- Check for blocking
SELECT * FROM sys.dm_exec_requests WHERE blocking_session_id > 0;

-- Check active transactions
SELECT * FROM sys.dm_tran_active_transactions;
```

**Check SMTP Connectivity:**
```powershell
# Test port connectivity
Test-NetConnection -ComputerName smtp.office365.com -Port 587
```

**Enable Debug Logging:**
```json
// appsettings.json
{
  "Logging": {
    "LogLevel": {
      "IkeaDocuScan.Infrastructure.Services.ConfigurationManagerService": "Debug"
    }
  }
}
```

### Common SMTP Settings

**Office 365:**
```json
{
  "smtpHost": "smtp.office365.com",
  "smtpPort": 587,
  "useSsl": true,
  "smtpUsername": "your-email@company.com",
  "smtpPassword": "app-password"
}
```

**Gmail:**
```json
{
  "smtpHost": "smtp.gmail.com",
  "smtpPort": 587,
  "useSsl": true,
  "smtpUsername": "your-email@gmail.com",
  "smtpPassword": "app-specific-password"
}
```

**Internal Exchange:**
```json
{
  "smtpHost": "mail.company.local",
  "smtpPort": 25,
  "useSsl": false,
  "smtpUsername": "",
  "smtpPassword": ""
}
```

## Additional Help

- **TROUBLESHOOTING_SMTP.md** - Comprehensive troubleshooting guide
- **SMTP_CONFIGURATION_FIX.md** - Technical details of architecture changes
- **CONFIGURATION_API_REFERENCE.md** - Complete API documentation

## Summary

The timeout issue is now fixed with two approaches:

1. **Fast Failure**: SMTP updates fail in 15-20 seconds instead of 100+
2. **Skip Test Option**: Save settings immediately without testing (emergency use)

**Use the emergency script now to unblock yourself:**
```powershell
.\Update-SMTP-SkipTest.ps1 -SkipCertificateCheck
```
