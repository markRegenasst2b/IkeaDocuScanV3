# Document Links Fix Summary

**Date:** 2025-11-05  
**Issue:** Send email with document links not using database template and placeholders not replaced  
**Status:** ✅ FIXED

---

## Changes Made

### 1. ✅ EmailService.cs - Added Placeholders to DocumentLinks Template

**File:** `IkeaDocuScan-Web/Services/EmailService.cs` (line 334-341)

**Added:**
- `{{DocumentCount}}` placeholder
- `{{Barcodes}}` placeholder (comma-separated list)

### 2. ✅ Created New DTO for Document Links

**File:** `IkeaDocuScan.Shared/DTOs/Email/SendEmailWithLinksRequest.cs` (NEW)

```csharp
public class SendEmailWithLinksRequest
{
    public string ToEmail { get; set; } = string.Empty;
    public string? AdditionalMessage { get; set; }
    public List<int> DocumentIds { get; set; } = new();
}
```

### 3. ✅ Added Server Endpoint

**File:** `IkeaDocuScan-Web/Endpoints/EmailEndpoints.cs` (lines 35-40, 171-242)

**New endpoint:** `POST /api/email/send-with-links`

**Features:**
- Loads documents by IDs
- Generates document links
- Calls `EmailService.SendDocumentLinksAsync` with database template
- Sends ONE email with all document links

### 4. ✅ Added Client HTTP Service Method

**File:** `IkeaDocuScan-Web.Client/Services/EmailHttpService.cs` (lines 83-113)

**New method:** `SendEmailWithLinksAsync`

### 5. ✅ Updated Razor Component

**File:** `IkeaDocuScan-Web.Client/Pages/ComposeDocumentEmail.razor`

**Changes:**
- Email preview now shows database template notice for BOTH attachments AND links (line 131-147)
- Links now use new endpoint instead of client-built HTML (line 345-352)
- Sends `AdditionalMessage` instead of `Subject` and `HtmlBody`

---

## How It Works Now

### For Document Links:
1. Client sends document IDs and optional message to `/api/email/send-with-links`
2. Server loads documents and generates links
3. Server calls `EmailService.SendDocumentLinksAsync`
4. EmailService retrieves DocumentLinks template from database
5. Template is rendered with these placeholders:
   - `{{DocumentCount}}` - Number of documents
   - `{{Barcodes}}` - Comma-separated barcodes
   - `{{Message}}` - Additional message
   - `{{Date}}` - Current date
   - `{{#DocumentRows}}` loop for individual documents
6. ONE email sent with all document links

---

## Supported Placeholders

### DocumentLinks Template
- `{{Count}}` or `{{DocumentCount}}` - Number of documents
- `{{Barcodes}}` - Comma-separated barcodes (e.g., "12345, 67890")
- `{{Message}}` - Additional message from user
- `{{Date}}` - Current date/time

### DocumentRows Loop
```
{{#DocumentRows}}
  <li><a href="{{Link}}">{{BarCode}}</a></li>
{{/DocumentRows}}
```

---

## Email Preview

Both attachments and links now show:

```
📘 Database Template Will Be Used

The actual email will be generated from the DocumentAttachments (or DocumentLinks) 
database template with these details:
• Recipient: recipient@example.com
• Document Count: 3
• Barcodes: 12345, 67890, 11111
• Additional Message: Your message here

Preview not available. The email template is managed via Configuration Management.
```

---

## Testing

### 1. Rebuild and Restart
```bash
dotnet build
dotnet run --project IkeaDocuScan-Web/IkeaDocuScan-Web
```

### 2. Test Document Links
1. Go to https://localhost:44101/documents/search
2. Select multiple documents
3. Click "Send as Email"
4. Choose "Links" option
5. Send email

### 3. Verify
Check the received email:
- `{{DocumentCount}}` shows the number
- `{{Barcodes}}` shows comma-separated list
- `{{Message}}` shows your additional text
- Uses DocumentLinks template from database

### 4. Check Logs
You should see:
```
[Information] Using DocumentLinks template from database
[Information] Email with 3 document links sent successfully to recipient@example.com
```

---

## Files Modified/Created

| File | Status | Changes |
|------|--------|---------|
| `EmailService.cs` | Modified | Added DocumentCount and Barcodes placeholders (line 337-338) |
| `SendEmailWithLinksRequest.cs` | NEW | DTO for sending document links |
| `EmailEndpoints.cs` | Modified | Added SendEmailWithLinksAsync endpoint |
| `EmailHttpService.cs` | Modified | Added SendEmailWithLinksAsync method |
| `ComposeDocumentEmail.razor` | Modified | Updated preview and sending for links |
| `LINKS_FIX_SUMMARY.md` | NEW | This documentation |

---

## Summary

✅ **DocumentLinks template** now used from database  
✅ **{{DocumentCount}}** placeholder supported  
✅ **{{Barcodes}}** placeholder supported  
✅ **Email preview** shows database template notice  
✅ **ONE email** sent with all document links  
✅ **Same behavior** as attachments for consistency

Both document attachments AND document links now use database templates! 🎉
