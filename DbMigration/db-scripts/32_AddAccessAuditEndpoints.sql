-- ============================================================================
-- Script: 32_AddAccessAuditEndpoints.sql
-- Purpose: Register Access Audit endpoints for SuperUser-only access
-- Date: 2025-01-28
-- Author: System
-- Description: Creates endpoints for the Access Audit feature which allows
--              SuperUsers to audit document type access permissions.
--              Two views: By Document Type and By User
-- ============================================================================

SET NOCOUNT ON;

BEGIN TRANSACTION;

BEGIN TRY

    PRINT '============================================================================';
    PRINT 'Adding Access Audit Endpoints';
    PRINT '============================================================================';

    -- ========================================================================
    -- 1. Insert Access Audit endpoints into EndpointRegistry
    -- ========================================================================

    -- Endpoint 1: Get users with access to specific document type
    IF NOT EXISTS (
        SELECT 1 FROM EndpointRegistry
        WHERE HttpMethod = 'GET'
        AND Route = '/api/access-audit/document-type/{documentTypeId}'
    )
    BEGIN
        INSERT INTO EndpointRegistry (
            HttpMethod,
            Route,
            EndpointName,
            Description,
            Category,
            IsActive
        )
        VALUES (
            'GET',
            '/api/access-audit/document-type/{documentTypeId}',
            'GetDocumentTypeAccessAudit',
            'Get all users who have access to a specific document type (direct or via global access)',
            'AccessAudit',
            1
        );
        PRINT 'Inserted: GET /api/access-audit/document-type/{documentTypeId}';
    END
    ELSE
        PRINT 'Already exists: GET /api/access-audit/document-type/{documentTypeId}';

    -- Endpoint 2: Get document types a user has access to
    IF NOT EXISTS (
        SELECT 1 FROM EndpointRegistry
        WHERE HttpMethod = 'GET'
        AND Route = '/api/access-audit/user/{userId}'
    )
    BEGIN
        INSERT INTO EndpointRegistry (
            HttpMethod,
            Route,
            EndpointName,
            Description,
            Category,
            IsActive
        )
        VALUES (
            'GET',
            '/api/access-audit/user/{userId}',
            'GetUserAccessAudit',
            'Get all document types a specific user has access to',
            'AccessAudit',
            1
        );
        PRINT 'Inserted: GET /api/access-audit/user/{userId}';
    END
    ELSE
        PRINT 'Already exists: GET /api/access-audit/user/{userId}';

    -- Endpoint 3: Excel export for document type view
    IF NOT EXISTS (
        SELECT 1 FROM EndpointRegistry
        WHERE HttpMethod = 'GET'
        AND Route = '/api/access-audit/document-type/{documentTypeId}/export'
    )
    BEGIN
        INSERT INTO EndpointRegistry (
            HttpMethod,
            Route,
            EndpointName,
            Description,
            Category,
            IsActive
        )
        VALUES (
            'GET',
            '/api/access-audit/document-type/{documentTypeId}/export',
            'ExportDocumentTypeAccessAudit',
            'Export users with access to a document type as Excel file',
            'AccessAudit',
            1
        );
        PRINT 'Inserted: GET /api/access-audit/document-type/{documentTypeId}/export';
    END
    ELSE
        PRINT 'Already exists: GET /api/access-audit/document-type/{documentTypeId}/export';

    -- Endpoint 4: Excel export for user view
    IF NOT EXISTS (
        SELECT 1 FROM EndpointRegistry
        WHERE HttpMethod = 'GET'
        AND Route = '/api/access-audit/user/{userId}/export'
    )
    BEGIN
        INSERT INTO EndpointRegistry (
            HttpMethod,
            Route,
            EndpointName,
            Description,
            Category,
            IsActive
        )
        VALUES (
            'GET',
            '/api/access-audit/user/{userId}/export',
            'ExportUserAccessAudit',
            'Export document types a user has access to as Excel file',
            'AccessAudit',
            1
        );
        PRINT 'Inserted: GET /api/access-audit/user/{userId}/export';
    END
    ELSE
        PRINT 'Already exists: GET /api/access-audit/user/{userId}/export';

    -- Endpoint 5: Search users (for By User tab)
    IF NOT EXISTS (
        SELECT 1 FROM EndpointRegistry
        WHERE HttpMethod = 'GET'
        AND Route = '/api/access-audit/users'
    )
    BEGIN
        INSERT INTO EndpointRegistry (
            HttpMethod,
            Route,
            EndpointName,
            Description,
            Category,
            IsActive
        )
        VALUES (
            'GET',
            '/api/access-audit/users',
            'SearchUsersForAccessAudit',
            'Search users for access audit (supports filtering by account name, active status, superuser)',
            'AccessAudit',
            1
        );
        PRINT 'Inserted: GET /api/access-audit/users';
    END
    ELSE
        PRINT 'Already exists: GET /api/access-audit/users';

    -- ========================================================================
    -- 2. Grant SuperUser role access to all Access Audit endpoints
    -- ========================================================================

    PRINT '';
    PRINT 'Granting SuperUser access to Access Audit endpoints...';

    INSERT INTO EndpointRolePermission (EndpointId, RoleName, CreatedBy)
    SELECT e.EndpointId, 'SuperUser', 'SYSTEM'
    FROM EndpointRegistry e
    WHERE e.Category = 'AccessAudit'
    AND e.IsActive = 1
    AND NOT EXISTS (
        SELECT 1 FROM EndpointRolePermission erp
        WHERE erp.EndpointId = e.EndpointId
        AND erp.RoleName = 'SuperUser'
    );

    PRINT 'SuperUser access granted to ' + CAST(@@ROWCOUNT AS VARCHAR(10)) + ' endpoint(s)';

    -- ========================================================================
    -- 3. Log the permission changes in audit log
    -- ========================================================================

    PRINT '';
    PRINT 'Creating audit log entries...';

    INSERT INTO PermissionChangeAuditLog (
        EndpointId,
        ChangedBy,
        ChangeType,
        OldValue,
        NewValue,
        ChangeReason
    )
    SELECT
        e.EndpointId,
        'SYSTEM',
        'EndpointCreated',
        NULL,
        '{"roles": ["SuperUser"]}',
        'Initial creation of Access Audit endpoint for document type and user access review'
    FROM EndpointRegistry e
    WHERE e.Category = 'AccessAudit'
    AND e.IsActive = 1
    AND NOT EXISTS (
        SELECT 1 FROM PermissionChangeAuditLog pal
        WHERE pal.EndpointId = e.EndpointId
        AND pal.ChangeType = 'EndpointCreated'
    );

    PRINT 'Audit log entries created: ' + CAST(@@ROWCOUNT AS VARCHAR(10));

    COMMIT TRANSACTION;

    PRINT '';
    PRINT '============================================================================';
    PRINT 'Access Audit Endpoints added successfully';
    PRINT '============================================================================';

END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    PRINT '';
    PRINT '============================================================================';
    PRINT 'ERROR: Transaction rolled back';
    PRINT 'Error Message: ' + ERROR_MESSAGE();
    PRINT 'Error Line: ' + CAST(ERROR_LINE() AS VARCHAR(10));
    PRINT '============================================================================';
    THROW;
END CATCH;

GO

-- ============================================================================
-- Verification Query
-- ============================================================================

PRINT '';
PRINT 'Verification - Access Audit Endpoints:';
PRINT '---------------------------------------';

SELECT
    er.EndpointId,
    er.HttpMethod,
    er.Route,
    er.EndpointName,
    er.Category,
    er.IsActive,
    STRING_AGG(erp.RoleName, ', ') AS AllowedRoles
FROM EndpointRegistry er
LEFT JOIN EndpointRolePermission erp ON er.EndpointId = erp.EndpointId
WHERE er.Category = 'AccessAudit'
GROUP BY er.EndpointId, er.HttpMethod, er.Route, er.EndpointName, er.Category, er.IsActive
ORDER BY er.EndpointId;

GO
