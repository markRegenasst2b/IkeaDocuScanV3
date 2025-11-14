# PowerShell Test Script Fixes

## Issue Reported
**Test 1: Get System Configuration failed with 404 Not Found**

## Root Cause
The PowerShell test script (`Test-ConfigurationManager.ps1`) was calling API endpoints that don't exist. The script was written based on assumptions about the API structure rather than the actual endpoints defined in `ConfigurationEndpoints.cs`.

## Fixes Applied

### 1. Fixed Test 1 - Get System Configuration
**Before:**
```powershell
GET /api/configuration/system
```

**After:**
```powershell
GET /api/configuration/sections
```

**Reason:** There is no `/system` endpoint. The correct endpoint to list configuration sections is `/sections`.

---

### 2. Fixed Test 2 - Get Specific Configuration Key
**Before:**
```powershell
GET /api/configuration/system/Email/SmtpHost
```

**After:**
```powershell
GET /api/configuration/Email/SmtpHost
```

**Reason:** Removed incorrect `/system` path segment. The endpoint pattern is `/{section}/{key}`.

---

### 3. Fixed Test 6 - Update SMTP Configuration
**Before:**
```powershell
POST /api/configuration/smtp
Body: {
    smtpHost = "smtp.office365.com"
    smtpPort = 587
    enableSsl = $true
    ...
}
```

**After:**
```powershell
# Individual configuration updates
POST /api/configuration/Email/SmtpHost
Body: { value = "smtp.office365.com", reason = "..." }

POST /api/configuration/Email/SmtpPort
Body: { value = "587", reason = "..." }
```

**Reason:** There is no bulk SMTP update endpoint. Each configuration setting must be updated individually using the `/{section}/{key}` pattern with a request body containing `value` and optional `reason`.

---

### 4. Fixed Test 7 - Email Recipient Group
**Before:**
```powershell
POST /api/configuration/email-recipients
Body: {
    groupKey = "TestGroup"
    groupName = "Test Recipient Group"
    recipients = [...]
}
```

**After:**
```powershell
POST /api/configuration/email-recipients/{groupKey}
Body: {
    emailAddresses = ["email1@company.com", "email2@company.com"]
    reason = "Testing recipient group update"
}
```

**Reason:** The endpoint expects the group key in the URL path, not in the body. The body should contain a simple array of email addresses and an optional reason, not complex recipient objects.

---

### 5. Removed Test 11 - Configuration Audit Trail
**Before:**
```powershell
GET /api/configuration/audit?limit=10
```

**After:**
```powershell
# Removed - no such endpoint exists
```

**Reason:** There is no audit trail GET endpoint in `ConfigurationEndpoints.cs`. Audit trail data can only be queried directly from the database using SQL.

---

### 6. Fixed Test 12 - Get Specific Email Template
**Before:**
```powershell
GET /api/configuration/email-templates/{templateId}
```

**After:**
```powershell
GET /api/configuration/email-templates/{templateKey}
```

**Reason:** The endpoint uses template key (string) not template ID (numeric). Changed from `$templates[0].templateId` to `$templates[0].templateKey`.

---

### 7. Added Test 11 - Get Available Placeholders
**New Test:**
```powershell
GET /api/configuration/email-templates/placeholders
```

**Reason:** Added test for the placeholder documentation endpoint that was missing from the original script.

---

## Updated Test Sequence

The script now executes these tests in order:

1. **Test 1:** Get Configuration Sections - Lists all available config sections
2. **Test 2:** Get Specific Configuration Key - Retrieves Email/SmtpHost value
3. **Test 3:** Get Email Recipients - Lists all recipient groups
4. **Test 4:** Get Email Templates - Lists all templates
5. **Test 5:** Test SMTP Connection - Tests current SMTP settings
6. **Test 6:** Update SMTP Configuration (COMMENTED OUT) - Individual setting updates
7. **Test 7:** Update Email Recipient Group (COMMENTED OUT) - Update recipient emails
8. **Test 8:** Run Configuration Migration - Migrate from appsettings.json
9. **Test 9:** Reload Configuration Cache - Clear cache and reload
10. **Test 10:** Create Email Template (COMMENTED OUT) - Create custom template
11. **Test 11:** Get Available Placeholders - List available template placeholders
12. **Test 12:** Get Specific Email Template by Key - Retrieve template details

## Verification

All endpoints now match the actual routes defined in `ConfigurationEndpoints.cs:14-399`.

### Endpoint Mapping Verification Table

| Test | Script Endpoint | Actual Endpoint | Status |
|------|----------------|-----------------|--------|
| 1 | `/api/configuration/sections` | Line 210 | ✅ Valid |
| 2 | `/api/configuration/Email/SmtpHost` | Line 225 | ✅ Valid |
| 3 | `/api/configuration/email-recipients` | Line 22 | ✅ Valid |
| 4 | `/api/configuration/email-templates` | Line 68 | ✅ Valid |
| 5 | `/api/configuration/test-smtp` | Line 267 | ✅ Valid |
| 6 | `/api/configuration/Email/{key}` | Line 242 | ✅ Valid |
| 7 | `/api/configuration/email-recipients/{groupKey}` | Line 45 | ✅ Valid |
| 8 | `/api/configuration/migrate` | Line 297 | ✅ Valid |
| 9 | `/api/configuration/reload` | Line 288 | ✅ Valid |
| 10 | `/api/configuration/email-templates` | Line 91 | ✅ Valid |
| 11 | `/api/configuration/email-templates/placeholders` | Line 372 | ✅ Valid |
| 12 | `/api/configuration/email-templates/{key}` | Line 77 | ✅ Valid |

## Testing the Fixed Script

To verify the fixes work:

```powershell
# Run the script
.\Test-ConfigurationManager.ps1 -SkipCertificateCheck

# Expected results:
# - Test 1: SUCCESS - Returns 4 configuration sections
# - Test 2: SUCCESS or 404 (if SmtpHost not in database yet)
# - Test 3: SUCCESS - Returns empty array or existing groups
# - Test 4: SUCCESS - Returns empty array or existing templates
# - Test 5: SUCCESS or FAIL (depends on SMTP configuration)
# - Test 8: User prompted to confirm migration
# - Test 9: SUCCESS - Cache reloaded
# - Test 11: SUCCESS - Returns placeholder documentation
# - Test 12: SUCCESS (if templates exist from migration)
```

## Additional Resources Created

1. **CONFIGURATION_API_REFERENCE.md** - Complete API documentation with examples
2. **Verify-ConfigurationDatabase.sql** - SQL script to verify database state
3. **CONFIGURATION_TESTING_GUIDE.md** - Comprehensive testing scenarios

## Next Steps

1. Run the fixed PowerShell script to test all endpoints
2. Execute the migration (Test 8) when prompted
3. Verify database state using `Verify-ConfigurationDatabase.sql`
4. Test the UI at `https://localhost:44101/configuration-management`
5. Review CONFIGURATION_API_REFERENCE.md for detailed endpoint documentation
