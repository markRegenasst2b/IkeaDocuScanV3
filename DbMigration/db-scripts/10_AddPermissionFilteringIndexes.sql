-- =============================================
-- Migration: Add Permission Filtering Performance Indexes
-- Date: 2025-11-06
-- Description: Adds indexes to optimize document permission filtering queries
-- =============================================

-- Check if running on correct database
IF DB_NAME() != 'IkeaDocuScan'
BEGIN
    RAISERROR('This script must be run on the IkeaDocuScan database', 16, 1)
    RETURN
END
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

-- =============================================
-- Verify Indexes
-- =============================================

PRINT ''
PRINT 'Verifying indexes...'

IF EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_Document_PermissionFilter'
    AND object_id = OBJECT_ID('dbo.Document')
)
    PRINT '✓ IX_Document_PermissionFilter exists'
ELSE
    PRINT '✗ IX_Document_PermissionFilter missing'

IF EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_CounterParty_Country'
    AND object_id = OBJECT_ID('dbo.CounterParty')
)
    PRINT '✓ IX_CounterParty_Country exists'
ELSE
    PRINT '✗ IX_CounterParty_Country missing'

GO

-- =============================================
-- Index Statistics
-- =============================================

PRINT ''
PRINT 'Index statistics:'
PRINT '=================='

SELECT
    OBJECT_NAME(i.object_id) AS TableName,
    i.name AS IndexName,
    i.type_desc AS IndexType,
    s.user_seeks AS UserSeeks,
    s.user_scans AS UserScans,
    s.user_lookups AS UserLookups,
    s.user_updates AS UserUpdates
FROM sys.indexes i
LEFT JOIN sys.dm_db_index_usage_stats s
    ON i.object_id = s.object_id
    AND i.index_id = s.index_id
    AND s.database_id = DB_ID()
WHERE i.name IN ('IX_Document_PermissionFilter', 'IX_CounterParty_Country')
ORDER BY TableName, IndexName

GO

PRINT ''
PRINT 'Migration completed successfully!'
PRINT 'Date: ' + CONVERT(VARCHAR, GETDATE(), 120)
GO
