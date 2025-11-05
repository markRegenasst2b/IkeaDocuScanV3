# AccountName vs UserIdentifier Consolidation Plan

**Date:** 2025-11-05
**Issue:** `DocuScanUser` entity has both `AccountName` and `UserIdentifier` fields with identical values
**Status:** 🔄 Analysis Complete - Ready for Implementation

---

## Executive Summary

After analyzing the codebase, **AccountName** is the field actively used throughout the application for authentication, authorization, and user display. **UserIdentifier** is redundant and only used for:
- Database storage (duplicate data)
- Uniqueness validation
- Single UI display in delete confirmation

**RECOMMENDATION:** Remove `UserIdentifier` and keep `AccountName` as the single source of truth.

---

## Usage Analysis

### AccountName Usage (CRITICAL - 4 Files)

#### 1. WindowsIdentityMiddleware.cs
**Purpose:** Windows Authentication - Maps Windows identity to database user
```csharp
// Line 89: Critical authentication lookup
var user = await dbContext.DocuScanUsers
    .AsNoTracking()
    .FirstOrDefaultAsync(u => u.AccountName == username);
```

#### 2. CurrentUserService.cs
**Purpose:** User session and authorization context
```csharp
// Line 60: User lookup by AccountName
var docuScanUser = await _context.DocuScanUsers
    .AsNoTracking()
    .FirstOrDefaultAsync(u => u.AccountName == username);

// Lines 81, 105, 136: Used in CurrentUser model (authorization context)
AccountName = docuScanUser.AccountName

// Lines 198, 270: User existence check and last logon update
var existingUser = await _context.DocuScanUsers
    .FirstOrDefaultAsync(u => u.AccountName == username);
```

#### 3. UserPermissionService.cs
**Purpose:** User management and permission operations
```csharp
// Lines 39, 43: Filter and sort by AccountName
query = query.Where(u => u.AccountName.ToLower().Contains(filter));
.OrderBy(u => u.AccountName)

// Lines 75, 79: Permission queries use AccountName
query = query.Where(up => up.User.AccountName.ToLower().Contains(filter));
.OrderBy(up => up.User.AccountName)

// Lines 235, 239: Validation and error messages
if (existsByAccountName)
    throw new ValidationException($"User with account name '{dto.AccountName}' already exists");

// Line 340: Permission DTO population
AccountName = entity.User?.AccountName ?? string.Empty
```

#### 4. EditUserPermissions.razor
**Purpose:** User interface - displays and manages users
```csharp
// Line 95: Main user list display
<td><strong>@user.AccountName</strong></td>

// Line 136: Page header
<h5 class="mb-0">Permissions for @selectedUser.AccountName</h5>

// Lines 340, 395, 406, 443, 856, 961, 987: All user-facing messages
```

#### 5. CurrentUser.cs (Model)
**Purpose:** Authorization context throughout application
```csharp
// Line 9: Core authorization property
public string AccountName { get; set; } = string.Empty;
```

### UserIdentifier Usage (MINIMAL - 2 Files)

#### 1. UserPermissionService.cs
**Purpose:** Validation and DTO population only
```csharp
// Lines 244, 248: Uniqueness validation (duplicate check)
var existsByIdentifier = await context.DocuScanUsers
    .AnyAsync(u => u.UserIdentifier == dto.UserIdentifier);

// Lines 50, 254, 270: DTO/Entity population (parallel to AccountName)
UserIdentifier = u.UserIdentifier

// Lines 305, 309: Update validation (duplicate check)
var duplicateIdentifier = await context.DocuScanUsers
    .AnyAsync(u => u.UserIdentifier == dto.UserIdentifier && u.UserId != dto.UserId);
```

#### 2. EditUserPermissions.razor
**Purpose:** Single display in delete confirmation only
```csharp
// Line 396: Delete confirmation dialog
<li><strong>User Identifier:</strong> @userToDelete.UserIdentifier</li>
```

---

## Comparison Matrix

