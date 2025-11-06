-- =============================================
-- Script: Add Foreign Key Constraint to UserPermissions
-- Purpose: Add foreign key constraint on UserId column
--          referencing DocuScanUser table
-- =============================================

USE IkeaDocuScan;
GO

-- Step 1: Make UserId column NOT NULL (after data has been migrated)
PRINT 'Making UserId column NOT NULL...';
GO

-- First, verify all records have a UserId
DECLARE @NullCount INT;
SELECT @NullCount = COUNT(*) FROM [dbo].[UserPermissions] WHERE [UserId] IS NULL;

IF @NullCount > 0
BEGIN
    RAISERROR('Cannot proceed: %d UserPermissions records have NULL UserId. Run migration script first.', 16, 1, @NullCount);
END
ELSE
BEGIN
    -- Alter column to NOT NULL
    ALTER TABLE [dbo].[UserPermissions]
    ALTER COLUMN [UserId] INT NOT NULL;

    PRINT 'UserId column set to NOT NULL successfully.';
END
GO

-- Step 2: Add foreign key constraint
PRINT 'Adding foreign key constraint to DocuScanUser...';
GO

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
ELSE
BEGIN
    PRINT 'Foreign key constraint FK_UserPermissions_DocuScanUser already exists.';
END
GO

-- Step 3: Create index on UserId for performance
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_UserPermissions_UserId')
BEGIN
    CREATE INDEX IX_UserPermissions_UserId
    ON [dbo].[UserPermissions] ([UserId]);

    PRINT 'Index IX_UserPermissions_UserId created successfully.';
END
ELSE
BEGIN
    PRINT 'Index IX_UserPermissions_UserId already exists.';
END
GO

-- Step 4: Verify the constraint
PRINT 'Verifying foreign key constraint...';
GO

DECLARE @ConstraintCount INT;
SELECT @ConstraintCount = COUNT(*)
FROM sys.foreign_keys
WHERE name = 'FK_UserPermissions_DocuScanUser'
    AND parent_object_id = OBJECT_ID(N'[dbo].[UserPermissions]')
    AND referenced_object_id = OBJECT_ID(N'[dbo].[DocuScanUser]');

IF @ConstraintCount = 1
BEGIN
    PRINT 'Foreign key constraint verified successfully.';
END
ELSE
BEGIN
    RAISERROR('Foreign key constraint verification failed!', 16, 1);
END
GO

PRINT 'Script 06_Add_FK_Constraint_UserPermissions.sql completed successfully.';
GO


-- Composite index for permission matching on Document table
CREATE NONCLUSTERED INDEX IX_Document_PermissionFilter
ON dbo.Document (DT_ID, CounterPartyId)
INCLUDE (BarCode, Name);
GO

-- Index on CounterParty.Country for permission filtering
CREATE NONCLUSTERED INDEX IX_CounterParty_Country
ON dbo.CounterParty (Country)
INCLUDE (CounterPartyId);
GO