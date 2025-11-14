# CounterParty Country Validation Fix

## Problem Summary

**Error:** `DbUpdateException` with inner `SqlException` - Foreign key constraint violation

**Full Error:**
```
The INSERT statement conflicted with the FOREIGN KEY constraint "FK__CounterPa__Count__3D5E1FD2".
The conflict occurred in database "IkeaDocuScan", table "dbo.Country", column 'CountryCode'.
```

**Root Cause:** When creating or updating a CounterParty, the code didn't validate that the provided `Country` code exists in the `Country` table before attempting to save. This caused a foreign key constraint violation at the database level.

## Foreign Key Relationship

The `CounterParty` entity has a foreign key relationship to the `Country` table:

```csharp
// CounterParty.cs
[StringLength(2)]
[Unicode(false)]
public string Country { get; set; } = null!;  // Foreign key column

[ForeignKey("Country")]
[InverseProperty("CounterParties")]
public virtual Country CountryNavigation { get; set; } = null!;  // Navigation property
```

```csharp
// Country.cs
[Key]
[StringLength(2)]
[Unicode(false)]
public string CountryCode { get; set; } = null!;  // Primary key
```

The `CounterParty.Country` property is a 2-character string (e.g., "US", "SE", "DE") that must match an existing `Country.CountryCode` in the database.

## Solution Implemented

Added validation to both `CreateAsync` and `UpdateAsync` methods in `CounterPartyService.cs` to check that the country code exists before attempting to save.

### CreateAsync - Lines 116-123

**Added:**
```csharp
// Validate that the country code exists in the Country table
var countryExists = await context.Countries
    .AnyAsync(c => c.CountryCode == dto.Country);

if (!countryExists)
{
    throw new ValidationException($"Invalid country code '{dto.Country}'. Country does not exist in the system.");
}
```

### UpdateAsync - Lines 170-177

**Added:**
```csharp
// Validate that the country code exists in the Country table
var countryExists = await context.Countries
    .AnyAsync(c => c.CountryCode == dto.Country);

if (!countryExists)
{
    throw new ValidationException($"Invalid country code '{dto.Country}'. Country does not exist in the system.");
}
```

## Benefits

1. **Clear Error Messages:** Users now get a clear `ValidationException` with a helpful message instead of a cryptic database constraint error
2. **Fail Fast:** Validation happens before database save, preventing unnecessary database operations
3. **Consistent Behavior:** Same validation logic in both create and update operations
4. **Better User Experience:** API consumers receive HTTP 400 Bad Request with a clear error message

## Error Response

**Before (500 Internal Server Error):**
```json
{
  "error": "An error occurred while saving the entity changes. See the inner exception for details."
}
```

**After (400 Bad Request):**
```json
{
  "error": "Invalid country code 'XX'. Country does not exist in the system."
}
```

## Checking Available Countries

To see what countries are available in the system, use the Country API endpoints:

### SQL Query
```sql
SELECT CountryCode, Name
FROM Country
ORDER BY Name;
```

### API Endpoint
```http
GET /api/countries
Authorization: Windows Authentication
```

### PowerShell Script
```powershell
# Get list of all countries
$response = Invoke-RestMethod -Uri "https://localhost:44101/api/countries" `
    -Method GET `
    -UseDefaultCredentials `
    -SkipCertificateCheck

# Display country codes and names
$response | ForEach-Object {
    Write-Host "$($_.countryCode): $($_.name)"
}
```

## Common Country Codes

Typical country codes in the system (ISO 3166-1 alpha-2):

| Code | Country |
|------|---------|
| SE   | Sweden |
| US   | United States |
| DE   | Germany |
| GB   | United Kingdom |
| FR   | France |
| DK   | Denmark |
| NO   | Norway |
| FI   | Finland |

**Note:** The actual available countries depend on your database configuration.

## Adding New Countries

If you need to add a new country to the system:

### Method 1: SQL Insert
```sql
INSERT INTO Country (CountryCode, Name)
VALUES ('XX', 'Country Name');
```

### Method 2: API Endpoint (if available)
```http
POST /api/countries
Content-Type: application/json

{
  "countryCode": "XX",
  "name": "Country Name"
}
```

### Method 3: Migration
```csharp
// In a new EF Core migration
migrationBuilder.InsertData(
    table: "Country",
    columns: new[] { "CountryCode", "Name" },
    values: new object[] { "XX", "Country Name" });
```

## Testing Checklist

After deploying this fix, verify:

### Create CounterParty
- [ ] Create counterparty with valid country code (should succeed)
- [ ] Create counterparty with invalid country code (should return 400 with clear error)
- [ ] Error message indicates invalid country code

### Update CounterParty
- [ ] Update counterparty with valid country code (should succeed)
- [ ] Update counterparty with invalid country code (should return 400 with clear error)
- [ ] Error message indicates invalid country code

### Edge Cases
- [ ] Empty country code (should be caught by required validation)
- [ ] Country code with wrong length (should be caught by length validation)
- [ ] Null country code (should be caught by required validation)
- [ ] Lowercase country code (test if case-sensitive)

## Related Entities

Other entities that also have foreign keys to the `Country` table:

### UserPermission
```csharp
[StringLength(2)]
[Unicode(false)]
public string? CountryCode { get; set; }  // Optional foreign key

[ForeignKey("CountryCode")]
[InverseProperty("UserPermissions")]
public virtual Country? CountryCodeNavigation { get; set; }
```

**Status:** CountryCode is optional (nullable) in UserPermission, so validation should allow null but validate non-null values.

## Future Enhancements

### 1. Country Validation Service
Create a dedicated `CountryValidationService` to centralize country validation logic:

```csharp
public interface ICountryValidationService
{
    Task<bool> CountryExistsAsync(string countryCode);
    Task<List<string>> GetValidCountryCodesAsync();
}
```

### 2. Client-Side Validation
Add country dropdown in the UI populated from `/api/countries` endpoint to prevent invalid codes from being submitted.

### 3. Caching
Cache the list of valid country codes to avoid repeated database queries:

```csharp
private static readonly MemoryCache _countryCache = new MemoryCache(new MemoryCacheOptions());

public async Task<bool> CountryExistsAsync(string countryCode)
{
    var validCodes = await _countryCache.GetOrCreateAsync("ValidCountryCodes", async entry =>
    {
        entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
        return await _context.Countries.Select(c => c.CountryCode).ToListAsync();
    });

    return validCodes.Contains(countryCode);
}
```

## Files Modified

| File | Lines | Description |
|------|-------|-------------|
| `CounterPartyService.cs` | 116-123 | Added country validation to CreateAsync |
| `CounterPartyService.cs` | 170-177 | Added country validation to UpdateAsync |
| `COUNTERPARTY_COUNTRY_VALIDATION_FIX.md` | New | This documentation file |

## Best Practices

When working with foreign key relationships:

1. **Always validate foreign keys** before attempting to save
2. **Provide clear error messages** indicating what went wrong
3. **Fail fast** - validate early to avoid unnecessary operations
4. **Consider caching** reference data for frequently validated foreign keys
5. **Document** available reference values for API consumers
6. **Test** foreign key validation in both create and update operations

## Conclusion

This fix prevents foreign key constraint violations by validating that country codes exist before attempting to save CounterParty entities. Users now receive clear, actionable error messages instead of cryptic database errors, improving the overall user experience and making debugging easier.
