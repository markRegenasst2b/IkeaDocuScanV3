# DocumentAttachment Template Not Being Used - Diagnostic Guide

**Issue:** "Send documents as email" feature is not using the DocumentAttachment template from the database.

**Status:** Needs diagnosis to identify root cause

---

## 📋 Quick Diagnosis Steps

### Step 1: Check if Template Exists and is Active

Run the diagnostic script:
```powershell
.\Test-DocumentAttachmentTemplate.ps1
```

This will show:
- ✓ Whether the template exists
- ✓ Whether it's active
- ✓ The template content
- ✓ Which placeholders are defined

**Expected Result:**
```
✓ DocumentAttachment template found!
✓ Template is ACTIVE
✓ All required placeholders found
```

### Step 2: Check Application Logs

When sending a document as email, check the logs for this message:

**If using database template (GOOD):**
```
[Information] Using DocumentAttachment template from database
```

**If using hard-coded template (BAD - this is the issue):**
```
[Information] Using hard-coded DocumentAttachment template
```

**If template retrieval failed:**
```
[Warning] Failed to load email template from database: DocumentAttachment
[Information] Email template not found in database: DocumentAttachment
```

### Step 3: Check Cache

The template is cached for 5 minutes. If someone updated/created the template recently, the cache might still be returning the old value (null).

**Solution:** Reload cache via API:
```powershell
Invoke-RestMethod -Uri "https://localhost:44101/api/configuration/reload" `
    -Method POST `
    -UseDefaultCredentials `
    -SkipCertificateCheck
```

---

## 🔍 Root Cause Analysis

Based on the code flow, here are the possible causes:

### Cause 1: Template Doesn't Exist in Database

**Symptoms:**
- Template not found when querying `/api/configuration/email-templates`
- Logs show: "Email template not found in database: DocumentAttachment"

**Diagnosis:**
```powershell
# Check if template exists
$templates = Invoke-RestMethod -Uri "https://localhost:44101/api/configuration/email-templates" `
    -Method GET `
    -UseDefaultCredentials `
    -SkipCertificateCheck

$templates | Where-Object { $_.templateKey -eq "DocumentAttachment" }
```

**Solution:**
Run migration to create default templates:
```powershell
$body = @{ overwriteExisting = $false } | ConvertTo-Json

Invoke-RestMethod -Uri "https://localhost:44101/api/configuration/migrate" `
    -Method POST `
    -Body $body `
    -ContentType "application/json" `
    -UseDefaultCredentials `
    -SkipCertificateCheck
```

### Cause 2: Template Exists but is Inactive

**Symptoms:**
- Template found but `isActive = false`
- Logs show: "Email template not found in database: DocumentAttachment"

**Why This Happens:**
The query in `ConfigurationManagerService.cs` line 413 filters by `IsActive`:
```csharp
.Where(t => t.TemplateKey == templateKey && t.IsActive)
```

**Solution:**
Activate the template:
```powershell
# Get template
$template = (Invoke-RestMethod -Uri "https://localhost:44101/api/configuration/email-templates" -Method GET -UseDefaultCredentials -SkipCertificateCheck) | Where-Object { $_.templateKey -eq "DocumentAttachment" }

# Update to active
$template.isActive = $true

# Save
Invoke-RestMethod -Uri "https://localhost:44101/api/configuration/email-templates/$($template.templateId)" `
    -Method PUT `
    -Body ($template | ConvertTo-Json) `
    -ContentType "application/json" `
    -UseDefaultCredentials `
    -SkipCertificateCheck
```

### Cause 3: Template Cache is Stale

**Symptoms:**
- Template exists and is active in database
- But logs still show hard-coded template being used
- Recently created/updated the template

**Why This Happens:**
Templates are cached for 5 minutes (TTL). If template was just created, cache might still have `null`.

**Solution:**
```powershell
# Reload cache
Invoke-RestMethod -Uri "https://localhost:44101/api/configuration/reload" `
    -Method POST `
    -UseDefaultCredentials `
    -SkipCertificateCheck