| Aspect | AccountName | UserIdentifier |
|--------|-------------|----------------|
| **Used in Authentication** | ✅ Yes (WindowsIdentityMiddleware) | ❌ No |
| **Used in Authorization** | ✅ Yes (CurrentUser model) | ❌ No |
| **Used for Lookups** | ✅ Yes (All queries) | ❌ No |
| **Displayed in UI** | ✅ Yes (Throughout) | ⚠️ Once (delete dialog) |
| **Semantic Meaning** | ✅ Clear (Windows account name) | ⚠️ Vague (identifier of what?) |
| **Database Constraint** | ✅ Unique index | ✅ Unique index |
| **Used in DTOs** | ✅ Yes | ✅ Yes (redundant) |
| **Required Field** | ✅ Yes (not null) | ✅ Yes (not null) |

---

## Consolidation Plan

### Phase 1: Code Changes (Remove UserIdentifier References)

#### Step 1.1: Update DTOs

**File:** `IkeaDocuScan.Shared/DTOs/UserPermissions/DocuScanUserDto.cs`
```csharp
public class DocuScanUserDto
{
    public int UserId { get; set; }
    public string AccountName { get; set; } = string.Empty;
    // ❌ REMOVE: public string UserIdentifier { get; set; } = string.Empty;
    public DateTime? LastLogon { get; set; }
    public bool IsSuperUser { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public int PermissionCount { get; set; }
}
```

**File:** `IkeaDocuScan.Shared/DTOs/UserPermissions/CreateDocuScanUserDto.cs`
```csharp
public class CreateDocuScanUserDto
{
    [Required(ErrorMessage = "Account name is required")]
    [StringLength(255, ErrorMessage = "Account name cannot exceed 255 characters")]
    public string AccountName { get; set; } = string.Empty;

    // ❌ REMOVE: UserIdentifier property and validation

    public bool IsSuperUser { get; set; } = false;
}
```

**File:** `IkeaDocuScan.Shared/DTOs/UserPermissions/UpdateDocuScanUserDto.cs`
```csharp
public class UpdateDocuScanUserDto
{
    [Required]
    public int UserId { get; set; }

    [Required(ErrorMessage = "Account name is required")]
    [StringLength(255, ErrorMessage = "Account name cannot exceed 255 characters")]
    public string AccountName { get; set; } = string.Empty;

    // ❌ REMOVE: UserIdentifier property and validation

    public bool IsSuperUser { get; set; }
}
```

#### Step 1.2: Update Service Layer

**File:** `IkeaDocuScan-Web/Services/UserPermissionService.cs`

Remove all references to `UserIdentifier`:
- Line 50: Remove `UserIdentifier = u.UserIdentifier,` from DTO mapping
- Lines 244-249: Remove UserIdentifier uniqueness check in CreateUserAsync
- Line 254: Remove `UserIdentifier = dto.UserIdentifier,` from entity creation
- Line 270: Remove `UserIdentifier = entity.UserIdentifier,` from return DTO
- Lines 305-310: Remove UserIdentifier duplicate check in UpdateUserAsync
- Line 313: Remove `entity.UserIdentifier = dto.UserIdentifier;`
- Line 325: Remove `UserIdentifier = entity.UserIdentifier,` from return DTO

**Changes:**
```csharp
// OLD CreateUserAsync validation:
var existsByIdentifier = await context.DocuScanUsers
    .AnyAsync(u => u.UserIdentifier == dto.UserIdentifier);
if (existsByIdentifier)
    throw new ValidationException($"User with identifier '{dto.UserIdentifier}' already exists");

// ✅ REMOVE entire block

// OLD entity creation:
var entity = new DocuScanUser
{
    AccountName = dto.AccountName,
    UserIdentifier = dto.UserIdentifier,  // ❌ REMOVE
    IsSuperUser = dto.IsSuperUser,
    CreatedOn = DateTime.UtcNow,
    LastLogon = null,
    ModifiedOn = null
};

// NEW entity creation:
var entity = new DocuScanUser
{
    AccountName = dto.AccountName,
    IsSuperUser = dto.IsSuperUser,
    CreatedOn = DateTime.UtcNow,
    LastLogon = null,
    ModifiedOn = null
};
```

