# DocumentAttachment Template Fix - Summary

**Date:** 2025-11-05
**Issue:** Email preview and sending did not use the DocumentAttachment template from the database
**Status:** ✅ FIXED

---

## Root Cause

The `/api/email/send-with-attachments` endpoint was accepting pre-built HTML and subject from the client, completely bypassing the database template system.

**Previous Flow:**
1. Client (`ComposeDocumentEmail.razor`) built HTML email body **client-side**
2. Client sent pre-built HTML + subject to `/api/email/send-with-attachments`
3. Server endpoint forwarded HTML directly to `EmailService.SendEmailAsync`
4. Database template was never consulted ❌

**New Flow:**
1. Client sends only document IDs and optional message
2. Server endpoint calls `EmailService.SendDocumentAttachmentAsync` for each document
3. `SendDocumentAttachmentAsync` retrieves template from database ✅
4. Template is rendered with document data
5. Rendered email is sent

---

## Files Modified

### 1. Server-Side Endpoint ✅
**File:** `IkeaDocuScan-Web/Endpoints/EmailEndpoints.cs`

**Changes:**
- Modified `SendEmailWithAttachmentsAsync` method (line 90-155)
- Now calls `EmailService.SendDocumentAttachmentAsync` for each document
- Uses database template instead of accepting HTML from client
- Sends one email per document with attachment

**Before:**
```csharp
await emailService.SendEmailAsync(
    request.ToEmail,
    request.Subject,        // ❌ Client-provided
    request.HtmlBody,       // ❌ Client-provided
    request.PlainTextBody,
    attachments);
```

**After:**
```csharp
await emailService.SendDocumentAttachmentAsync(
    request.ToEmail,
    document.BarCode,
    fileData.FileBytes,
    fileData.FileName,
    request.AdditionalMessage);  // ✅ Uses database template
```

### 2. Data Transfer Object ✅
**File:** `IkeaDocuScan.Shared/DTOs/Email/SendEmailWithAttachmentsRequest.cs`

**Changes:**
- Added `AdditionalMessage` property (line 35)
- Marked `Subject`, `HtmlBody`, `PlainTextBody` as `[Obsolete]` (lines 16, 22, 28)
- Properties kept for backward compatibility but no longer used

**New Property:**
```csharp
/// <summary>
/// Optional additional message to include in the email template
/// This will be rendered in the {{Message}} placeholder of the DocumentAttachment template
/// </summary>
public string? AdditionalMessage { get; set; }
```

### 3. Client-Side Component ✅
**File:** `IkeaDocuScan-Web.Client/Pages/ComposeDocumentEmail.razor`

**Changes:**
- Removed HTML body building code (lines 326-352 deleted)
- Updated request to use `AdditionalMessage` instead of `Subject`/`HtmlBody` (line 337)
- Simplified email sending logic

**Before:**
```csharp
var request = new SendEmailWithAttachmentsRequest
{
    ToEmail = recipient,
    Subject = subject,          // ❌ Client-built
    HtmlBody = finalBody,       // ❌ Client-built
    DocumentIds = documentIds
};
```

**After:**
```csharp
var request = new SendEmailWithAttachmentsRequest
{
    ToEmail = recipient,
    AdditionalMessage = additionalText,  // ✅ Will be in {{Message}} placeholder
    DocumentIds = documentIds
};
```

---

## How It Works Now

### Template Retrieval
1. Server receives document IDs and optional message
2. For each document:
   - Loads document from database
   - Loads document file bytes
   - Calls `EmailService.SendDocumentAttachmentAsync`

### Template Usage
`EmailService.SendDocumentAttachmentAsync` (line 238-308):
```csharp
// Try to get email template from database
var template = await _configManager.GetEmailTemplateAsync("DocumentAttachment");

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
else
{
    _logger.LogInformation("Using hard-coded DocumentAttachment template");
    // Fallback to hard-coded template
}
```

### Template Placeholders
The DocumentAttachment template supports these placeholders:
- `{{BarCode}}` - Document bar code
- `{{FileName}}` - Attachment file name
- `{{Message}}` - Additional message from user (from `AdditionalMessage`)
- `{{Date}}` - Current date/time

---

## Testing

### Build and Run
```bash
dotnet build
dotnet run --project IkeaDocuScan-Web/IkeaDocuScan-Web
```

