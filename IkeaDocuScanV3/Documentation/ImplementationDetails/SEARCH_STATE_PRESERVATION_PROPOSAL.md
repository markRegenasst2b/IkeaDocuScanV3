# Search State Preservation Proposal

**Date:** 2025-11-05
**Feature:** Preserve search results and parameters when navigating away and returning
**Status:** 📋 PROPOSAL

---

## Problem Statement

### Current Behavior (❌ Poor UX)

When users perform a document search on `/documents/search`:
1. User fills in multiple search filters (Document Type, Counterparty, Date ranges, etc.)
2. User clicks "Search" and gets results
3. User clicks on a document to view details or perform an action
4. User uses browser back button or navigates back to `/documents/search`
5. **❌ All search filters are cleared**
6. **❌ Search results are gone**
7. User must re-enter all filters and search again

### Desired Behavior (✅ Good UX)

1. User performs search with filters
2. User navigates away (to document detail, email compose, etc.)
3. User returns to search page
4. **✅ Search filters are still populated**
5. **✅ Search results are still displayed**
6. **✅ Pagination state is preserved**
7. **✅ Selected documents remain selected**

---

## Proposed Solution: Query String Parameters

### Why Query Strings?

| Approach | Shareable | Bookmarkable | Back Button | Refresh | Cross-Session | Complexity |
|----------|-----------|--------------|-------------|---------|---------------|------------|
| **Query Strings** | ✅ | ✅ | ✅ | ✅ | ✅ | Medium |
| Session Storage | ❌ | ❌ | ✅ | ✅ | ❌ | Low |
| Local Storage | ❌ | ❌ | ✅ | ✅ | ✅ | Low |
| Component State | ❌ | ❌ | ❌ | ❌ | ❌ | Low |

**Query Strings are the clear winner** because they provide:
- **Shareable URLs**: Users can send search links to colleagues
- **Bookmarkable**: Save frequently used searches
- **Browser integration**: Back/forward buttons work naturally
- **Refresh-safe**: F5 doesn't lose the search
- **Standard web pattern**: Users expect this behavior

---

## Current Search Parameters

Based on analysis of `SearchDocuments.razor` and `DocumentSearchRequestDto`:

### General Filters
- `SearchString` - PDF content search
- `Barcodes` - Comma-separated barcode list
- `DocumentTypeIds` - Multiple document type IDs
- `DocumentNameId` - Single document name ID
- `DocumentNumber` - Document number
- `VersionNo` - Version number
- `AssociatedToPua` - PUA/Agreement number
- `AssociatedToAppendix` - Appendix number

### Counterparty Filters
- `CounterpartyName` - Counterparty or third party name
- `CounterpartyNo` - Counterparty number
- `CounterpartyCountry` - Country code
- `CounterpartyCity` - City name

### Date Filters
- `DateOfContractFrom` - Contract date range start
- `DateOfContractTo` - Contract date range end
- `CreatedOnFrom` - Creation date range start
- `CreatedOnTo` - Creation date range end

### Financial Filters
- `CurrencyCode` - Currency code
- `TotalAmountFrom` - Amount range start
- `TotalAmountTo` - Amount range end

### Pagination
- `PageNumber` - Current page (default: 1)
- `PageSize` - Results per page (default: 25)

### Sorting
- `SortColumn` - Column to sort by
- `SortDirection` - Asc or Desc

---

## Implementation Strategy

### Phase 1: Add Query Parameter Support (Core)

#### 1.1. Add Parameter Properties

In `SearchDocuments.razor.cs`, add `[SupplyParameterFromQuery]` attributes:

