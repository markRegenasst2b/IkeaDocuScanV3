-- =============================================
-- Script: Remove AccountName Column from UserPermissions
-- Purpose: Complete the refactoring by removing the AccountName column
--          from UserPermissions table (now replaced by UserId foreign key)
-- =============================================

USE IkeaDocuScan;
GO

-- Step 1: Verify foreign key constraint exists before removing AccountName
PRINT 'Verifying foreign key constraint exists...';
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_UserPermissions_DocuScanUser')
BEGIN
    RAISERROR('Cannot proceed: Foreign key constraint FK_UserPermissions_DocuScanUser does not exist. Run script 06 first.', 16, 1);
END
ELSE
BEGIN
    PRINT 'Foreign key constraint verified.';
END
GO

-- Step 2: Create a backup verification query (optional - for safety)
PRINT 'Creating verification data for safety...';
GO

-- Display count of records by AccountName vs UserId to ensure no data loss
SELECT
    COUNT(*) as TotalRecords,
    COUNT(DISTINCT [AccountName]) as DistinctAccountNames,
    COUNT(DISTINCT [UserId]) as DistinctUserIds
FROM [dbo].[UserPermissions];
GO

-- Step 3: Drop AccountName column
PRINT 'Removing AccountName column from UserPermissions...';
GO

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[UserPermissions]') AND name = 'AccountName')
BEGIN
    -- Drop the column
    ALTER TABLE [dbo].[UserPermissions]
    DROP COLUMN [AccountName];

    PRINT 'AccountName column removed successfully from UserPermissions table.';
END
ELSE
BEGIN
    PRINT 'AccountName column does not exist in UserPermissions table.';
END
GO

-- Step 4: Verify final table structure
PRINT 'Verifying final table structure...';
GO

SELECT
    c.name AS ColumnName,
    t.name AS DataType,
    c.max_length AS MaxLength,
    c.is_nullable AS IsNullable,
    CASE WHEN pk.column_id IS NOT NULL THEN 'Yes' ELSE 'No' END AS IsPrimaryKey,
    CASE WHEN fk.parent_column_id IS NOT NULL THEN 'Yes' ELSE 'No' END AS IsForeignKey
FROM sys.columns c
INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
LEFT JOIN (
    SELECT ic.object_id, ic.column_id
    FROM sys.index_columns ic
    INNER JOIN sys.indexes i ON ic.object_id = i.object_id AND ic.index_id = i.index_id
    WHERE i.is_primary_key = 1
) pk ON c.object_id = pk.object_id AND c.column_id = pk.column_id
LEFT JOIN sys.foreign_key_columns fk ON c.object_id = fk.parent_object_id AND c.column_id = fk.parent_column_id
WHERE c.object_id = OBJECT_ID(N'[dbo].[UserPermissions]')
ORDER BY c.column_id;
GO

-- Step 5: Verify data integrity after refactoring
PRINT 'Verifying data integrity after refactoring...';
GO

DECLARE @RecordCount INT;
DECLARE @OrphanedRecords INT;

SELECT @RecordCount = COUNT(*) FROM [dbo].[UserPermissions];

SELECT @OrphanedRecords = COUNT(*)
FROM [dbo].[UserPermissions] up
LEFT JOIN [dbo].[DocuScanUser] dsu ON up.[UserId] = dsu.[UserId]
WHERE dsu.[UserId] IS NULL;

PRINT 'Total records in UserPermissions: ' + CAST(@RecordCount AS VARCHAR(10));

IF @OrphanedRecords > 0
BEGIN
    RAISERROR('WARNING: %d orphaned records found in UserPermissions!', 16, 1, @OrphanedRecords);
END
ELSE
BEGIN
    PRINT 'Data integrity verified: All UserPermissions records have valid user references.';
END
GO

PRINT 'Script 07_Remove_AccountName_From_UserPermissions.sql completed successfully.';
PRINT '========================================';
PRINT 'REFACTORING COMPLETE!';
PRINT '========================================';
PRINT 'Summary:';
PRINT '- Created DocuScanUser table with UserId, AccountName, UserIdentifier, LastLogon, and IsSuperUser fields';
PRINT '- Migrated user data from UserPermissions to DocuScanUser';
PRINT '- Added UserId foreign key to UserPermissions';
PRINT '- Removed AccountName column from UserPermissions';
PRINT '========================================';
GO
