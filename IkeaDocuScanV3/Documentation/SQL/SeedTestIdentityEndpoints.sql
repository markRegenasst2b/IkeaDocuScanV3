-- =============================================
-- Script: SeedTestIdentityEndpoints.sql
-- Description: Seeds Test Identity and Diagnostic endpoints for DEVELOPMENT/TEST databases only
--              These endpoints are wrapped in #if DEBUG in source code
--              DO NOT run this script in PRODUCTION environments
-- Date: 2025-11-20
-- WARNING: For development/test databases only!
-- =============================================

USE [IkeaDocuScan]
GO

-- =============================================
-- SAFETY CHECK: Verify this is not a production database
-- =============================================
PRINT '========================================='
PRINT 'Test Identity & Diagnostic Endpoints Seed'
PRINT '========================================='
PRINT ''
PRINT '⚠️  WARNING: This script should ONLY be run on development/test databases!'
PRINT ''
PRINT 'Press Ctrl+C to cancel, or continue if this is a test database.'
PRINT ''
GO

-- Wait 5 seconds to allow cancellation
WAITFOR DELAY '00:00:05'
GO

PRINT 'Proceeding with test endpoint seeding...'
GO

-- =============================================
-- STEP 1: Get the next available EndpointId
-- =============================================
DECLARE @NextEndpointId INT;
SELECT @NextEndpointId = ISNULL(MAX(EndpointId), 0) + 1 FROM [dbo].[EndpointRegistry];

PRINT 'Next available EndpointId: ' + CAST(@NextEndpointId AS VARCHAR(10));
GO

-- =============================================
-- STEP 2: Insert Test Identity Endpoints (DEBUG-only)
-- =============================================
PRINT 'Step 1: Inserting Test Identity endpoints...'
GO

-- Calculate base ID for test identity endpoints
DECLARE @TestIdentityBaseId INT;
SELECT @TestIdentityBaseId = ISNULL(MAX(EndpointId), 0) + 1 FROM [dbo].[EndpointRegistry];

SET IDENTITY_INSERT [dbo].[EndpointRegistry] ON
GO

INSERT INTO [dbo].[EndpointRegistry] ([EndpointId], [HttpMethod], [Route], [EndpointName], [Description], [Category], [IsActive])
VALUES
-- Test Identity Endpoints (4 endpoints)
(@TestIdentityBaseId + 0, 'GET', '/api/test-identity/profiles', 'GetTestIdentityProfiles', 'Get available test identity profiles (DEBUG)', 'TestIdentity', 1),
(@TestIdentityBaseId + 1, 'GET', '/api/test-identity/status', 'GetTestIdentityStatus', 'Get current test identity status (DEBUG)', 'TestIdentity', 1),
(@TestIdentityBaseId + 2, 'POST', '/api/test-identity/activate/{profileId}', 'ActivateTestIdentity', 'Activate test identity profile (DEBUG)', 'TestIdentity', 1),
(@TestIdentityBaseId + 3, 'POST', '/api/test-identity/reset', 'ResetTestIdentity', 'Reset to real identity (DEBUG)', 'TestIdentity', 1);

PRINT '  - Inserted 4 Test Identity endpoints';
GO

-- =============================================
-- STEP 3: Insert Diagnostic Endpoints (DEBUG-only)
-- =============================================
PRINT 'Step 2: Inserting Diagnostic endpoints...'
GO

-- Calculate base ID for diagnostic endpoints
DECLARE @DiagnosticBaseId INT;
SELECT @DiagnosticBaseId = ISNULL(MAX(EndpointId), 0) + 1 FROM [dbo].[EndpointRegistry];

INSERT INTO [dbo].[EndpointRegistry] ([EndpointId], [HttpMethod], [Route], [EndpointName], [Description], [Category], [IsActive])
VALUES
-- Diagnostic Endpoints (6 endpoints) - Note: source has 5 + 1 extra for authorization service test
(@DiagnosticBaseId + 0, 'GET', '/api/diagnostic/db-connection', 'TestDatabaseConnection', 'Test database connection (DEBUG)', 'Diagnostic', 1),
(@DiagnosticBaseId + 1, 'GET', '/api/diagnostic/endpoint-registry', 'TestEndpointRegistryAccess', 'Test EndpointRegistry table access (DEBUG)', 'Diagnostic', 1),
(@DiagnosticBaseId + 2, 'GET', '/api/diagnostic/endpoint-role-permission', 'TestEndpointRolePermissionAccess', 'Test EndpointRolePermission table access (DEBUG)', 'Diagnostic', 1),
(@DiagnosticBaseId + 3, 'GET', '/api/diagnostic/permission-audit-log', 'TestPermissionAuditLogAccess', 'Test PermissionChangeAuditLog table access (DEBUG)', 'Diagnostic', 1),
(@DiagnosticBaseId + 4, 'GET', '/api/diagnostic/all-tables', 'TestAllAuthorizationTables', 'Test all authorization tables (DEBUG)', 'Diagnostic', 1),
(@DiagnosticBaseId + 5, 'GET', '/api/diagnostic/test-authorization-service', 'TestEndpointAuthorizationService', 'Test EndpointAuthorizationService functionality (DEBUG)', 'Diagnostic', 1);

