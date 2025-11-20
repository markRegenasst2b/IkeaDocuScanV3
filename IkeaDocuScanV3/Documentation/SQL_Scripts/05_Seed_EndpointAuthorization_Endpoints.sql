-- =============================================
-- Script: 05_Seed_EndpointAuthorization_Endpoints.sql
-- Description: Seeds EndpointRegistry and EndpointRolePermission tables with Endpoint Authorization API endpoints
-- Date: 2025-11-20
-- Author: Role Extension Implementation
-- =============================================
-- NOTE: Per user request, ALL ROLES have access to endpoint-authorization endpoints
--       This allows all users to check their permissions and enables menu visibility for all roles
-- =============================================

USE [IkeaDocuScan]
GO

-- Variables to store endpoint IDs
DECLARE @EndpointId INT;

PRINT 'Seeding Endpoint Authorization API endpoints...'
PRINT ''

-- =============================================
-- ENDPOINT AUTHORIZATION (10 endpoints)
-- Category: Endpoint Authorization
-- All roles have access per user requirement
-- =============================================

-- 1. GET /api/endpoint-authorization/endpoints
-- Get all endpoints with their role permissions
IF NOT EXISTS (SELECT 1 FROM EndpointRegistry WHERE HttpMethod = 'GET' AND Route = '/api/endpoint-authorization/endpoints')
BEGIN
    INSERT INTO EndpointRegistry (HttpMethod, Route, EndpointName, Description, Category, IsActive, CreatedOn)
    VALUES ('GET', '/api/endpoint-authorization/endpoints', 'GetAllEndpoints', 'Get all endpoints with their role permissions', 'Endpoint Authorization', 1, GETUTCDATE());

    SET @EndpointId = SCOPE_IDENTITY();

    INSERT INTO EndpointRolePermission (EndpointId, RoleName) VALUES (@EndpointId, 'Reader');
    INSERT INTO EndpointRolePermission (EndpointId, RoleName) VALUES (@EndpointId, 'Publisher');
    INSERT INTO EndpointRolePermission (EndpointId, RoleName) VALUES (@EndpointId, 'ADAdmin');
    INSERT INTO EndpointRolePermission (EndpointId, RoleName) VALUES (@EndpointId, 'SuperUser');

    PRINT 'Added: GET /api/endpoint-authorization/endpoints (All roles)'
END
ELSE
BEGIN
    PRINT 'Already exists: GET /api/endpoint-authorization/endpoints'
END

-- 2. GET /api/endpoint-authorization/endpoints/{id}
-- Get specific endpoint by ID
IF NOT EXISTS (SELECT 1 FROM EndpointRegistry WHERE HttpMethod = 'GET' AND Route = '/api/endpoint-authorization/endpoints/{id}')
BEGIN
    INSERT INTO EndpointRegistry (HttpMethod, Route, EndpointName, Description, Category, IsActive, CreatedOn)
    VALUES ('GET', '/api/endpoint-authorization/endpoints/{id}', 'GetEndpointById', 'Get specific endpoint by ID', 'Endpoint Authorization', 1, GETUTCDATE());

    SET @EndpointId = SCOPE_IDENTITY();

    INSERT INTO EndpointRolePermission (EndpointId, RoleName) VALUES (@EndpointId, 'Reader');
    INSERT INTO EndpointRolePermission (EndpointId, RoleName) VALUES (@EndpointId, 'Publisher');
    INSERT INTO EndpointRolePermission (EndpointId, RoleName) VALUES (@EndpointId, 'ADAdmin');
    INSERT INTO EndpointRolePermission (EndpointId, RoleName) VALUES (@EndpointId, 'SuperUser');

    PRINT 'Added: GET /api/endpoint-authorization/endpoints/{id} (All roles)'
END
ELSE
BEGIN
    PRINT 'Already exists: GET /api/endpoint-authorization/endpoints/{id}'
END

-- 3. GET /api/endpoint-authorization/endpoints/{id}/roles
-- Get roles that have access to a specific endpoint
IF NOT EXISTS (SELECT 1 FROM EndpointRegistry WHERE HttpMethod = 'GET' AND Route = '/api/endpoint-authorization/endpoints/{id}/roles')
BEGIN
    INSERT INTO EndpointRegistry (HttpMethod, Route, EndpointName, Description, Category, IsActive, CreatedOn)
    VALUES ('GET', '/api/endpoint-authorization/endpoints/{id}/roles', 'GetEndpointRoles', 'Get roles that have access to a specific endpoint', 'Endpoint Authorization', 1, GETUTCDATE());

    SET @EndpointId = SCOPE_IDENTITY();

    INSERT INTO EndpointRolePermission (EndpointId, RoleName) VALUES (@EndpointId, 'Reader');
    INSERT INTO EndpointRolePermission (EndpointId, RoleName) VALUES (@EndpointId, 'Publisher');
    INSERT INTO EndpointRolePermission (EndpointId, RoleName) VALUES (@EndpointId, 'ADAdmin');
    INSERT INTO EndpointRolePermission (EndpointId, RoleName) VALUES (@EndpointId, 'SuperUser');

    PRINT 'Added: GET /api/endpoint-authorization/endpoints/{id}/roles (All roles)'
