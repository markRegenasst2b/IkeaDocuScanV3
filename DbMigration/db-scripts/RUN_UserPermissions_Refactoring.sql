-- =============================================
-- Master Script: User Permissions Refactoring
-- Purpose: Execute all scripts to refactor UserPermissions table
--          by creating a separate DocuScanUser table
-- =============================================
--
-- WHAT THIS REFACTORING DOES:
-- 1. Creates a new DocuScanUser table with:
--    - UserId (Primary Key)
--    - AccountName
--    - UserIdentifier
--    - LastLogon (timestamp)
--    - IsSuperUser (flag)
--
-- 2. Migrates existing user data from UserPermissions to DocuScanUser
--
-- 3. Updates UserPermissions to use UserId foreign key instead of AccountName
--
-- 4. Removes AccountName column from UserPermissions
--
-- =============================================
-- PREREQUISITES:
-- - Database backup recommended before running
-- - Ensure no active transactions on UserPermissions table
-- =============================================

USE IkeaDocuScan;
GO

PRINT '========================================';
PRINT 'STARTING USER PERMISSIONS REFACTORING';
PRINT '========================================';
PRINT 'Start Time: ' + CONVERT(VARCHAR(20), GETDATE(), 120);
GO

-- Set error handling
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

-- =============================================
-- SCRIPT 1: Create DocuScanUser Table
-- =============================================
PRINT '';
PRINT '----------------------------------------';
PRINT 'STEP 1: Creating DocuScanUser Table';
PRINT '----------------------------------------';
GO

-- Create DocuScanUser table
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE ID = object_id(N'[dbo].[DocuScanUser]')
	AND OBJECTPROPERTY(ID, N'IsTable') = 1)
BEGIN
    CREATE TABLE [dbo].[DocuScanUser]
    (
        [UserId] INT IDENTITY NOT NULL CONSTRAINT DOCUSCANUSER_PK PRIMARY KEY,
        [AccountName] VARCHAR(255) NOT NULL,
        [UserIdentifier] VARCHAR(255) NOT NULL,
        [LastLogon] DATETIME NULL,
        [IsSuperUser] BIT NOT NULL CONSTRAINT DF_DocuScanUser_IsSuperUser DEFAULT(0),
        [CreatedOn] DATETIME NOT NULL CONSTRAINT DF_DocuScanUser_CreatedOn DEFAULT(GETDATE()),
        [ModifiedOn] DATETIME NULL,

        CONSTRAINT UK_DocuScanUser_AccountName UNIQUE ([AccountName]),
        CONSTRAINT UK_DocuScanUser_UserIdentifier UNIQUE ([UserIdentifier])
    );
    PRINT 'DocuScanUser table created successfully.';
END
ELSE
BEGIN
    PRINT 'DocuScanUser table already exists.';
END
GO

-- Create indexes
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_DocuScanUser_LastLogon')
    CREATE INDEX IX_DocuScanUser_LastLogon ON [dbo].[DocuScanUser] ([LastLogon]);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_DocuScanUser_IsSuperUser')
    CREATE INDEX IX_DocuScanUser_IsSuperUser ON [dbo].[DocuScanUser] ([IsSuperUser]);
GO

PRINT 'Step 1 completed successfully.';
GO

-- =============================================
-- SCRIPT 2: Migrate Users to DocuScanUser
-- =============================================
PRINT '';
PRINT '----------------------------------------';
PRINT 'STEP 2: Migrating Users to DocuScanUser';
PRINT '----------------------------------------';
GO

-- Populate DocuScanUser with distinct users
INSERT INTO [dbo].[DocuScanUser] ([AccountName], [UserIdentifier], [LastLogon], [IsSuperUser], [CreatedOn])
SELECT DISTINCT
    [AccountName],
    [AccountName] AS [UserIdentifier],
    NULL AS [LastLogon],
    0 AS [IsSuperUser],
    GETDATE() AS [CreatedOn]
FROM [dbo].[UserPermissions]
WHERE [AccountName] IS NOT NULL
    AND [AccountName] NOT IN (SELECT [AccountName] FROM [dbo].[DocuScanUser])
ORDER BY [AccountName];
GO

DECLARE @UserCount INT;
SELECT @UserCount = COUNT(*) FROM [dbo].[DocuScanUser];
PRINT 'Total users in DocuScanUser table: ' + CAST(@UserCount AS VARCHAR(10));
GO

-- Add UserId column to UserPermissions
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[UserPermissions]') AND name = 'UserId')
BEGIN
    ALTER TABLE [dbo].[UserPermissions]
    ADD [UserId] INT NULL;
    PRINT 'UserId column added to UserPermissions table.';
END
GO

-- Update UserPermissions with UserId
UPDATE up
SET up.[UserId] = dsu.[UserId]
FROM [dbo].[UserPermissions] up
INNER JOIN [dbo].[DocuScanUser] dsu ON up.[AccountName] = dsu.[AccountName];
GO

DECLARE @UpdatedCount INT;
SELECT @UpdatedCount = COUNT(*) FROM [dbo].[UserPermissions] WHERE [UserId] IS NOT NULL;
PRINT 'UserPermissions records updated with UserId: ' + CAST(@UpdatedCount AS VARCHAR(10));
GO

