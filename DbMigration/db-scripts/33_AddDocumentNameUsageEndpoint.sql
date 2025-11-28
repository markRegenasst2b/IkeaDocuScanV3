-- ============================================================================
-- Script: 33_AddDocumentNameUsageEndpoint.sql
-- Purpose: Register Document Name usage endpoint for referential integrity check
-- Date: 2025-01-28
-- Author: System
-- Description: Creates endpoint for checking document name usage before deletion.
--              This endpoint allows the UI to pre-check if a document name is
--              in use by any documents before attempting deletion.
-- ============================================================================

SET NOCOUNT ON;

BEGIN TRANSACTION;

BEGIN TRY

    PRINT '============================================================================';
    PRINT 'Adding Document Name Usage Endpoint';
    PRINT '============================================================================';

    -- ========================================================================
    -- 1. Insert Document Name Usage endpoint into EndpointRegistry
    -- ========================================================================

    IF NOT EXISTS (
        SELECT 1 FROM EndpointRegistry
        WHERE HttpMethod = 'GET'
        AND Route = '/api/documentnames/{id}/usage'
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
            '/api/documentnames/{id}/usage',
            'GetDocumentNameUsage',
            'Get count of documents using this document name (for referential integrity check before deletion)',
            'DocumentNames',
            1
        );
        PRINT 'Inserted: GET /api/documentnames/{id}/usage';
    END
    ELSE
        PRINT 'Already exists: GET /api/documentnames/{id}/usage';

    -- ========================================================================
    -- 2. Grant SuperUser role access to the endpoint
    -- ========================================================================

    PRINT '';
    PRINT 'Granting SuperUser access to Document Name Usage endpoint...';

    INSERT INTO EndpointRolePermission (EndpointId, RoleName, CreatedBy)
    SELECT e.EndpointId, 'SuperUser', 'SYSTEM'
    FROM EndpointRegistry e
    WHERE e.HttpMethod = 'GET'
    AND e.Route = '/api/documentnames/{id}/usage'
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
        'Initial creation of Document Name usage endpoint for referential integrity check'
    FROM EndpointRegistry e
    WHERE e.HttpMethod = 'GET'
    AND e.Route = '/api/documentnames/{id}/usage'
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
    PRINT 'Document Name Usage Endpoint added successfully';
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
PRINT 'Verification - Document Name Usage Endpoint:';
PRINT '---------------------------------------------';

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
WHERE er.Route = '/api/documentnames/{id}/usage'
GROUP BY er.EndpointId, er.HttpMethod, er.Route, er.EndpointName, er.Category, er.IsActive
ORDER BY er.EndpointId;

GO