END
ELSE
BEGIN
    PRINT 'Already exists: GET /api/endpoint-authorization/endpoints/{id}/roles'
END

-- 4. POST /api/endpoint-authorization/endpoints/{id}/roles
-- Update roles for a specific endpoint
IF NOT EXISTS (SELECT 1 FROM EndpointRegistry WHERE HttpMethod = 'POST' AND Route = '/api/endpoint-authorization/endpoints/{id}/roles')
BEGIN
    INSERT INTO EndpointRegistry (HttpMethod, Route, EndpointName, Description, Category, IsActive, CreatedOn)
    VALUES ('POST', '/api/endpoint-authorization/endpoints/{id}/roles', 'UpdateEndpointRoles', 'Update roles for a specific endpoint', 'Endpoint Authorization', 1, GETUTCDATE());

    SET @EndpointId = SCOPE_IDENTITY();

    INSERT INTO EndpointRolePermission (EndpointId, RoleName) VALUES (@EndpointId, 'Reader');
    INSERT INTO EndpointRolePermission (EndpointId, RoleName) VALUES (@EndpointId, 'Publisher');
    INSERT INTO EndpointRolePermission (EndpointId, RoleName) VALUES (@EndpointId, 'ADAdmin');
    INSERT INTO EndpointRolePermission (EndpointId, RoleName) VALUES (@EndpointId, 'SuperUser');

    PRINT 'Added: POST /api/endpoint-authorization/endpoints/{id}/roles (All roles)'
END
ELSE
BEGIN
    PRINT 'Already exists: POST /api/endpoint-authorization/endpoints/{id}/roles'
END

-- 5. GET /api/endpoint-authorization/roles
-- Get all available role names
IF NOT EXISTS (SELECT 1 FROM EndpointRegistry WHERE HttpMethod = 'GET' AND Route = '/api/endpoint-authorization/roles')
BEGIN
    INSERT INTO EndpointRegistry (HttpMethod, Route, EndpointName, Description, Category, IsActive, CreatedOn)
    VALUES ('GET', '/api/endpoint-authorization/roles', 'GetAvailableRoles', 'Get all available role names', 'Endpoint Authorization', 1, GETUTCDATE());

    SET @EndpointId = SCOPE_IDENTITY();

    INSERT INTO EndpointRolePermission (EndpointId, RoleName) VALUES (@EndpointId, 'Reader');
    INSERT INTO EndpointRolePermission (EndpointId, RoleName) VALUES (@EndpointId, 'Publisher');
    INSERT INTO EndpointRolePermission (EndpointId, RoleName) VALUES (@EndpointId, 'ADAdmin');
    INSERT INTO EndpointRolePermission (EndpointId, RoleName) VALUES (@EndpointId, 'SuperUser');

    PRINT 'Added: GET /api/endpoint-authorization/roles (All roles)'
END
ELSE
BEGIN
    PRINT 'Already exists: GET /api/endpoint-authorization/roles'
END

-- 6. GET /api/endpoint-authorization/audit
-- Get audit log for permission changes
IF NOT EXISTS (SELECT 1 FROM EndpointRegistry WHERE HttpMethod = 'GET' AND Route = '/api/endpoint-authorization/audit')
BEGIN
    INSERT INTO EndpointRegistry (HttpMethod, Route, EndpointName, Description, Category, IsActive, CreatedOn)
    VALUES ('GET', '/api/endpoint-authorization/audit', 'GetPermissionAuditLog', 'Get audit log for permission changes', 'Endpoint Authorization', 1, GETUTCDATE());

    SET @EndpointId = SCOPE_IDENTITY();

    INSERT INTO EndpointRolePermission (EndpointId, RoleName) VALUES (@EndpointId, 'Reader');
    INSERT INTO EndpointRolePermission (EndpointId, RoleName) VALUES (@EndpointId, 'Publisher');
    INSERT INTO EndpointRolePermission (EndpointId, RoleName) VALUES (@EndpointId, 'ADAdmin');
    INSERT INTO EndpointRolePermission (EndpointId, RoleName) VALUES (@EndpointId, 'SuperUser');

    PRINT 'Added: GET /api/endpoint-authorization/audit (All roles)'
END
ELSE
BEGIN
    PRINT 'Already exists: GET /api/endpoint-authorization/audit'
END

