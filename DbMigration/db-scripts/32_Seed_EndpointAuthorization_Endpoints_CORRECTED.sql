-- =============================================
-- Script: 32_Seed_EndpointAuthorization_Endpoints_CORRECTED.sql
-- Description: Seeds EndpointRegistry and EndpointRolePermission tables with Endpoint Authorization API endpoints
-- Date: 2025-01-20
-- Author: Endpoint Management Implementation
-- =============================================
-- IMPORTANT SECURITY NOTE:
--   - Most endpoints require SUPERUSER role only (for permission management)
--   - EXCEPTION: /check endpoint is accessible to ALL ROLES (needed for menu visibility)
-- =============================================

USE [IkeaDocuScan]
GO

-- Variables to store endpoint IDs
DECLARE @EndpointId INT;

PRINT 'Seeding Endpoint Authorization API endpoints (CORRECTED VERSION)...'
PRINT ''
PRINT 'SECURITY POLICY:'
PRINT '  - Management endpoints: SuperUser ONLY'
PRINT '  - Check endpoint: All authenticated users (for menu visibility)'
PRINT ''

-- =============================================
-- ENDPOINT AUTHORIZATION (10 endpoints)
-- Category: Endpoint Authorization
-- MOST endpoints require SuperUser role
-- EXCEPT: /check endpoint (all roles for menu visibility)
-- =============================================

-- 1. GET /api/endpoint-authorization/endpoints
-- Get all endpoints with their role permissions (SuperUser only)
IF NOT EXISTS (SELECT 1 FROM EndpointRegistry WHERE HttpMethod = 'GET' AND Route = '/api/endpoint-authorization/endpoints')
BEGIN
    INSERT INTO EndpointRegistry (HttpMethod, Route, EndpointName, Description, Category, IsActive, CreatedOn)
    VALUES ('GET', '/api/endpoint-authorization/endpoints', 'GetAllEndpoints', 'Get all endpoints with their role permissions', 'Endpoint Authorization', 1, GETUTCDATE());

    SET @EndpointId = SCOPE_IDENTITY();

    -- SuperUser ONLY
    INSERT INTO EndpointRolePermission (EndpointId, RoleName) VALUES (@EndpointId, 'SuperUser');

    PRINT 'Added: GET /api/endpoint-authorization/endpoints (SuperUser only)'
END
ELSE
BEGIN
    PRINT 'Already exists: GET /api/endpoint-authorization/endpoints'
END

-- 2. GET /api/endpoint-authorization/endpoints/{id}
-- Get specific endpoint by ID (SuperUser only)
IF NOT EXISTS (SELECT 1 FROM EndpointRegistry WHERE HttpMethod = 'GET' AND Route = '/api/endpoint-authorization/endpoints/{id}')
BEGIN
    INSERT INTO EndpointRegistry (HttpMethod, Route, EndpointName, Description, Category, IsActive, CreatedOn)
    VALUES ('GET', '/api/endpoint-authorization/endpoints/{id}', 'GetEndpointById', 'Get specific endpoint by ID', 'Endpoint Authorization', 1, GETUTCDATE());

    SET @EndpointId = SCOPE_IDENTITY();

    -- SuperUser ONLY
    INSERT INTO EndpointRolePermission (EndpointId, RoleName) VALUES (@EndpointId, 'SuperUser');

    PRINT 'Added: GET /api/endpoint-authorization/endpoints/{id} (SuperUser only)'
END
ELSE
BEGIN
    PRINT 'Already exists: GET /api/endpoint-authorization/endpoints/{id}'
END

-- 3. GET /api/endpoint-authorization/endpoints/{id}/roles
-- Get roles that have access to a specific endpoint (SuperUser only)
IF NOT EXISTS (SELECT 1 FROM EndpointRegistry WHERE HttpMethod = 'GET' AND Route = '/api/endpoint-authorization/endpoints/{id}/roles')
BEGIN
    INSERT INTO EndpointRegistry (HttpMethod, Route, EndpointName, Description, Category, IsActive, CreatedOn)
    VALUES ('GET', '/api/endpoint-authorization/endpoints/{id}/roles', 'GetEndpointRoles', 'Get roles that have access to a specific endpoint', 'Endpoint Authorization', 1, GETUTCDATE());

    SET @EndpointId = SCOPE_IDENTITY();

    -- SuperUser ONLY
    INSERT INTO EndpointRolePermission (EndpointId, RoleName) VALUES (@EndpointId, 'SuperUser');

    PRINT 'Added: GET /api/endpoint-authorization/endpoints/{id}/roles (SuperUser only)'
END
ELSE
BEGIN
    PRINT 'Already exists: GET /api/endpoint-authorization/endpoints/{id}/roles'
END