# Wait a moment then try sending email again
```

### Cause 4: Configuration Manager Not Injected Properly

**Symptoms:**
- Template exists and is active
- Logs show error or warning about configuration manager

**Diagnosis:**
Check `Program.cs` for DI registration:
```csharp
builder.Services.AddScoped<ISystemConfigurationManager, ConfigurationManagerService>();
```

**Solution:**
Verify dependency injection is configured correctly and restart the application.

### Cause 5: Wrong Template Key

**Symptoms:**
- Template exists but with a different key
- Logs show template not found

**Diagnosis:**
```powershell
# List all template keys
(Invoke-RestMethod -Uri "https://localhost:44101/api/configuration/email-templates" -Method GET -UseDefaultCredentials -SkipCertificateCheck) | Select-Object templateKey, templateName
```

**Expected Keys:**
- AccessRequestNotification
- AccessRequestConfirmation
- ActionReminderDaily
- DocumentLink
- **DocumentAttachment** ← Should be exactly this
- DocumentLinks (for multiple)
- DocumentAttachments (for multiple)

**Solution:**
If key is misspelled, update it in the database or create correct one.

### Cause 6: Exception During Template Retrieval

**Symptoms:**
- Logs show exception/error when retrieving template
- Falls back to hard-coded template

**Diagnosis:**
Check logs for exceptions around this code:
```
[Warning] Failed to load email template from database: DocumentAttachment
```

**Common Exceptions:**
- Database connection issues
- EF Core query errors
- Serialization issues

**Solution:**
Fix the underlying exception (database connectivity, permissions, etc.).

---

## 🛠️ Code Flow Analysis

### How Templates are Retrieved and Used

**Step 1: EmailService.SendDocumentAttachmentAsync (line 238)**
```csharp
var template = await _configManager.GetEmailTemplateAsync("DocumentAttachment");
```

**Step 2: ConfigurationManagerService.GetEmailTemplateAsync (line 393)**
```csharp
// Check cache first
if (_cache.TryGetValue(cacheKey, out var cachedEntry) && !cachedEntry.IsExpired)
{
    return cachedValue;
}

// Query database
var template = await context.EmailTemplates
    .AsNoTracking()
    .Where(t => t.TemplateKey == templateKey && t.IsActive)  // ← Must be Active!
    .OrderByDescending(t => t.IsDefault)
    .ThenByDescending(t => t.ModifiedDate ?? t.CreatedDate)
    .FirstOrDefaultAsync();
```

**Step 3: If template found (EmailService line 256)**
```csharp
if (template != null)
{
    _logger.LogInformation("Using DocumentAttachment template from database");

    var data = new Dictionary<string, object>
    {
        { "BarCode", documentBarCode },
        { "FileName", fileName },
        { "Message", message ?? string.Empty },
        { "Date", DateTime.Now }
    };

    htmlBody = _templateService.RenderTemplate(template.HtmlBody, data);
    subject = _templateService.RenderTemplate(template.Subject, data);
}
```

**Step 4: If template NOT found (EmailService line 278)**
```csharp
else
{
    _logger.LogInformation("Using hard-coded DocumentAttachment template");
    (htmlBody, plainText) = EmailTemplates.BuildDocumentAttachment(
        documentBarCode,
        fileName,
        message);
    subject = $"Document: {documentBarCode}";
}
```

### Placeholders Expected

The DocumentAttachment template should support these placeholders:
- `{{BarCode}}` - Document bar code
- `{{FileName}}` - Attachment file name
- `{{Message}}` - Optional message from sender
- `{{Date}}` - Current date/time

---

## ✅ Verification Steps

After applying fixes, verify the template is being used:

### 1. Test Template Retrieval
```powershell
# Get template by key
Invoke-RestMethod -Uri "https://localhost:44101/api/configuration/email-templates/DocumentAttachment" `
    -Method GET `
    -UseDefaultCredentials `
    -SkipCertificateCheck
```

### 2. Preview Template
```powershell
$preview = @{
    template = "<html><body>Hello {{BarCode}}, file: {{FileName}}</body></html>"
    data = @{
        BarCode = "12345"
        FileName = "test.pdf"
        Message = "Test message"
    }
} | ConvertTo-Json

Invoke-RestMethod -Uri "https://localhost:44101/api/configuration/email-templates/preview" `
    -Method POST `
    -Body $preview `
    -ContentType "application/json" `
    -UseDefaultCredentials `
    -SkipCertificateCheck
