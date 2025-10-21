# User Permissions Refactoring

## Overview

This refactoring separates user information from the `UserPermissions` table into a dedicated `DocuScanUser` table, establishing a proper normalized database structure with foreign key relationships.

## What Changes

### Before Refactoring

**UserPermissions Table:**
```sql
Id (PK)
AccountName (VARCHAR(255))
DocumentTypeId (FK)
CounterPartyId (FK)
CountryCode (FK)
```

### After Refactoring

**DocuScanUser Table (NEW):**
```sql
UserId (PK, Identity)
AccountName (VARCHAR(255), Unique)
UserIdentifier (VARCHAR(255), Unique)
LastLogon (DATETIME, Nullable)
IsSuperUser (BIT, Default: 0)
CreatedOn (DATETIME, Default: GETDATE())
ModifiedOn (DATETIME, Nullable)
```

**UserPermissions Table (MODIFIED):**
```sql
Id (PK)
UserId (FK -> DocuScanUser.UserId)  -- NEW COLUMN
DocumentTypeId (FK)
CounterPartyId (FK)
CountryCode (FK)
-- AccountName column REMOVED
```

## Benefits

1. **Data Normalization**: User information is stored in one place
2. **User Tracking**: Track last logon timestamps for each user
3. **Role Management**: IsSuperUser flag for administrative privileges
4. **Better Performance**: Indexed foreign key relationships
5. **Data Integrity**: Foreign key constraints ensure referential integrity
6. **Audit Trail**: CreatedOn and ModifiedOn timestamps

## Migration Scripts

The refactoring consists of 4 sequential scripts:

### Script 04: Create DocuScanUser Table
- Creates the new `DocuScanUser` table
- Adds indexes for performance
- Location: `04_Create_DocuScanUser_Table.sql`

### Script 05: Migrate Users to DocuScanUser
- Extracts distinct users from `UserPermissions`
- Populates `DocuScanUser` table
- Adds `UserId` column to `UserPermissions`
- Updates `UserPermissions` with `UserId` values
- Location: `05_Migrate_Users_To_DocuScanUser.sql`

### Script 06: Add Foreign Key Constraint
- Makes `UserId` column NOT NULL
- Adds foreign key constraint to `DocuScanUser`
- Creates index on `UserId`
- Location: `06_Add_FK_Constraint_UserPermissions.sql`

### Script 07: Remove AccountName Column
- Removes the `AccountName` column from `UserPermissions`
- Verifies data integrity
- Location: `07_Remove_AccountName_From_UserPermissions.sql`

## Execution Options

### Option 1: Run All Scripts at Once (Recommended)

Execute the master script that runs all migration steps in sequence:

```sql
-- Location: RUN_UserPermissions_Refactoring.sql
sqlcmd -S your_server -d IkeaDocuScan -E -i RUN_UserPermissions_Refactoring.sql
```

### Option 2: Run Scripts Individually

Execute scripts one at a time for more control:

```sql
sqlcmd -S your_server -d IkeaDocuScan -E -i 04_Create_DocuScanUser_Table.sql
sqlcmd -S your_server -d IkeaDocuScan -E -i 05_Migrate_Users_To_DocuScanUser.sql
sqlcmd -S your_server -d IkeaDocuScan -E -i 06_Add_FK_Constraint_UserPermissions.sql
sqlcmd -S your_server -d IkeaDocuScan -E -i 07_Remove_AccountName_From_UserPermissions.sql
```

## Pre-Migration Checklist

- [ ] **Backup Database**: Create a full backup of the database
- [ ] **Review Current Data**: Check existing `UserPermissions` records
- [ ] **Schedule Downtime**: Plan maintenance window if needed
- [ ] **Test on Non-Production**: Run scripts on test/dev environment first
- [ ] **Notify Users**: Inform users of the maintenance window

## Post-Migration Tasks

### 1. Update Application Code

Update your application to reference the new structure:

```csharp
// OLD: Query UserPermissions with AccountName
var permissions = dbContext.UserPermissions
    .Where(up => up.AccountName == userName);

// NEW: Query UserPermissions with UserId via DocuScanUser
var permissions = dbContext.UserPermissions
    .Include(up => up.User)
    .Where(up => up.User.AccountName == userName);
```

