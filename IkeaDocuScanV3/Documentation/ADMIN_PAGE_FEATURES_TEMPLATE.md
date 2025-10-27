# Admin Page Features Template

This document lists all features implemented in the Currency and Country administration pages that should be replicated in other admin pages.

## Feature List

1. **Full CRUD Operations**
   - Create new entities via modal dialog
   - Update existing entities via modal dialog
   - Delete entities with confirmation modal
   - View all entities in a table

2. **Relational Integrity Checks**
   - Check if entity is in use before allowing deletion
   - Display usage count in delete confirmation modal
   - Show detailed breakdown of where entity is used
   - Prevent deletion if entity has dependencies

3. **Real-time Search**
   - Search input with icon in card header
   - Case-insensitive search across multiple fields
   - Filter as user types (`@bind:event="oninput"`)
   - Clear search button (X icon) when search is active
   - Smart result count display (e.g., "5 of 150" or just "150")
   - "No results" message with inline clear option

4. **Column Sorting**
   - Click column header to sort by that column
   - Click same header again to reverse sort order
   - Visual indicator (up/down arrow) showing current sort column and direction
   - Cursor changes to pointer on sortable headers
   - Default sort on primary key column (ascending)

5. **Modal Error Handling**
   - Errors displayed inside modal where user is entering data
   - Modal stays open on error (user doesn't lose input)
   - Success messages displayed on parent page after modal closes
   - Clear error messages when opening/closing modals
   - Parse JSON error responses from API

6. **Loading States**
   - Loading spinner while fetching data
   - Loading message ("Loading currencies...")
   - Saving spinner on save button
   - Checking usage spinner in delete modal
   - Disable buttons while operations in progress

7. **User Experience**
   - Responsive Bootstrap 5 layout
   - Font Awesome icons for visual clarity
   - Toast-style success/error alerts on parent page
   - Dismissible alerts
   - Empty state message when no data exists
   - Form validation with required field indicators (*)
   - Input field constraints (maxlength, placeholder text)
   - Disabled fields for non-editable data (e.g., primary key in edit mode)

8. **Authorization**
   - `@attribute [Authorize(Policy = "HasAccess")]`
   - Render mode: `@rendermode @(new InteractiveWebAssemblyRenderMode(prerender: false))`

## Quick Implementation Checklist

When creating a new admin page, implement in this order:

- [ ] Create DTOs (Create, Update, Read)
- [ ] Extend service interface with CRUD methods
- [ ] Implement service with relational integrity checks
- [ ] Add API endpoints (POST, PUT, DELETE, GET, GET usage)
- [ ] Extend HTTP service client with error extraction
- [ ] Create Razor page with table, modals, search, and sorting
- [ ] Add navigation menu item
- [ ] Test all CRUD operations
- [ ] Test relational integrity checks
- [ ] Test search functionality
- [ ] Test column sorting
- [ ] Test error handling (duplicate keys, validation errors, etc.)

## Code Patterns

### State Fields Required
```csharp
private List<EntityDto>? entities;
private List<EntityDto>? filteredEntities;
private string searchTerm = string.Empty;
private string sortColumn = nameof(EntityDto.PrimaryKey);
private bool sortAscending = true;
private bool isLoading = false;
private bool isSaving = false;
private bool isCheckingUsage = false;
private string? errorMessage;
private string? successMessage;
private string? modalErrorMessage;
```

### ApplyFilter Pattern
```csharp
private void ApplyFilter()
{
    if (entities == null) return;

    // Search filter
    var filtered = string.IsNullOrWhiteSpace(searchTerm)
        ? entities
        : entities.Where(e => /* search conditions */);

    // Sorting
    filtered = sortColumn switch
    {
        nameof(EntityDto.Field1) => sortAscending
            ? filtered.OrderBy(e => e.Field1)
            : filtered.OrderByDescending(e => e.Field1),
        // ... other columns
        _ => filtered.OrderBy(e => e.PrimaryKey)
    };

    filteredEntities = filtered.ToList();
}
```

### SortBy Pattern
```csharp
private void SortBy(string columnName)
{
    if (sortColumn == columnName)
        sortAscending = !sortAscending;
    else
    {
        sortColumn = columnName;
        sortAscending = true;
    }
    ApplyFilter();
}
```

### Error Extraction Pattern (HTTP Service)
```csharp
private string TryExtractErrorMessage(string errorContent)
{
    try
    {
        var options = new System.Text.Json.JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        var errorResponse = System.Text.Json.JsonSerializer
            .Deserialize<ErrorResponse>(errorContent, options);
        if (errorResponse?.Error != null)
            return errorResponse.Error;
    }
    catch (Exception ex)
    {
        _logger.LogWarning(ex, "Failed to deserialize error response: {Content}", errorContent);
    }
    return !string.IsNullOrEmpty(errorContent) ? errorContent : "An error occurred";
}
```

## Sample Prompt for New Admin Page

```
Create a [EntityName] administration page with the following features:

1. Full CRUD operations (create, update, delete, view all)
2. Relational integrity checks before deletion - check if [EntityName] is used by [RelatedEntity1, RelatedEntity2, etc.]
3. Real-time search filtering by [field1, field2, etc.]
4. Column sorting on all table columns with ascending/descending toggle
5. Modal error handling - show errors inside the modal, not on parent page
6. Loading states for all async operations
7. Bootstrap 5 styling with Font Awesome icons
8. Authorization with HasAccess policy
9. Parse and display user-friendly error messages from API

The entity has these fields:
- [PrimaryKey] (cannot be changed after creation)
- [Field1, Field2, etc.] (editable)

Follow the same implementation patterns as CurrencyAdministration.razor and CountryAdministration.razor.
```
