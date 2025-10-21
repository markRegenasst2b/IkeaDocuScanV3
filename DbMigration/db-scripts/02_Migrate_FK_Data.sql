-- =============================================
-- Script: Migrate Data to New Foreign Key Columns
-- Purpose: Populate ParentCounterPartyId and ChildCounterPartyId
--          by looking up CounterPartyId using the composite keys
-- =============================================

USE IkeaDocuScan;
GO

-- Enable reporting of affected rows
SET NOCOUNT OFF;
GO

PRINT 'Starting data migration...';
PRINT '';

-- =============================================
-- Update ParentCounterPartyId
-- =============================================
PRINT 'Updating ParentCounterPartyId...';
GO

UPDATE cpr
SET cpr.ParentCounterPartyId = cp.CounterPartyId
FROM CounterPartyRelation cpr
INNER JOIN CounterParty cp ON
    cpr.ParentCounterPartyNo = cp.CounterPartyNo
    AND cpr.ParentCounterPartyNoAlpha = cp.CounterPartyNoAlpha
WHERE cpr.ParentCounterPartyNo IS NOT NULL
  AND cpr.ParentCounterPartyNoAlpha IS NOT NULL;
GO

PRINT 'ParentCounterPartyId updated.';
PRINT '';

-- =============================================
-- Update ChildCounterPartyId
-- =============================================
PRINT 'Updating ChildCounterPartyId...';
GO

UPDATE cpr
SET cpr.ChildCounterPartyId = cp.CounterPartyId
FROM CounterPartyRelation cpr
INNER JOIN CounterParty cp ON
    cpr.ChildCounterPartyNo = cp.CounterPartyNo
    AND cpr.ChildCounterPartyNoAlpha = cp.CounterPartyNoAlpha
WHERE cpr.ChildCounterPartyNo IS NOT NULL
  AND cpr.ChildCounterPartyNoAlpha IS NOT NULL;
GO

PRINT 'ChildCounterPartyId updated.';
PRINT '';

-- =============================================
-- Validation Report
-- =============================================
PRINT 'Migration Validation Report:';
PRINT '============================';
PRINT '';

-- Total rows
DECLARE @TotalRows INT;
SELECT @TotalRows = COUNT(*) FROM CounterPartyRelation;
PRINT 'Total CounterPartyRelation rows: ' + CAST(@TotalRows AS VARCHAR(10));

-- Parent relationships migrated
DECLARE @ParentMigrated INT;
SELECT @ParentMigrated = COUNT(*)
FROM CounterPartyRelation
WHERE ParentCounterPartyId IS NOT NULL;
PRINT 'Parent relationships migrated: ' + CAST(@ParentMigrated AS VARCHAR(10));

-- Child relationships migrated
DECLARE @ChildMigrated INT;
SELECT @ChildMigrated = COUNT(*)
FROM CounterPartyRelation
WHERE ChildCounterPartyId IS NOT NULL;
PRINT 'Child relationships migrated: ' + CAST(@ChildMigrated AS VARCHAR(10));

-- Orphaned parent relationships (no match found)
DECLARE @OrphanedParent INT;
SELECT @OrphanedParent = COUNT(*)
FROM CounterPartyRelation
WHERE ParentCounterPartyId IS NULL
  AND (ParentCounterPartyNo IS NOT NULL OR ParentCounterPartyNoAlpha IS NOT NULL);
PRINT 'Orphaned parent relationships (no match): ' + CAST(@OrphanedParent AS VARCHAR(10));

-- Orphaned child relationships (no match found)
DECLARE @OrphanedChild INT;
SELECT @OrphanedChild = COUNT(*)
FROM CounterPartyRelation
WHERE ChildCounterPartyId IS NULL
  AND (ChildCounterPartyNo IS NOT NULL OR ChildCounterPartyNoAlpha IS NOT NULL);
PRINT 'Orphaned child relationships (no match): ' + CAST(@OrphanedChild AS VARCHAR(10));

PRINT '';

-- Show orphaned records if any exist
IF @OrphanedParent > 0
BEGIN
    PRINT 'WARNING: Orphaned parent relationships found:';
    SELECT
        ID,
        ParentCounterPartyNo,
        ParentCounterPartyNoAlpha,
        'No matching CounterParty found' AS Issue
    FROM CounterPartyRelation
    WHERE ParentCounterPartyId IS NULL
      AND (ParentCounterPartyNo IS NOT NULL OR ParentCounterPartyNoAlpha IS NOT NULL);
    PRINT '';
END

IF @OrphanedChild > 0
BEGIN
    PRINT 'WARNING: Orphaned child relationships found:';
    SELECT
        ID,
        ChildCounterPartyNo,
        ChildCounterPartyNoAlpha,
        'No matching CounterParty found' AS Issue
    FROM CounterPartyRelation
    WHERE ChildCounterPartyId IS NULL
      AND (ChildCounterPartyNo IS NOT NULL OR ChildCounterPartyNoAlpha IS NOT NULL);
    PRINT '';
END

PRINT 'Data migration completed.';
GO
