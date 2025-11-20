-- =============================================
-- Script: Finalize Foreign Key Refactoring
-- Purpose: 1. Add foreign key constraints
--          2. Drop old composite key columns
--          3. Drop old indexes
-- WARNING: Run this ONLY after verifying data migration
--          and handling any orphaned records!
-- =============================================

USE PPDOCUSCAN;
GO

PRINT 'Starting finalization of foreign key refactoring...';
PRINT '';


ALTER TABLE [dbo].[Document] DROP CONSTRAINT [FK__Document__FileId__656C112C]
GO

/****** Object:  Index [COUNTERPARTY_NO_IDX]    Script Date: 11/18/2025 1:57:19 PM ******/
DROP INDEX [COUNTERPARTY_NO_IDX] ON [dbo].[CounterParty]
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