```csharp
public partial class SearchDocuments : ComponentBase
{
    // Query parameters for search filters
    [Parameter]
    [SupplyParameterFromQuery(Name = "q")]
    public string? SearchString { get; set; }

    [Parameter]
    [SupplyParameterFromQuery(Name = "barcodes")]
    public string? Barcodes { get; set; }

    [Parameter]
    [SupplyParameterFromQuery(Name = "types")]
    public string? DocumentTypeIds { get; set; } // Comma-separated

    [Parameter]
    [SupplyParameterFromQuery(Name = "nameId")]
    public int? DocumentNameId { get; set; }

    [Parameter]
    [SupplyParameterFromQuery(Name = "docNo")]
    public string? DocumentNumber { get; set; }

    [Parameter]
    [SupplyParameterFromQuery(Name = "version")]
    public string? VersionNo { get; set; }

    [Parameter]
    [SupplyParameterFromQuery(Name = "pua")]
    public string? AssociatedToPua { get; set; }

    [Parameter]
    [SupplyParameterFromQuery(Name = "appendix")]
    public string? AssociatedToAppendix { get; set; }

    [Parameter]
    [SupplyParameterFromQuery(Name = "cpName")]
    public string? CounterpartyName { get; set; }

    [Parameter]
    [SupplyParameterFromQuery(Name = "cpNo")]
    public string? CounterpartyNo { get; set; }

    [Parameter]
    [SupplyParameterFromQuery(Name = "cpCountry")]
    public string? CounterpartyCountry { get; set; }

    [Parameter]
    [SupplyParameterFromQuery(Name = "cpCity")]
    public string? CounterpartyCity { get; set; }

    [Parameter]
    [SupplyParameterFromQuery(Name = "contractFrom")]
    public string? DateOfContractFrom { get; set; } // ISO date string

    [Parameter]
    [SupplyParameterFromQuery(Name = "contractTo")]
    public string? DateOfContractTo { get; set; }

    [Parameter]
    [SupplyParameterFromQuery(Name = "createdFrom")]
    public string? CreatedOnFrom { get; set; }

    [Parameter]
    [SupplyParameterFromQuery(Name = "createdTo")]
    public string? CreatedOnTo { get; set; }

    [Parameter]
    [SupplyParameterFromQuery(Name = "currency")]
    public string? CurrencyCode { get; set; }

    [Parameter]
    [SupplyParameterFromQuery(Name = "amountFrom")]
    public decimal? TotalAmountFrom { get; set; }

    [Parameter]
    [SupplyParameterFromQuery(Name = "amountTo")]
    public decimal? TotalAmountTo { get; set; }

    [Parameter]
    [SupplyParameterFromQuery(Name = "page")]
    public int? PageNumber { get; set; }

    [Parameter]
    [SupplyParameterFromQuery(Name = "pageSize")]
    public int? PageSize { get; set; }

    [Parameter]
    [SupplyParameterFromQuery(Name = "sort")]
    public string? SortColumn { get; set; }

    [Parameter]
    [SupplyParameterFromQuery(Name = "dir")]
    public string? SortDirection { get; set; }

    // Existing fields
    private DocumentSearchRequestDto searchRequest = new();
    private DocumentSearchResultDto? searchResults;
    // ... rest of fields
}
```

#### 1.2. Initialize Search Request from Query Parameters

Modify `OnInitializedAsync` to populate search request from query parameters:

