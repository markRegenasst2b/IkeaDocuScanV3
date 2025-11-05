# SMTP Configuration Troubleshooting Guide

## Common Issues and Solutions

### Issue: "Request timed out after 100 seconds"

**Symptoms:**
- Bulk SMTP update fails with timeout error
- Error message: "The request was canceled due to the configured HttpClient.Timeout of 100 seconds elapsing"

**Root Causes:**

1. **SMTP Server Unreachable**
   - The SMTP host in your configuration doesn't exist or is blocked by firewall
   - The SMTP port is incorrect or blocked
   - Network connectivity issues

2. **Database Performance**
   - Slow database queries
   - Database server overloaded
   - Network latency to database

**Solutions:**

#### Solution 1: Verify SMTP Settings Before Saving

Use the standalone test endpoint first:

```powershell
# Test current SMTP configuration without saving
Invoke-RestMethod -Uri "https://localhost:44101/api/configuration/test-smtp" `
    -Method POST `
    -UseDefaultCredentials `
    -SkipCertificateCheck
```

This tests your CURRENT configuration without the overhead of a transaction.

#### Solution 2: Use Correct SMTP Settings

Common SMTP configurations:

**Office 365:**
```json
{
  "smtpHost": "smtp.office365.com",
  "smtpPort": 587,
  "useSsl": true,
  "smtpUsername": "your-email@company.com",
  "smtpPassword": "your-password",
  "fromAddress": "noreply@company.com"
}
```

**Gmail:**
```json
{
  "smtpHost": "smtp.gmail.com",
  "smtpPort": 587,
  "useSsl": true,
  "smtpUsername": "your-email@gmail.com",
  "smtpPassword": "app-specific-password",
  "fromAddress": "your-email@gmail.com"
}
```

**Exchange Server:**
```json
{
  "smtpHost": "mail.company.com",
  "smtpPort": 25,
  "useSsl": false,
  "smtpUsername": "",
  "smtpPassword": "",
  "fromAddress": "noreply@company.com"
}
```

#### Solution 3: Check Firewall Rules

Ensure outbound connections are allowed:
- Port 587 (TLS/STARTTLS)
- Port 465 (SSL)
- Port 25 (Plain/No encryption)

**Windows Firewall Test:**
```powershell
Test-NetConnection -ComputerName smtp.office365.com -Port 587
```

**Expected Output:**
```
TcpTestSucceeded : True
```

If `False`, firewall is blocking the connection.

#### Solution 4: Increase HTTP Client Timeout (if needed)

If your SMTP server is legitimately slow, you can increase the timeout in the client code:

**File:** `Program.cs` (Client project)

```csharp
builder.Services.AddHttpClient<ConfigurationHttpService>(client =>
{
    client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
    client.Timeout = TimeSpan.FromSeconds(180); // Increase from 100 to 180 seconds
});
```

#### Solution 5: Skip SMTP Testing (Not Recommended)

For emergency situations where you need to update SMTP settings even if they can't be tested:

**Temporarily comment out SMTP test in ConfigurationManagerService.cs:**

```csharp
// var testResult = await TestSmtpConnectionAsync();
// if (!testResult) { throw ... }
var testResult = true; // Skip test temporarily
```

**⚠️ WARNING:** This bypasses the safety mechanism and could save invalid SMTP configuration.

## SMTP Test Timeout Behavior

### Current Timeouts

| Operation | Timeout | Behavior |
|-----------|---------|----------|
| SMTP Connection | 15 seconds | Test fails if connection takes longer |
| SMTP Operations | 10 seconds | Read/write operations timeout |
| HTTP Request | 100 seconds | Client-side timeout for entire endpoint call |

### Timeout Hierarchy

```
HTTP Request (100s)
  └─ Database Transaction
      └─ Save All Settings
      └─ SMTP Test (15s total)
          └─ Connect (cancellable)
          └─ Authenticate (cancellable)
          └─ Disconnect (cancellable)
      └─ Commit or Rollback
```

### What Happens on Timeout

1. **SMTP Test Timeout (15s):**
   - Test returns `false`
   - Transaction rolls back
   - No settings saved
   - Error message: "SMTP configuration test failed"

2. **HTTP Request Timeout (100s):**
   - Entire request cancelled
   - Transaction may or may not complete
   - Client receives timeout error
   - Check database to verify state

## Debugging Steps

### Step 1: Check Current Configuration

```sql
SELECT ConfigKey, ConfigValue, ModifiedDate
FROM SystemConfigurations
WHERE ConfigSection = 'Email'
ORDER BY ConfigKey;
```

### Step 2: Test SMTP Connection Manually

```powershell
# Using telnet (if available)
telnet smtp.office365.com 587

