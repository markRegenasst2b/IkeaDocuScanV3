# User Permission Filtering - Implementation Proposal

**Date:** 2025-11-06
**Author:** Claude Code
**Status:** Proposal - Ready for Review

---

## Executive Summary

This proposal outlines the implementation of fine-grained document access control based on `UserPermission` entries. Users will only see documents that match at least one of their assigned permissions, where each permission defines optional filters for `DocumentTypeId`, `CounterPartyId`, and `CountryCode`.

**Current State Analysis:**
- ✅ Data model already exists (`UserPermission`, `Document`, `CounterParty` entities)
- ✅ `CurrentUserService` loads and caches user permissions
- ✅ `CurrentUser` model has helper methods for permission checking
- ❌ **Critical Issue:** `CurrentUser.CanAccessDocument()` uses incorrect AND logic on flattened lists
- ❌ **Missing:** `DocumentService` does not filter documents by permissions
- ❌ **Missing:** Efficient database-level filtering in queries

**Impact:**
- **HIGH** - All document query methods must be updated
- **MEDIUM** - Permission checking logic needs correction
- **LOW** - Database indexes recommended for performance

---

## 1. Data Model Changes

### 1.1 Current Entity Structure

**UserPermission Entity** (Already Exists - No Changes Needed)
```csharp
public partial class UserPermission
{
    [Key]
    public int Id { get; set; }

    public int? DocumentTypeId { get; set; }
    public int? CounterPartyId { get; set; }

    [StringLength(2)]
    [Unicode(false)]
    public string? CountryCode { get; set; }

    public int UserId { get; set; }

    // Navigation properties
    [ForeignKey("UserId")]
    public virtual DocuScanUser User { get; set; } = null!;

    [ForeignKey("DocumentTypeId")]
    public virtual DocumentType? DocumentType { get; set; }

    [ForeignKey("CounterPartyId")]
    public virtual CounterParty? CounterParty { get; set; }

    [ForeignKey("CountryCode")]
    public virtual Country? CountryCodeNavigation { get; set; }
}
```

**Document Entity** (No Changes Needed)
```csharp
public partial class Document
{
    public int Id { get; set; }
    public int BarCode { get; set; }

    [Column("DT_ID")]
    public int? DtId { get; set; }  // DocumentTypeId

    public int? CounterPartyId { get; set; }

    // Country comes through CounterParty.Country

    [ForeignKey("DtId")]
    public virtual DocumentType? Dt { get; set; }

    [ForeignKey("CounterPartyId")]
    public virtual CounterParty? CounterParty { get; set; }
}
```

**CounterParty Entity** (No Changes Needed)
```csharp
public partial class CounterParty
{
    public int CounterPartyId { get; set; }

    [StringLength(2)]
    [Unicode(false)]
    public string Country { get; set; } = null!;  // Country code

    [ForeignKey("Country")]
    public virtual Country CountryNavigation { get; set; } = null!;
}
```

### 1.2 Recommended Index Additions

**Performance Optimization Indexes:**

```sql
-- Composite index for permission matching on Document table
CREATE NONCLUSTERED INDEX IX_Document_PermissionFilter
ON dbo.Document (DT_ID, CounterPartyId)
INCLUDE (BarCode, Name);

-- Index on CounterParty.Country for permission filtering
CREATE NONCLUSTERED INDEX IX_CounterParty_Country
ON dbo.CounterParty (Country)
INCLUDE (CounterPartyId);

-- Already exists: IX_UserPermissions_UserId
-- No additional indexes needed on UserPermission table
```

**Rationale:**
- `IX_Document_PermissionFilter`: Speeds up filtering by DocumentTypeId and CounterPartyId
- `IX_CounterParty_Country`: Speeds up country-based permission checks via JOIN
- Includes covering columns to avoid key lookups

### 1.3 Schema Summary

**No Schema Changes Required** - All necessary columns and relationships already exist.

---

## 2. Repository / EF Core Query Layer

### 2.1 Implementation Strategy Comparison

#### Strategy 1: LINQ with Any() - Subquery Approach ⭐ RECOMMENDED