```csharp
protected override async Task OnInitializedAsync()
{
    try
    {
        Logger.LogInformation("SearchDocuments page initializing");
        await LoadReferenceData();

        // Populate search request from query parameters
        InitializeSearchFromQueryParameters();

        // If query parameters present, execute search automatically
        if (HasQueryParameters())
        {
            Logger.LogInformation("Query parameters detected, executing search automatically");
            await ExecuteSearch();
        }
    }
    catch (Exception ex)
    {
        Logger.LogError(ex, "Error initializing SearchDocuments page");
        errorMessage = $"Failed to load reference data: {ex.Message}";
    }
    finally
    {
        isLoadingReferenceData = false;
    }
}

private void InitializeSearchFromQueryParameters()
{
    searchRequest = new DocumentSearchRequestDto
    {
        SearchString = SearchString,
        Barcodes = Barcodes,
        DocumentNumber = DocumentNumber,
        VersionNo = VersionNo,
        AssociatedToPua = AssociatedToPua,
        AssociatedToAppendix = AssociatedToAppendix,
        CounterpartyName = CounterpartyName,
        CounterpartyNo = CounterpartyNo,
        CounterpartyCountry = CounterpartyCountry,
        CounterpartyCity = CounterpartyCity,
        CurrencyCode = CurrencyCode,
        TotalAmountFrom = TotalAmountFrom,
        TotalAmountTo = TotalAmountTo,
        PageNumber = PageNumber ?? 1,
        PageSize = PageSize ?? 25,
        SortColumn = SortColumn,
        SortDirection = SortDirection
    };

    // Parse DocumentNameId
    if (DocumentNameId.HasValue)
    {
        searchRequest.DocumentNameId = DocumentNameId.Value;
    }

    // Parse comma-separated DocumentTypeIds
    if (!string.IsNullOrWhiteSpace(DocumentTypeIds))
    {
        searchRequest.DocumentTypeIds = DocumentTypeIds
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(int.Parse)
            .ToList();
    }

    // Parse date strings to DateTime
    if (DateTime.TryParse(DateOfContractFrom, out var contractFrom))
        searchRequest.DateOfContractFrom = contractFrom;

    if (DateTime.TryParse(DateOfContractTo, out var contractTo))
        searchRequest.DateOfContractTo = contractTo;

    if (DateTime.TryParse(CreatedOnFrom, out var createdFrom))
        searchRequest.CreatedOnFrom = createdFrom;

    if (DateTime.TryParse(CreatedOnTo, out var createdTo))
        searchRequest.CreatedOnTo = createdTo;

    Logger.LogInformation("Search request initialized from query parameters");
}

private bool HasQueryParameters()
{
    return !string.IsNullOrWhiteSpace(SearchString) ||
           !string.IsNullOrWhiteSpace(Barcodes) ||
           !string.IsNullOrWhiteSpace(DocumentTypeIds) ||
           DocumentNameId.HasValue ||
           !string.IsNullOrWhiteSpace(DocumentNumber) ||
           !string.IsNullOrWhiteSpace(VersionNo) ||
           !string.IsNullOrWhiteSpace(CounterpartyName) ||
           !string.IsNullOrWhiteSpace(CounterpartyNo) ||
           PageNumber.HasValue && PageNumber.Value > 1;
}
```

#### 1.3. Update URL When Search is Executed

Add method to update URL with search parameters:

