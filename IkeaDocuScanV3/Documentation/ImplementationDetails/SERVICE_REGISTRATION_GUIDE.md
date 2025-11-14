# Service Registration Guide - DocumentName and Currency Services

**Date:** 2025-01-24
**Status:** Ready for Implementation
**Services Added:** DocumentNameService, CurrencyService

---

## Overview

This guide documents the required service registrations for the newly implemented DocumentName and Currency services that support the DocumentPropertiesPage migration.

## Files Modified

### 1. Server-Side: `/IkeaDocuScan-Web/Program.cs`

**Two changes required:**

#### Change #1: Register Services (around line 123)

Add these two lines **after** the existing service registrations:

```csharp
// Data access services
builder.Services.AddScoped<IDocumentService, DocumentService>();
builder.Services.AddScoped<IAuditTrailService, AuditTrailService>();
builder.Services.AddScoped<IScannedFileService, ScannedFileService>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ICounterPartyService, CounterPartyService>();
builder.Services.AddScoped<ICountryService, CountryService>();
builder.Services.AddScoped<IDocumentTypeService, DocumentTypeService>();
builder.Services.AddScoped<IUserPermissionService, UserPermissionService>();

// NEW: Add these two lines
builder.Services.AddScoped<IDocumentNameService, DocumentNameService>();
builder.Services.AddScoped<ICurrencyService, CurrencyService>();
```

#### Change #2: Map Endpoints (around line 174)

Add these two lines **after** the existing endpoint mappings:

```csharp
// Map API endpoints
app.MapDocumentEndpoints();
app.MapCounterPartyEndpoints();
app.MapCountryEndpoints();
app.MapDocumentTypeEndpoints();
app.MapUserPermissionEndpoints();
app.MapScannedFileEndpoints();
app.MapAuditTrailEndpoints();

// NEW: Add these two lines
app.MapDocumentNameEndpoints();
app.MapCurrencyEndpoints();
```

---

### 2. Client-Side: `/IkeaDocuScan-Web.Client/Program.cs`

**One change required:**

#### Register HTTP Services (around line 37)

Add these two lines **after** the existing HTTP service registrations:

```csharp
// Data services
builder.Services.AddScoped<IDocumentService, DocumentHttpService>();
builder.Services.AddScoped<ICounterPartyService, CounterPartyHttpService>();
builder.Services.AddScoped<ICountryService, CountryHttpService>();
builder.Services.AddScoped<IDocumentTypeService, DocumentTypeHttpService>();
builder.Services.AddScoped<IUserPermissionService, UserPermissionHttpService>();
builder.Services.AddScoped<IScannedFileService, ScannedFileHttpService>();
builder.Services.AddScoped<IAuditTrailService, AuditTrailHttpService>();

// NEW: Add these two lines
builder.Services.AddScoped<IDocumentNameService, DocumentNameHttpService>();
builder.Services.AddScoped<ICurrencyService, CurrencyHttpService>();
```

---

## API Endpoints Created

### DocumentName Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/documentnames` | Get all document names |
| GET | `/api/documentnames/bytype/{documentTypeId}` | Get document names filtered by type |
| GET | `/api/documentnames/{id}` | Get a specific document name by ID |

**Authorization:** Requires `HasAccess` policy

### Currency Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/currencies` | Get all currencies |
| GET | `/api/currencies/{code}` | Get a specific currency by code |

**Authorization:** Requires `HasAccess` policy

---

## Service Files Created

### Shared Layer (Interfaces and DTOs)
- ✅ `/IkeaDocuScan.Shared/Interfaces/IDocumentNameService.cs`
- ✅ `/IkeaDocuScan.Shared/Interfaces/ICurrencyService.cs`
- ✅ `/IkeaDocuScan.Shared/DTOs/DocumentNames/DocumentNameDto.cs`
- ✅ `/IkeaDocuScan.Shared/DTOs/Currencies/CurrencyDto.cs`

### Server Layer (Services and Endpoints)
- ✅ `/IkeaDocuScan-Web/Services/DocumentNameService.cs`
- ✅ `/IkeaDocuScan-Web/Services/CurrencyService.cs`
- ✅ `/IkeaDocuScan-Web/Endpoints/DocumentNameEndpoints.cs`
- ✅ `/IkeaDocuScan-Web/Endpoints/CurrencyEndpoints.cs`

### Client Layer (HTTP Services)
- ✅ `/IkeaDocuScan-Web.Client/Services/DocumentNameHttpService.cs`
- ✅ `/IkeaDocuScan-Web.Client/Services/CurrencyHttpService.cs`

### Component Integration
- ✅ `/IkeaDocuScan-Web.Client/Components/DocumentManagement/AdditionalInfoFields.razor` (updated)

---

## Testing After Registration

### Build Test

```bash
cd /app/data/IkeaDocuScanV3
dotnet build
```

**Expected:** Zero compilation errors

### Runtime Test

1. Run the application:
   ```bash
   dotnet run --project IkeaDocuScanV3.AppHost
   ```

2. Navigate to: `http://localhost:44100/documents/register`

3. Verify:
   - Select a Document Type
   - Document Name dropdown populates with filtered names
   - Currency dropdown shows all currencies (USD, EUR, GBP, etc.)

### API Test (Optional)

Use Swagger or curl to test endpoints:

```bash
# Get all document names
curl -X GET http://localhost:44100/api/documentnames

# Get document names for type ID 1
curl -X GET http://localhost:44100/api/documentnames/bytype/1

# Get all currencies
curl -X GET http://localhost:44100/api/currencies

# Get specific currency
curl -X GET http://localhost:44100/api/currencies/USD
```

---

## Troubleshooting

### Build Errors

**Error:** `The type or namespace name 'IDocumentNameService' could not be found`

**Fix:** Check that all files are created in correct locations and namespaces match.

**Error:** `No overload for 'AddScoped' matches delegate 'Func<...>'`

**Fix:** Verify service class names match interface names exactly.

### Runtime Errors

**Error:** `Unable to resolve service for type 'IDocumentNameService'`

**Fix:** Ensure service registration was added to **both** server and client Program.cs files.

**Error:** `404 Not Found` when calling API

**Fix:** Ensure endpoint mapping was added to server Program.cs.

### Data Not Loading

**Issue:** Document Name dropdown stays empty

**Causes:**
1. No data in DocumentName table - add test data
2. DocumentTypeId is null - select a Document Type first
3. DisplayAtCheckIn = false for all records - update database

**Issue:** Currency dropdown stays empty

**Causes:**
1. No data in Currency table - add test data (USD, EUR, GBP)
2. API returning empty list - check database connection

---

## Database Requirements

### DocumentName Table

Ensure at least one row exists:
```sql
INSERT INTO DocumentName (Name, DocumentTypeId)
VALUES ('Standard Agreement', 1);
```

### Currency Table

Ensure currency data exists:
```sql
INSERT INTO Currency (CurrencyCode, Name, DecimalPlaces)
VALUES
    ('USD', 'United States Dollar', 2),
    ('EUR', 'Euro', 2),
    ('GBP', 'British Pound', 2);
```

---

## Next Steps After Registration

1. ✅ **Build and test** the application
2. ⏳ **Implement duplicate detection** backend
3. ⏳ **Add file upload functionality** for Check-in mode
4. ⏳ **Test all three modes** end-to-end

---

**Status:** Ready for user to apply changes
**Estimated Time:** 5 minutes
**Risk Level:** Low (additive changes only)