#### Step 1.3: Update UI Layer

**File:** `IkeaDocuScan-Web.Client/Pages/EditUserPermissions.razor`

Remove Line 396:
```razor
<!-- OLD: -->
<li><strong>Account Name:</strong> @userToDelete.AccountName</li>
<li><strong>User Identifier:</strong> @userToDelete.UserIdentifier</li>

<!-- NEW: -->
<li><strong>Account Name:</strong> @userToDelete.AccountName</li>
```

Also update the user form modal to remove UserIdentifier field (if present in form).

#### Step 1.4: Test Code Changes

Before proceeding to database changes, verify:
1. ✅ Application compiles without errors
2. ✅ User list displays correctly
3. ✅ User creation works with only AccountName
4. ✅ User update works with only AccountName
5. ✅ User deletion works
6. ✅ Authentication still works (WindowsIdentityMiddleware)
7. ✅ Authorization still works (CurrentUserService)

### Phase 2: Database Migration

#### Step 2.1: Create Entity Framework Migration

**File:** `IkeaDocuScan.Infrastructure/Entities/DocuScanUser.cs`

Remove UserIdentifier property and index:
```csharp
[Table("DocuScanUser")]
[Index("IsSuperUser", Name = "IX_DocuScanUser_IsSuperUser")]
[Index("LastLogon", Name = "IX_DocuScanUser_LastLogon")]
[Index("AccountName", Name = "UK_DocuScanUser_AccountName", IsUnique = true)]
// ❌ REMOVE: [Index("UserIdentifier", Name = "UK_DocuScanUser_UserIdentifier", IsUnique = true)]
public partial class DocuScanUser
{
    [Key]
    public int UserId { get; set; }

    [StringLength(255)]
    [Unicode(false)]
    public string AccountName { get; set; } = null!;

    // ❌ REMOVE: UserIdentifier property

    [Column(TypeName = "datetime")]
    public DateTime? LastLogon { get; set; }

    public bool IsSuperUser { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime CreatedOn { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ModifiedOn { get; set; }

    [InverseProperty("User")]
    public virtual ICollection<UserPermission> UserPermissions { get; set; } = new List<UserPermission>();
}
```

#### Step 2.2: Generate Migration

```bash
cd IkeaDocuScan.Infrastructure
dotnet ef migrations add RemoveUserIdentifierFromDocuScanUser --startup-project ../IkeaDocuScan-Web/IkeaDocuScan-Web
```

#### Step 2.3: Review Generated Migration

The migration should contain:
```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    // Drop unique index
    migrationBuilder.DropIndex(
        name: "UK_DocuScanUser_UserIdentifier",
        table: "DocuScanUser");

    // Drop column
    migrationBuilder.DropColumn(
        name: "UserIdentifier",
        table: "DocuScanUser");
}

protected override void Down(MigrationBuilder migrationBuilder)
{
    // Re-add column
    migrationBuilder.AddColumn<string>(
        name: "UserIdentifier",
        table: "DocuScanUser",
        type: "varchar(255)",
        unicode: false,
        maxLength: 255,
        nullable: false,
        defaultValue: "");

    // Re-add unique index
    migrationBuilder.CreateIndex(
        name: "UK_DocuScanUser_UserIdentifier",
        table: "DocuScanUser",
        column: "UserIdentifier",
        unique: true);
}
```

#### Step 2.4: Backup Database

**CRITICAL:** Before applying migration, back up the database:
```sql
-- SQL Server backup command
BACKUP DATABASE [IkeaDocuScan]
TO DISK = 'C:\Backups\IkeaDocuScan_BeforeUserIdentifierRemoval_20251105.bak'
WITH FORMAT, COMPRESSION;
```