```csharp
private async Task ExecuteSearch()
{
    try
    {
        isSearching = true;
        errorMessage = null;
        StateHasChanged();

        Logger.LogInformation("Executing document search");

        // Update URL with current search parameters
        UpdateUrlWithSearchParameters();

        searchResults = await DocumentService.SearchAsync(searchRequest);

        Logger.LogInformation(
            "Search completed: {TotalCount} results, Page {CurrentPage}/{TotalPages}",
            searchResults.TotalCount, searchResults.CurrentPage, searchResults.TotalPages);

        if (searchResults.MaxLimitReached)
        {
            Logger.LogWarning("Search hit max limit of {MaxLimit} results", searchResults.MaxLimit);
        }
    }
    catch (Exception ex)
    {
        Logger.LogError(ex, "Error executing search");
        errorMessage = $"Search failed: {ex.Message}";
        searchResults = null;
    }
    finally
    {
        isSearching = false;
        StateHasChanged();
    }
}

private void UpdateUrlWithSearchParameters()
{
    var queryParams = new Dictionary<string, string?>();

    // Add non-empty parameters
    if (!string.IsNullOrWhiteSpace(searchRequest.SearchString))
        queryParams["q"] = searchRequest.SearchString;

    if (!string.IsNullOrWhiteSpace(searchRequest.Barcodes))
        queryParams["barcodes"] = searchRequest.Barcodes;

    if (searchRequest.DocumentTypeIds?.Any() == true)
        queryParams["types"] = string.Join(",", searchRequest.DocumentTypeIds);

    if (searchRequest.DocumentNameId.HasValue && searchRequest.DocumentNameId.Value > 0)
        queryParams["nameId"] = searchRequest.DocumentNameId.Value.ToString();

    if (!string.IsNullOrWhiteSpace(searchRequest.DocumentNumber))
        queryParams["docNo"] = searchRequest.DocumentNumber;

    if (!string.IsNullOrWhiteSpace(searchRequest.VersionNo))
        queryParams["version"] = searchRequest.VersionNo;

    if (!string.IsNullOrWhiteSpace(searchRequest.AssociatedToPua))
        queryParams["pua"] = searchRequest.AssociatedToPua;

    if (!string.IsNullOrWhiteSpace(searchRequest.AssociatedToAppendix))
        queryParams["appendix"] = searchRequest.AssociatedToAppendix;

    if (!string.IsNullOrWhiteSpace(searchRequest.CounterpartyName))
        queryParams["cpName"] = searchRequest.CounterpartyName;

    if (!string.IsNullOrWhiteSpace(searchRequest.CounterpartyNo))
        queryParams["cpNo"] = searchRequest.CounterpartyNo;

    if (!string.IsNullOrWhiteSpace(searchRequest.CounterpartyCountry))
        queryParams["cpCountry"] = searchRequest.CounterpartyCountry;

    if (!string.IsNullOrWhiteSpace(searchRequest.CounterpartyCity))
        queryParams["cpCity"] = searchRequest.CounterpartyCity;

    if (searchRequest.DateOfContractFrom.HasValue)
        queryParams["contractFrom"] = searchRequest.DateOfContractFrom.Value.ToString("yyyy-MM-dd");

    if (searchRequest.DateOfContractTo.HasValue)
        queryParams["contractTo"] = searchRequest.DateOfContractTo.Value.ToString("yyyy-MM-dd");

    if (searchRequest.CreatedOnFrom.HasValue)
        queryParams["createdFrom"] = searchRequest.CreatedOnFrom.Value.ToString("yyyy-MM-dd");

    if (searchRequest.CreatedOnTo.HasValue)
        queryParams["createdTo"] = searchRequest.CreatedOnTo.Value.ToString("yyyy-MM-dd");

    if (!string.IsNullOrWhiteSpace(searchRequest.CurrencyCode))
        queryParams["currency"] = searchRequest.CurrencyCode;

    if (searchRequest.TotalAmountFrom.HasValue)
        queryParams["amountFrom"] = searchRequest.TotalAmountFrom.Value.ToString();

    if (searchRequest.TotalAmountTo.HasValue)
        queryParams["amountTo"] = searchRequest.TotalAmountTo.Value.ToString();

    // Always include pagination
    if (searchRequest.PageNumber > 1)
        queryParams["page"] = searchRequest.PageNumber.ToString();

    if (searchRequest.PageSize != 25) // 25 is default
        queryParams["pageSize"] = searchRequest.PageSize.ToString();

    // Add sorting if present
    if (!string.IsNullOrWhiteSpace(searchRequest.SortColumn))
        queryParams["sort"] = searchRequest.SortColumn;

    if (!string.IsNullOrWhiteSpace(searchRequest.SortDirection))
        queryParams["dir"] = searchRequest.SortDirection;

    // Build query string
    var queryString = string.Join("&", queryParams.Select(kvp =>
        $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value ?? "")}"));

    var newUrl = $"/documents/search?{queryString}";

    // Navigate without forcing a reload (replaceHistoryEntry: false means add to history)
    NavigationManager.NavigateTo(newUrl, forceLoad: false);

    Logger.LogInformation("Updated URL with search parameters: {Url}", newUrl);
}
```

#### 1.4. Update Pagination Methods

Modify pagination to update search request and URL:

```csharp
private async Task GoToPage(int pageNumber)
{
    if (searchResults == null || pageNumber < 1 || pageNumber > searchResults.TotalPages)
        return;

    searchRequest.PageNumber = pageNumber;
    await ExecuteSearch(); // This will update URL automatically
}

private async Task OnPageSizeChanged()
{
    searchRequest.PageNumber = 1; // Reset to first page when page size changes
    await ExecuteSearch(); // This will update URL automatically
}
```

#### 1.5. Update Clear Filters

Modify clear filters to reset URL:

```csharp
private void ClearFilters()
{
    Logger.LogInformation("Clearing all search filters");

    searchRequest = new DocumentSearchRequestDto
    {
        PageSize = searchRequest.PageSize, // Keep page size
        PageNumber = 1
    };

    searchResults = null;
    selectedDocumentIds.Clear();

    // Clear URL parameters
    NavigationManager.NavigateTo("/documents/search", forceLoad: false);

    StateHasChanged();
}
```

