# DocumentAttachment Template Diagnostic Solution

**Date:** 2025-11-05
**Issue:** DocumentAttachment email template not being used when sending documents as email attachments
**Status:** ✅ Diagnostic tools created

---

## What Was Done

### 1. ✅ Created Diagnostic API Endpoint

**File:** `IkeaDocuScan-Web/Endpoints/ConfigurationEndpoints.cs` (lines 447-591)

**Endpoint:** `GET /api/configuration/email-templates/diagnostic/DocumentAttachment`

**Authorization:** Requires `SuperUser` policy

**Purpose:** Comprehensive diagnostic that performs 5 different checks to identify why the template isn't being used.

#### Diagnostic Checks Performed

1. **Database Query - All Document Templates**
   - Searches for all templates with "Document" in the key
   - Shows character-level analysis of TemplateKey
   - Displays hex byte representation to identify hidden characters
   - Reveals all related templates in database

2. **Exact Match Query**
   - Tests exact match: `TemplateKey == "DocumentAttachment"`
   - Shows template details if found
   - Displays subject/body previews
   - Shows creation and modification metadata

3. **Active Template Query**
   - Mimics the exact query used by `GetEmailTemplateAsync`
   - Filters by: `TemplateKey == "DocumentAttachment" && IsActive`
   - This is the critical check - matches production code logic

4. **Service Layer Retrieval**
   - Tests retrieval through `ISystemConfigurationManager.GetEmailTemplateAsync`
   - Uses the same code path as the EmailService
   - Includes cache behavior
   - **Most important check** - if this fails, the issue is isolated

5. **Expected Key Analysis**
   - Shows expected "DocumentAttachment" string
   - Character length (18 characters)
   - Hex byte representation for comparison
   - Use to compare against actual database values

#### Diagnostic Response Format

```json
{
  "diagnostic": {
    "timestamp": "2025-11-05T12:00:00Z",
    "checks": [
      {
        "checkName": "Database Query - Templates with 'Document' in key",
        "status": "success",
        "foundCount": 4,
        "templates": [...]
      },
      // ... more checks
    ]
  },
  "summary": {
    "templatesInDatabase": 4,
    "exactMatchFound": true,
    "activeMatchFound": true,
    "serviceRetrievalSuccessful": false,
    "recommendation": "Template exists and is active in database, but service retrieval failed. Try clearing cache with POST /api/configuration/reload"
  }
}
```

---

### 2. ✅ Updated PowerShell Diagnostic Script

**File:** `Test-DocumentAttachmentTemplate.ps1` (128 lines)

**Usage:**
```powershell
# Default (uses https://localhost:44101)
.\Test-DocumentAttachmentTemplate.ps1

# Custom URL
.\Test-DocumentAttachmentTemplate.ps1 -BaseUrl "https://docuscan.company.com"
```

**Features:**
- Calls the new diagnostic endpoint
- Color-coded output (Green = success, Red = error, Yellow = warning)
- Displays all 5 diagnostic checks
- Shows comprehensive summary with actionable recommendation
- Clean error handling with helpful error messages

**Sample Output:**
```
================================================================================
DocumentAttachment Template Diagnostic Tool
================================================================================

Connecting to: https://localhost:44101
Running comprehensive diagnostic...

Check: Database Query - Templates with 'Document' in key
  Status: SUCCESS
  Found 4 template(s)
    - TemplateKey: DocumentAttachment (Length: 18)
      TemplateName: Document Attachment Email
      IsActive: True
      Category: Documents
      Hex Bytes: 44-6F-63-75-6D-65-6E-74-41-74-74-61-63-68-6D-65-6E-74

...

================================================================================
SUMMARY
================================================================================

Templates in database: 4
Exact match found: True
Active match found: True
Service retrieval successful: False

RECOMMENDATION:
  Template exists and is active in database, but service retrieval failed. Try clearing cache with POST /api/configuration/reload
```

---

## How to Use the Diagnostic Tools

### Step 1: Run the Diagnostic

**Option A: PowerShell Script (Easiest)**
```powershell
cd /path/to/IkeaDocuScanV3
.\Test-DocumentAttachmentTemplate.ps1
```

**Option B: Direct API Call**
```powershell
$result = Invoke-RestMethod `
    -Uri "https://localhost:44101/api/configuration/email-templates/diagnostic/DocumentAttachment" `
    -Method GET `
    -UseDefaultCredentials `
    -SkipCertificateCheck

$result.summary
```

**Option C: Web Browser**
```
https://localhost:44101/api/configuration/email-templates/diagnostic/DocumentAttachment
```
(Requires Windows authentication and SuperUser permission)

---

### Step 2: Interpret the Results

The diagnostic will identify one of these scenarios:

#### Scenario A: Template Does Not Exist
**Symptoms:**
- `exactMatchFound: false`
- `activeMatchFound: false`
- `serviceRetrievalSuccessful: false`

**Solution:**
```powershell
# Run migration to create default templates
Invoke-RestMethod `
    -Uri "https://localhost:44101/api/configuration/migrate" `
    -Method POST `
    -Body '{"overwriteExisting":false}' `
    -ContentType "application/json" `
    -UseDefaultCredentials `
    -SkipCertificateCheck
```

#### Scenario B: Template Exists but Inactive
**Symptoms:**
- `exactMatchFound: true`
- `activeMatchFound: false` ← Template is inactive
- `serviceRetrievalSuccessful: false`

**Solution:**
```powershell
# Get the template
$templates = Invoke-RestMethod `
    -Uri "https://localhost:44101/api/configuration/email-templates" `
    -Method GET `
    -UseDefaultCredentials `
    -SkipCertificateCheck

$template = $templates | Where-Object { $_.templateKey -eq "DocumentAttachment" }

# Activate it
$template.isActive = $true

# Save
Invoke-RestMethod `
    -Uri "https://localhost:44101/api/configuration/email-templates/$($template.templateId)" `
    -Method PUT `
    -Body ($template | ConvertTo-Json -Depth 10) `
    -ContentType "application/json" `
    -UseDefaultCredentials `
    -SkipCertificateCheck
```

#### Scenario C: Template Active but Service Retrieval Fails
**Symptoms:**
- `exactMatchFound: true`
- `activeMatchFound: true`
- `serviceRetrievalSuccessful: false` ← Cache or service issue

**Solution:**
```powershell
# Clear the configuration cache
Invoke-RestMethod `
    -Uri "https://localhost:44101/api/configuration/reload" `
    -Method POST `
    -UseDefaultCredentials `
    -SkipCertificateCheck

# Wait a moment
Start-Sleep -Seconds 2

# Run diagnostic again
.\Test-DocumentAttachmentTemplate.ps1
```

#### Scenario D: Character Encoding Issue
**Symptoms:**
- Diagnostic shows template exists
- TemplateKey looks correct visually
- Hex bytes don't match expected values

**Expected Hex Bytes:**
```
44-6F-63-75-6D-65-6E-74-41-74-74-61-63-68-6D-65-6E-74
(DocumentAttachment in UTF-8)
```

**Solution:**
If hex bytes don't match, the database has incorrect characters (trailing spaces, different encoding, etc.):

```sql
-- Fix in SQL Server Management Studio
UPDATE EmailTemplate
SET TemplateKey = 'DocumentAttachment'
WHERE TemplateId = <template_id>
```

Then reload cache:
```powershell
Invoke-RestMethod -Uri "https://localhost:44101/api/configuration/reload" -Method POST -UseDefaultCredentials -SkipCertificateCheck
```

---

## Verification After Fix

### 1. Run Diagnostic Again
```powershell
.\Test-DocumentAttachmentTemplate.ps1
```

**Expected Result:**
```
Service retrieval successful: True
RECOMMENDATION:
  Template is being retrieved successfully.
```

### 2. Check Application Logs

When sending a document as email, logs should show:
```
[Information] Using DocumentAttachment template from database
```

**NOT:**
```
[Information] Using hard-coded DocumentAttachment template
```

### 3. Send Test Email

1. Navigate to a document in the application
2. Click "Send as Email Attachment"
3. Check the received email:
   - ✅ Subject should match template subject: `Document: {BarCode}`
   - ✅ Email body should have the styled HTML from template
   - ✅ Placeholders should be replaced: `{{BarCode}}`, `{{FileName}}`, `{{Message}}`

---

## Technical Implementation Details

### Code Flow

**EmailService.SendDocumentAttachmentAsync**
↓ (line 253)
**ConfigurationManagerService.GetEmailTemplateAsync("DocumentAttachment")**
↓
**Check cache first** (line 398-405)
↓ if cache miss
**Query database** (line 411-416)
```csharp
var template = await context.EmailTemplates
    .AsNoTracking()
    .Where(t => t.TemplateKey == templateKey && t.IsActive)  // Must be Active!
    .OrderByDescending(t => t.IsDefault)
    .ThenByDescending(t => t.ModifiedDate ?? t.CreatedDate)
    .FirstOrDefaultAsync();
```
↓
**If template found:** Cache it (line 435) and return
**If template null:** Log warning (line 446) and return null
↓
**EmailService line 256:** Check if template != null
↓
**If template found:** Use database template (line 259)
**If template null:** Use hard-coded template (line 278)

### Cache Behavior

- **TTL:** 5 minutes (`ConfigurationManagerService.cs` line 30)
- **Key Format:** `"EmailTemplate:{templateKey}"` (line 395)
- **Cache Structure:** `ConcurrentDictionary<string, CacheEntry>` (line 29)
- **Expiration:** Checked via `IsExpired` property (line 907)
- **Clearing:** `POST /api/configuration/reload` calls `ReloadAsync()` which calls `_cache.Clear()` (line 567)