#### Step 2.5: Apply Migration

**Development:**
```bash
dotnet ef database update --startup-project ../IkeaDocuScan-Web/IkeaDocuScan-Web
```

**Production:**
Generate SQL script for manual review and execution:
```bash
dotnet ef migrations script --startup-project ../IkeaDocuScan-Web/IkeaDocuScan-Web --output RemoveUserIdentifier.sql
```

Review `RemoveUserIdentifier.sql` and execute during maintenance window.

### Phase 3: Verification

#### Step 3.1: Database Verification

```sql
-- Verify column is removed
SELECT COLUMN_NAME
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'DocuScanUser';
-- Should NOT contain 'UserIdentifier'

-- Verify unique index is removed
SELECT *
FROM sys.indexes
WHERE object_id = OBJECT_ID('DocuScanUser')
AND name = 'UK_DocuScanUser_UserIdentifier';
-- Should return 0 rows

-- Verify AccountName unique index still exists
SELECT *
FROM sys.indexes
WHERE object_id = OBJECT_ID('DocuScanUser')
AND name = 'UK_DocuScanUser_AccountName';
-- Should return 1 row
```

#### Step 3.2: Application Testing

1. ✅ **Authentication Test:** Log in with Windows authentication
2. ✅ **User List:** Navigate to `/userpermissions/edit` - verify users display
3. ✅ **Create User:** Create a new user with only AccountName
4. ✅ **Update User:** Update an existing user's AccountName
5. ✅ **Delete User:** Delete a user and verify confirmation message
6. ✅ **Permission Assignment:** Assign permissions to a user
7. ✅ **Authorization Test:** Verify document access control works
8. ✅ **API Test:** Call `/api/userpermissions/users` endpoint

#### Step 3.3: Rollback Plan (If Issues Found)

If issues are discovered:

1. **Immediate Rollback:**
   ```bash
   dotnet ef database update <PreviousMigrationName> --startup-project ../IkeaDocuScan-Web/IkeaDocuScan-Web
   ```

2. **Restore from Backup:**
   ```sql
   RESTORE DATABASE [IkeaDocuScan]
   FROM DISK = 'C:\Backups\IkeaDocuScan_BeforeUserIdentifierRemoval_20251105.bak'
   WITH REPLACE;
   ```

3. **Code Rollback:**
   - Revert code changes using Git
   - Rebuild application
   - Restart IIS/application

---

## Files to Modify

### Phase 1: Code Changes (8 files)

| File | Action | Lines |
|------|--------|-------|
| `IkeaDocuScan.Shared/DTOs/UserPermissions/DocuScanUserDto.cs` | Remove property | Line 10 |
| `IkeaDocuScan.Shared/DTOs/UserPermissions/CreateDocuScanUserDto.cs` | Remove property | ~Line 14-17 |
| `IkeaDocuScan.Shared/DTOs/UserPermissions/UpdateDocuScanUserDto.cs` | Remove property | ~Line 17-20 |
| `IkeaDocuScan-Web/Services/UserPermissionService.cs` | Remove validation and mapping | Lines 50, 244-249, 254, 270, 305-310, 313, 325 |
| `IkeaDocuScan-Web.Client/Pages/EditUserPermissions.razor` | Remove display | Line 396 |
| `IkeaDocuScan.Infrastructure/Entities/DocuScanUser.cs` | Remove property and index | Lines ~23, 26-29 |

### Phase 2: Database Migration (1 migration)

| Migration | Description |
|-----------|-------------|
| `RemoveUserIdentifierFromDocuScanUser` | Drops UK_DocuScanUser_UserIdentifier index and UserIdentifier column |

---

## Risk Assessment

| Risk | Likelihood | Impact | Mitigation |
|------|------------|--------|------------|
| Data loss | Low | High | Database backup before migration |
| Authentication failure | Low | High | Test authentication after code changes, before DB migration |
| User creation fails | Low | Medium | Test user CRUD operations thoroughly |
| Existing integrations break | Low | Medium | Check for external systems using UserIdentifier |
| Migration fails | Low | High | Generate SQL script for manual review |