---

## Example URLs

### Simple Search
```
/documents/search?q=contract&page=1&pageSize=25
```

### Complex Search
```
/documents/search?q=agreement
  &types=1,3,5
  &cpName=IKEA
  &cpCountry=SE
  &contractFrom=2024-01-01
  &contractTo=2024-12-31
  &page=2
  &pageSize=100
  &sort=DateOfContract
  &dir=desc
```

### Shareable Search Link
User can copy this URL and send to colleague:
```
https://docuscan.company.com/documents/search?types=1&cpCountry=US&contractFrom=2024-01-01
```

Colleague opens link → Filters are pre-filled → Clicks "Search" → Gets same results

---

## Benefits

### 1. Improved User Experience
- ✅ No need to re-enter filters after viewing a document
- ✅ Browser back button works intuitively
- ✅ F5 refresh preserves search
- ✅ Can bookmark frequent searches

### 2. Collaboration
- ✅ Share search links with colleagues
- ✅ "Hey, check out these documents: [URL]"
- ✅ Team can quickly access same filtered view

### 3. Productivity
- ✅ Save time not re-entering filters
- ✅ Work faster through search results
- ✅ Less frustration with navigation

### 4. Analytics (Bonus)
- Server logs can see which searches are most common
- Can optimize UI for common search patterns

---

## Implementation Phases

### Phase 1: Core Query Parameter Support ⭐ START HERE
**Effort:** 4-6 hours
**Files to Modify:**
- `SearchDocuments.razor.cs` (add parameters, init logic, URL update)

**Testing:**
- Perform search → Navigate away → Back button → Verify filters preserved
- Bookmark search → Close browser → Open bookmark → Verify it works
- Copy URL → Open in new tab → Verify same results

### Phase 2: Enhanced Features (Optional)
**Effort:** 2-4 hours
**Features:**
- "Copy Search Link" button
- "Save Search" feature (saved searches in localStorage)
- Search history dropdown

### Phase 3: Advanced (Future)
**Effort:** Variable
**Features:**
- URL shortening for complex searches
- Named search templates shared across organization
- "Recent Searches" in user profile

---

## Potential Issues & Solutions

### Issue 1: URL Too Long
**Problem:** URLs have ~2000 character limit, complex searches might exceed
**Solution:**
- Compress parameter names (already done: `q` instead of `searchString`)
- Optional: Add "Share Search" button that saves to database and generates short ID
- Example: `/documents/search?id=abc123` → Loads search params from database

### Issue 2: Dates in Different Formats
**Problem:** Date parsing can fail in different cultures
**Solution:**
- Always use ISO 8601 format: `yyyy-MM-dd`
- Use `DateTime.TryParseExact` with specific format

### Issue 3: Special Characters in Search Terms
**Problem:** Search strings with special characters might break URL
**Solution:**
- Use `Uri.EscapeDataString()` when building URL (already in example code)
- Use `Uri.UnescapeDataString()` when parsing (Blazor handles this automatically)

### Issue 4: Multi-Select Document Types
**Problem:** Need to encode multiple IDs
**Solution:**
- Comma-separated: `types=1,3,5`
- Parse back: `string.Split(',').Select(int.Parse).ToList()`

### Issue 5: Performance - Page Loads Slowly with Auto-Search
**Problem:** If search takes time, page seems slow to load
**Solution:**
- Show loading indicator immediately
- Consider debouncing auto-search if user is still typing in URL
- Cache search results in memory for 5 minutes

---

## Testing Checklist

### Basic Functionality
- [ ] Search with single filter → URL updates
- [ ] Navigate away → Back button → Filters preserved
- [ ] Refresh page (F5) → Filters and results preserved
- [ ] Clear filters → URL resets to `/documents/search`

### Pagination
- [ ] Click "Next Page" → URL updates with `page=2`
- [ ] Change page size → URL updates with `pageSize=100`
- [ ] Direct URL with `page=5` → Opens to page 5

