# Add New User Feature Documentation

**Date:** 2025-11-05
**Feature:** User Permissions Management - Add New User Functionality

## Overview

Added complete "Add New User" functionality to the User Permissions Management system, allowing administrators to create new DocuScanUser accounts directly from the UI.

---

## Changes Made

### 1. ✅ Data Transfer Objects (DTOs)

**Created Two New DTOs:**

#### CreateDocuScanUserDto.cs
**Location:** `IkeaDocuScan.Shared/DTOs/UserPermissions/CreateDocuScanUserDto.cs`

```csharp
public class CreateDocuScanUserDto
{
    [Required]
    [StringLength(255)]
    public string AccountName { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string UserIdentifier { get; set; } = string.Empty;

    public bool IsSuperUser { get; set; } = false;
}
```

**Fields:**
- `AccountName` - Windows account name (e.g., `DOMAIN\username` or `username@domain.com`)
- `UserIdentifier` - Unique identifier (typically Windows SID or GUID)
- `IsSuperUser` - Whether the user has super user privileges (default: false)

#### UpdateDocuScanUserDto.cs
**Location:** `IkeaDocuScan.Shared/DTOs/UserPermissions/UpdateDocuScanUserDto.cs`

```csharp
public class UpdateDocuScanUserDto
{
    [Required]
    public int UserId { get; set; }

    [Required]
    [StringLength(255)]
    public string AccountName { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string UserIdentifier { get; set; } = string.Empty;

    public bool IsSuperUser { get; set; }
}
```

---

### 2. ✅ Service Layer

**File:** `IkeaDocuScan-Web/Services/UserPermissionService.cs`

**Added Methods:**

#### CreateUserAsync
```csharp
public async Task<DocuScanUserDto> CreateUserAsync(CreateDocuScanUserDto dto)
```

**Features:**
- Validates account name uniqueness
- Validates user identifier uniqueness
- Sets `CreatedOn` to current UTC time
- Returns complete `DocuScanUserDto` with generated `UserId`

**Validation:**
- ❌ Throws `ValidationException` if account name already exists
- ❌ Throws `ValidationException` if user identifier already exists

#### UpdateUserAsync
```csharp
public async Task<DocuScanUserDto> UpdateUserAsync(UpdateDocuScanUserDto dto)
```

**Features:**
- Validates user exists
- Validates account name uniqueness (excluding current user)
- Validates user identifier uniqueness (excluding current user)
- Updates `ModifiedOn` to current UTC time
- Returns updated `DocuScanUserDto` with permission count

**Validation:**
- ❌ Throws `ValidationException` if user not found
- ❌ Throws `ValidationException` if duplicate account name
- ❌ Throws `ValidationException` if duplicate user identifier

---

### 3. ✅ Service Interface

**File:** `IkeaDocuScan.Shared/Interfaces/IUserPermissionService.cs`

**Added Methods:**
```csharp
Task<DocuScanUserDto> CreateUserAsync(CreateDocuScanUserDto dto);
Task<DocuScanUserDto> UpdateUserAsync(UpdateDocuScanUserDto dto);
```

---

### 4. ✅ API Endpoints

**File:** `IkeaDocuScan-Web/Endpoints/UserPermissionEndpoints.cs`

**Added Endpoints:**

#### Create User
```http
POST /api/userpermissions/user
Authorization: Required
Content-Type: application/json

{
  "accountName": "DOMAIN\\username",
  "userIdentifier": "S-1-5-21-...",
  "isSuperUser": false
}
```

**Response (201 Created):**
```json
{
  "userId": 123,
  "accountName": "DOMAIN\\username",
  "userIdentifier": "S-1-5-21-...",
  "lastLogon": null,
  "isSuperUser": false,
  "createdOn": "2025-11-05T12:00:00Z",
  "modifiedOn": null,
  "permissionCount": 0
}
```

**Errors:**
- 400 Bad Request - Validation failed (duplicate account name or identifier)
- 401 Unauthorized - Authentication required

#### Update User
```http
PUT /api/userpermissions/user/{userId}
Authorization: Required
Content-Type: application/json

{
  "userId": 123,
  "accountName": "DOMAIN\\username",
  "userIdentifier": "S-1-5-21-...",
  "isSuperUser": true
}
```

**Response (200 OK):**
```json
{
  "userId": 123,
  "accountName": "DOMAIN\\username",
  "userIdentifier": "S-1-5-21-...",
  "lastLogon": "2025-11-04T10:30:00Z",
  "isSuperUser": true,
  "createdOn": "2025-11-01T12:00:00Z",
  "modifiedOn": "2025-11-05T12:00:00Z",
  "permissionCount": 5
}
```

**Errors:**
- 400 Bad Request - Validation failed or ID mismatch
- 404 Not Found - User not found
- 401 Unauthorized - Authentication required

---

### 5. ✅ Client HTTP Service

**File:** `IkeaDocuScan-Web.Client/Services/UserPermissionHttpService.cs`

**Added Methods:**

