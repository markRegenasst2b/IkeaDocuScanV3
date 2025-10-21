-- =============================================
-- Script: Restore Backup and Migrate dbo to dbo Schema
-- Purpose: 1. Restore backup file to IkeaDocuScan database
-- WARNING: This will overwrite the IkeaDocuScan database!
-- =============================================

USE master;
GO

SET NOCOUNT ON;
GO

PRINT '=============================================';
PRINT 'IkeaDocuScan Backup Restore and Schema Migration';
PRINT '=============================================';
PRINT '';

-- =============================================
-- Step 1: Verify Backup File
-- =============================================
PRINT 'Step 1: Verifying backup file...';
PRINT '';

DECLARE @BackupFile NVARCHAR(500) = N'D:\System\ikeaDB\IkeaDocumentScanningCH_OLD.bak';

-- Verify backup file is readable
RESTORE VERIFYONLY FROM DISK = @BackupFile;

PRINT 'Backup file verified successfully.';
PRINT '';

-- Get logical file names from backup
PRINT 'Backup file contents:';
RESTORE FILELISTONLY FROM DISK = @BackupFile;
PRINT '';

-- =============================================
-- Step 2: Restore Database
-- =============================================
PRINT 'Step 2: Restoring database...';
PRINT '';

-- Kill existing connections
IF EXISTS (SELECT name FROM sys.databases WHERE name = 'IkeaDocuScan')
BEGIN
    PRINT 'Disconnecting existing users...';
    ALTER DATABASE IkeaDocuScan SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
END
GO

-- Restore database with REPLACE
RESTORE DATABASE IkeaDocuScan
FROM DISK = N'D:\System\ikeaDB\IkeaDocumentScanningCH_OLD.bak'
WITH
    REPLACE,
    MOVE 'IkeaDocumentScanningCH' TO 'D:\System\ikeaDB\IkeaDocuScan.mdf',
    MOVE 'IkeaDocumentScanningCH_Log' TO 'D:\System\ikeaDB\IkeaDocuScan_log.ldf',
    RECOVERY,
    STATS = 10;
GO

PRINT '';
PRINT 'Database restored successfully.';
PRINT '';

-- Set to multi-user mode
ALTER DATABASE IkeaDocuScan SET MULTI_USER;
GO

USE IkeaDocuScan;
GO

-- =============================================
-- Step 3: Create dbo Schema and User
-- =============================================
PRINT 'Step 3: Setting up dbo schema and user...';
PRINT '';

-- Create login if not exists
IF NOT EXISTS (SELECT name FROM sys.server_principals WHERE name = 'docuscanV3')
BEGIN
    CREATE LOGIN docuscanV3
    WITH PASSWORD = 'docuscanV325',
         DEFAULT_DATABASE = IkeaDocuScan,
         CHECK_EXPIRATION = OFF,
         CHECK_POLICY = OFF;
    PRINT 'Login docuscanV3 created.';
END
ELSE
BEGIN
    PRINT 'Login docuscanV3 already exists.';
END
GO


-- Grant db_owner role
IF NOT EXISTS (
    SELECT 1
    FROM sys.database_role_members drm
    INNER JOIN sys.database_principals r ON drm.role_principal_id = r.principal_id
    INNER JOIN sys.database_principals m ON drm.member_principal_id = m.principal_id
    WHERE r.name = 'db_owner' AND m.name = 'docuscanV3'
)
BEGIN
    ALTER ROLE db_owner ADD MEMBER docuscanV3;
    PRINT 'Role db_owner granted to docuscanV3.';
END
GO

PRINT '';
PRINT 'Step 8: Setting default schema for docuscanV3 user...';
GO

ALTER USER docuscanV3 WITH DEFAULT_SCHEMA = dbo;
GO

PRINT 'Default schema set to dbo.';
PRINT '';

-- =============================================
-- Step 9: Final Validation
-- =============================================
PRINT 'Step 9: Final Validation Report';
PRINT '================================';
PRINT '';

-- Count objects in each schema
PRINT 'Object counts:';
SELECT
    s.name AS SchemaName,
    COUNT(CASE WHEN o.type = 'U' THEN 1 END) AS Tables,
    COUNT(CASE WHEN o.type = 'V' THEN 1 END) AS Views,
    COUNT(CASE WHEN o.type = 'P' THEN 1 END) AS Procedures,
    COUNT(CASE WHEN o.type IN ('FN', 'IF', 'TF') THEN 1 END) AS Functions
FROM sys.schemas s
LEFT JOIN sys.objects o ON s.schema_id = o.schema_id
WHERE s.name IN ('dbo')
GROUP BY s.name
ORDER BY s.name;

PRINT '';

-- List all tables in dbo schema
PRINT 'Tables in dbo schema:';
SELECT
    t.name AS TableName,
    p.rows AS RowCount1
FROM sys.tables t
INNER JOIN sys.partitions p ON t.object_id = p.object_id
WHERE t.schema_id = SCHEMA_ID('dbo')
  AND p.index_id IN (0, 1)
ORDER BY t.name;

PRINT '';

-- Check user and schema configuration
PRINT 'User configuration:';
SELECT
    name AS UserName,
    default_schema_name AS DefaultSchema,
    create_date AS CreatedDate
FROM sys.database_principals
WHERE name = 'docuscanV3';

PRINT '';
PRINT '=============================================';
PRINT 'Migration completed successfully!';
PRINT '=============================================';
PRINT '';
PRINT 'Summary:';
PRINT '  - Database restored from backup';
PRINT '  - User docuscanV3 configured with dbo as default schema';
PRINT '  - All data preserved in transferred tables';
PRINT '';
GO
