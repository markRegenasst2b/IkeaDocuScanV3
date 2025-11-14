# Registration Request Filter - User Permissions Management

**Date:** 2025-11-05
**Feature:** Quick filter to show users without permissions (registration requests)
**Status:** ✅ IMPLEMENTED

---

## Feature Description

Added a quick filter checkbox on the User Permissions Management page to show only users who don't have any permissions assigned. This helps identify users who have logged in and sent registration requests but haven't been granted permissions yet.

---

## Changes Made

### 1. Added Filter Checkbox UI

**Location:** `EditUserPermissions.razor` (Lines 48-55)

Added a checkbox below the search box:

```razor
<div class="form-check">
    <input class="form-check-input" type="checkbox" id="filterNoPermissions"
           @bind="showOnlyUsersWithoutPermissions"
           @onchange="OnFilterChanged">
    <label class="form-check-label" for="filterNoPermissions">
        <i class="fa fa-filter"></i> Show only users without permissions (registration requests)
    </label>
</div>
```

### 2. Added State Variable

**Location:** `EditUserPermissions.razor` (Line 547)

```csharp
private bool showOnlyUsersWithoutPermissions = false;
```

### 3. Created FilteredUsers Property

**Location:** `EditUserPermissions.razor` (Lines 517-534)

Added a computed property that filters users based on `PermissionCount`:

```csharp
private List<DocuScanUserDto>? FilteredUsers
{
    get
    {
        if (users == null) return null;

        var filtered = users.AsEnumerable();

        // Apply permission count filter
        if (showOnlyUsersWithoutPermissions)
        {
            filtered = filtered.Where(u => u.PermissionCount == 0);
        }

        return filtered.ToList();
    }
}
```

### 4. Updated User Count Display

**Location:** `EditUserPermissions.razor` (Lines 67-78)

Enhanced the header to show filtered count with context:

```razor
<h5 class="mb-0">
    DocuScan Users
    @if (showOnlyUsersWithoutPermissions)
    {
        <span class="badge bg-warning text-dark">@(FilteredUsers?.Count ?? 0) without permissions</span>
        <span class="text-muted small">(of @(users?.Count ?? 0) total)</span>
    }
    else
    {
        <span>(@(FilteredUsers?.Count ?? 0))</span>
    }
</h5>
```

### 5. Updated Table to Use FilteredUsers

**Location:** `EditUserPermissions.razor` (Lines 96, 111)

Changed from `users` to `FilteredUsers`:

```razor
else if (FilteredUsers != null && FilteredUsers.Count > 0)
{
    // ...
    @foreach (var user in FilteredUsers)
    {
        // Display user rows
    }
}
```

### 6. Enhanced "No Users Found" Message

**Location:** `EditUserPermissions.razor` (Lines 143-152)

Added context-aware message:

```razor
<div class="alert alert-info">
    @if (showOnlyUsersWithoutPermissions)
    {
        <span>No users without permissions found. All users have been assigned permissions.</span>
    }
    else
    {
        <span>No users found. @(string.IsNullOrWhiteSpace(searchTerm) ? "" : "Try a different search term.")</span>
    }
</div>
```

### 7. Added Filter Change Handler

**Location:** `EditUserPermissions.razor` (Lines 648-653)

```csharp
private void OnFilterChanged()
{
    // Filter is applied via the FilteredUsers property
    // Just trigger UI update
    StateHasChanged();
}
```

---

## How It Works

### User Flow:

1. **Navigate** to User Permissions Management page (`/edit-userpermissions`)
2. **See the filter checkbox** below the search box
3. **Check the box** to show only users without permissions
4. **View filtered list** - Only users with `PermissionCount == 0` are displayed
5. **See the badge** showing count of users without permissions
6. **Uncheck** to return to full list

### Technical Flow:

1. User checks the "Show only users without permissions" checkbox
2. `showOnlyUsersWithoutPermissions` state variable is set to `true`
3. `OnFilterChanged()` method is called, triggering `StateHasChanged()`
4. UI re-renders, calling the `FilteredUsers` property getter
5. `FilteredUsers` applies the filter: `users.Where(u => u.PermissionCount == 0)`
6. Table displays only users with 0 permissions
7. Header shows: "X without permissions (of Y total)"

---

## UI Examples

### Filter Off (Default View)

```
Search Users
[Search box: Type at least 3 characters...]

☐ Show only users without permissions (registration requests)

[Add New User]

DocuScan Users (25)
+--------+-------------------+-----------+------------+-------------+---------+
| User ID| Account Name      | Last Logon| Super User | Permissions | Actions |
+--------+-------------------+-----------+------------+-------------+---------+
| 1      | DOMAIN\john.doe   | 2025-11-05| No         | 5           | Manage  |
| 2      | DOMAIN\jane.smith | 2025-11-04| Yes        | 0           | Manage  |
| 3      | DOMAIN\bob.jones  | 2025-11-03| No         | 3           | Manage  |
...
```

### Filter On (Registration Requests Only)