### Complex Searches
- [ ] Multiple document types → URL has `types=1,2,3`
- [ ] Date ranges → URL has properly formatted dates
- [ ] Sorting → URL has `sort` and `dir` parameters

### Sharing
- [ ] Copy URL → Send to colleague → They see same search
- [ ] Bookmark → Close browser → Open bookmark → Still works

### Edge Cases
- [ ] Empty search (no filters) → Clean URL
- [ ] Invalid page number in URL (e.g., `page=999999`) → Handles gracefully
- [ ] Invalid date format → Falls back to default
- [ ] Special characters in search string → URL encodes properly
- [ ] Very long search string → Handles without error

---

## Migration Path

### Before Implementation
Document current behavior for comparison:
1. Take screenshots of current search page
2. Document current user complaints
3. Measure time spent re-entering searches (survey or analytics)

### During Implementation
1. Implement Phase 1 in feature branch
2. Test thoroughly with QA team
3. Get user acceptance testing with power users

### After Implementation
1. Monitor for issues in logs
2. Survey users on improved experience
3. Measure time savings (fewer duplicate searches)
4. Document new feature for users

---

## Code Files to Create/Modify

### Files to Modify
1. **SearchDocuments.razor.cs** (~300 lines added)
   - Add `[SupplyParameterFromQuery]` properties
   - Add `InitializeSearchFromQueryParameters()` method
   - Add `UpdateUrlWithSearchParameters()` method
   - Add `HasQueryParameters()` method
   - Modify `OnInitializedAsync()`
   - Modify `ExecuteSearch()`
   - Modify `GoToPage()`
   - Modify `OnPageSizeChanged()`
   - Modify `ClearFilters()`

### No New Files Needed
All changes are in existing files.

---

## Estimated Effort

| Task | Hours | Complexity |
|------|-------|------------|
| Add query parameters | 1 | Low |
| Initialize from URL | 1 | Medium |
| Update URL on search | 2 | Medium |
| Update pagination | 0.5 | Low |
| Testing | 1.5 | Medium |
| **Total Phase 1** | **6 hours** | **Medium** |

---

## Success Criteria

After implementation, the feature is successful if:

✅ **User can perform search and navigate away**
✅ **Browser back button restores search**
✅ **Page refresh preserves search**
✅ **Users can share search URLs**
✅ **Users can bookmark searches**
✅ **No performance degradation**
✅ **No bugs introduced in existing search**

---

## Alternative: Session Storage (Quick Win)

If query string approach is too complex for initial release, consider session storage as Phase 0:

### Pros
- Simpler implementation (2-3 hours)
- No URL changes
- Works for browser back button

### Cons
- Not shareable
- Not bookmarkable
- Lost on browser close
- Not a long-term solution

### Quick Implementation
```csharp
// On search:
sessionStorage.setItem('lastSearch', JSON.stringify(searchRequest));

// On init:
var savedSearch = sessionStorage.getItem('lastSearch');
if (savedSearch) {
    searchRequest = JSON.parse(savedSearch);
    await ExecuteSearch();
}
```

**Recommendation:** Implement query strings (Phase 1) directly. It's only 3-4 hours more work and provides much better UX.

---

## Summary

| Aspect | Current | After Implementation |
|--------|---------|---------------------|
| **Back button** | ❌ Loses search | ✅ Preserves search |
| **Refresh (F5)** | ❌ Loses search | ✅ Preserves search |
| **Shareable** | ❌ No | ✅ Yes |
| **Bookmarkable** | ❌ No | ✅ Yes |
| **User frustration** | 😤 High | 😊 Low |
| **Implementation** | N/A | 6 hours |

---

## Next Steps

1. **Review this proposal** with team
2. **Get approval** from product owner
3. **Create feature branch**: `feature/search-state-preservation`
4. **Implement Phase 1** following code examples above
5. **Test thoroughly** using checklist
6. **Deploy to staging** for user acceptance testing
7. **Deploy to production**
8. **Monitor** and gather feedback
9. **Consider Phase 2** enhancements based on usage

---

**Status:** 📋 **READY FOR IMPLEMENTATION**

This is a high-value UX improvement with moderate implementation complexity. Recommended to prioritize this feature.