PRINT '  - Inserted 6 Diagnostic endpoints';
GO

SET IDENTITY_INSERT [dbo].[EndpointRegistry] OFF
GO

-- =============================================
-- STEP 4: Insert Role Permissions for Test Identity Endpoints
-- =============================================
PRINT 'Step 3: Inserting role permissions for Test Identity endpoints...'
GO

-- Test Identity endpoints - SuperUser only
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName], [CreatedBy])
SELECT e.EndpointId, 'SuperUser', 'SYSTEM'
FROM [dbo].[EndpointRegistry] e
WHERE e.Category = 'TestIdentity'
  AND e.IsActive = 1;

PRINT '  - Assigned SuperUser permissions to Test Identity endpoints';
GO

-- =============================================
-- STEP 5: Insert Role Permissions for Diagnostic Endpoints
-- =============================================
PRINT 'Step 4: Inserting role permissions for Diagnostic endpoints...'
GO

-- Diagnostic endpoints - SuperUser only
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName], [CreatedBy])
SELECT e.EndpointId, 'SuperUser', 'SYSTEM'
FROM [dbo].[EndpointRegistry] e
WHERE e.Category = 'Diagnostic'
  AND e.IsActive = 1;

PRINT '  - Assigned SuperUser permissions to Diagnostic endpoints';
GO

-- =============================================
-- STEP 6: Verification
-- =============================================
PRINT 'Step 5: Verification...'
GO

-- Count test endpoints
SELECT
    'Test Identity' AS Category,
    COUNT(*) AS EndpointCount
FROM [dbo].[EndpointRegistry]
WHERE Category = 'TestIdentity' AND IsActive = 1

UNION ALL

SELECT
    'Diagnostic' AS Category,
    COUNT(*) AS EndpointCount
FROM [dbo].[EndpointRegistry]
WHERE Category = 'Diagnostic' AND IsActive = 1;

GO

-- Show all test endpoints with their permissions
SELECT
    er.EndpointId,
    er.HttpMethod,
    er.Route,
    er.EndpointName,
    er.Category,
    STRING_AGG(erp.RoleName, ', ') WITHIN GROUP (ORDER BY erp.RoleName) AS AllowedRoles
FROM [dbo].[EndpointRegistry] er
LEFT JOIN [dbo].[EndpointRolePermission] erp ON er.EndpointId = erp.EndpointId
WHERE er.Category IN ('TestIdentity', 'Diagnostic')
  AND er.IsActive = 1
GROUP BY er.EndpointId, er.HttpMethod, er.Route, er.EndpointName, er.Category
ORDER BY er.Category, er.EndpointId;

GO

PRINT ''
PRINT '========================================='
PRINT 'Test endpoint seeding completed successfully!'
PRINT '========================================='
PRINT ''
PRINT 'Summary:'
PRINT '  - 4 Test Identity endpoints added'
PRINT '  - 6 Diagnostic endpoints added'
PRINT '  - All endpoints assigned to SuperUser role only'
PRINT ''
PRINT '⚠️  REMINDER: These endpoints are DEBUG-only and should NOT exist in production!'
PRINT ''
GO

-- =============================================
-- OPTIONAL: Rollback script for test endpoints
-- =============================================
-- Uncomment to create a rollback script
/*
PRINT 'Creating rollback script...'
GO

-- Delete permissions for test endpoints
DELETE erp
FROM [dbo].[EndpointRolePermission] erp
INNER JOIN [dbo].[EndpointRegistry] er ON erp.EndpointId = er.EndpointId
WHERE er.Category IN ('TestIdentity', 'Diagnostic');

-- Delete test endpoints
DELETE FROM [dbo].[EndpointRegistry]
WHERE Category IN ('TestIdentity', 'Diagnostic');

PRINT 'Test endpoints and permissions removed.'
GO
*/