```

### 3. Send Test Email
Send a document as email attachment and check:
- **Application logs** for: "Using DocumentAttachment template from database"
- **Email received** has correct formatting from template
- **Subject line** matches template subject
- **Placeholders** are replaced with actual values

---

## 🔧 Manual Fix: Create Template if Missing

If migration didn't create the template, create it manually:

```powershell
$template = @{
    templateName = "Document Attachment Email"
    templateKey = "DocumentAttachment"
    category = "Documents"
    subject = "Document: {{BarCode}}"
    isActive = $true
    isDefault = $true
    htmlBody = @"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body { font-family: Arial, sans-serif; margin: 0; padding: 0; background-color: #f4f4f4; }
        .container { max-width: 600px; margin: 20px auto; background-color: #fff; border-radius: 8px; overflow: hidden; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }
        .header { background-color: #0051A5; color: #fff; padding: 30px 20px; text-align: center; }
        .header h1 { margin: 0; font-size: 24px; }
        .content { padding: 30px 20px; color: #333; line-height: 1.6; }
        .info-box { background-color: #f8f9fa; border-left: 4px solid #0051A5; padding: 15px; margin: 20px 0; }
        .info-box strong { color: #0051A5; }
        .footer { background-color: #f8f9fa; padding: 20px; text-align: center; font-size: 12px; color: #666; border-top: 1px solid #e0e0e0; }
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>IKEA DocuScan</h1>
        </div>
        <div class='content'>
            <h2>Document Attached</h2>
            <p>A document has been sent to you from the IKEA DocuScan system.</p>
            {{#Message}}
            <div class='info-box'><p>{{Message}}</p></div>
            {{/Message}}
            <div class='info-box'>
                <p><strong>Document Bar Code:</strong> {{BarCode}}</p>
                <p><strong>File Name:</strong> {{FileName}}</p>
            </div>
            <p>The document is attached to this email.</p>
        </div>
        <div class='footer'>
            <p>This is an automated message from IKEA DocuScan System.</p>
            <p>Please do not reply to this email.</p>
        </div>
    </div>
</body>
</html>
"@
    plainTextBody = @"
IKEA DocuScan - Document Attached

A document has been sent to you from the IKEA DocuScan system.

{{#Message}}
Message: {{Message}}
{{/Message}}

Document Bar Code: {{BarCode}}
File Name: {{FileName}}

The document is attached to this email.

---
This is an automated message from IKEA DocuScan System.
Please do not reply to this email.
"@
    placeholderDefinitions = @"
{
  "placeholders": [
    { "name": "BarCode", "description": "Document bar code", "example": "12345" },
    { "name": "FileName", "description": "Attachment file name", "example": "invoice.pdf" },
    { "name": "Message", "description": "Optional message", "example": "Please review" }
  ]
}
"@
} | ConvertTo-Json -Depth 10

Invoke-RestMethod -Uri "https://localhost:44101/api/configuration/email-templates" `
    -Method POST `
    -Body $template `
    -ContentType "application/json" `
    -UseDefaultCredentials `
    -SkipCertificateCheck
```

---

## 📝 Summary

**Most Likely Causes (in order):**
1. ✅ Template doesn't exist in database → Run migration
2. ✅ Template is inactive → Activate it
3. ✅ Cache is stale → Reload cache
4. ✅ Wrong template key → Verify key is exactly "DocumentAttachment"

**Quick Fix:**
```powershell
# 1. Run migration
Invoke-RestMethod -Uri "https://localhost:44101/api/configuration/migrate" -Method POST -Body '{"overwriteExisting":false}' -ContentType "application/json" -UseDefaultCredentials -SkipCertificateCheck

# 2. Reload cache
Invoke-RestMethod -Uri "https://localhost:44101/api/configuration/reload" -Method POST -UseDefaultCredentials -SkipCertificateCheck

# 3. Test
.\Test-DocumentAttachmentTemplate.ps1
```

---

## 📞 Next Steps

1. Run `.\Test-DocumentAttachmentTemplate.ps1` to diagnose
2. Check application logs when sending document email
3. Apply fix based on diagnostic results
4. Verify email uses database template
5. If issue persists, check logs for exceptions
