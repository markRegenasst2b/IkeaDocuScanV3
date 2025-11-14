# Configuration Manager Testing Guide

This guide provides step-by-step instructions for testing the ConfigurationManagerService implementation (Phase 7).

## Prerequisites

1. Application is running on `https://localhost:44101` (or update scripts with your URL)
2. Database is accessible
3. You have appropriate authentication credentials
4. PowerShell 5.1+ or PowerShell Core 6+

## Testing Tools

This directory contains three testing tools:

| Tool | Purpose |
|------|---------|
| `Test-ConfigurationManager.ps1` | PowerShell script to test all configuration API endpoints |
| `Verify-ConfigurationDatabase.sql` | SQL script to verify database state after migration |
| `CONFIGURATION_TESTING_GUIDE.md` | This guide |

## Quick Start

### 1. Run the Migration

First, run the configuration migration to populate the database with settings from appsettings.json:

```powershell
# Run the test script
.\Test-ConfigurationManager.ps1 -SkipCertificateCheck

# When prompted "Do you want to run the migration? (Y/N)", type: Y
```

This will:
- Migrate SMTP settings from appsettings.json to database
- Migrate email recipient groups to database
- Create 5 default email templates
- Create audit trail entries for all migrations

### 2. Verify Database State

Run the SQL verification script against your database:

```sql
-- Execute in SQL Server Management Studio or Azure Data Studio
-- Update connection details as needed
:CONNECT localhost -d IkeaDocuScanDb -E

-- Run the verification script
:r Verify-ConfigurationDatabase.sql
```

Or using sqlcmd:

```powershell
sqlcmd -S localhost -d IkeaDocuScanDb -E -i Verify-ConfigurationDatabase.sql
```

### 3. Test the UI

Navigate to the configuration management page:

```
https://localhost:44101/configuration-management
```

Test each tab:
1. **Email Recipients** - View/edit recipient groups
2. **Email Templates** - View/create/edit/preview templates
3. **SMTP Settings** - Configure and test SMTP connection

## Detailed Testing Scenarios

### Scenario 1: Test SMTP Configuration

**Using PowerShell Script:**

1. Edit `Test-ConfigurationManager.ps1`
2. Uncomment the "Test 6" section (lines 86-100)
3. Update the SMTP settings with your values:
   ```powershell
   $smtpConfig = @{
       smtpHost = "smtp.office365.com"
       smtpPort = 587
       enableSsl = $true
       username = "noreply@yourcompany.com"
       password = "YourPassword"
       fromAddress = "noreply@yourcompany.com"
       fromName = "IkeaDocuScan System"
   }
   ```
4. Run the script:
   ```powershell
   .\Test-ConfigurationManager.ps1 -SkipCertificateCheck
   ```

**Expected Result:**
- Settings saved to database
- SMTP connection automatically tested
- If test fails, transaction is rolled back (settings NOT saved)
- Success message shown if connection test passes

**Using UI:**

1. Navigate to `https://localhost:44101/configuration-management`
2. Click "SMTP Settings" tab
3. Fill in SMTP configuration
4. Click "Test Connection" button
5. If successful, click "Save Settings"

**Verify in Database:**

```sql
SELECT ConfigKey, ConfigValue, ModifiedBy, ModifiedDate
FROM SystemConfigurations
WHERE ConfigSection = 'Email' AND ConfigKey LIKE '%Smtp%'
ORDER BY ConfigKey;
```

### Scenario 2: Create Email Recipient Group

**Using PowerShell Script:**

1. Edit `Test-ConfigurationManager.ps1`
2. Uncomment "Test 7" section (lines 106-122)
3. Customize the recipient group:
   ```powershell
   $recipientGroup = @{
       groupKey = "TestGroup"
       groupName = "Test Recipient Group"
       recipients = @(
           @{ emailAddress = "user1@company.com"; sortOrder = 1 },
           @{ emailAddress = "user2@company.com"; sortOrder = 2 }
       )
   }
   ```
4. Run the script

**Using UI:**

1. Navigate to "Email Recipients" tab
2. Expand or create a recipient group accordion
3. Add email addresses
4. Set sort order
5. Click "Save Recipients"

**Verify in Database:**

```sql
SELECT
    erg.GroupKey,
    erg.GroupName,
    er.EmailAddress,
    er.SortOrder
FROM EmailRecipients er
INNER JOIN EmailRecipientGroups erg ON er.GroupId = erg.GroupId
WHERE erg.GroupKey = 'TestGroup'
ORDER BY er.SortOrder;
```