-- Verify data integrity
DECLARE @UnmatchedCount INT;
SELECT @UnmatchedCount = COUNT(*)
FROM [dbo].[UserPermissions]
WHERE [UserId] IS NULL AND [AccountName] IS NOT NULL;

IF @UnmatchedCount > 0
BEGIN
    PRINT 'WARNING: ' + CAST(@UnmatchedCount AS VARCHAR(10)) + ' UserPermissions records could not be matched to a user!';
    SELECT [Id], [AccountName]
    FROM [dbo].[UserPermissions]
    WHERE [UserId] IS NULL AND [AccountName] IS NOT NULL;
END
ELSE
BEGIN
    PRINT 'Data integrity verified: All UserPermissions records have valid UserId references.';
END
GO

PRINT 'Step 2 completed successfully.';
GO

-- =============================================
-- SCRIPT 3: Add Foreign Key Constraint
-- =============================================
PRINT '';
PRINT '----------------------------------------';
PRINT 'STEP 3: Adding Foreign Key Constraint';
PRINT '----------------------------------------';
GO

-- Verify all records have a UserId
DECLARE @NullCount INT;
SELECT @NullCount = COUNT(*) FROM [dbo].[UserPermissions] WHERE [UserId] IS NULL;

IF @NullCount > 0
BEGIN
    RAISERROR('Cannot proceed: %d UserPermissions records have NULL UserId.', 16, 1, @NullCount);
END
ELSE
BEGIN
    -- Make UserId NOT NULL
    ALTER TABLE [dbo].[UserPermissions]
    ALTER COLUMN [UserId] INT NOT NULL;
    PRINT 'UserId column set to NOT NULL successfully.';
END
GO

-- Add foreign key constraint
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_UserPermissions_DocuScanUser')
BEGIN
    ALTER TABLE [dbo].[UserPermissions]
    ADD CONSTRAINT FK_UserPermissions_DocuScanUser
        FOREIGN KEY ([UserId])
        REFERENCES [dbo].[DocuScanUser]([UserId])
        ON DELETE CASCADE
        ON UPDATE CASCADE;
    PRINT 'Foreign key constraint FK_UserPermissions_DocuScanUser added successfully.';
END
GO

-- Create index on UserId
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_UserPermissions_UserId')
    CREATE INDEX IX_UserPermissions_UserId ON [dbo].[UserPermissions] ([UserId]);
GO

PRINT 'Step 3 completed successfully.';
GO

-- =============================================
-- SCRIPT 4: Remove AccountName Column
-- =============================================
PRINT '';
PRINT '----------------------------------------';
PRINT 'STEP 4: Removing AccountName Column';
PRINT '----------------------------------------';
GO

-- Verify foreign key constraint exists
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_UserPermissions_DocuScanUser')
BEGIN
    RAISERROR('Cannot proceed: Foreign key constraint FK_UserPermissions_DocuScanUser does not exist.', 16, 1);
END
GO

-- Drop AccountName column
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[UserPermissions]') AND name = 'AccountName')
BEGIN
    ALTER TABLE [dbo].[UserPermissions]
    DROP COLUMN [AccountName];
    PRINT 'AccountName column removed successfully from UserPermissions table.';
END
GO

PRINT 'Step 4 completed successfully.';
GO

-- =============================================
-- FINAL VERIFICATION
-- =============================================
PRINT '';
PRINT '----------------------------------------';
PRINT 'FINAL VERIFICATION';
PRINT '----------------------------------------';
GO

-- Verify table structures
PRINT 'DocuScanUser Table Structure:';
SELECT
    c.name AS ColumnName,
    t.name AS DataType,
    c.max_length AS MaxLength,
    c.is_nullable AS IsNullable
FROM sys.columns c
INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
WHERE c.object_id = OBJECT_ID(N'[dbo].[DocuScanUser]')
ORDER BY c.column_id;
GO

PRINT '';
PRINT 'UserPermissions Table Structure:';
SELECT
    c.name AS ColumnName,
    t.name AS DataType,
    c.max_length AS MaxLength,
    c.is_nullable AS IsNullable
FROM sys.columns c
INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
WHERE c.object_id = OBJECT_ID(N'[dbo].[UserPermissions]')
ORDER BY c.column_id;
GO

-- Verify data integrity
DECLARE @RecordCount INT;
DECLARE @OrphanedRecords INT;

SELECT @RecordCount = COUNT(*) FROM [dbo].[UserPermissions];
SELECT @OrphanedRecords = COUNT(*)
FROM [dbo].[UserPermissions] up
LEFT JOIN [dbo].[DocuScanUser] dsu ON up.[UserId] = dsu.[UserId]
WHERE dsu.[UserId] IS NULL;

PRINT '';
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

PRINT '';
PRINT '========================================';
PRINT 'REFACTORING COMPLETED SUCCESSFULLY!';
PRINT '========================================';
PRINT 'Summary:';
PRINT '- Created DocuScanUser table with UserId, AccountName, UserIdentifier, LastLogon, and IsSuperUser fields';
PRINT '- Migrated user data from UserPermissions to DocuScanUser';
PRINT '- Added UserId foreign key to UserPermissions';
PRINT '- Removed AccountName column from UserPermissions';
PRINT '';
PRINT 'End Time: ' + CONVERT(VARCHAR(20), GETDATE(), 120);
PRINT '========================================';
GO