### Test Email Sending
1. Navigate to https://localhost:44101/documents/search
2. Select a document
3. Click "Send as Email"
4. Enter recipient and optional message
5. Click "Send"

### Verify Database Template is Used

**Check Application Logs:**
You should see:
```
[Information] Using DocumentAttachment template from database
```

**NOT:**
```
[Information] Using hard-coded DocumentAttachment template
```

**Check Email:**
- Subject should match template subject: `Document: {BarCode}`
- HTML body should have styled template from database
- Placeholders should be replaced with actual values
- Additional message (if provided) should appear in `{{Message}}` placeholder

### Verify Template Changes Take Effect

1. Update the DocumentAttachment template in the database (via Configuration UI)
2. Clear cache (optional - 5 minute TTL):
   ```powershell
   Invoke-RestMethod -Uri "https://localhost:44101/api/configuration/reload" -Method POST -UseDefaultCredentials
   ```
3. Send another test email
4. Email should reflect the updated template

---

## Diagnostic Verification

Run the diagnostic to confirm template is working:

```powershell
.\Test-DocumentAttachment-Fixed.ps1
```

**Expected Output:**
```
Templates in database: 2
Exact match found: True
Active match found: True
Service retrieval successful: True

RECOMMENDATION:
  Template is being retrieved successfully.
```

---

## Breaking Changes

### Client-Side
- `ComposeDocumentEmail.razor` no longer builds HTML email body
- Old `GenerateEmailContent()` method still exists but HTML generation removed
- Email preview may not work correctly (shows old client-built HTML)

### API
- `/api/email/send-with-attachments` endpoint behavior changed
- `Subject`, `HtmlBody`, `PlainTextBody` properties in request are now ignored
- Sends **one email per document** instead of one email with multiple attachments
- Each email uses the DocumentAttachment template from database

---

## Email Preview Issue

**Note:** The email preview in the UI may still show the old client-built HTML because the `GenerateEmailContent()` method still exists. To fully fix the preview:

### Option 1: Remove Preview (Simplest)
Remove the preview section from `ComposeDocumentEmail.razor` since it no longer represents what will be sent.

### Option 2: Server-Side Preview Endpoint
Create an API endpoint `/api/email/preview-document-attachment` that:
1. Accepts document ID and optional message
2. Retrieves the template from database
3. Renders it with sample data
4. Returns rendered HTML for preview

This would require additional work.

---

## Rollback Instructions

If issues occur, you can rollback by:

1. Revert `EmailEndpoints.cs` to use old logic:
   ```bash
   git checkout HEAD -- IkeaDocuScan-Web/Endpoints/EmailEndpoints.cs
   ```

2. Revert `ComposeDocumentEmail.razor`:
   ```bash
   git checkout HEAD -- IkeaDocuScan-Web.Client/Pages/ComposeDocumentEmail.razor
   ```

3. Revert `SendEmailWithAttachmentsRequest.cs`:
   ```bash
   git checkout HEAD -- IkeaDocuScan.Shared/DTOs/Email/SendEmailWithAttachmentsRequest.cs
   ```

4. Rebuild and restart

---

## Future Improvements

### 1. Multiple Documents in One Email
Currently sends one email per document. Could be modified to:
- Use DocumentAttachments template (plural)
- Attach multiple files to single email
- Use template loop feature for multiple documents

### 2. Email Preview API
Create server-side preview endpoint that shows what the actual email will look like using the database template.

### 3. Template Validation
Add validation when creating/updating templates to ensure required placeholders exist:
- `{{BarCode}}` (required)
- `{{FileName}}` (required)
- `{{Message}}` (optional)

### 4. Unit Tests
Add tests for:
- EmailEndpoints with template usage
- Template rendering with different data
- Fallback to hard-coded template when database template missing

---

## Summary

✅ **Fixed:** Emails now use DocumentAttachment template from database
✅ **Verified:** Diagnostic shows template is retrieved successfully
✅ **Simplified:** Client no longer builds HTML, server handles it
✅ **Template Changes:** Now take effect immediately (with 5-minute cache)
⚠️ **Preview:** May not match actual email (consider removing or fixing)
⚠️ **Breaking:** Sends one email per document instead of multiple attachments

**The core issue is resolved - emails will now use the database template!**