#### CreateUserAsync
```csharp
public async Task<DocuScanUserDto> CreateUserAsync(CreateDocuScanUserDto dto)
{
    var response = await _http.PostAsJsonAsync("/api/userpermissions/user", dto);
    response.EnsureSuccessStatusCode();
    return await response.Content.ReadFromJsonAsync<DocuScanUserDto>();
}
```

#### UpdateUserAsync
```csharp
public async Task<DocuScanUserDto> UpdateUserAsync(UpdateDocuScanUserDto dto)
{
    var response = await _http.PutAsJsonAsync($"/api/userpermissions/user/{dto.UserId}", dto);
    response.EnsureSuccessStatusCode();
    return await response.Content.ReadFromJsonAsync<DocuScanUserDto>();
}
```

---

### 6. ✅ User Interface

**File:** `IkeaDocuScan-Web.Client/Pages/EditUserPermissions.razor`

**Added Components:**

#### "Add New User" Button
**Location:** After search box, before user list

```razor
<div class="mt-3 mb-3">
    <button class="btn btn-primary" @onclick="ShowAddUserForm">
        <i class="fa fa-user-plus"></i> Add New User
    </button>
</div>
```

#### User Form Modal
**Features:**
- Modal dialog with primary blue header
- Three input fields:
  1. Account Name (required)
  2. User Identifier (required)
  3. Super User checkbox (optional)
- Input validation with error messages
- Save/Cancel buttons with loading spinner
- Info message about adding permissions after user creation

**Modal Styling:**
- Large modal (`modal-lg`)
- Primary blue header for visibility
- Form fields with helper text
- Error message display with dismissible alerts
- Loading spinner during save operation

#### Code Behind
**New Fields:**
```csharp
private bool showUserFormModal = false;
private bool isSavingUser = false;
private DocuScanUserDto? editingUser = null;
private string newUserAccountName = string.Empty;
private string newUserIdentifier = string.Empty;
private bool newUserIsSuperUser = false;
private string? userFormErrorMessage = null;
```

**New Methods:**
- `ShowAddUserForm()` - Opens modal with empty form
- `CancelUserForm()` - Closes modal and clears form
- `SaveUser()` - Validates and saves user (create or update)
- `ClearUserForm()` - Resets form fields

---

## User Workflow

### Creating a New User

1. Navigate to **Edit User Permissions** page
2. Click **"Add New User"** button (blue button below search box)
3. Fill in the user form:
   - **Account Name**: Enter Windows account name (e.g., `DOMAIN\jdoe`)
   - **User Identifier**: Enter unique identifier (e.g., SID: `S-1-5-21-...`)
   - **Super User**: Check if user should have administrator privileges
4. Click **"Create User"** button
5. User is created and appears in the user list
6. Success message displays at top of page
7. Click user row to add permissions (Document Types, Counter Parties, Countries)

### Validation Rules

**Account Name:**
- ✅ Required
- ✅ Maximum 255 characters
- ✅ Must be unique (case-sensitive)
- ❌ Cannot be empty or whitespace

**User Identifier:**
- ✅ Required
- ✅ Maximum 255 characters
- ✅ Must be unique (case-sensitive)
- ❌ Cannot be empty or whitespace
- 💡 Typically a Windows SID or GUID

**Super User:**
- Optional (default: false)
- Super users have access to:
  - Configuration management
  - All document types, counter parties, and countries
  - User management

---

## Database Schema

**Table:** `DocuScanUser`

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| UserId | int | PK, Identity | Auto-generated primary key |
| AccountName | varchar(255) | NOT NULL, Unique | Windows account name |
| UserIdentifier | varchar(255) | NOT NULL, Unique | Unique identifier (SID/GUID) |
| LastLogon | datetime | NULL | Last login timestamp |
| IsSuperUser | bit | NOT NULL, Default: 0 | Super user flag |
| CreatedOn | datetime | NOT NULL | Creation timestamp (UTC) |
| ModifiedOn | datetime | NULL | Last modification timestamp (UTC) |

**Indexes:**
- `UK_DocuScanUser_AccountName` - Unique index on AccountName
- `UK_DocuScanUser_UserIdentifier` - Unique index on UserIdentifier
- `IX_DocuScanUser_IsSuperUser` - Non-unique index for filtering
- `IX_DocuScanUser_LastLogon` - Non-unique index for sorting

---

## Error Handling

### Duplicate Account Name
**User Action:** Tries to create user with existing account name

**Error Response (400):**
```json
{
  "error": "User with account name 'DOMAIN\\username' already exists"
}
```

**UI Display:**
```
❌ Error saving user: User with account name 'DOMAIN\username' already exists
```

### Duplicate User Identifier
**User Action:** Tries to create user with existing identifier

**Error Response (400):**
```json
{
  "error": "User with identifier 'S-1-5-21-...' already exists"
}
```

**UI Display:**
```
❌ Error saving user: User with identifier 'S-1-5-21-...' already exists
```

### Missing Required Fields
**User Action:** Leaves Account Name or User Identifier blank

**UI Display:**
```
❌ Account Name is required
```
or
```
❌ User Identifier is required
```