### Scenario 3: Create Custom Email Template

**Using PowerShell Script:**

1. Edit `Test-ConfigurationManager.ps1`
2. Uncomment "Test 10" section (lines 153-186)
3. Customize the template
4. Run the script

**Using UI:**

1. Navigate to "Email Templates" tab
2. Click "Create New Template"
3. Fill in template details:
   - Template Name: "My Custom Template"
   - Template Key: "MyCustomTemplate"
   - Subject: "Custom Email: {{Username}}"
   - HTML Body: Your HTML content with placeholders
4. Click "Show Available Placeholders" for help
5. Click "Preview" to test rendering
6. Click "Save Template"

**Available Placeholders:**

| Placeholder | Description | Example |
|-------------|-------------|---------|
| `{{Username}}` | User's username | john.doe |
| `{{Date}}` | Current date/time | 04/11/2025 14:30 |
| `{{Count}}` | Number of items | 5 |
| `{{BarCode}}` | Document barcode | 12345 |
| `{{ApplicationUrl}}` | Base app URL | https://docuscan.company.com |
| `{{DocumentLink}}` | Document link URL | https://example.com/document/12345 |
| `{{FileName}}` | File name | invoice.pdf |
| `{{Message}}` | Custom message | Sample message |

**Loop Structures:**

```html
{{#ActionRows}}
  <tr>
    <td>{{BarCode}}</td>
    <td>{{DocumentType}}</td>
    <td>{{ActionDate}}</td>
  </tr>
{{/ActionRows}}
```

**Verify in Database:**

```sql
SELECT
    TemplateKey,
    TemplateName,
    Subject,
    Category,
    IsActive,
    IsDefault,
    LEN(HtmlBody) AS HtmlBodyLength
FROM EmailTemplates
WHERE TemplateKey = 'MyCustomTemplate';
```

### Scenario 4: Test Execution Strategy Fix

This scenario specifically tests the SQL Server execution strategy fix that allows SMTP testing within a transaction.

**Steps:**

1. Configure invalid SMTP settings:
   ```powershell
   $smtpConfig = @{
       smtpHost = "invalid.smtp.server.com"
       smtpPort = 587
       enableSsl = $true
       username = "test@test.com"
       password = "invalid"
       fromAddress = "test@test.com"
       fromName = "Test"
   }
   ```

2. Send the update request

**Expected Result:**
- SMTP connection test should fail
- Transaction should be rolled back
- Settings should NOT be saved to database
- Error message returned: "SMTP configuration test failed. Configuration not saved."

**Verify Transaction Rollback:**

```sql
-- Should return previous values, not the invalid ones you just sent
SELECT ConfigKey, ConfigValue
FROM SystemConfigurations
WHERE ConfigSection = 'Email' AND ConfigKey = 'SmtpHost';
```

### Scenario 5: Configuration Cache Reload

**Using PowerShell Script:**

The script automatically tests cache reload in "Test 9"

**Using UI:**

1. Navigate to Configuration Management page
2. Click "Reload Cache" button in top-right
3. Wait for success message

**Expected Result:**
- Success message: "Configuration cache reloaded successfully"
- All configuration changes now active without application restart

**Verify:**

1. Make a configuration change
2. Reload cache
3. Configuration should be immediately effective

### Scenario 6: View Audit Trail

**Using UI:**

Currently not exposed in UI (future enhancement)

**Using Database:**

```sql
-- View recent configuration changes
SELECT TOP 20
    ConfigKey,
    OldValue,
    NewValue,
    ChangedBy,
    ChangedDate,
    ChangeReason
FROM SystemConfigurationAudits
ORDER BY ChangedDate DESC;
```

**Using API:**

```powershell
Invoke-RestMethod -Uri "https://localhost:44101/api/configuration/audit?limit=20" `
                  -UseDefaultCredentials `
                  -SkipCertificateCheck
```

## PowerShell Script Parameters

The `Test-ConfigurationManager.ps1` script supports several parameters:

```powershell
# Basic usage
.\Test-ConfigurationManager.ps1

# Custom base URL
.\Test-ConfigurationManager.ps1 -BaseUrl "https://myserver.com"

# Skip SSL certificate validation (for self-signed certs)
.\Test-ConfigurationManager.ps1 -SkipCertificateCheck

# Don't use default credentials (will prompt for auth)
.\Test-ConfigurationManager.ps1 -UseDefaultCredentials:$false

# Combined
.\Test-ConfigurationManager.ps1 -BaseUrl "https://localhost:44101" -SkipCertificateCheck
```