-- 4. POST /api/endpoint-authorization/endpoints/{id}/roles
-- Update roles for a specific endpoint (SuperUser only - CRITICAL SECURITY)
IF NOT EXISTS (SELECT 1 FROM EndpointRegistry WHERE HttpMethod = 'POST' AND Route = '/api/endpoint-authorization/endpoints/{id}/roles')
BEGIN
    INSERT INTO EndpointRegistry (HttpMethod, Route, EndpointName, Description, Category, IsActive, CreatedOn)
    VALUES ('POST', '/api/endpoint-authorization/endpoints/{id}/roles', 'UpdateEndpointRoles', 'Update roles for a specific endpoint', 'Endpoint Authorization', 1, GETUTCDATE());

    SET @EndpointId = SCOPE_IDENTITY();

    -- SuperUser ONLY
    INSERT INTO EndpointRolePermission (EndpointId, RoleName) VALUES (@EndpointId, 'SuperUser');

    PRINT 'Added: POST /api/endpoint-authorization/endpoints/{id}/roles (SuperUser only - CRITICAL)'
END
ELSE
BEGIN
    PRINT 'Already exists: POST /api/endpoint-authorization/endpoints/{id}/roles'
END

-- 5. GET /api/endpoint-authorization/roles
-- Get all available role names (SuperUser only)
IF NOT EXISTS (SELECT 1 FROM EndpointRegistry WHERE HttpMethod = 'GET' AND Route = '/api/endpoint-authorization/roles')
BEGIN
    INSERT INTO EndpointRegistry (HttpMethod, Route, EndpointName, Description, Category, IsActive, CreatedOn)
    VALUES ('GET', '/api/endpoint-authorization/roles', 'GetAvailableRoles', 'Get all available role names', 'Endpoint Authorization', 1, GETUTCDATE());

    SET @EndpointId = SCOPE_IDENTITY();

    -- SuperUser ONLY
    INSERT INTO EndpointRolePermission (EndpointId, RoleName) VALUES (@EndpointId, 'SuperUser');

    PRINT 'Added: GET /api/endpoint-authorization/roles (SuperUser only)'
END
ELSE
BEGIN
    PRINT 'Already exists: GET /api/endpoint-authorization/roles'
END

-- 6. GET /api/endpoint-authorization/audit
-- Get audit log for permission changes (SuperUser only)
IF NOT EXISTS (SELECT 1 FROM EndpointRegistry WHERE HttpMethod = 'GET' AND Route = '/api/endpoint-authorization/audit')
BEGIN
    INSERT INTO EndpointRegistry (HttpMethod, Route, EndpointName, Description, Category, IsActive, CreatedOn)
    VALUES ('GET', '/api/endpoint-authorization/audit', 'GetPermissionAuditLog', 'Get audit log for permission changes', 'Endpoint Authorization', 1, GETUTCDATE());

    SET @EndpointId = SCOPE_IDENTITY();

    -- SuperUser ONLY
    INSERT INTO EndpointRolePermission (EndpointId, RoleName) VALUES (@EndpointId, 'SuperUser');

    PRINT 'Added: GET /api/endpoint-authorization/audit (SuperUser only)'
END
ELSE
BEGIN
    PRINT 'Already exists: GET /api/endpoint-authorization/audit'
END

-- 7. POST /api/endpoint-authorization/cache/invalidate
-- Invalidate the authorization cache (SuperUser only)
IF NOT EXISTS (SELECT 1 FROM EndpointRegistry WHERE HttpMethod = 'POST' AND Route = '/api/endpoint-authorization/cache/invalidate')
BEGIN
    INSERT INTO EndpointRegistry (HttpMethod, Route, EndpointName, Description, Category, IsActive, CreatedOn)
    VALUES ('POST', '/api/endpoint-authorization/cache/invalidate', 'InvalidateAuthorizationCache', 'Invalidate the authorization cache', 'Endpoint Authorization', 1, GETUTCDATE());

    SET @EndpointId = SCOPE_IDENTITY();

    -- SuperUser ONLY
    INSERT INTO EndpointRolePermission (EndpointId, RoleName) VALUES (@EndpointId, 'SuperUser');

    PRINT 'Added: POST /api/endpoint-authorization/cache/invalidate (SuperUser only)'
END
ELSE
BEGIN
    PRINT 'Already exists: POST /api/endpoint-authorization/cache/invalidate'
END

-- 8. POST /api/endpoint-authorization/sync
-- Sync endpoints from code to database (SuperUser only)
IF NOT EXISTS (SELECT 1 FROM EndpointRegistry WHERE HttpMethod = 'POST' AND Route = '/api/endpoint-authorization/sync')
BEGIN
    INSERT INTO EndpointRegistry (HttpMethod, Route, EndpointName, Description, Category, IsActive, CreatedOn)
    VALUES ('POST', '/api/endpoint-authorization/sync', 'SyncEndpointsFromCode', 'Sync endpoints from code to database', 'Endpoint Authorization', 1, GETUTCDATE());

    SET @EndpointId = SCOPE_IDENTITY();

    -- SuperUser ONLY
    INSERT INTO EndpointRolePermission (EndpointId, RoleName) VALUES (@EndpointId, 'SuperUser');

    PRINT 'Added: POST /api/endpoint-authorization/sync (SuperUser only)'