-- 7. POST /api/endpoint-authorization/cache/invalidate
-- Invalidate the authorization cache
IF NOT EXISTS (SELECT 1 FROM EndpointRegistry WHERE HttpMethod = 'POST' AND Route = '/api/endpoint-authorization/cache/invalidate')
BEGIN
    INSERT INTO EndpointRegistry (HttpMethod, Route, EndpointName, Description, Category, IsActive, CreatedOn)
    VALUES ('POST', '/api/endpoint-authorization/cache/invalidate', 'InvalidateAuthorizationCache', 'Invalidate the authorization cache', 'Endpoint Authorization', 1, GETUTCDATE());

    SET @EndpointId = SCOPE_IDENTITY();

    INSERT INTO EndpointRolePermission (EndpointId, RoleName) VALUES (@EndpointId, 'Reader');
    INSERT INTO EndpointRolePermission (EndpointId, RoleName) VALUES (@EndpointId, 'Publisher');
    INSERT INTO EndpointRolePermission (EndpointId, RoleName) VALUES (@EndpointId, 'ADAdmin');
    INSERT INTO EndpointRolePermission (EndpointId, RoleName) VALUES (@EndpointId, 'SuperUser');

    PRINT 'Added: POST /api/endpoint-authorization/cache/invalidate (All roles)'
END
ELSE
BEGIN
    PRINT 'Already exists: POST /api/endpoint-authorization/cache/invalidate'
END

-- 8. POST /api/endpoint-authorization/sync
-- Sync endpoints from code to database
IF NOT EXISTS (SELECT 1 FROM EndpointRegistry WHERE HttpMethod = 'POST' AND Route = '/api/endpoint-authorization/sync')
BEGIN
    INSERT INTO EndpointRegistry (HttpMethod, Route, EndpointName, Description, Category, IsActive, CreatedOn)
    VALUES ('POST', '/api/endpoint-authorization/sync', 'SyncEndpointsFromCode', 'Sync endpoints from code to database', 'Endpoint Authorization', 1, GETUTCDATE());

    SET @EndpointId = SCOPE_IDENTITY();

    INSERT INTO EndpointRolePermission (EndpointId, RoleName) VALUES (@EndpointId, 'Reader');
    INSERT INTO EndpointRolePermission (EndpointId, RoleName) VALUES (@EndpointId, 'Publisher');
    INSERT INTO EndpointRolePermission (EndpointId, RoleName) VALUES (@EndpointId, 'ADAdmin');
    INSERT INTO EndpointRolePermission (EndpointId, RoleName) VALUES (@EndpointId, 'SuperUser');

    PRINT 'Added: POST /api/endpoint-authorization/sync (All roles)'
END
ELSE
BEGIN
    PRINT 'Already exists: POST /api/endpoint-authorization/sync'
END

-- 9. GET /api/endpoint-authorization/check
-- Check if current user has access to a specific endpoint (CRITICAL for menu visibility)
IF NOT EXISTS (SELECT 1 FROM EndpointRegistry WHERE HttpMethod = 'GET' AND Route = '/api/endpoint-authorization/check')
BEGIN
    INSERT INTO EndpointRegistry (HttpMethod, Route, EndpointName, Description, Category, IsActive, CreatedOn)
    VALUES ('GET', '/api/endpoint-authorization/check', 'CheckEndpointAccess', 'Check if current user has access to a specific endpoint', 'Endpoint Authorization', 1, GETUTCDATE());

    SET @EndpointId = SCOPE_IDENTITY();

    INSERT INTO EndpointRolePermission (EndpointId, RoleName) VALUES (@EndpointId, 'Reader');
    INSERT INTO EndpointRolePermission (EndpointId, RoleName) VALUES (@EndpointId, 'Publisher');
    INSERT INTO EndpointRolePermission (EndpointId, RoleName) VALUES (@EndpointId, 'ADAdmin');
    INSERT INTO EndpointRolePermission (EndpointId, RoleName) VALUES (@EndpointId, 'SuperUser');

    PRINT 'Added: GET /api/endpoint-authorization/check (All roles) - CRITICAL for NavMenu'
END
ELSE
BEGIN
    PRINT 'Already exists: GET /api/endpoint-authorization/check'
END

-- 10. POST /api/endpoint-authorization/validate
-- Validate a permission change before applying it
IF NOT EXISTS (SELECT 1 FROM EndpointRegistry WHERE HttpMethod = 'POST' AND Route = '/api/endpoint-authorization/validate')
BEGIN
    INSERT INTO EndpointRegistry (HttpMethod, Route, EndpointName, Description, Category, IsActive, CreatedOn)
    VALUES ('POST', '/api/endpoint-authorization/validate', 'ValidatePermissionChange', 'Validate a permission change before applying it', 'Endpoint Authorization', 1, GETUTCDATE());

    SET @EndpointId = SCOPE_IDENTITY();

    INSERT INTO EndpointRolePermission (EndpointId, RoleName) VALUES (@EndpointId, 'Reader');
    INSERT INTO EndpointRolePermission (EndpointId, RoleName) VALUES (@EndpointId, 'Publisher');
    INSERT INTO EndpointRolePermission (EndpointId, RoleName) VALUES (@EndpointId, 'ADAdmin');
    INSERT INTO EndpointRolePermission (EndpointId, RoleName) VALUES (@EndpointId, 'SuperUser');

    PRINT 'Added: POST /api/endpoint-authorization/validate (All roles)'
END
ELSE
BEGIN
    PRINT 'Already exists: POST /api/endpoint-authorization/validate'
END

PRINT ''
PRINT '========================================'
PRINT 'Endpoint Authorization Seeding Complete'
PRINT '========================================'

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
