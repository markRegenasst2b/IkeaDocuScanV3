-- =============================================
-- Script: Create IkeaDocuScan Database and User
-- Purpose: 1. Create IkeaDocuScan database
--          2. Create SQL login 'docuscanch'
--          3. Create schema 'ds' and set ownership
-- WARNING: This script will DROP the database if it exists!
-- =============================================

USE master;
GO

PRINT 'Starting database creation process...';
PRINT '';

-- =============================================
-- Step 1: Drop Database if Exists
-- =============================================
IF EXISTS (SELECT name FROM sys.databases WHERE name = 'IkeaDocuScan')
BEGIN
    PRINT 'WARNING: Database IkeaDocuScan already exists. Dropping...';

    -- Set to single user mode to force disconnect all users
    ALTER DATABASE IkeaDocuScan SET SINGLE_USER WITH ROLLBACK IMMEDIATE;

    -- Drop the database
    DROP DATABASE IkeaDocuScan;

    PRINT 'Database dropped successfully.';
    PRINT '';
END
ELSE
BEGIN
    PRINT 'Database IkeaDocuScan does not exist. Creating new database...';
    PRINT '';
END
GO

-- =============================================
-- Step 2: Create Database
-- =============================================
PRINT 'Creating database IkeaDocuScan...';
GO

CREATE DATABASE IkeaDocuScan
ON PRIMARY
(
    NAME = N'IkeaDocuScan_Data',
    FILENAME = N'D:\System\ikeaDB\IkeaDocuScan.mdf',
    SIZE = 100MB,
    MAXSIZE = UNLIMITED,
    FILEGROWTH = 10MB
)
LOG ON
(
    NAME = N'IkeaDocuScan_Log',
    FILENAME = N'D:\System\ikeaDB\IkeaDocuScan_log.ldf',
    SIZE = 50MB,
    MAXSIZE = 2GB,
    FILEGROWTH = 10MB
);
GO

PRINT 'Database created successfully.';
PRINT '';

-- =============================================
-- Step 3: Configure Database Settings
-- =============================================
PRINT 'Configuring database settings...';
GO

ALTER DATABASE IkeaDocuScan SET RECOVERY SIMPLE;
ALTER DATABASE IkeaDocuScan SET AUTO_CLOSE OFF;
ALTER DATABASE IkeaDocuScan SET AUTO_SHRINK OFF;
ALTER DATABASE IkeaDocuScan SET PAGE_VERIFY CHECKSUM;
GO

PRINT 'Database settings configured.';
PRINT '';

-- =============================================
-- Step 4: Create SQL Server Login
-- =============================================
PRINT 'Creating SQL Server login: docuscanch...';
GO

-- Drop login if exists
IF EXISTS (SELECT name FROM sys.server_principals WHERE name = 'docuscanch')
BEGIN
    DROP LOGIN docuscanch;
    PRINT 'Existing login dropped.';
END
GO

-- Create new login
CREATE LOGIN docuscanch
WITH PASSWORD = 'docuscanch25',
     DEFAULT_DATABASE = IkeaDocuScan,
     CHECK_EXPIRATION = OFF,
     CHECK_POLICY = OFF;
GO

PRINT 'Login created successfully.';
PRINT '';

-- =============================================
-- Step 5: Create Database User
-- =============================================
USE IkeaDocuScan;
GO

PRINT 'Creating database user: docuscanch...';
GO

CREATE USER docuscanch FOR LOGIN docuscanch;
GO

PRINT 'Database user created.';
PRINT '';

-- =============================================
-- Step 6: Create Schema 'ds'
-- =============================================
PRINT 'Creating schema: ds...';
GO

CREATE SCHEMA ds AUTHORIZATION docuscanch;
GO

PRINT 'Schema created with docuscanch as owner.';
PRINT '';

-- =============================================
-- Step 7: Grant Database Roles
-- =============================================
PRINT 'Granting database roles to docuscanch...';
GO

-- Grant db_owner role for full database control
ALTER ROLE db_owner ADD MEMBER docuscanch;
GO

PRINT 'Database role db_owner granted.';
PRINT '';

-- =============================================
-- Step 8: Validation
-- =============================================
PRINT 'Validation Report:';
PRINT '==================';
PRINT '';

-- Database info
SELECT
    name AS DatabaseName,
    database_id AS DatabaseId,
    create_date AS CreatedDate,
    recovery_model_desc AS RecoveryModel,
    state_desc AS State,
    compatibility_level AS CompatibilityLevel
FROM sys.databases
WHERE name = 'IkeaDocuScan';

PRINT '';

-- Login info
SELECT
    name AS LoginName,
    type_desc AS LoginType,
    create_date AS CreatedDate,
    default_database_name AS DefaultDatabase,
    is_disabled AS IsDisabled
FROM sys.server_principals
WHERE name = 'docuscanch';

PRINT '';

-- User info
SELECT
    name AS UserName,
    type_desc AS UserType,
    create_date AS CreatedDate,
    default_schema_name AS DefaultSchema
FROM sys.database_principals
WHERE name = 'docuscanch';

PRINT '';

-- Schema info
SELECT
    s.name AS SchemaName,
    dp.name AS SchemaOwner,
    s.schema_id AS SchemaId
FROM sys.schemas s
INNER JOIN sys.database_principals dp ON s.principal_id = dp.principal_id
WHERE s.name = 'ds';

PRINT '';

-- Role membership
SELECT
    r.name AS RoleName,
    m.name AS MemberName
FROM sys.database_role_members drm
INNER JOIN sys.database_principals r ON drm.role_principal_id = r.principal_id
INNER JOIN sys.database_principals m ON drm.member_principal_id = m.principal_id
WHERE m.name = 'docuscanch';

PRINT '';
PRINT '==========================================';
PRINT 'Database setup completed successfully!';
PRINT '==========================================';
PRINT '';
PRINT 'Connection Details:';
PRINT '  Server: localhost (or your SQL Server instance)';
PRINT '  Database: IkeaDocuScan';
PRINT '  Username: docuscanch';
PRINT '  Password: docuscanch25';
PRINT '  Default Schema: ds';
PRINT '';
GO
