# NULL Bytes Graceful Handling Implementation

**Date:** 2025-01-25
**Version:** 1.0
**Status:** Implemented

---

## Overview

This document describes the implementation of graceful handling for `DocumentFile.Bytes` being NULL or empty. This allows the system to maintain data integrity and metadata even when file content is unavailable (e.g., after storage optimization, archival, or data issues).

## Business Problem

**Scenario:** To optimize database storage, the `DocumentFile.Bytes` column may be set to NULL while preserving file metadata (FileName, FileType). Without proper handling, this causes:

- Empty files downloaded (0 bytes)
- Empty email attachments sent
- Misleading UI indicators
- Silent failures (no error messages)
- Poor user experience

**Solution:** Detect NULL/empty Bytes and handle gracefully with clear error messages and accurate UI indicators.

---

## Implementation Summary

### Files Modified

1. **DocumentService.cs**
   - `GetDocumentFileAsync()`: Added NULL/empty Bytes detection
   - `SearchAsync()`: Added `.Include(d => d.File)` to load file metadata
   - `MapToDto()`: Added `HasFileContent` property mapping
   - `MapToSearchItemDto()`: Added `HasFileContent` property mapping

2. **DocumentDto.cs**
   - Added `HasFileContent` property (bool)

3. **DocumentSearchItemDto.cs**
   - Added `HasFileContent` property (bool)

4. **EmailEndpoints.cs**
   - No changes needed (already checks for NULL fileData)

---

## Detailed Changes

### 1. DocumentService.GetDocumentFileAsync()

**Location:** `IkeaDocuScan-Web/Services/DocumentService.cs:1048-1088`

**Change:** Added validation to detect NULL or empty Bytes and return `null` instead of empty array.

**Before:**
```csharp
if (document?.File == null)
{
    _logger.LogWarning("Document file not found...");
    return null;
}

return new DocumentFileDto
{
    FileBytes = document.File.Bytes ?? Array.Empty<byte>(), // ❌ Returns empty array
    FileName = document.File.FileName,
    ContentType = DetermineContentType(document.File.FileName)
};
```

**After:**
```csharp
if (document?.File == null)
{
    _logger.LogWarning("Document file not found...");
    return null;
}

// ✅ Check if file content (Bytes) is NULL or empty
if (document.File.Bytes == null || document.File.Bytes.Length == 0)
{
    _logger.LogWarning("Document file exists but has no content (NULL or empty Bytes) for document ID: {Id}, FileName: {FileName}, user: {User}",
        id, document.File.FileName, currentUser.AccountName);
    return null; // ✅ Return null instead of empty array
}

return new DocumentFileDto
{
    FileBytes = document.File.Bytes, // ✅ Guaranteed to have content
    FileName = document.File.FileName,
    ContentType = DetermineContentType(document.File.FileName)
};
```

**Impact:**
- **Download endpoint** (`/api/documents/{id}/download`) returns **404 Not Found** instead of 0-byte file
- **Stream endpoint** (`/api/documents/{id}/stream`) returns **404 Not Found** instead of blank viewer
- **Email attachments** skip documents with NULL bytes (existing check already handled this)
- **Logging** now captures specific reason (NULL bytes) for troubleshooting

---

### 2. DocumentDto and DocumentSearchItemDto

**New Property:** `HasFileContent`

**Purpose:** Allows UI to distinguish between:
- Document has no file attached (`FileId == null`)
- Document has file metadata but no content (`FileId != null` AND `HasFileContent == false`)
- Document has file with content (`FileId != null` AND `HasFileContent == true`)

**DocumentDto.cs:**
```csharp
public int? FileId { get; set; }
public string? FileName { get; set; }
public bool HasFileContent { get; set; }  // ✅ NEW: True if file exists AND has content
```

**DocumentSearchItemDto.cs:**
```csharp
public bool HasFile { get; set; }
public int? FileId { get; set; }
public bool HasFileContent { get; set; }  // ✅ NEW
```

---

### 3. DocumentService Mapping Methods

**MapToDto() - Line 834:**
```csharp
FileId = entity.FileId,
FileName = entity.File?.FileName,
HasFileContent = entity.File != null && entity.File.Bytes != null && entity.File.Bytes.Length > 0,
```

**MapToSearchItemDto() - Line 810:**
```csharp
HasFile = entity.FileId.HasValue,
FileId = entity.FileId,
HasFileContent = entity.File != null && entity.File.Bytes != null && entity.File.Bytes.Length > 0,
```