**Implementation:**
```csharp
public static IQueryable<Document> FilterByUserPermissions(
    this IQueryable<Document> query,
    int userId,
    bool isSuperUser,
    AppDbContext context)
{
    if (isSuperUser)
        return query; // SuperUser sees all documents

    return query.Where(doc =>
        context.UserPermissions
            .Where(p => p.UserId == userId)
            .Any(perm =>
                // DocumentType filter
                (doc.DtId == null || perm.DocumentTypeId == null || doc.DtId == perm.DocumentTypeId)
                &&
                // CounterParty filter
                (doc.CounterPartyId == null || perm.CounterPartyId == null || doc.CounterPartyId == perm.CounterPartyId)
                &&
                // Country filter (through CounterParty relationship)
                (doc.CounterParty == null ||
                 doc.CounterParty.Country == null ||
                 perm.CountryCode == null ||
                 doc.CounterParty.Country == perm.CountryCode)
            )
    );
}
```

**Generated SQL (Approximate):**
```sql
SELECT d.*
FROM Document d
LEFT JOIN CounterParty cp ON d.CounterPartyId = cp.CounterPartyId
WHERE EXISTS (
    SELECT 1
    FROM UserPermission up
    WHERE up.UserId = @userId
      AND (d.DT_ID IS NULL OR up.DocumentTypeId IS NULL OR d.DT_ID = up.DocumentTypeId)
      AND (d.CounterPartyId IS NULL OR up.CounterPartyId IS NULL OR d.CounterPartyId = up.CounterPartyId)
      AND (cp.Country IS NULL OR up.CountryCode IS NULL OR cp.Country = up.CountryCode)
)
```

**Pros:**
- ✅ Clean, readable LINQ syntax
- ✅ Correctly implements "ANY permission matches" logic
- ✅ SQL Server optimizes EXISTS clauses efficiently
- ✅ Null-safe comparisons built-in
- ✅ Easy to maintain and debug

**Cons:**
- ⚠️ Requires LEFT JOIN for CounterParty (minor overhead)
- ⚠️ Subquery executed for each row (optimized by SQL Server)

**Performance:**
- **Small datasets (< 10,000 docs):** Excellent
- **Large datasets (> 100,000 docs):** Good with proper indexes

---

#### Strategy 2: Precompute + Cache Accessible Documents

**Implementation:**
```csharp
public static async Task<HashSet<int>> GetAccessibleDocumentIdsAsync(
    int userId,
    bool isSuperUser,
    AppDbContext context)
{
    if (isSuperUser)
        return null; // null = all documents

    var permissions = await context.UserPermissions
        .Where(p => p.UserId == userId)
        .ToListAsync();

    var documentIds = await context.Documents
        .Include(d => d.CounterParty)
        .Where(doc => permissions.Any(perm =>
            (doc.DtId == null || perm.DocumentTypeId == null || doc.DtId == perm.DocumentTypeId) &&
            (doc.CounterPartyId == null || perm.CounterPartyId == null || doc.CounterPartyId == perm.CounterPartyId) &&
            (doc.CounterParty == null || doc.CounterParty.Country == null || perm.CountryCode == null || doc.CounterParty.Country == perm.CountryCode)
        ))
        .Select(d => d.Id)
        .ToHashSetAsync();

    return documentIds;
}

// Then filter:
query = query.Where(d => accessibleIds.Contains(d.Id));
```

**Pros:**
- ✅ Ultra-fast filtering once cached (simple IN clause)
- ✅ No subqueries in document searches
- ✅ Can be cached per user for duration of request

**Cons:**
- ❌ Requires loading all documents into memory for initial computation
- ❌ Cache invalidation complexity if permissions change mid-request
- ❌ Not suitable for large document sets (> 100,000 documents)
- ❌ Extra query overhead to precompute IDs

**Performance:**
- **Small datasets:** Overkill, slower than Strategy 1
- **Large datasets with high query frequency:** Could be beneficial with proper caching

**Verdict:** Not recommended for current use case.

---

#### Strategy 3: Raw SQL with JOIN/EXISTS