**Important:** Only successful retrievals are cached. If template is not found, nothing is cached, so subsequent calls will retry the database query.

### Database Schema

```sql
CREATE TABLE EmailTemplate (
    TemplateId INT IDENTITY(1,1) PRIMARY KEY,
    TemplateName NVARCHAR(100) NOT NULL,
    TemplateKey NVARCHAR(100) NOT NULL,  -- "DocumentAttachment"
    Subject NVARCHAR(500) NOT NULL,
    HtmlBody NVARCHAR(MAX) NOT NULL,
    PlainTextBody NVARCHAR(MAX) NULL,
    PlaceholderDefinitions NVARCHAR(MAX) NULL,
    Category NVARCHAR(50) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    IsDefault BIT NOT NULL DEFAULT 0,
    CreatedBy NVARCHAR(100) NOT NULL,
    CreatedDate DATETIME NOT NULL DEFAULT GETUTCDATE(),
    ModifiedBy NVARCHAR(100) NULL,
    ModifiedDate DATETIME NULL
)

-- Indexes
CREATE UNIQUE INDEX IX_EmailTemplate_Name ON EmailTemplate(TemplateName)
CREATE UNIQUE INDEX IX_EmailTemplate_Key ON EmailTemplate(TemplateKey)  -- Ensures unique keys
CREATE INDEX IX_EmailTemplate_Key_Active ON EmailTemplate(TemplateKey, IsActive)  -- Query optimization
```

### Why This Matters

1. **TemplateKey must be exactly "DocumentAttachment"** - case-sensitive, no extra spaces
2. **IsActive must be true** - the query filters inactive templates
3. **Only one template can have this key** - enforced by unique index
4. **Cache lasts 5 minutes** - changes won't be visible until cache expires or is cleared

---

## Related Files

| File | Purpose | Lines Modified |
|------|---------|----------------|
| `ConfigurationEndpoints.cs` | Added diagnostic endpoint | 447-591 (145 lines added) |
| `Test-DocumentAttachmentTemplate.ps1` | PowerShell diagnostic script | Complete rewrite (128 lines) |
| `DOCUMENT_ATTACHMENT_TEMPLATE_ISSUE.md` | Original troubleshooting guide | Reference |
| `ConfigurationManagerService.cs` | Template retrieval logic | Reference (no changes) |
| `EmailService.cs` | Email sending with templates | Reference (no changes) |

---

## Testing Checklist

- [ ] Build application successfully (diagnostic endpoint compiles)
- [ ] Run application
- [ ] Run PowerShell diagnostic script
- [ ] Verify diagnostic endpoint returns JSON response
- [ ] Verify all 5 checks execute
- [ ] Verify summary provides actionable recommendation
- [ ] Test cache reload endpoint
- [ ] Test template migration endpoint
- [ ] Send test email and verify database template is used
- [ ] Check application logs for "Using DocumentAttachment template from database"

---

## Next Steps

1. **Build and start the application**
   ```bash
   dotnet build
   dotnet run --project IkeaDocuScan-Web/IkeaDocuScan-Web
   ```

2. **Run the diagnostic**
   ```powershell
   .\Test-DocumentAttachmentTemplate.ps1
   ```

3. **Follow the recommendation** in the diagnostic output

4. **Verify the fix** by sending a test email

5. **Check logs** to confirm database template is being used

---

## Troubleshooting the Diagnostic Tool Itself

### Error: "401 Unauthorized"
**Cause:** Not authenticated or not a SuperUser

**Solution:**
- Ensure you're running PowerShell as the Windows user configured in the database
- Verify the user has `IsSuperUser = true` in the `DocuScanUser` table
- Check Windows authentication is enabled

### Error: "404 Not Found"
**Cause:** Endpoint doesn't exist (application not rebuilt after adding endpoint)

**Solution:**
```bash
dotnet build
dotnet run --project IkeaDocuScan-Web/IkeaDocuScan-Web
```

### Error: "Connection refused"
**Cause:** Application not running

**Solution:**
```bash
dotnet run --project IkeaDocuScan-Web/IkeaDocuScan-Web
```

Verify it's listening on `https://localhost:44101`

---

## Summary

✅ **Diagnostic endpoint created:** Performs comprehensive 5-step analysis
✅ **PowerShell script updated:** Clean, simple, uses new endpoint
✅ **Character-level analysis:** Identifies encoding issues
✅ **Actionable recommendations:** Tells you exactly what to do
✅ **Cache testing:** Verifies service layer retrieval

**The diagnostic will pinpoint exactly why the DocumentAttachment template isn't being used.**
