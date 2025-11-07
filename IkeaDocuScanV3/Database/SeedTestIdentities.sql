/*
=========================================================================
IkeaDocuScan - Test Identity Seeding Script
=========================================================================
⚠️ WARNING: THIS SCRIPT IS FOR DEVELOPMENT/TESTING ENVIRONMENTS ONLY ⚠️

Purpose: Seeds test user identities and permissions for testing different
         authentication and authorization scenarios.

Usage:   Run this script ONCE manually in your development database.
         Do NOT run in production environments.

Author:  Generated for IkeaDocuScan Development
Date:    2025-01-07
=========================================================================
*/

USE [IkeaDocuScan];  -- Change to your database name if different
GO

-- Check if we're in a production environment (basic check)
DECLARE @Environment NVARCHAR(50);
SELECT @Environment = [Value]
FROM [dbo].[SystemConfiguration]
WHERE [Key] = 'Environment';

IF @Environment = 'Production'
BEGIN
    RAISERROR('⚠️ CANNOT SEED TEST DATA IN PRODUCTION ENVIRONMENT!', 16, 1);
    RETURN;
END
GO

PRINT '=========================================================================';
PRINT 'Starting Test Identity Seeding...';
PRINT '=========================================================================';
PRINT '';

-- Disable constraints temporarily for easier insertion
ALTER TABLE [dbo].[UserPermissions] NOCHECK CONSTRAINT ALL;
GO

/*
=========================================================================
1. SEED TEST USERS IN DocuScanUsers TABLE
=========================================================================
*/

PRINT 'Seeding DocuScanUsers...';
GO

-- Check if test users already exist and delete them first (for re-running script)
DELETE FROM [dbo].[UserPermissions] WHERE UserId IN (1001, 1002, 1003, 1004, 1005, 1006);
DELETE FROM [dbo].[DocuScanUsers] WHERE UserId IN (1001, 1002, 1003, 1004, 1005, 1006);
GO

-- Enable IDENTITY_INSERT to use specific UserIds
SET IDENTITY_INSERT [dbo].[DocuScanUsers] ON;
GO

-- 1. SuperUser (Database Flag)
INSERT INTO [dbo].[DocuScanUsers] (
    UserId, AccountName, IsSuperUser, CreatedOn, LastLogon
)
VALUES (
    1001,
    'TEST\SuperUserTest',
    1, -- IsSuperUser = true
    GETDATE(),
    NULL
);

-- 2. SuperUser (AD Group - will be granted via AD group membership, not DB flag)
INSERT INTO [dbo].[DocuScanUsers] (
    UserId, AccountName, IsSuperUser, CreatedOn, LastLogon
)
VALUES (
    1002,
    'TEST\SuperUserAD',
    0, -- Will get SuperUser via AD group
    GETDATE(),
    NULL
);

-- 3. Publisher (Has database permissions + AD groups)
INSERT INTO [dbo].[DocuScanUsers] (
    UserId, AccountName, IsSuperUser, CreatedOn, LastLogon
)
VALUES (
    1003,
    'TEST\PublisherTest',
    0,
    GETDATE(),
    NULL
);

-- 4. Reader (Has limited database permissions + AD groups)
INSERT INTO [dbo].[DocuScanUsers] (
    UserId, AccountName, IsSuperUser, CreatedOn, LastLogon
)
VALUES (
    1004,
    'TEST\ReaderTest',
    0,
    GETDATE(),
    NULL
);

-- 5. Database Permissions Only (No AD groups)
INSERT INTO [dbo].[DocuScanUsers] (
    UserId, AccountName, IsSuperUser, CreatedOn, LastLogon
)
VALUES (
    1005,
    'TEST\DatabaseOnlyTest',
    0,
    GETDATE(),
    NULL
);

-- 6. No Access (User exists but has no permissions)
INSERT INTO [dbo].[DocuScanUsers] (
    UserId, AccountName, IsSuperUser, CreatedOn, LastLogon
)
VALUES (
    1006,
    'TEST\NoAccessTest',
    0,
    GETDATE(),
    NULL
);

SET IDENTITY_INSERT [dbo].[DocuScanUsers] OFF;
GO

PRINT 'DocuScanUsers seeded successfully.';
PRINT '';

/*
=========================================================================
2. SEED USER PERMISSIONS
=========================================================================
Note: SuperUser (UserId 1001) doesn't need explicit permissions -
      SuperUser flag grants access to everything.

      SuperUserAD (UserId 1002) also doesn't need DB permissions -
      will get access via AD group membership.
*/

PRINT 'Seeding UserPermissions...';
GO

-- Get sample DocumentTypeIds, CounterPartyIds, and CountryCodes from existing data
-- If your database doesn't have these yet, you'll need to seed them first
DECLARE @DocType1 INT, @DocType2 INT, @DocType3 INT;
DECLARE @CounterParty1 INT, @CounterParty2 INT;
DECLARE @Country1 NVARCHAR(2), @Country2 NVARCHAR(2);

-- Get some document types (or use NULL for "all")
SELECT TOP 1 @DocType1 = Id FROM [dbo].[DocumentTypes] ORDER BY Id;
SELECT @DocType2 = Id FROM [dbo].[DocumentTypes] WHERE Id != ISNULL(@DocType1, 0) ORDER BY Id;
SELECT @DocType3 = Id FROM [dbo].[DocumentTypes] WHERE Id NOT IN (ISNULL(@DocType1, 0), ISNULL(@DocType2, 0)) ORDER BY Id;