**Note:** Search query now includes `.Include(d => d.File)` to load file metadata for HasFileContent check.

---

### 4. Search Query Optimization Note

**Location:** `DocumentService.SearchAsync():498-506`

**Change:** Added `.Include(d => d.File)` to search query

```csharp
IQueryable<Document> query = _context.Documents
    .Include(d => d.Dt)
    .Include(d => d.DocumentName)
    .Include(d => d.CounterParty)
        .ThenInclude(cp => cp!.CountryNavigation)
    .Include(d => d.File)  // ✅ NEW: Include File to check Bytes availability
                           // NOTE: This loads file bytes into memory. Consider optimizing with a
                           // projection or computed column if performance becomes an issue.
    .AsQueryable();
```

**Performance Consideration:**
- This loads all file bytes into memory for search results
- May impact performance with large result sets or large files
- **Future optimization:** Use SQL projection to check `Bytes IS NOT NULL` without loading actual bytes

**Optimization Example (Future):**
```csharp
// Instead of Include, use Select to project only needed data
var results = await query.Select(d => new {
    Document = d,
    HasFileContent = d.File != null && d.File.Bytes != null && d.File.Bytes.Length > 0
}).ToListAsync();
```

---

## API Behavior Changes

### GET /api/documents/{id}/stream

**Before:** Returns HTTP 200 with 0 bytes when Bytes is NULL
**After:** Returns HTTP 404 Not Found

**Response:**
```json
{
  "error": "Document file not found for document ID 123"
}
```

### GET /api/documents/{id}/download

**Before:** Downloads empty file "document.pdf (0 KB)"
**After:** Returns HTTP 404 Not Found

**Response:**
```json
{
  "error": "Document file not found for document ID 123"
}
```

### POST /api/email/send-with-attachments

**Before:** Sends email with 0-byte attachments (silent failure)
**After:** Skips documents with NULL bytes, logs warning

**Log Entry:**
```
[Warning] Document file not found for document ID {DocumentId}
```

**Behavior:**
- If ALL documents have NULL bytes: Returns 400 BadRequest "No document files found"
- If SOME documents have NULL bytes: Sends email with only valid attachments

---

## UI Integration

### Displaying File Status

Use the new `HasFileContent` property to show appropriate UI indicators:

**Razor Example:**
```razor
@if (document.FileId.HasValue)
{
    @if (document.HasFileContent)
    {
        <!-- File available - show download/view buttons -->
        <button @onclick="DownloadFile">📄 Download</button>
        <button @onclick="ViewFile">👁️ View</button>
    }
    else
    {
        <!-- File metadata exists but content missing -->
        <span class="text-warning">⚠️ File content unavailable</span>
        <small class="text-muted">(@document.FileName)</small>
    }
}
else
{
    <!-- No file attached -->
    <span class="text-muted">No file attached</span>
}
```

### Search Results Grid

```razor
@foreach (var doc in searchResults.Items)
{
    <tr>
        <td>@doc.BarCode</td>
        <td>@doc.DocumentType</td>
        <td>
            @if (doc.HasFile)
            {
                @if (doc.HasFileContent)
                {
                    <span class="badge bg-success">📄 File Available</span>
                }
                else
                {
                    <span class="badge bg-warning">⚠️ Content Missing</span>
                }
            }
            else
            {
                <span class="badge bg-secondary">No File</span>
            }
        </td>
    </tr>
}
```

---

## Logging

### New Log Entries

**When NULL Bytes detected:**
```
[Warning] Document file exists but has no content (NULL or empty Bytes)
for document ID: 123, FileName: contract.pdf, user: DOMAIN\username
```

**When file not found:**
```
[Warning] Document file not found or access denied for document ID: 123, user: DOMAIN\username
```

**When email skips document:**
```
[Warning] Document file not found for document ID 123
```

### Log Analysis Queries

**Find NULL byte warnings in logs:**
```sql
SELECT *
FROM YourLogTable
WHERE Message LIKE '%has no content (NULL or empty Bytes)%'
  AND Timestamp >= DATEADD(DAY, -7, GETDATE())
ORDER BY Timestamp DESC;
```

---

## Testing Guide

### Test Scenario 1: NULL Bytes in Database

**Setup:**
```sql
-- Set specific document file bytes to NULL
UPDATE DocumentFile
SET Bytes = NULL
WHERE Id = 123;
```