### 2. Set SuperUser Flags

Update the `IsSuperUser` flag for administrator accounts:

```sql
UPDATE DocuScanUser
SET IsSuperUser = 1
WHERE AccountName IN ('admin@example.com', 'superuser@example.com');
```

### 3. Update LastLogon Timestamps

Implement code to update the `LastLogon` field when users log in:

```csharp
public async Task UpdateLastLogon(int userId)
{
    var user = await dbContext.DocuScanUsers.FindAsync(userId);
    if (user != null)
    {
        user.LastLogon = DateTime.UtcNow;
        user.ModifiedOn = DateTime.UtcNow;
        await dbContext.SaveChangesAsync();
    }
}
```

### 4. Verify Data Integrity

Run verification queries:

```sql
-- Check all users have permissions
SELECT u.UserId, u.AccountName, COUNT(up.Id) as PermissionCount
FROM DocuScanUser u
LEFT JOIN UserPermissions up ON u.UserId = up.UserId
GROUP BY u.UserId, u.AccountName
ORDER BY u.AccountName;

-- Check for orphaned permissions (should be 0)
SELECT COUNT(*)
FROM UserPermissions up
LEFT JOIN DocuScanUser u ON up.UserId = u.UserId
WHERE u.UserId IS NULL;
```

## Rollback Procedure

If you need to rollback the changes:

1. **Restore from backup** (safest option)
2. Or manually reverse the changes:

```sql
-- Add AccountName back to UserPermissions
ALTER TABLE UserPermissions
ADD AccountName VARCHAR(255) NULL;

-- Repopulate AccountName from DocuScanUser
UPDATE up
SET up.AccountName = dsu.AccountName
FROM UserPermissions up
INNER JOIN DocuScanUser dsu ON up.UserId = dsu.UserId;

-- Make AccountName NOT NULL
ALTER TABLE UserPermissions
ALTER COLUMN AccountName VARCHAR(255) NOT NULL;

-- Drop foreign key constraint
ALTER TABLE UserPermissions
DROP CONSTRAINT FK_UserPermissions_DocuScanUser;

-- Drop UserId column
ALTER TABLE UserPermissions
DROP COLUMN UserId;

-- Drop DocuScanUser table
DROP TABLE DocuScanUser;
```

## Troubleshooting

### Issue: NULL UserId values after migration

**Symptom**: Some `UserPermissions` records have NULL `UserId`

**Solution**:
```sql
-- Find records with NULL UserId
SELECT * FROM UserPermissions WHERE UserId IS NULL;

-- Check if users exist in DocuScanUser
SELECT DISTINCT AccountName
FROM UserPermissions
WHERE UserId IS NULL;

-- Add missing users manually if needed
INSERT INTO DocuScanUser (AccountName, UserIdentifier, IsSuperUser)
VALUES ('missinguser@example.com', 'missinguser@example.com', 0);

-- Update UserPermissions
UPDATE up
SET up.UserId = dsu.UserId
FROM UserPermissions up
INNER JOIN DocuScanUser dsu ON up.AccountName = dsu.AccountName
WHERE up.UserId IS NULL;
```

### Issue: Foreign key constraint violation

**Symptom**: Cannot delete users from DocuScanUser

**Solution**: The foreign key has CASCADE delete, so deleting a user will automatically delete their permissions. If you want to keep permissions:

```sql
-- Option 1: Reassign permissions to another user
UPDATE UserPermissions
SET UserId = @newUserId
WHERE UserId = @oldUserId;

-- Option 2: Temporarily disable constraint (not recommended for production)
ALTER TABLE UserPermissions NOCHECK CONSTRAINT FK_UserPermissions_DocuScanUser;
-- Perform operations
ALTER TABLE UserPermissions CHECK CONSTRAINT FK_UserPermissions_DocuScanUser;
```

## Support

For questions or issues with the migration:
- Review the script output for detailed error messages
- Check the verification queries in each script
- Contact the database administrator

## Version History

- **v1.0** - Initial refactoring scripts created
  - Creates DocuScanUser table
  - Migrates existing user data
  - Establishes foreign key relationships
  - Removes redundant AccountName column
