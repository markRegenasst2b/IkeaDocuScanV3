-- =============================================
-- Script: 32_Update_EndpointAuthorization_Permissions.sql
-- Description: Updates existing Endpoint Authorization permissions to correct security policy
-- Date: 2025-01-20
-- Author: Endpoint Management Implementation
-- =============================================
-- Purpose: Fixes permissions if the old script (with all roles) was already run
--          Changes from "All roles" to "SuperUser only" (except for /check endpoint)
-- =============================================

USE [IkeaDocuScan]
GO

PRINT 'Updating Endpoint Authorization permissions to correct security policy...'
PRINT ''
PRINT 'Target Security Policy:'
PRINT '  - Management endpoints: SuperUser ONLY'
PRINT '  - Check endpoint (/check): ALL ROLES (for menu visibility)'
PRINT ''

-- =============================================
-- STEP 1: Remove all role permissions from management endpoints
-- =============================================

PRINT 'Step 1: Removing old role permissions from management endpoints...'

-- Delete all role permissions for management endpoints (not /check)
DELETE erp
FROM EndpointRolePermission erp
INNER JOIN EndpointRegistry er ON erp.EndpointId = er.EndpointId
WHERE er.Category = 'Endpoint Authorization'
    AND er.Route <> '/api/endpoint-authorization/check'  -- Keep /check as-is
    AND erp.RoleName IN ('Reader', 'Publisher', 'ADAdmin');  -- Remove non-SuperUser roles

PRINT 'Removed non-SuperUser roles from management endpoints'

-- =============================================
-- STEP 2: Ensure SuperUser role exists on all management endpoints
-- =============================================

PRINT ''
PRINT 'Step 2: Ensuring SuperUser role on all management endpoints...'

-- List of management endpoints that should be SuperUser only
DECLARE @Endpoints TABLE (
    Route VARCHAR(500)
);

INSERT INTO @Endpoints VALUES ('/api/endpoint-authorization/endpoints');
INSERT INTO @Endpoints VALUES ('/api/endpoint-authorization/endpoints/{id}');
INSERT INTO @Endpoints VALUES ('/api/endpoint-authorization/endpoints/{id}/roles');
INSERT INTO @Endpoints VALUES ('/api/endpoint-authorization/roles');
INSERT INTO @Endpoints VALUES ('/api/endpoint-authorization/audit');
INSERT INTO @Endpoints VALUES ('/api/endpoint-authorization/cache/invalidate');
INSERT INTO @Endpoints VALUES ('/api/endpoint-authorization/sync');
INSERT INTO @Endpoints VALUES ('/api/endpoint-authorization/validate');

-- Add SuperUser role if not exists
INSERT INTO EndpointRolePermission (EndpointId, RoleName)
SELECT
    er.EndpointId,
    'SuperUser'
FROM EndpointRegistry er
INNER JOIN @Endpoints e ON er.Route = e.Route
WHERE er.Category = 'Endpoint Authorization'
    AND NOT EXISTS (
        SELECT 1
        FROM EndpointRolePermission erp
        WHERE erp.EndpointId = er.EndpointId
            AND erp.RoleName = 'SuperUser'
    );

PRINT 'Added SuperUser role to management endpoints (if missing)'

-- =============================================
-- STEP 3: Ensure /check endpoint has all roles
-- =============================================

PRINT ''
PRINT 'Step 3: Ensuring ALL ROLES on /check endpoint...'

DECLARE @CheckEndpointId INT;

SELECT @CheckEndpointId = EndpointId
FROM EndpointRegistry
WHERE HttpMethod = 'GET'
    AND Route = '/api/endpoint-authorization/check';

IF @CheckEndpointId IS NOT NULL
BEGIN
    -- Add Reader if not exists
    IF NOT EXISTS (SELECT 1 FROM EndpointRolePermission WHERE EndpointId = @CheckEndpointId AND RoleName = 'Reader')
    BEGIN
        INSERT INTO EndpointRolePermission (EndpointId, RoleName) VALUES (@CheckEndpointId, 'Reader');
        PRINT '  Added Reader role to /check endpoint'
    END

    -- Add Publisher if not exists
    IF NOT EXISTS (SELECT 1 FROM EndpointRolePermission WHERE EndpointId = @CheckEndpointId AND RoleName = 'Publisher')
    BEGIN
        INSERT INTO EndpointRolePermission (EndpointId, RoleName) VALUES (@CheckEndpointId, 'Publisher');
        PRINT '  Added Publisher role to /check endpoint'
    END

    -- Add ADAdmin if not exists
    IF NOT EXISTS (SELECT 1 FROM EndpointRolePermission WHERE EndpointId = @CheckEndpointId AND RoleName = 'ADAdmin')
    BEGIN
        INSERT INTO EndpointRolePermission (EndpointId, RoleName) VALUES (@CheckEndpointId, 'ADAdmin');
        PRINT '  Added ADAdmin role to /check endpoint'
    END

    -- Add SuperUser if not exists
    IF NOT EXISTS (SELECT 1 FROM EndpointRolePermission WHERE EndpointId = @CheckEndpointId AND RoleName = 'SuperUser')
    BEGIN
        INSERT INTO EndpointRolePermission (EndpointId, RoleName) VALUES (@CheckEndpointId, 'SuperUser');
        PRINT '  Added SuperUser role to /check endpoint'
    END

    PRINT '/check endpoint now has all roles'
END
ELSE
BEGIN
    PRINT 'WARNING: /check endpoint not found in database!'
END

PRINT ''
PRINT '========================================'
PRINT 'Permission Update Complete'
PRINT '========================================'
PRINT ''

-- =============================================
-- VERIFICATION: Show current permissions
-- =============================================

PRINT 'Current Endpoint Authorization permissions:'
PRINT ''

SELECT
    er.HttpMethod,
    er.Route,
    er.EndpointName,
    STRING_AGG(erp.RoleName, ', ') WITHIN GROUP (ORDER BY erp.RoleName) AS AllowedRoles,
    CASE
        WHEN er.Route = '/api/endpoint-authorization/check' THEN 'All Roles (Correct)'
        WHEN STRING_AGG(erp.RoleName, ', ') = 'SuperUser' THEN 'SuperUser Only (Correct)'
        ELSE 'INCORRECT - Review!'
    END AS SecurityStatus
FROM EndpointRegistry er
LEFT JOIN EndpointRolePermission erp ON er.EndpointId = erp.EndpointId
WHERE er.Category = 'Endpoint Authorization'
    AND er.IsActive = 1
GROUP BY er.EndpointId, er.HttpMethod, er.Route, er.EndpointName
ORDER BY er.Route;

PRINT ''
PRINT 'Verify that:'
PRINT '  1. /check endpoint has all roles (Reader, Publisher, ADAdmin, SuperUser)'
PRINT '  2. All other endpoints have SuperUser ONLY'
PRINT ''

GO