END
ELSE
BEGIN
    PRINT 'Already exists: POST /api/endpoint-authorization/sync'
END

-- 9. GET /api/endpoint-authorization/check
-- Check if current user has access to a specific endpoint
-- CRITICAL: ALL ROLES must have access for menu visibility to work!
IF NOT EXISTS (SELECT 1 FROM EndpointRegistry WHERE HttpMethod = 'GET' AND Route = '/api/endpoint-authorization/check')
BEGIN
    INSERT INTO EndpointRegistry (HttpMethod, Route, EndpointName, Description, Category, IsActive, CreatedOn)
    VALUES ('GET', '/api/endpoint-authorization/check', 'CheckEndpointAccess', 'Check if current user has access to a specific endpoint', 'Endpoint Authorization', 1, GETUTCDATE());

    SET @EndpointId = SCOPE_IDENTITY();

    -- ALL ROLES (required for menu visibility)
    INSERT INTO EndpointRolePermission (EndpointId, RoleName) VALUES (@EndpointId, 'Reader');
    INSERT INTO EndpointRolePermission (EndpointId, RoleName) VALUES (@EndpointId, 'Publisher');
    INSERT INTO EndpointRolePermission (EndpointId, RoleName) VALUES (@EndpointId, 'ADAdmin');
    INSERT INTO EndpointRolePermission (EndpointId, RoleName) VALUES (@EndpointId, 'SuperUser');

    PRINT 'Added: GET /api/endpoint-authorization/check (ALL ROLES - CRITICAL for NavMenu)'
END
ELSE
BEGIN
    PRINT 'Already exists: GET /api/endpoint-authorization/check'
END

-- 10. POST /api/endpoint-authorization/validate
-- Validate a permission change before applying it (SuperUser only)
IF NOT EXISTS (SELECT 1 FROM EndpointRegistry WHERE HttpMethod = 'POST' AND Route = '/api/endpoint-authorization/validate')
BEGIN
    INSERT INTO EndpointRegistry (HttpMethod, Route, EndpointName, Description, Category, IsActive, CreatedOn)
    VALUES ('POST', '/api/endpoint-authorization/validate', 'ValidatePermissionChange', 'Validate a permission change before applying it', 'Endpoint Authorization', 1, GETUTCDATE());

    SET @EndpointId = SCOPE_IDENTITY();

    -- SuperUser ONLY
    INSERT INTO EndpointRolePermission (EndpointId, RoleName) VALUES (@EndpointId, 'SuperUser');

    PRINT 'Added: POST /api/endpoint-authorization/validate (SuperUser only)'
END
ELSE
BEGIN
    PRINT 'Already exists: POST /api/endpoint-authorization/validate'
END

-- =============================================
-- USER PERMISSION BATCH OPERATIONS
-- Category: User Permissions
-- =============================================

-- 11. POST /api/userpermissions/user/{userId}/batch-document-types
-- Batch update document type permissions for a user (SuperUser only)
IF NOT EXISTS (SELECT 1 FROM EndpointRegistry WHERE HttpMethod = 'POST' AND Route = '/api/userpermissions/user/{userId}/batch-document-types')
BEGIN
    INSERT INTO EndpointRegistry (HttpMethod, Route, EndpointName, Description, Category, IsActive, CreatedOn)
    VALUES ('POST', '/api/userpermissions/user/{userId}/batch-document-types', 'BatchUpdateDocumentTypePermissions', 'Batch update document type permissions for a user', 'User Permissions', 1, GETUTCDATE());

    SET @EndpointId = SCOPE_IDENTITY();

    -- SuperUser ONLY
    INSERT INTO EndpointRolePermission (EndpointId, RoleName) VALUES (@EndpointId, 'SuperUser');

    PRINT 'Added: POST /api/userpermissions/user/{userId}/batch-document-types (SuperUser only)'
END
ELSE
BEGIN
    PRINT 'Already exists: POST /api/userpermissions/user/{userId}/batch-document-types'
END

PRINT ''
PRINT '========================================'
PRINT 'Endpoint Authorization Seeding Complete'
PRINT '========================================'
PRINT ''
PRINT 'Security Summary:'
PRINT '  - 9 management endpoints: SuperUser ONLY'
PRINT '  - 1 check endpoint: ALL ROLES (for menu visibility)'
PRINT '  - 1 batch permission endpoint: SuperUser ONLY'
PRINT ''

-- Summary query
SELECT
    COUNT(*) AS EndpointAuthorizationEndpointCount
FROM EndpointRegistry
WHERE Category = 'Endpoint Authorization'
    AND IsActive = 1;

SELECT
    er.HttpMethod,
    er.Route,
    er.EndpointName,
    STRING_AGG(erp.RoleName, ', ') AS AllowedRoles
FROM EndpointRegistry er
LEFT JOIN EndpointRolePermission erp ON er.EndpointId = erp.EndpointId
WHERE er.Category = 'Endpoint Authorization'
    AND er.IsActive = 1
GROUP BY er.EndpointId, er.HttpMethod, er.Route, er.EndpointName
ORDER BY er.Route;

GO
