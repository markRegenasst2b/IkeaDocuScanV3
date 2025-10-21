-- =============================================
-- Script: Finalize Foreign Key Refactoring
-- Purpose: 1. Add foreign key constraints
--          2. Drop old composite key columns
--          3. Drop old indexes
-- WARNING: Run this ONLY after verifying data migration
--          and handling any orphaned records!
-- =============================================

USE IkeaDocuScan;
GO

PRINT 'Starting finalization of foreign key refactoring...';
PRINT '';

-- =============================================
-- Step 1: Add Foreign Key Constraints
-- =============================================
PRINT 'Step 1: Adding foreign key constraints...';
GO

-- Add FK constraint for ParentCounterPartyId
ALTER TABLE CounterPartyRelation
ADD CONSTRAINT FK_CounterPartyRelation_Parent_CounterParty
FOREIGN KEY (ParentCounterPartyId)
REFERENCES CounterParty(CounterPartyId)
ON DELETE NO ACTION
ON UPDATE NO ACTION;
GO

PRINT 'FK constraint for ParentCounterPartyId added.';

-- Add FK constraint for ChildCounterPartyId
ALTER TABLE CounterPartyRelation
ADD CONSTRAINT FK_CounterPartyRelation_Child_CounterParty
FOREIGN KEY (ChildCounterPartyId)
REFERENCES CounterParty(CounterPartyId)
ON DELETE NO ACTION
ON UPDATE NO ACTION;
GO

PRINT 'FK constraint for ChildCounterPartyId added.';
PRINT '';

-- =============================================
-- Step 2: Drop Old Indexes
-- =============================================
PRINT 'Step 2: Dropping old indexes...';
GO

-- Drop old index on ParentCounterPartyNoAlpha
DROP INDEX IF EXISTS COUNTERPARTYRELATION_PARENT_IDX ON CounterPartyRelation;
GO

PRINT 'Dropped index: COUNTERPARTYRELATION_PARENT_IDX';

-- Drop old index on ChildCounterPartyNoAlpha
DROP INDEX IF EXISTS COUNTERPARTYRELATION_CHILD_IDX ON CounterPartyRelation;
GO

PRINT 'Dropped index: COUNTERPARTYRELATION_CHILD_IDX';
PRINT '';

-- =============================================
-- Step 3: Drop Old Columns
-- =============================================
PRINT 'Step 3: Dropping old columns...';
GO

-- Drop old parent columns
ALTER TABLE CounterPartyRelation
DROP COLUMN ParentCounterPartyNo;
GO

PRINT 'Dropped column: ParentCounterPartyNo';

ALTER TABLE CounterPartyRelation
DROP COLUMN ParentCounterPartyNoAlpha;
GO

PRINT 'Dropped column: ParentCounterPartyNoAlpha';

-- Drop old child columns
ALTER TABLE CounterPartyRelation
DROP COLUMN ChildCounterPartyNo;
GO

PRINT 'Dropped column: ChildCounterPartyNo';

ALTER TABLE CounterPartyRelation
DROP COLUMN ChildCounterPartyNoAlpha;
GO

PRINT 'Dropped column: ChildCounterPartyNoAlpha';
PRINT '';


ALTER TABLE [dbo].[Document] DROP CONSTRAINT [FK__Document__FileId__656C112C]
GO



ALTER TABLE CounterParty DROP CONSTRAINT [DF_CounterParty_CounterPartyNo]
GO
ALTER TABLE CounterParty
DROP COLUMN CounterPartyNo;
GO

PRINT 'Dropped column: CounterPartyNo';
PRINT '';

-- =============================================
-- Final Validation
-- =============================================
PRINT 'Final Validation:';
PRINT '==================';
PRINT '';

-- Show current schema
SELECT
    c.name AS ColumnName,
    t.name AS DataType,
    c.max_length AS MaxLength,
    c.is_nullable AS IsNullable
FROM sys.columns c
INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
WHERE c.object_id = OBJECT_ID('CounterPartyRelation')
ORDER BY c.column_id;

PRINT '';

-- Show foreign keys
SELECT
    fk.name AS ForeignKeyName,
    OBJECT_NAME(fk.parent_object_id) AS TableName,
    COL_NAME(fc.parent_object_id, fc.parent_column_id) AS ColumnName,
    OBJECT_NAME(fk.referenced_object_id) AS ReferencedTable,
    COL_NAME(fc.referenced_object_id, fc.referenced_column_id) AS ReferencedColumn
FROM sys.foreign_keys fk
INNER JOIN sys.foreign_key_columns fc ON fk.object_id = fc.constraint_object_id
WHERE fk.parent_object_id = OBJECT_ID('CounterPartyRelation');

PRINT '';
PRINT 'Foreign key refactoring completed successfully!';
GO