**Expected Results:**
1. ✅ GET `/api/documents/{id}/stream` returns 404
2. ✅ GET `/api/documents/{id}/download` returns 404
3. ✅ Search results show `HasFileContent = false`
4. ✅ Document detail shows `HasFileContent = false`
5. ✅ Email attachment skips this document
6. ✅ Log entry created with "has no content" message

### Test Scenario 2: Empty Bytes (0 length)

**Setup:**
```sql
-- Set bytes to empty array
UPDATE DocumentFile
SET Bytes = 0x
WHERE Id = 456;
```

**Expected Results:** Same as Scenario 1 (empty bytes treated as NULL)

### Test Scenario 3: Valid Bytes

**Setup:** Use document with actual file content

**Expected Results:**
1. ✅ GET `/api/documents/{id}/stream` returns 200 with file content
2. ✅ GET `/api/documents/{id}/download` returns 200 with file
3. ✅ Search results show `HasFileContent = true`
4. ✅ Email includes attachment

### Test Scenario 4: Mixed Batch Email

**Setup:**
```json
POST /api/email/send-with-attachments
{
  "toEmail": "test@example.com",
  "documentIds": [100, 123, 456], // 100 has content, 123 and 456 have NULL bytes
  "message": "Test"
}
```

**Expected Results:**
1. ✅ Email sent successfully
2. ✅ Only document 100 attached
3. ✅ Log warnings for documents 123 and 456
4. ✅ No error returned (partial success)

---

## SQL Scripts

See `Documentation/SQL_Scripts/NULL_Bytes_Management.sql` for:
- Analyze current state
- NULL out bytes (with options)
- Backup/restore strategies
- Monitoring queries

---

## Performance Considerations

### Current Implementation

**Search Query:**
- Loads all file bytes into memory via `.Include(d => d.File)`
- Necessary to check `HasFileContent` property
- May impact performance with large result sets

**Performance Impact:**
- **Low** for small files (<1 MB)
- **Medium** for moderate files (1-10 MB) and <100 results
- **High** for large files (>10 MB) or >100 results

### Future Optimization Options

**Option 1: Database Computed Column**
```sql
ALTER TABLE DocumentFile
ADD HasContent AS (
    CASE WHEN Bytes IS NOT NULL AND DATALENGTH(Bytes) > 0
    THEN CAST(1 AS BIT)
    ELSE CAST(0 AS BIT)
    END
) PERSISTED;
```
Then query without loading Bytes.

**Option 2: LINQ Projection**
```csharp
var query = _context.Documents
    .Select(d => new {
        Document = d,
        HasFileContent = d.File != null && d.File.Bytes != null && d.File.Bytes.Length > 0
    });
```

**Option 3: Separate Metadata Table**
- Move Bytes to `DocumentFileContent` table
- Keep FileName, FileType, HasContent in `DocumentFile`

---

## Migration Checklist

- [x] Update `DocumentService.GetDocumentFileAsync()`
- [x] Add `HasFileContent` to `DocumentDto`
- [x] Add `HasFileContent` to `DocumentSearchItemDto`
- [x] Update `MapToDto()` method
- [x] Update `MapToSearchItemDto()` method
- [x] Update search query to include File
- [x] Create SQL scripts for NULL bytes management
- [x] Document implementation
- [ ] Update UI components to show content status
- [ ] Add client-side error handling for 404 responses
- [ ] Update user documentation
- [ ] Add monitoring/alerting for NULL bytes detection

---

## Known Issues / Future Work

1. **Performance:** Search query loads all file bytes into memory
   - **Mitigation:** Consider computed column or projection optimization

2. **UI Updates:** Frontend components need updates to display `HasFileContent` indicator
   - **Location:** `DocumentPropertiesPage.razor`, `SearchDocuments.razor`

3. **Reports:** Reports still reference `DocumentFile.FileName` which exists even when Bytes is NULL
   - **Impact:** Low - reports show filename but no way to indicate content missing
   - **Solution:** Add HasFileContent column to report DTOs

4. **Audit Trail:** Doesn't track when Bytes are NULLed
   - **Solution:** Add audit log entry when Bytes set to NULL

---

## Rollback Plan

If issues arise, rollback is simple since no database schema changes were made:

1. Revert code changes to `DocumentService.cs`
2. Revert DTO property additions
3. Previous behavior: Returns empty byte arrays instead of NULL

**No data migration needed for rollback.**

---

## Contact

For questions or issues with this implementation, contact the development team.
