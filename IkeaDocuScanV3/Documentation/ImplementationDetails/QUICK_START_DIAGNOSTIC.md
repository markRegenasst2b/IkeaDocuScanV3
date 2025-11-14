# Quick Start: Diagnose DocumentAttachment Template Issue

## TL;DR - Run This Now

```powershell
cd /path/to/IkeaDocuScanV3
.\Test-DocumentAttachmentTemplate.ps1
```

**The script will tell you exactly what's wrong and how to fix it.**

---

## What Was Created

### 1. New Diagnostic API Endpoint ✅

**Endpoint:** `GET /api/configuration/email-templates/diagnostic/DocumentAttachment`

Performs 5 comprehensive checks:
1. All templates with "Document" in the key
2. Exact match for "DocumentAttachment"
3. Active template query (matches production logic)
4. Service layer retrieval (includes cache)
5. Character encoding analysis

### 2. Updated PowerShell Script ✅

**File:** `Test-DocumentAttachmentTemplate.ps1`

Clean, simple script that calls the diagnostic endpoint and displays results with color-coded output.

---

## Quick Fix Guide

### If Template Doesn't Exist
```powershell
Invoke-RestMethod -Uri "https://localhost:44101/api/configuration/migrate" -Method POST -Body '{"overwriteExisting":false}' -ContentType "application/json" -UseDefaultCredentials -SkipCertificateCheck
```

### If Template is Inactive
Use the Configuration UI to activate it, or:
```powershell
$templates = Invoke-RestMethod -Uri "https://localhost:44101/api/configuration/email-templates" -Method GET -UseDefaultCredentials -SkipCertificateCheck
$template = $templates | Where-Object { $_.templateKey -eq "DocumentAttachment" }
$template.isActive = $true
Invoke-RestMethod -Uri "https://localhost:44101/api/configuration/email-templates/$($template.templateId)" -Method PUT -Body ($template | ConvertTo-Json -Depth 10) -ContentType "application/json" -UseDefaultCredentials -SkipCertificateCheck
```

### If Cache is Stale
```powershell
Invoke-RestMethod -Uri "https://localhost:44101/api/configuration/reload" -Method POST -UseDefaultCredentials -SkipCertificateCheck
```

---

## Files Created/Modified

| File | Status | Purpose |
|------|--------|---------|
| `ConfigurationEndpoints.cs` | Modified | Added diagnostic endpoint (145 lines) |
| `Test-DocumentAttachmentTemplate.ps1` | Rewritten | Simplified diagnostic script (128 lines) |
| `TEMPLATE_DIAGNOSTIC_SOLUTION.md` | Created | Comprehensive documentation |
| `QUICK_START_DIAGNOSTIC.md` | Created | This file - quick reference |

---

## How It Works

1. **PowerShell script** calls the diagnostic endpoint
2. **Diagnostic endpoint** performs 5 different database queries and service calls
3. **Results analyzed** to identify the exact issue
4. **Recommendation provided** with specific fix commands
5. **Color-coded output** makes it easy to see what's wrong

---

## Next Steps

1. Build and run the application:
   ```bash
   dotnet build
   dotnet run --project IkeaDocuScan-Web/IkeaDocuScan-Web
   ```

2. Run the diagnostic:
   ```powershell
   .\Test-DocumentAttachmentTemplate.ps1
   ```

3. Follow the recommendation in the output

4. Verify by sending a test email

---

## Expected Successful Output

```
================================================================================
SUMMARY
================================================================================

Templates in database: 4
Exact match found: True
Active match found: True
Service retrieval successful: True

RECOMMENDATION:
  Template is being retrieved successfully.
```

If you see this, the DocumentAttachment template will be used when sending emails!

---

## For More Details

See `TEMPLATE_DIAGNOSTIC_SOLUTION.md` for:
- Complete technical documentation
- Code flow analysis
- Database schema details
- All possible scenarios and fixes
- Verification steps
