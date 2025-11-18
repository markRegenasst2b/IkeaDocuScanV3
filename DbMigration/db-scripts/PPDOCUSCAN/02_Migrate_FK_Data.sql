-- =============================================
-- Script: Migrate Data to New Foreign Key Columns
-- Purpose: Populate ParentCounterPartyId and ChildCounterPartyId
--          by looking up CounterPartyId using the composite keys
-- =============================================

USE PPDOCUSCAN;
GO

-- Enable reporting of affected rows
SET NOCOUNT OFF;
GO

PRINT 'Starting data migration...';
PRINT '';

-- =============================================
-- Drop ParentCounterParty
-- =============================================
PRINT 'Dropping CounterPartyRelation';
GO

DROP TABLE CounterPartyRelation;
PRINT 'CounterPartyRelation dropped.';

PRINT 'Dropping CounterPartyRelationType';
GO
DROP TABLE CounterPartyRelationType;
PRINT 'CounterPartyRelationType dropped.';

PRINT 'Data migration completed.';
GO