**Implementation:**
```csharp
public static IQueryable<Document> FilterByUserPermissionsRawSql(
    this IQueryable<Document> query,
    int userId,
    AppDbContext context)
{
    return query.FromSqlRaw(@"
        SELECT d.*
        FROM Document d
        LEFT JOIN CounterParty cp ON d.CounterPartyId = cp.CounterPartyId
        WHERE EXISTS (
            SELECT 1
            FROM UserPermission up
            WHERE up.UserId = @p0
              AND (d.DT_ID IS NULL OR up.DocumentTypeId IS NULL OR d.DT_ID = up.DocumentTypeId)
              AND (d.CounterPartyId IS NULL OR up.CounterPartyId IS NULL OR d.CounterPartyId = up.CounterPartyId)
              AND (cp.Country IS NULL OR up.CountryCode IS NULL OR cp.Country = up.CountryCode)
        )", userId);
}
```

**Pros:**
- ✅ Full control over SQL execution plan
- ✅ Can be tuned with query hints if needed
- ✅ Potentially fastest with expert SQL optimization

**Cons:**
- ❌ Breaks EF Core composability (can't chain further LINQ queries)
- ❌ Harder to maintain (SQL in string)
- ❌ Loses type safety
- ❌ Defeats purpose of using EF Core

**Verdict:** Only use if Strategy 1 shows performance issues (unlikely).

---

### 2.2 Recommended Approach

**Use Strategy 1: LINQ with Any() - Subquery Approach**

**Reasons:**
1. Clean, maintainable code
2. Composable with other LINQ queries
3. SQL Server optimizes EXISTS efficiently
4. Proper indexes make performance excellent
5. Type-safe and compile-time checked

**Implementation Pattern:**
```csharp
// Extension method in IkeaDocuScan.Infrastructure/Extensions/QueryExtensions.cs
public static class DocumentQueryExtensions
{
    public static IQueryable<Document> FilterByUserPermissions(
        this IQueryable<Document> query,
        CurrentUser currentUser,
        AppDbContext context)
    {
        // SuperUser bypass
        if (currentUser.IsSuperUser)
            return query;

        // No access = no documents
        if (!currentUser.HasAccess)
            return query.Where(d => false); // Empty result

        int userId = currentUser.UserId;

        return query.Where(doc =>
            context.UserPermissions
                .Where(p => p.UserId == userId)
                .Any(perm =>
                    // All three conditions must match for a permission to grant access
                    (doc.DtId == null || perm.DocumentTypeId == null || doc.DtId == perm.DocumentTypeId)
                    &&
                    (doc.CounterPartyId == null || perm.CounterPartyId == null || doc.CounterPartyId == perm.CounterPartyId)
                    &&
                    (doc.CounterParty == null ||
                     doc.CounterParty.Country == null ||
                     perm.CountryCode == null ||
                     doc.CounterParty.Country == perm.CountryCode)
                )
        );
    }
}
```

---

## 3. Service / Business Layer

### 3.1 Integration Points

**All document retrieval methods in `DocumentService` must apply permission filtering:**

1. `GetAllAsync()` - List all documents
2. `GetByIdAsync(int id)` - Get single document
3. `GetByBarCodeAsync(string barCode)` - Get by barcode
4. `SearchAsync(DocumentSearchRequestDto request)` - Advanced search
5. `GetDocumentFileAsync(int id)` - File download (ensure document access first)

### 3.2 Service Layer Changes

**DocumentService.cs Modifications:**

```csharp
public class DocumentService : IDocumentService
{
    private readonly AppDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<DocumentService> _logger;

    // ... constructor

    public async Task<List<DocumentDto>> GetAllAsync()
    {
        _logger.LogInformation("Fetching all documents");

        var currentUser = await _currentUserService.GetCurrentUserAsync();

        var query = _context.Documents
            .Include(d => d.DocumentName)
            .Include(d => d.Dt)
            .Include(d => d.CounterParty)
            .AsQueryable();

        // Apply permission filter
        query = query.FilterByUserPermissions(currentUser, _context);

        var entities = await query.ToListAsync();

        return entities.Select(d => MapToDto(d)).ToList();
    }

    public async Task<DocumentDto?> GetByIdAsync(int id)
    {
        _logger.LogInformation("Fetching document {DocumentId}", id);

        var currentUser = await _currentUserService.GetCurrentUserAsync();

        var query = _context.Documents
            .Include(d => d.DocumentName)
            .Include(d => d.Dt)
            .Include(d => d.CounterParty)
            .Where(d => d.Id == id);

        // Apply permission filter
        query = query.FilterByUserPermissions(currentUser, _context);

        var entity = await query.FirstOrDefaultAsync();

        if (entity == null)
            throw new DocumentNotFoundException(id);

        return MapToDto(entity);
    }

    public async Task<DocumentDto?> GetByBarCodeAsync(string barCode)
    {
        _logger.LogInformation("Fetching document by BarCode {BarCode}", barCode);

        if (!int.TryParse(barCode, out int barCodeInt))
        {
            _logger.LogWarning("Invalid BarCode format: {BarCode}", barCode);
            return null;
        }

        var currentUser = await _currentUserService.GetCurrentUserAsync();

        var query = _context.Documents
            .Include(d => d.DocumentName)
            .Include(d => d.Dt)
            .Include(d => d.CounterParty)
            .Where(d => d.BarCode == barCodeInt);

        // Apply permission filter
        query = query.FilterByUserPermissions(currentUser, _context);

        var entity = await query.FirstOrDefaultAsync();

        if (entity == null)
        {
            _logger.LogWarning("Document not found or access denied for BarCode {BarCode}", barCode);
            return null;
        }

        return MapToDto(entity);
    }

    public async Task<DocumentSearchResultDto> SearchAsync(DocumentSearchRequestDto request)
    {
        _logger.LogInformation("Executing document search");

        var currentUser = await _currentUserService.GetCurrentUserAsync();

        var query = _context.Documents
            .Include(d => d.Dt)
            .Include(d => d.CounterParty)
            .Include(d => d.DocumentName)
            .AsQueryable();

        // Apply permission filter FIRST (before search criteria)
        query = query.FilterByUserPermissions(currentUser, _context);

        // Then apply search criteria
        if (!string.IsNullOrWhiteSpace(request.SearchString))
        {
            query = query.Where(d =>
                d.Name.Contains(request.SearchString) ||
                d.BarCode.ToString().Contains(request.SearchString) ||
                // ... other search conditions
            );
        }

        if (request.DocumentTypeIds?.Any() == true)
        {
            query = query.Where(d => request.DocumentTypeIds.Contains(d.DtId));
        }

        // ... rest of search logic

        // Get total count AFTER filtering
        var totalCount = await query.CountAsync();

        // Apply pagination
        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(d => MapToSearchItemDto(d))
            .ToListAsync();

        return new DocumentSearchResultDto
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
        };
    }
}
```

### 3.3 Dependency Injection Updates

**Program.cs - Ensure ICurrentUserService is registered as Scoped:**

```csharp
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
```

**This is critical** because:
- `CurrentUserService` caches user permissions for the request lifetime
- Multiple service calls within same request reuse cached data
- Scoped lifetime ensures each HTTP request gets fresh user context

---

## 4. Controller / API Layer

### 4.1 Endpoint Enforcement

**All document endpoints already use `DocumentService`, so no changes needed** at the API layer. Permission filtering is enforced at the service layer, which is the correct architectural pattern.

**DocumentEndpoints.cs** (No Changes Required):
```csharp
public static void MapDocumentEndpoints(this IEndpointRouteBuilder routes)
{
    var group = routes.MapGroup("/api/documents")
        .RequireAuthorization()  // Already requires authentication
        .WithTags("Documents");

    group.MapGet("/", async (IDocumentService service) =>
    {
        var documents = await service.GetAllAsync();  // Filtered internally
        return Results.Ok(documents);
    });

    group.MapGet("/{id}", async (int id, IDocumentService service) =>
    {
        var document = await service.GetByIdAsync(id);  // Filtered internally
        return Results.Ok(document);
    });

    group.MapPost("/search", async (DocumentSearchRequestDto request, IDocumentService service) =>
    {
        var results = await service.SearchAsync(request);  // Filtered internally
        return Results.Ok(results);
    });

    // ... other endpoints
}
```

### 4.2 Consistency Guarantees

**Enforcement at Service Layer Ensures:**
1. ✅ All API endpoints automatically filter documents
2. ✅ No way to bypass filtering (centralized logic)
3. ✅ Consistent behavior across all entry points
4. ✅ Easy to unit test filtering logic
5. ✅ Future endpoints automatically inherit filtering

**If a new endpoint is added**, it simply calls `DocumentService` methods, which automatically apply filtering. No additional code needed.

---

## 5. Performance Considerations

### 5.1 SQL Server Execution Plan Analysis

**Expected Execution Plan for Filtered Query:**

```
Nested Loops (Left Outer Join) ← CounterParty for Country
├─ Index Seek (IX_Document_PermissionFilter) ← DT_ID, CounterPartyId
└─ Filter (EXISTS subquery)
   └─ Index Seek (IX_UserPermissions_UserId) ← UserId
      └─ Filter (permission conditions)
```

**Key Optimizations:**
1. `IX_Document_PermissionFilter` allows efficient seek on DocumentTypeId + CounterPartyId
2. `IX_UserPermissions_UserId` allows efficient lookup of user's permissions
3. SQL Server short-circuits EXISTS as soon as first match is found
4. LEFT JOIN for CounterParty is minimized with covering index

### 5.2 Index Strategy

**Required Indexes:**
```sql
-- Already exists
CREATE NONCLUSTERED INDEX IX_UserPermissions_UserId
ON dbo.UserPermission (UserId);

-- New (recommended)
CREATE NONCLUSTERED INDEX IX_Document_PermissionFilter
ON dbo.Document (DT_ID, CounterPartyId)
INCLUDE (BarCode, Name, FileId);

-- New (recommended)
CREATE NONCLUSTERED INDEX IX_CounterParty_Country
ON dbo.CounterParty (Country)
INCLUDE (CounterPartyId, Name);
```

**Index Selectivity:**
- `IX_Document_PermissionFilter`: High selectivity on DocumentTypeId (typically 5-20 types)
- `IX_UserPermissions_UserId`: Very high selectivity (one user)
- `IX_CounterParty_Country`: Medium selectivity (typically 10-50 countries)

### 5.3 Caching Options

**CurrentUser Permissions Caching** (Already Implemented):
```csharp
public class CurrentUserService : ICurrentUserService
{
    private CurrentUser? _cachedUser;  // Scoped lifetime = per-request cache
    private bool _loaded = false;

    public async Task<CurrentUser> GetCurrentUserAsync()
    {
        if (_loaded && _cachedUser != null)
            return _cachedUser;  // Return cached instance

        // Load from database, cache, return
        // ...
    }
}
```

**Benefits:**
- User permissions loaded once per HTTP request
- Multiple service calls reuse same `CurrentUser` instance
- No additional caching infrastructure needed

**Additional Caching (Optional - NOT RECOMMENDED for initial implementation):**
- ❌ Distributed cache (Redis) for user permissions → Adds complexity, invalidation issues
- ❌ In-memory cache for accessible document IDs → Stale data risk
- ✅ Database query result caching (SQL Server built-in) → Already optimized

### 5.4 Performance Benchmarks (Estimated)

**Assumptions:**
- 100,000 documents
- 1,000 users
- Average 5 permissions per user
- Proper indexes in place

**Expected Query Performance:**

| Operation | Without Filtering | With Filtering (Strategy 1) | Difference |
|-----------|-------------------|----------------------------|------------|
| GetAllAsync() | 250ms | 280ms | +12% |
| GetByIdAsync() | 5ms | 8ms | +60% (still very fast) |
| SearchAsync() (10 results) | 150ms | 175ms | +17% |
| SearchAsync() (100 results) | 450ms | 500ms | +11% |

**Scalability:**
- Linear scaling up to 1M documents with proper indexes
- Sub-linear scaling for user permission lookups (small table)

### 5.5 Trade-offs: LINQ vs. Stored Procedures

| Aspect | LINQ (Recommended) | Stored Procedures |
|--------|-------------------|-------------------|
| **Maintainability** | ✅ High - C# code | ❌ Low - SQL strings |
| **Type Safety** | ✅ Compile-time | ❌ Runtime only |
| **Testability** | ✅ Easy to mock | ❌ Requires DB |
| **Performance** | ✅ Good with indexes | ✅ Slightly faster |
| **Composability** | ✅ Chainable | ❌ Fixed queries |
| **Debugging** | ✅ Easy | ❌ Harder |

**Recommendation:** Use LINQ (Strategy 1) unless profiling shows specific performance bottleneck.

---

## 6. Testing and Validation

### 6.1 Unit Tests

**Test File:** `DocumentServiceTests.cs`

```csharp
public class DocumentServicePermissionTests
{
    [Fact]
    public async Task GetAllAsync_SuperUser_ReturnsAllDocuments()
    {
        // Arrange
        var context = CreateInMemoryContext();
        SeedDocuments(context, count: 10);

        var currentUserService = CreateMockCurrentUserService(isSuperUser: true);
        var service = new DocumentService(context, currentUserService, ...);

        // Act
        var result = await service.GetAllAsync();

        // Assert
        Assert.Equal(10, result.Count);
    }

    [Fact]
    public async Task GetAllAsync_UserWithNoPermissions_ReturnsEmpty()
    {
        // Arrange
        var context = CreateInMemoryContext();
        SeedDocuments(context, count: 10);

        var currentUserService = CreateMockCurrentUserService(
            isSuperUser: false,
            hasAccess: false
        );
        var service = new DocumentService(context, currentUserService, ...);

        // Act
        var result = await service.GetAllAsync();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllAsync_UserWithDocumentTypePermission_ReturnsMatchingDocuments()
    {
        // Arrange
        var context = CreateInMemoryContext();
        SeedDocuments(context, new[]
        {
            new Document { DtId = 1, CounterPartyId = null },  // Type 1
            new Document { DtId = 2, CounterPartyId = null },  // Type 2
            new Document { DtId = 1, CounterPartyId = null },  // Type 1
        });

        var user = CreateUser(userId: 1);
        CreatePermission(context, userId: 1, documentTypeId: 1, counterPartyId: null, countryCode: null);

        var currentUserService = CreateMockCurrentUserService(user);
        var service = new DocumentService(context, currentUserService, ...);

        // Act
        var result = await service.GetAllAsync();

        // Assert
        Assert.Equal(2, result.Count);  // Only Type 1 documents
        Assert.All(result, doc => Assert.Equal(1, doc.DocumentTypeId));
    }

    [Fact]
    public async Task GetAllAsync_UserWithCountryPermission_ReturnsMatchingDocuments()
    {
        // Arrange
        var context = CreateInMemoryContext();

        var cpUS = new CounterParty { CounterPartyId = 1, Country = "US" };
        var cpSE = new CounterParty { CounterPartyId = 2, Country = "SE" };
        context.CounterParties.AddRange(cpUS, cpSE);

        SeedDocuments(context, new[]
        {
            new Document { DtId = 1, CounterPartyId = 1 },  // US
            new Document { DtId = 1, CounterPartyId = 2 },  // SE
            new Document { DtId = 1, CounterPartyId = 1 },  // US
        });

        var user = CreateUser(userId: 1);
        CreatePermission(context, userId: 1, documentTypeId: null, counterPartyId: null, countryCode: "US");

        var currentUserService = CreateMockCurrentUserService(user);
        var service = new DocumentService(context, currentUserService, ...);

        // Act
        var result = await service.GetAllAsync();

        // Assert
        Assert.Equal(2, result.Count);  // Only US documents
    }

    [Fact]
    public async Task GetAllAsync_UserWithMultiplePermissions_ReturnsUnionOfMatches()
    {
        // Arrange
        var context = CreateInMemoryContext();
        SeedDocuments(context, new[]
        {
            new Document { DtId = 1, CounterPartyId = 1 },  // Type 1, CP 1
            new Document { DtId = 2, CounterPartyId = 2 },  // Type 2, CP 2
            new Document { DtId = 3, CounterPartyId = 3 },  // Type 3, CP 3
        });

        var user = CreateUser(userId: 1);
        CreatePermission(context, userId: 1, documentTypeId: 1, counterPartyId: null, countryCode: null);
        CreatePermission(context, userId: 1, documentTypeId: 2, counterPartyId: null, countryCode: null);

        var currentUserService = CreateMockCurrentUserService(user);
        var service = new DocumentService(context, currentUserService, ...);

        // Act
        var result = await service.GetAllAsync();

        // Assert
        Assert.Equal(2, result.Count);  // Type 1 OR Type 2
    }

    [Fact]
    public async Task GetByIdAsync_UserWithoutAccess_ThrowsNotFoundException()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var doc = new Document { Id = 1, DtId = 1, CounterPartyId = null };
        context.Documents.Add(doc);
        context.SaveChanges();

        var user = CreateUser(userId: 1);
        CreatePermission(context, userId: 1, documentTypeId: 2, counterPartyId: null, countryCode: null);  // Different type

        var currentUserService = CreateMockCurrentUserService(user);
        var service = new DocumentService(context, currentUserService, ...);

        // Act & Assert
        await Assert.ThrowsAsync<DocumentNotFoundException>(() => service.GetByIdAsync(1));
    }
}
```

### 6.2 Integration Tests

**Test File:** `DocumentEndpointIntegrationTests.cs`

```csharp
public class DocumentEndpointIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public DocumentEndpointIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetDocuments_WithPermissionFilter_ReturnsFilteredResults()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Seed database with test data
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Create user and permissions
        var user = new DocuScanUser { UserId = 100, AccountName = "TEST\\user", IsSuperUser = false };
        context.DocuScanUsers.Add(user);

        var permission = new UserPermission { UserId = 100, DocumentTypeId = 1 };
        context.UserPermissions.Add(permission);

        // Create documents
        context.Documents.AddRange(
            new Document { BarCode = 1001, DtId = 1, Name = "Doc1" },  // Should be visible
            new Document { BarCode = 1002, DtId = 2, Name = "Doc2" }   // Should be hidden
        );

        await context.SaveChangesAsync();

        // Mock authentication
        client.DefaultRequestHeaders.Add("X-Test-User", "TEST\\user");

        // Act
        var response = await client.GetAsync("/api/documents");

        // Assert
        response.EnsureSuccessStatusCode();
        var documents = await response.Content.ReadFromJsonAsync<List<DocumentDto>>();

        Assert.Single(documents);
        Assert.Equal(1001, documents[0].BarCode);
    }

    [Fact]
    public async Task SearchDocuments_WithoutMatchingPermissions_ReturnsEmpty()
    {
        // Arrange
        var client = _factory.CreateClient();

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var user = new DocuScanUser { UserId = 101, AccountName = "TEST\\user2", IsSuperUser = false };
        context.DocuScanUsers.Add(user);
        // No permissions!

        context.Documents.Add(new Document { BarCode = 2001, DtId = 1, Name = "Doc" });
        await context.SaveChangesAsync();

        client.DefaultRequestHeaders.Add("X-Test-User", "TEST\\user2");

        var searchRequest = new DocumentSearchRequestDto { SearchString = "Doc" };

        // Act
        var response = await client.PostAsJsonAsync("/api/documents/search", searchRequest);

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<DocumentSearchResultDto>();

        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalCount);
    }
}
```

### 6.3 Performance Tests

**Test File:** `DocumentServicePerformanceTests.cs`

```csharp
public class DocumentServicePerformanceTests
{
    [Fact]
    public async Task SearchAsync_With100KDocuments_CompletesIn500ms()
    {
        // Arrange
        var context = CreateTestDatabaseContext();  // Real SQL Server test DB
        SeedLargeDataset(context, documentCount: 100_000, userCount: 100);

        var user = CreateUser(userId: 1);
        CreatePermission(context, userId: 1, documentTypeId: 1, counterPartyId: null, countryCode: null);

        var currentUserService = CreateMockCurrentUserService(user);
        var service = new DocumentService(context, currentUserService, ...);

        var request = new DocumentSearchRequestDto
        {
            SearchString = "Contract",
            PageNumber = 1,
            PageSize = 25
        };

        // Act
        var stopwatch = Stopwatch.StartNew();
        var result = await service.SearchAsync(request);
        stopwatch.Stop();

        // Assert
        Assert.NotNull(result);
        Assert.True(stopwatch.ElapsedMilliseconds < 500,
            $"Query took {stopwatch.ElapsedMilliseconds}ms, expected < 500ms");
    }

    [Fact]
    public async Task GetAllAsync_WithProperIndexes_UsesIndexSeek()
    {
        // Arrange
        var context = CreateTestDatabaseContext();
        var service = CreateDocumentService(context);

        // Act
        var result = await service.GetAllAsync();

        // Assert - Check execution plan
        var executionPlan = await context.Database.ExecuteSqlRawAsync(
            "SELECT query_plan FROM sys.dm_exec_query_plan(...)");

        Assert.Contains("Index Seek", executionPlan);  // Not "Table Scan"
        Assert.Contains("IX_Document_PermissionFilter", executionPlan);
    }
}
```

### 6.4 Test Scenarios Checklist

**Correctness Tests:**
- [ ] SuperUser sees all documents
- [ ] User with no permissions sees zero documents
- [ ] User with DocumentTypeId permission sees only matching documents
- [ ] User with CounterPartyId permission sees only matching documents
- [ ] User with CountryCode permission sees only matching documents (via CounterParty)
- [ ] User with multiple permissions sees union of matches (OR logic)
- [ ] User with composite permission (Type + Country) sees only documents matching ALL conditions (AND logic within permission)
- [ ] Documents with NULL DocumentTypeId are accessible if user has ANY permission with NULL DocumentTypeId
- [ ] Documents with NULL CounterPartyId are accessible if user has ANY permission with NULL CounterPartyId
- [ ] GetByIdAsync() throws DocumentNotFoundException if user lacks access
- [ ] SearchAsync() returns filtered results with correct TotalCount
- [ ] Pagination works correctly with filtered results

**Performance Tests:**
- [ ] Query execution time < 500ms for 100K documents
- [ ] Index seeks used (not table scans)
- [ ] No N+1 query issues
- [ ] Memory usage remains constant under load

**Edge Cases:**
- [ ] User with HasAccess = false sees nothing
- [ ] Document with all NULLs (DocumentTypeId, CounterPartyId) is accessible
- [ ] Permission with all NULLs grants access to all documents
- [ ] Empty UserPermission table results in zero access (non-SuperUser)
- [ ] Multiple permissions with overlapping criteria don't cause duplicates

---

## Implementation Roadmap

### Phase 1: Core Infrastructure (2-3 days)
1. ✅ Create `QueryExtensions.cs` with `FilterByUserPermissions()` method
2. ✅ Add database indexes (IX_Document_PermissionFilter, IX_CounterParty_Country)
3. ✅ Update `DocumentService` methods to apply filtering
4. ✅ Write unit tests for permission logic

### Phase 2: Testing & Validation (2 days)
1. ✅ Write integration tests for API endpoints
2. ✅ Perform manual testing with different user roles
3. ✅ Run performance tests on test database
4. ✅ Verify SQL execution plans

### Phase 3: Deployment (1 day)
1. ✅ Deploy database index changes to production
2. ✅ Deploy code changes
3. ✅ Monitor query performance
4. ✅ Verify no regressions

**Total Estimated Effort:** 5-6 days

---

## Rollback Plan

**If performance issues occur:**
1. Add query hint to force index usage
2. Temporarily cache accessible document IDs (Strategy 2)
3. Revert to raw SQL (Strategy 3) for critical endpoints

**If logic errors occur:**
1. Hotfix: Add SuperUser bypass for all endpoints temporarily
2. Fix permission logic in `FilterByUserPermissions()`
3. Redeploy

---

## Summary

**Recommended Implementation:**
- ✅ Use Strategy 1 (LINQ with Any() subquery)
- ✅ Apply filtering at service layer (centralized)
- ✅ Add database indexes for performance
- ✅ Leverage existing `CurrentUserService` caching
- ✅ Comprehensive unit and integration tests

**Expected Outcomes:**
- ✅ Users see only authorized documents
- ✅ Performance overhead < 20% with indexes
- ✅ Maintainable, type-safe code
- ✅ No breaking changes to API layer
- ✅ Easy to extend for future permission types

**Next Steps:**
1. Review and approve this proposal
2. Create database migration for indexes
3. Implement `QueryExtensions.FilterByUserPermissions()`
4. Update `DocumentService` methods
5. Write and run tests
6. Deploy to staging environment
7. Performance validation
8. Deploy to production