---

## Testing Checklist

### Create User Flow
- [ ] Click "Add New User" button opens modal
- [ ] Enter valid account name and identifier
- [ ] Toggle "Super User" checkbox
- [ ] Click "Create User" saves successfully
- [ ] User appears in user list
- [ ] Success message displays
- [ ] Modal closes automatically

### Validation Testing
- [ ] Empty account name shows error
- [ ] Empty user identifier shows error
- [ ] Duplicate account name shows clear error
- [ ] Duplicate user identifier shows clear error
- [ ] Whitespace-only values are rejected

### UI/UX Testing
- [ ] Modal is responsive and centered
- [ ] Form fields have clear labels and helper text
- [ ] Loading spinner shows during save
- [ ] Cancel button closes modal without saving
- [ ] Click outside modal does NOT close it (user must explicitly cancel)
- [ ] Error messages are dismissible
- [ ] Success message shows after creation

### Integration Testing
- [ ] Created user can be selected to manage permissions
- [ ] Created user can have permissions added
- [ ] Created super user can access configuration management
- [ ] Created user appears in search results
- [ ] User list updates immediately after creation

### Edge Cases
- [ ] Very long account names (250+ characters)
- [ ] Special characters in account name (`\`, `@`, `/`)
- [ ] Mixed case account names
- [ ] User identifiers with special formats (SIDs, GUIDs, emails)

---

## PowerShell Testing Examples

### Create User via API
```powershell
$newUser = @{
    accountName = "DOMAIN\testuser"
    userIdentifier = "S-1-5-21-1234567890-1234567890-1234567890-1001"
    isSuperUser = $false
} | ConvertTo-Json

Invoke-RestMethod -Uri "https://localhost:44101/api/userpermissions/user" `
    -Method POST `
    -Body $newUser `
    -ContentType "application/json" `
    -UseDefaultCredentials `
    -SkipCertificateCheck
```

### Update User via API
```powershell
$updateUser = @{
    userId = 123
    accountName = "DOMAIN\testuser"
    userIdentifier = "S-1-5-21-1234567890-1234567890-1234567890-1001"
    isSuperUser = $true
} | ConvertTo-Json

Invoke-RestMethod -Uri "https://localhost:44101/api/userpermissions/user/123" `
    -Method PUT `
    -Body $updateUser `
    -ContentType "application/json" `
    -UseDefaultCredentials `
    -SkipCertificateCheck
```

### Get All Users
```powershell
Invoke-RestMethod -Uri "https://localhost:44101/api/userpermissions/users" `
    -Method GET `
    -UseDefaultCredentials `
    -SkipCertificateCheck
```

---

## Files Modified/Created

| File | Type | Description |
|------|------|-------------|
| `CreateDocuScanUserDto.cs` | New | DTO for creating users |
| `UpdateDocuScanUserDto.cs` | New | DTO for updating users |
| `IUserPermissionService.cs` | Modified | Added interface methods |
| `UserPermissionService.cs` | Modified | Added create/update methods |
| `UserPermissionEndpoints.cs` | Modified | Added API endpoints |
| `UserPermissionHttpService.cs` | Modified | Added client HTTP methods |
| `EditUserPermissions.razor` | Modified | Added UI button and modal |
| `ADD_NEW_USER_FEATURE.md` | New | This documentation file |

---

## Future Enhancements

### 1. Edit User Button
Add "Edit" button next to "Manage" button in user list to allow inline editing of user properties without needing to select the user first.

### 2. Bulk User Import
Add CSV import functionality to create multiple users at once:
```csv
AccountName,UserIdentifier,IsSuperUser
DOMAIN\user1,S-1-5-21-...,false
DOMAIN\user2,S-1-5-21-...,true
```

### 3. Active Directory Integration
Add "Import from AD" button to search Active Directory and import users automatically with their SIDs.

### 4. User Role Templates
Create predefined permission templates (e.g., "Basic User", "Department Admin") that can be applied when creating users.

### 5. User Activity Log
Show recent activity for each user (last documents accessed, permissions added/removed).

### 6. Email Notifications
Send email to new users when their account is created, including instructions for accessing the system.

---

## Security Considerations

### Authorization
- ✅ All endpoints require authentication
- ✅ All endpoints require `HasAccess` policy
- ⚠️ Consider adding `SuperUser` policy requirement for user creation/modification

### Input Validation
- ✅ Server-side validation for all required fields
- ✅ String length validation (255 characters max)
- ✅ Uniqueness validation for account name and identifier
- ✅ Whitespace trimming before saving

### Audit Trail
- ⚠️ Consider adding audit logging for user creation/modification
- ⚠️ Track who created/modified each user
- ⚠️ Log all user management actions

---

## Conclusion

The "Add New User" feature is now fully implemented and integrated into the User Permissions Management system. Administrators can create new users directly from the UI with proper validation, error handling, and user feedback. The feature follows the existing architectural patterns and maintains consistency with the rest of the application.

**Status:** ✅ Ready for testing and deployment