## Troubleshooting

### Issue: "Certificate validation failed"

**Solution:**
```powershell
.\Test-ConfigurationManager.ps1 -SkipCertificateCheck
```

### Issue: "Unauthorized (401)"

**Solution:**
- Ensure Windows Authentication is enabled
- Use `-UseDefaultCredentials:$false` and provide credentials
- Check that your user has access in the database

### Issue: "Migration fails - records already exist"

**Solution:**
```powershell
# Run migration with overwrite flag
$body = @{ overwriteExisting = $true } | ConvertTo-Json
Invoke-RestMethod -Uri "https://localhost:44101/api/configuration/migrate" `
                  -Method POST `
                  -Body $body `
                  -ContentType "application/json" `
                  -UseDefaultCredentials
```

### Issue: "SMTP test fails with execution strategy error"

**Expected Behavior:** This error was fixed in ConfigurationManagerService.cs. If you still see this error:

```
The configured execution strategy 'SqlServerRetryingExecutionStrategy'
does not support user-initiated transactions
```

**Solution:**
1. Verify you have the latest ConfigurationManagerService.cs
2. Check lines 130-224 for the execution strategy pattern:
   ```csharp
   var strategy = context.Database.CreateExecutionStrategy();
   await strategy.ExecuteAsync(async () => {
       await using var transaction = await context.Database.BeginTransactionAsync();
       // ...
   });
   ```

### Issue: "No email templates found after migration"

**Solution:**
1. Check appsettings.json has default templates defined
2. Run migration with overwrite: `overwriteExisting: true`
3. Manually create templates via UI or API
4. Check error logs for migration failures

## Database Verification Queries

### Check All Configurations

```sql
SELECT ConfigSection, ConfigKey, ConfigValue, ModifiedBy, ModifiedDate
FROM SystemConfigurations
ORDER BY ConfigSection, ConfigKey;
```

### Check Email Template Count

```sql
SELECT
    Category,
    COUNT(*) AS TemplateCount,
    SUM(CASE WHEN IsActive = 1 THEN 1 ELSE 0 END) AS ActiveCount
FROM EmailTemplates
GROUP BY Category;
```

### Check Recipients by Group

```sql
SELECT
    erg.GroupKey,
    COUNT(er.RecipientId) AS RecipientCount
FROM EmailRecipientGroups erg
LEFT JOIN EmailRecipients er ON erg.GroupId = er.GroupId
GROUP BY erg.GroupKey;
```

### Check Recent Audit Trail

```sql
SELECT TOP 10 *
FROM SystemConfigurationAudits
ORDER BY ChangedDate DESC;
```

## Expected Default State After Migration

After running the migration successfully, you should have:

### System Configurations
- Minimum 8 SMTP-related settings (SmtpHost, SmtpPort, EnableSsl, etc.)
- All settings from appsettings.json "Email" section

### Email Recipient Groups
- At least 2 groups: AdminNotifications, AccessRequests
- All groups from appsettings.json "Email:RecipientGroups" section

### Email Recipients
- Multiple recipients distributed across groups
- Each with SortOrder for priority

### Email Templates
- Exactly 5 default templates:
  1. AccessRequestNotification
  2. AccessRequestConfirmation
  3. ActionReminderDaily
  4. DocumentLink
  5. DocumentAttachment

### Audit Trail
- Multiple entries documenting the migration
- All entries have ChangeReason = "Initial migration from appsettings.json"

## Next Steps

After successful testing:

1. **Production Deployment**
   - Review and update appsettings.json with production values
   - Run migration on production database
   - Test SMTP with production mail server
   - Configure actual recipient email addresses

2. **Ongoing Management**
   - Use UI for day-to-day configuration changes
   - Monitor audit trail for compliance
   - Backup configuration settings regularly
   - Document any custom templates created

3. **Integration Testing**
   - Test email sending with actual templates
   - Verify recipient group notifications work
   - Test configuration cache reload under load
   - Verify audit trail captures all changes

## Support

For issues or questions:
- Check server logs: `IkeaDocuScan-Web/Logs/`
- Review audit trail in database
- Check SQL Server error logs for database issues
- Enable detailed logging in appsettings.json: `"LogLevel": { "Default": "Debug" }`
