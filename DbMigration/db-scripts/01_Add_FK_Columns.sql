-- =============================================
-- Script: Add Foreign Key Columns to CounterPartyRelation
-- Purpose: Add ParentCounterPartyId and ChildCounterPartyId columns
--          to replace the composite key lookup (No + Alpha)
-- =============================================

USE IkeaDocuScan;
GO

-- Add new foreign key columns (nullable initially for data migration)
ALTER TABLE CounterPartyRelation
ADD ParentCounterPartyId INT NULL;
GO

ALTER TABLE CounterPartyRelation
ADD ChildCounterPartyId INT NULL;
GO

-- Create indexes on the new columns for performance
CREATE INDEX IX_CounterPartyRelation_ParentCounterPartyId
ON CounterPartyRelation(ParentCounterPartyId);
GO

CREATE INDEX IX_CounterPartyRelation_ChildCounterPartyId
ON CounterPartyRelation(ChildCounterPartyId);
GO

PRINT 'New foreign key columns added successfully.';
GO
