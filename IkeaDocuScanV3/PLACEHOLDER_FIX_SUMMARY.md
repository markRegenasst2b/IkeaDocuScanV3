# Placeholder Fix Summary - DocumentCount and Barcodes

**Date:** 2025-11-05  
**Issue:** `{{DocumentCount}}` and `{{Barcodes}}` placeholders not being replaced  
**Status:** ✅ FIXED

---

## Changes Made

### 1. ✅ EmailService.cs - Added New Placeholders

**File:** `IkeaDocuScan-Web/Services/EmailService.cs` (line 410-417)

**Added placeholders to the data dictionary:**
- `{{DocumentCount}}` - Number of documents (same as `{{Count}}`)
- `{{Barcodes}}` - Comma-separated list of all barcodes

**Before:**
```csharp
var data = new Dictionary<string, object>
{
    { "Count", documentList.Count },
    { "Message", message ?? string.Empty },
    { "Date", DateTime.Now }
};
```

**After:**
```csharp
var data = new Dictionary<string, object>
{
    { "Count", documentList.Count },
    { "DocumentCount", documentList.Count }, // NEW: Alternative placeholder name
    { "Barcodes", string.Join(", ", documentList.Select(d => d.BarCode)) }, // NEW: Comma-separated barcodes
    { "Message", message ?? string.Empty },
    { "Date", DateTime.Now }
};
```

### 2. ✅ EmailEndpoints.cs - Send One Email with All Documents

**File:** `IkeaDocuScan-Web/Endpoints/EmailEndpoints.cs` (line 112-146)

**Changed from:** Sending one email per document  
**Changed to:** Sending ONE email with ALL documents as attachments

### 3. ✅ ComposeDocumentEmail.razor - Fixed Preview

**File:** `IkeaDocuScan-Web.Client/Pages/ComposeDocumentEmail.razor` (line 131-174)

**For attachments:** Shows database template notice instead of old preview  
**For links:** Still shows regular preview (unchanged)

---

## Supported Placeholders

Your DocumentAttachments template now supports:

### Simple Placeholders
- `{{Count}}` or `{{DocumentCount}}` - Number of documents
- `{{Barcodes}}` - Comma-separated list of all barcodes (e.g., "12345, 67890, 11111")
- `{{Message}}` - Additional message from user
- `{{Date}}` - Current date/time

### Loop (for advanced templates)
```
{{#DocumentRows}}
  <li>{{BarCode}} - {{FileName}}</li>
{{/DocumentRows}}
```

---

## Testing

### 1. Rebuild and Restart
```bash
dotnet build
dotnet run --project IkeaDocuScan-Web/IkeaDocuScan-Web
```

### 2. Check Your Template
Run this script to see what placeholders your template uses:
```powershell
.\Check-Template-Placeholders.ps1
```

### 3. Send Test Email
1. Go to https://localhost:44101/documents/search
2. Select multiple documents
3. Click "Send as Email"
4. Enter recipient
5. Click "Send"

### 4. Verify Placeholders Are Replaced
Check the received email:
- `{{DocumentCount}}` should show the number (e.g., "3")
- `{{Barcodes}}` should show comma-separated list (e.g., "12345, 67890, 11111")
- `{{Message}}` should show your additional text
- Subject and body should use your database template

### 5. Check Logs
You should see:
```
[Information] Using DocumentAttachments template from database
[Information] Email with 3 attachments sent successfully to recipient@example.com
```

---

## Email Preview

The email preview now shows:
- **For Attachments:** A notice that the database template will be used, with:
  - Recipient
  - Document Count
  - Barcodes list
  - Additional Message (if provided)
- **For Links:** Regular preview (unchanged)

This is because the actual rendering happens server-side using the database template.

---

## Template Configuration

### Example Template Subject
```
Documents: {{DocumentCount}} file(s)
```

### Example Template Body
```html
<p>You have received {{DocumentCount}} documents:</p>
<p><strong>Barcodes:</strong> {{Barcodes}}</p>

{{#Message}}
<div class="message">
  <strong>Additional Message:</strong>
  <p>{{Message}}</p>
</div>
{{/Message}}

<p>Documents:</p>
<ul>
{{#DocumentRows}}
  <li>{{BarCode}} - {{FileName}}</li>
{{/DocumentRows}}
</ul>
```

---

## Summary of Fixes

✅ **ONE email** sent with all documents (not multiple emails)  
✅ **{{DocumentCount}}** placeholder supported  
✅ **{{Barcodes}}** placeholder supported (comma-separated)  
✅ **Email preview** updated to show database template notice  
✅ **Backward compatible** - still supports {{Count}} and {{#DocumentRows}} loop

---

## Files Modified

| File | Lines | Changes |
|------|-------|---------|
| `EmailService.cs` | 413-414 | Added DocumentCount and Barcodes placeholders |
| `EmailEndpoints.cs` | 112-146 | Changed to send one email with all documents |
| `ComposeDocumentEmail.razor` | 131-174 | Updated preview for attachments |
| `Check-Template-Placeholders.ps1` | New | Script to inspect template placeholders |

---

The fix is complete! Your `{{DocumentCount}}` and `{{Barcodes}}` placeholders will now be replaced correctly! 🎉
