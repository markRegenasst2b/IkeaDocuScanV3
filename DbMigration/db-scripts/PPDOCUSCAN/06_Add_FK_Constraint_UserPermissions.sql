-- =============================================
-- Script: Add Foreign Key Constraint to UserPermissions
-- Purpose: Add foreign key constraint on UserId column
--          referencing DocuScanUser table
-- =============================================

USE PPDOCUSCAN;
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


PRINT 'Starting migration: Add Permission Filtering Indexes'
PRINT 'Database: ' + DB_NAME()
PRINT 'Date: ' + CONVERT(VARCHAR, GETDATE(), 120)
GO

-- =============================================
-- Index 1: Document Permission Filter
-- Purpose: Optimize filtering by DocumentTypeId and CounterPartyId
-- Usage: Used in FilterByUserPermissions() extension method
-- =============================================

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_Document_PermissionFilter'
    AND object_id = OBJECT_ID('dbo.Document')
)
BEGIN
    PRINT 'Creating index: IX_Document_PermissionFilter on Document table...'

    CREATE NONCLUSTERED INDEX IX_Document_PermissionFilter
    ON dbo.Document (DT_ID, CounterPartyId)
    INCLUDE (BarCode, Name, FileId, CreatedOn, CreatedBy)
    WITH (
        PAD_INDEX = OFF,
        STATISTICS_NORECOMPUTE = OFF,
        SORT_IN_TEMPDB = ON,
        DROP_EXISTING = OFF,
        ONLINE = OFF,
        ALLOW_ROW_LOCKS = ON,
        ALLOW_PAGE_LOCKS = ON
    )

    PRINT 'Index IX_Document_PermissionFilter created successfully'
END
ELSE
BEGIN
    PRINT 'Index IX_Document_PermissionFilter already exists - skipping'
END
GO

-- =============================================
-- Index 2: CounterParty Country Filter
-- Purpose: Optimize country-based permission filtering via CounterParty
-- Usage: Used when filtering documents by user's allowed countries
-- =============================================

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_CounterParty_Country'
    AND object_id = OBJECT_ID('dbo.CounterParty')
)
BEGIN
    PRINT 'Creating index: IX_CounterParty_Country on CounterParty table...'

    CREATE NONCLUSTERED INDEX IX_CounterParty_Country
    ON dbo.CounterParty (Country)
    INCLUDE (CounterPartyId, Name, City)
    WITH (
        PAD_INDEX = OFF,
        STATISTICS_NORECOMPUTE = OFF,
        SORT_IN_TEMPDB = ON,
        DROP_EXISTING = OFF,
        ONLINE = OFF,
        ALLOW_ROW_LOCKS = ON,
        ALLOW_PAGE_LOCKS = ON
    )

    PRINT 'Index IX_CounterParty_Country created successfully'
END
ELSE
BEGIN
    PRINT 'Index IX_CounterParty_Country already exists - skipping'
END
GO