---

## Benefits of Consolidation

1. ✅ **Simplified Data Model** - One field instead of two redundant fields
2. ✅ **Reduced Storage** - Eliminates duplicate data (255 chars per user)
3. ✅ **Clearer Semantics** - "AccountName" clearly indicates Windows account
4. ✅ **Easier Maintenance** - No need to keep two fields in sync
5. ✅ **Reduced Complexity** - Less validation logic, fewer constraints
6. ✅ **Improved Performance** - One less index to maintain
7. ✅ **Better Code Quality** - Eliminates redundant DTO properties and mapping

---

## Timeline Estimate

| Phase | Duration | Notes |
|-------|----------|-------|
| **Phase 1: Code Changes** | 1-2 hours | Remove UserIdentifier from DTOs, services, UI |
| **Testing Phase 1** | 1 hour | Verify application works without UserIdentifier in code |
| **Phase 2: Database Migration** | 30 minutes | Create and review migration |
| **Database Backup** | 15 minutes | Full backup before applying migration |
| **Apply Migration** | 5 minutes | Execute migration (dev environment) |
| **Phase 3: Verification** | 1 hour | Comprehensive testing |
| **Total** | **4-5 hours** | Includes buffer time |

**Production Deployment:** Schedule during maintenance window (low-traffic period)

---

## Implementation Checklist

### Pre-Implementation
- [ ] Read this entire document
- [ ] Create feature branch: `feature/consolidate-account-name`
- [ ] Notify team of upcoming changes
- [ ] Schedule maintenance window (if production)

### Phase 1: Code Changes
- [ ] Remove UserIdentifier from `DocuScanUserDto.cs`
- [ ] Remove UserIdentifier from `CreateDocuScanUserDto.cs`
- [ ] Remove UserIdentifier from `UpdateDocuScanUserDto.cs`
- [ ] Update `UserPermissionService.cs` (remove validation and mapping)
- [ ] Update `EditUserPermissions.razor` (remove display)
- [ ] Build solution - verify no compilation errors
- [ ] Test user list page
- [ ] Test user creation
- [ ] Test user update
- [ ] Test user deletion
- [ ] Test authentication
- [ ] Commit code changes

### Phase 2: Database Migration
- [ ] Update `DocuScanUser.cs` entity (remove property and index)
- [ ] Generate EF migration
- [ ] Review generated migration code
- [ ] Test migration in development environment
- [ ] Backup production database (if applicable)
- [ ] Apply migration to production

### Phase 3: Verification
- [ ] Run database verification queries
- [ ] Test full application workflow
- [ ] Check logs for errors
- [ ] Monitor application for 24 hours
- [ ] Document any issues encountered

### Post-Implementation
- [ ] Update API documentation (if exists)
- [ ] Update user guides (if exists)
- [ ] Merge feature branch to main
- [ ] Tag release version
- [ ] Archive this document

---

## Summary

**Current State:**
- `DocuScanUser` has both `AccountName` and `UserIdentifier` with duplicate values
- Both fields have unique indexes and constraints
- `AccountName` is used in all critical paths (auth, authorization, UI)
- `UserIdentifier` is only used for validation and single UI display

**Target State:**
- `DocuScanUser` has only `AccountName`
- Single unique index on `AccountName`
- Simplified DTOs without redundant field
- Cleaner codebase with less duplication

**Recommendation:** ✅ **Proceed with consolidation - Remove UserIdentifier**

**Next Step:** Begin Phase 1 (Code Changes) after approval.

---

## Questions or Concerns

If you have any questions or concerns about this consolidation plan, please review:
1. The usage analysis section to understand field usage
2. The risk assessment and mitigation strategies
3. The rollback plan in case of issues

**READY TO PROCEED?** Follow the implementation checklist above.