# Using PowerShell
$client = New-Object System.Net.Sockets.TcpClient
$client.Connect("smtp.office365.com", 587)
$client.Connected  # Should return True
$client.Close()
```

### Step 3: Check Application Logs

Look for these log entries:

```
Testing SMTP connection...
SMTP connection test successful: smtp.office365.com:587
```

Or error messages:

```
SMTP connection test timed out after 15 seconds
SMTP connection test failed: Connection refused
```

### Step 4: Verify Database Transaction State

```sql
-- Check for locks or blocking
SELECT * FROM sys.dm_tran_locks
WHERE resource_database_id = DB_ID('IkeaDocuScan');

-- Check active transactions
SELECT * FROM sys.dm_tran_active_transactions;
```

### Step 5: Test Without Transaction

Use individual configuration endpoints to update settings without SMTP testing:

```powershell
# Update one setting at a time (NO automatic testing)
$body = @{ value = "smtp.office365.com"; reason = "Testing" } | ConvertTo-Json
Invoke-RestMethod -Uri "https://localhost:44101/api/configuration/Email/SmtpHost" `
    -Method POST -Body $body -ContentType "application/json"
```

## Performance Optimization

### Reduce Database Latency

1. **Check Connection String:**
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=localhost;Database=IkeaDocuScan;Trusted_Connection=True;TrustServerCertificate=True;Connection Timeout=30"
   }
   ```

2. **Add Indexes (if missing):**
   ```sql
   CREATE INDEX IX_SystemConfiguration_Section_Key
   ON SystemConfigurations(ConfigSection, ConfigKey);
   ```

3. **Check SQL Server Performance:**
   ```sql
   -- Check slow queries
   SELECT TOP 10
       total_elapsed_time / execution_count AS avg_time_ms,
       text
   FROM sys.dm_exec_query_stats
   CROSS APPLY sys.dm_exec_sql_text(sql_handle)
   ORDER BY avg_time_ms DESC;
   ```

### Reduce SMTP Test Time

1. **Use Faster SMTP Server:** Internal/local SMTP relay instead of cloud service
2. **Configure Firewall:** Ensure no packet filtering delays
3. **Use Correct Port:** Port 25 is often faster than 587 (if supported)

## Emergency Recovery

If SMTP configuration is completely broken and you can't update it:

### Option 1: Direct Database Update

```sql
UPDATE SystemConfigurations
SET ConfigValue = 'smtp.office365.com'
WHERE ConfigSection = 'Email' AND ConfigKey = 'SmtpHost';

UPDATE SystemConfigurations
SET ConfigValue = '587'
WHERE ConfigSection = 'Email' AND ConfigKey = 'SmtpPort';

-- Reload cache
-- Call POST /api/configuration/reload
```

### Option 2: Restore from appsettings.json

```sql
-- Delete all SMTP configurations to force fallback
DELETE FROM SystemConfigurations
WHERE ConfigSection = 'Email';

-- Application will now use appsettings.json values
```

### Option 3: Disable SMTP Override

```sql
-- Mark all SMTP configs as inactive
UPDATE SystemConfigurations
SET IsActive = 0, IsOverride = 0
WHERE ConfigSection = 'Email';
```

## Prevention

### Best Practices

1. **Test Before Saving:**
   - Always use `POST /api/configuration/test-smtp` first
   - Verify connectivity before bulk update

2. **Use Valid Credentials:**
   - Test credentials separately before configuring
   - Use app-specific passwords for Gmail
   - Verify account isn't locked

3. **Document Working Configuration:**
   - Keep a backup of working SMTP settings
   - Document firewall rules needed
   - Note any special configuration

4. **Monitor Performance:**
   - Watch application logs for SMTP test times
   - Alert on timeouts
   - Track database query performance

5. **Staged Rollout:**
   - Test in development first
   - Verify in staging environment
   - Then deploy to production

## Error Reference

| Error Message | Cause | Solution |
|---------------|-------|----------|
| "Request timed out after 100 seconds" | SMTP test taking too long | Check SMTP host reachability |
| "SMTP connection test timed out after 15 seconds" | Server unreachable | Verify firewall rules |
| "Connection refused" | Wrong port or server down | Check port and server status |
| "Authentication failed" | Wrong credentials | Verify username/password |
| "SSL/TLS negotiation failed" | SSL misconfiguration | Check UseSsl setting |
| "MERGE statement conflicted with FOREIGN KEY" | Database constraint | Fixed in code, update to latest |

## Support

For additional help:
1. Check application logs in `IkeaDocuScan-Web/Logs/`
2. Review audit trail: `SELECT * FROM SystemConfigurationAudits ORDER BY ChangedDate DESC`
3. Enable debug logging in `appsettings.json`:
   ```json
   "Logging": {
     "LogLevel": {
       "IkeaDocuScan.Infrastructure.Services.ConfigurationManagerService": "Debug"
     }
   }
   ```