-- Get some counter parties
SELECT TOP 1 @CounterParty1 = Id FROM [dbo].[CounterParties] ORDER BY Id;
SELECT @CounterParty2 = Id FROM [dbo].[CounterParties] WHERE Id != ISNULL(@CounterParty1, 0) ORDER BY Id;

-- Get some countries
SELECT TOP 1 @Country1 = CountryCode FROM [dbo].[Countries] ORDER BY CountryCode;
SELECT @Country2 = CountryCode FROM [dbo].[Countries] WHERE CountryCode != ISNULL(@Country1, '') ORDER BY CountryCode;

-- Publisher Test User (1003) - Has broad permissions
-- Permission 1: Access to specific document type and counter party
IF @DocType1 IS NOT NULL AND @CounterParty1 IS NOT NULL
BEGIN
    INSERT INTO [dbo].[UserPermissions] (UserId, DocumentTypeId, CounterPartyId, CountryCode, CreatedOn)
    VALUES (1003, @DocType1, @CounterParty1, NULL, GETDATE());
END

-- Permission 2: Access to specific document type and country
IF @DocType2 IS NOT NULL AND @Country1 IS NOT NULL
BEGIN
    INSERT INTO [dbo].[UserPermissions] (UserId, DocumentTypeId, CounterPartyId, CountryCode, CreatedOn)
    VALUES (1003, @DocType2, NULL, @Country1, GETDATE());
END

-- Permission 3: Full access to one document type (all counter parties, all countries)
IF @DocType3 IS NOT NULL
BEGIN
    INSERT INTO [dbo].[UserPermissions] (UserId, DocumentTypeId, CounterPartyId, CountryCode, CreatedOn)
    VALUES (1003, @DocType3, NULL, NULL, GETDATE());
END

-- Reader Test User (1004) - Has limited permissions
-- Permission 1: Read access to specific document type only
IF @DocType1 IS NOT NULL
BEGIN
    INSERT INTO [dbo].[UserPermissions] (UserId, DocumentTypeId, CounterPartyId, CountryCode, CreatedOn)
    VALUES (1004, @DocType1, NULL, NULL, GETDATE());
END

-- Database Only Test User (1005) - Has some permissions but no AD groups
-- Permission 1: Access to specific counter party only
IF @CounterParty1 IS NOT NULL
BEGIN
    INSERT INTO [dbo].[UserPermissions] (UserId, DocumentTypeId, CounterPartyId, CountryCode, CreatedOn)
    VALUES (1005, NULL, @CounterParty1, NULL, GETDATE());
END

-- Permission 2: Access to specific country only
IF @Country1 IS NOT NULL
BEGIN
    INSERT INTO [dbo].[UserPermissions] (UserId, DocumentTypeId, CounterPartyId, CountryCode, CreatedOn)
    VALUES (1005, NULL, NULL, @Country1, GETDATE());
END

-- NoAccessTest (1006) - Intentionally has NO permissions

PRINT 'UserPermissions seeded successfully.';
PRINT '';

-- Re-enable constraints
ALTER TABLE [dbo].[UserPermissions] CHECK CONSTRAINT ALL;
GO

/*
=========================================================================
3. VERIFICATION
=========================================================================
*/

PRINT '=========================================================================';
PRINT 'Verification Results:';
PRINT '=========================================================================';
PRINT '';

-- Show created users
PRINT 'Test Users Created:';
SELECT
    UserId,
    AccountName,
    IsSuperUser,
    CreatedOn,
    (SELECT COUNT(*) FROM [dbo].[UserPermissions] WHERE UserId = u.UserId) AS PermissionCount
FROM [dbo].[DocuScanUsers] u
WHERE UserId IN (1001, 1002, 1003, 1004, 1005, 1006)
ORDER BY UserId;

PRINT '';
PRINT 'Test User Permissions:';
SELECT
    u.AccountName,
    up.DocumentTypeId,
    up.CounterPartyId,
    up.CountryCode,
    up.CreatedOn
FROM [dbo].[UserPermissions] up
INNER JOIN [dbo].[DocuScanUsers] u ON up.UserId = u.UserId
WHERE u.UserId IN (1001, 1002, 1003, 1004, 1005, 1006)
ORDER BY u.UserId, up.Id;

PRINT '';
PRINT '=========================================================================';
PRINT '✅ Test Identity Seeding Complete!';
PRINT '=========================================================================';
PRINT '';
PRINT 'Test Users Summary:';
PRINT '-------------------';
PRINT '1001 - SuperUserTest        (Database SuperUser flag)';
PRINT '1002 - SuperUserAD          (AD SuperUser group member)';
PRINT '1003 - PublisherTest        (AD Publisher + Reader groups + DB permissions)';
PRINT '1004 - ReaderTest           (AD Reader group + limited DB permissions)';
PRINT '1005 - DatabaseOnlyTest     (DB permissions only, no AD groups)';
PRINT '1006 - NoAccessTest         (No permissions at all)';
PRINT '';
PRINT 'Next Steps:';
PRINT '-----------';
PRINT '1. Run your application in DEBUG mode';
PRINT '2. Navigate to the Home page';
PRINT '3. Use the "Developer Tools - Test Identity Switcher" panel';
PRINT '4. Select different test identities to verify authorization behavior';
PRINT '';
PRINT '⚠️ REMEMBER: These are TEST identities for DEVELOPMENT ONLY!';
PRINT '=========================================================================';
GO