```
Search Users
[Search box: Type at least 3 characters...]

☑ Show only users without permissions (registration requests)

[Add New User]

DocuScan Users [2 without permissions] (of 25 total)
+--------+-------------------+-----------+------------+-------------+---------+
| User ID| Account Name      | Last Logon| Super User | Permissions | Actions |
+--------+-------------------+-----------+------------+-------------+---------+
| 2      | DOMAIN\jane.smith | 2025-11-04| No         | 0           | Manage  |
| 8      | DOMAIN\new.user   | 2025-11-05| No         | 0           | Manage  |
+--------+-------------------+-----------+------------+-------------+---------+
```

### Filter On - No Results

```
☑ Show only users without permissions (registration requests)

[Add New User]

DocuScan Users [0 without permissions] (of 25 total)

ℹ No users without permissions found. All users have been assigned permissions.
```

---

## Use Cases

### Use Case 1: Process New User Registration Requests

**Scenario:** Administrator wants to grant permissions to users who have logged in but don't have access yet.

**Steps:**
1. Check "Show only users without permissions" filter
2. View list of users with 0 permissions (registration requests)
3. Click "Manage" on a user
4. Assign appropriate permissions (Document Types, Counter Parties, Countries)
5. Return to user list
6. Filter automatically shows remaining users without permissions
7. Repeat until all registration requests are processed

### Use Case 2: Audit New Users

**Scenario:** Weekly review of pending user access requests.

**Steps:**
1. Enable filter on Monday morning
2. See badge: "8 without permissions (of 150 total)"
3. Review each user's last logon date
4. Prioritize recent requests
5. Process permissions in batch

### Use Case 3: Identify Inactive Registration Requests

**Scenario:** Find users who logged in once but never followed up.

**Steps:**
1. Enable filter
2. Sort by Last Logon (oldest first)
3. Contact users with old logon dates
4. Remove users who no longer need access

---

## Integration with Existing Features

### Works With Search Filter

The permission filter works in combination with the account name search:

```
Search: "john"
☑ Show only users without permissions

Result: Shows only users whose AccountName contains "john" AND have 0 permissions
```

### Preserves State

- Filter state is preserved during page interactions
- Unchecking returns to full user list
- Compatible with Add/Edit/Delete user operations

### Real-time Updates

After assigning permissions to a user:
- User's PermissionCount changes from 0 to 1+
- User automatically disappears from filtered view
- Badge count decreases
- No page reload needed

---

## Performance Considerations

### Client-Side Filtering

The filter is applied **client-side** using LINQ:
```csharp
filtered = users.Where(u => u.PermissionCount == 0);
```

**Pros:**
- Instant filtering (no server roundtrip)
- No database query overhead
- Works with search filter seamlessly

**Cons:**
- All users loaded from database first
- For 100,000+ users, consider server-side filtering

**Current Implementation:** Sufficient for typical user counts (< 10,000 users)

### Future Enhancement (Optional)

If user count grows significantly, add server-side filtering:

```csharp
// In UserPermissionService.cs
public async Task<List<DocuScanUserDto>> GetAllUsersAsync(
    string? accountNameFilter = null,
    bool onlyWithoutPermissions = false)
{
    var query = _context.DocuScanUsers.Include(u => u.UserPermissions).AsQueryable();

    if (onlyWithoutPermissions)
    {
        query = query.Where(u => !u.UserPermissions.Any());
    }

    // ... rest of method
}
```

---

## Testing Checklist

### Manual Testing

- [ ] Navigate to User Permissions Management page
- [ ] Verify checkbox appears below search box
- [ ] Verify checkbox label is clear
- [ ] Check the checkbox
- [ ] Verify only users with 0 permissions are shown
- [ ] Verify badge shows correct count
- [ ] Verify header shows "X without permissions (of Y total)"
- [ ] Uncheck the checkbox
- [ ] Verify all users are shown again
- [ ] Search for a user by name
- [ ] Check the filter
- [ ] Verify search + filter work together
- [ ] Assign permissions to a filtered user
- [ ] Verify user disappears from filtered view
- [ ] Verify badge count decreases
- [ ] Create a new user (they have 0 permissions by default)
- [ ] Verify new user appears in filtered view
- [ ] Verify "No users found" message when filter returns no results

### Edge Cases

- [ ] Test with 0 users in database
- [ ] Test with all users having permissions (empty filter result)
- [ ] Test with all users having 0 permissions (full list in filter)
- [ ] Test filter + search returning 0 results
- [ ] Test rapidly toggling filter on/off

---

## Files Modified

| File | Lines Modified | Changes |
|------|----------------|---------|
| `EditUserPermissions.razor` | +65 lines | Added filter UI, property, and logic |

**Total Changes:** 1 file, ~65 lines of code

---

## Summary

✅ **Feature Complete**

The registration request filter is now available on the User Permissions Management page. Administrators can:

- ✅ Quickly identify users without permissions
- ✅ Process registration requests efficiently
- ✅ See at-a-glance count of pending requests
- ✅ Combine with search for specific users
- ✅ Real-time updates as permissions are assigned

**User Experience:**
- Simple checkbox interface
- Clear labeling ("registration requests")
- Visual badge showing filtered count
- Context-aware messages

**Performance:**
- Client-side filtering (instant response)
- No additional API calls
- Minimal overhead

**Next Steps:**
1. Build the solution
2. Test the feature
3. Train administrators on the new filter
4. Monitor usage for performance (if user count grows)

---

**Status:** ✅ Ready for build and testing
