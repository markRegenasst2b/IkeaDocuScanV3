# Migration Summary: Batch 2 - Documents, Counter Parties, Scanned Files

**Date:** 2025-11-20
**Categories:** 3
**Total Endpoints:** 23
**Status:** Code Complete - Ready for Build/Test

---

## Categories Migrated

### 1. DocumentEndpoints (10 endpoints)
**File:** `IkeaDocuScan-Web/Endpoints/DocumentEndpoints.cs`

| Endpoint | Method | Route |
|----------|--------|-------|
| GetAllDocuments | GET | /api/documents/ |
| GetDocumentById | GET | /api/documents/{id} |
| GetDocumentByBarCode | GET | /api/documents/barcode/{barCode} |
| GetDocumentsByIds | POST | /api/documents/by-ids |
| CreateDocument | POST | /api/documents/ |
| UpdateDocument | PUT | /api/documents/{id} |
| DeleteDocument | DELETE | /api/documents/{id} |
| SearchDocuments | POST | /api/documents/search |
| StreamDocumentFile | GET | /api/documents/{id}/stream |
| DownloadDocumentFile | GET | /api/documents/{id}/download |

**Authorization Pattern:**
- Read operations: All roles with HasAccess
- Write operations (POST/PUT): Publisher + SuperUser
- Delete operations: SuperUser only

### 2. CounterPartyEndpoints (7 endpoints)
**File:** `IkeaDocuScan-Web/Endpoints/CounterPartyEndpoints.cs`

| Endpoint | Method | Route |
|----------|--------|-------|
| GetAllCounterParties | GET | /api/counterparties/ |
| SearchCounterParties | GET | /api/counterparties/search |
| GetCounterPartyById | GET | /api/counterparties/{id} |
| CreateCounterParty | POST | /api/counterparties/ |
| UpdateCounterParty | PUT | /api/counterparties/{id} |
| DeleteCounterParty | DELETE | /api/counterparties/{id} |
| GetCounterPartyUsage | GET | /api/counterparties/{id}/usage |

**Authorization Pattern:**
- Read operations: All roles with HasAccess
- Write operations (POST/PUT): Publisher + SuperUser
- Delete operations: SuperUser only

### 3. ScannedFileEndpoints (6 endpoints)
**File:** `IkeaDocuScan-Web/Endpoints/ScannedFileEndpoints.cs`

| Endpoint | Method | Route |
|----------|--------|-------|
| GetAllScannedFiles | GET | /api/scannedfiles/ |
| GetScannedFileByName | GET | /api/scannedfiles/{fileName} |
| GetScannedFileContent | GET | /api/scannedfiles/{fileName}/content |
| CheckScannedFileExists | GET | /api/scannedfiles/{fileName}/exists |
| GetScannedFileStream | GET | /api/scannedfiles/{fileName}/stream |
| DeleteScannedFile | DELETE | /api/scannedfiles/{fileName} |

**Authorization Pattern:**
- Read operations: All roles with HasAccess
- Delete operations: Publisher + SuperUser (per database seed)

---

## Changes Made

**For each file:**
- Group authorization: Changed from `.RequireAuthorization("HasAccess")` to `.RequireAuthorization()`
- Endpoint policies: Added dynamic policy to each endpoint
- Documentation: Updated comments to reflect dynamic authorization

**Total Lines Changed:** ~46 (23 endpoint policies + 3 group changes + comments)

---

## Common Pattern

All three categories follow the same migration pattern:

```csharp
// BEFORE
var group = routes.MapGroup("/api/resource")
    .RequireAuthorization("HasAccess")
    .WithTags("Resource");

group.MapGet("/", handler)
    .WithName("GetAll")
    .Produces<List<Dto>>(200);

// AFTER
var group = routes.MapGroup("/api/resource")
    .RequireAuthorization()  // Base authentication
    .WithTags("Resource");

group.MapGet("/", handler)
    .WithName("GetAll")
    .RequireAuthorization("Endpoint:GET:/api/resource/")
    .Produces<List<Dto>>(200);
```

---

## Overall Progress

**After Batch 2:**
- **Categories Complete:** 6 of 15 (40%)
- **Endpoints Migrated:** 57 of 126 (45%)

**Completed Categories:**
1. ✅ LogViewerEndpoints (5 endpoints)
2. ✅ UserPermissionEndpoints (11 endpoints)
3. ✅ ConfigurationEndpoints (18 endpoints)
4. ✅ DocumentEndpoints (10 endpoints)
5. ✅ CounterPartyEndpoints (7 endpoints)
6. ✅ ScannedFileEndpoints (6 endpoints)

**Remaining Categories:** 9 (69 endpoints)

---

## Testing Status

**Ready for build and test** - All code changes complete for Batch 2

**Note:** These 3 categories are core features (documents, counter parties, file access), so thorough testing is recommended.

---

## Notes

- No breaking changes to endpoint signatures
- No changes to business logic
- Only authorization mechanism changed
- All endpoints with route parameters ({id}, {fileName}, {barCode}) benefit from the route template matching fix implemented earlier
- Authorization now database-driven, allowing runtime permission changes without code deployment

---

**Migration Status:** CODE COMPLETE
**Next Step:** Build and test
**Risk Level:** Low (established pattern, well-tested authorization infrastructure)
