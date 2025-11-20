-- =============================================
-- Script: Migrate Users to DocuScanUser Table
-- Purpose: Populate DocuScanUser table with distinct users from UserPermissions
--          and prepare UserPermissions for foreign key relationship
-- =============================================

USE PPDOCUSCAN;
GO

-- Step 1: Populate DocuScanUser with distinct users from UserPermissions
PRINT 'Migrating users from UserPermissions to DocuScanUser...';
GO

INSERT INTO [dbo].[DocuScanUser] ([AccountName], [LastLogon], [IsSuperUser], [CreatedOn])
SELECT DISTINCT
    [AccountName],
    NULL AS [LastLogon], -- No historical logon data available
    0 AS [IsSuperUser], -- Default to non-superuser; update manually if needed
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

-- Step 2: Add UserId column to UserPermissions (nullable for migration)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[UserPermissions]') AND name = 'UserId')
BEGIN
    ALTER TABLE [dbo].[UserPermissions]
    ADD [UserId] INT NULL;

    PRINT 'UserId column added to UserPermissions table.';
END
ELSE
BEGIN
    PRINT 'UserId column already exists in UserPermissions table.';
END
GO

-- Step 3: Update UserPermissions with corresponding UserId from DocuScanUser
PRINT 'Updating UserPermissions with UserId references...';
GO

UPDATE up
SET up.[UserId] = dsu.[UserId]
FROM [dbo].[UserPermissions] up
INNER JOIN [dbo].[DocuScanUser] dsu ON up.[AccountName] = dsu.[AccountName];
GO

DECLARE @UpdatedCount INT;
SELECT @UpdatedCount = COUNT(*) FROM [dbo].[UserPermissions] WHERE [UserId] IS NOT NULL;
PRINT 'UserPermissions records updated with UserId: ' + CAST(@UpdatedCount AS VARCHAR(10));
GO

-- Step 4: Verify data integrity
PRINT 'Verifying data integrity...';
GO

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

PRINT 'Script 05_Migrate_Users_To_DocuScanUser.sql completed successfully.';
GO
