-- Synchronize all migrated endpoints with the database
-- This script adds any missing endpoints from the migration to the database

-- Batch 5 Endpoints (Final Migration)

-- UserIdentityEndpoints (1 endpoint)
IF NOT EXISTS (SELECT 1 FROM EndpointRegistry WHERE Route = '/api/user/identity' AND HttpMethod = 'GET')
BEGIN
    INSERT INTO EndpointRegistry (HttpMethod, Route, EndpointName, Category, IsActive, Description)
    VALUES ('GET', '/api/user/identity', 'GetUserIdentity', 'UserIdentity', 1, 'Get current user identity and claims');

    DECLARE @UserIdentityId INT = SCOPE_IDENTITY();
    INSERT INTO EndpointRolePermission (EndpointId, RoleName) VALUES (@UserIdentityId, 'Reader'), (@UserIdentityId, 'Publisher'), (@UserIdentityId, 'ADAdmin'), (@UserIdentityId, 'SuperUser');
    PRINT 'Added: GET /api/user/identity';
END

-- TestIdentityEndpoints (4 endpoints - DEBUG ONLY)
IF NOT EXISTS (SELECT 1 FROM EndpointRegistry WHERE Route = '/api/test-identity/profiles' AND HttpMethod = 'GET')
BEGIN
    INSERT INTO EndpointRegistry (HttpMethod, Route, EndpointName, Category, IsActive, Description)
    VALUES ('GET', '/api/test-identity/profiles', 'GetTestIdentityProfiles', 'TestIdentity', 1, 'Get available test identity profiles (DEBUG)');

    DECLARE @ProfilesId INT = SCOPE_IDENTITY();
    INSERT INTO EndpointRolePermission (EndpointId, RoleName) VALUES (@ProfilesId, 'SuperUser');
    PRINT 'Added: GET /api/test-identity/profiles';
END

IF NOT EXISTS (SELECT 1 FROM EndpointRegistry WHERE Route = '/api/test-identity/status' AND HttpMethod = 'GET')
BEGIN
    INSERT INTO EndpointRegistry (HttpMethod, Route, EndpointName, Category, IsActive, Description)
    VALUES ('GET', '/api/test-identity/status', 'GetTestIdentityStatus', 'TestIdentity', 1, 'Get current test identity status (DEBUG)');

    DECLARE @StatusId INT = SCOPE_IDENTITY();
    INSERT INTO EndpointRolePermission (EndpointId, RoleName) VALUES (@StatusId, 'SuperUser');
    PRINT 'Added: GET /api/test-identity/status';
END

IF NOT EXISTS (SELECT 1 FROM EndpointRegistry WHERE Route = '/api/test-identity/activate/{profileId}' AND HttpMethod = 'POST')
BEGIN
    INSERT INTO EndpointRegistry (HttpMethod, Route, EndpointName, Category, IsActive, Description)
    VALUES ('POST', '/api/test-identity/activate/{profileId}', 'ActivateTestIdentity', 'TestIdentity', 1, 'Activate test identity profile (DEBUG)');

    DECLARE @ActivateId INT = SCOPE_IDENTITY();
    INSERT INTO EndpointRolePermission (EndpointId, RoleName) VALUES (@ActivateId, 'SuperUser');
    PRINT 'Added: POST /api/test-identity/activate/{profileId}';
END

IF NOT EXISTS (SELECT 1 FROM EndpointRegistry WHERE Route = '/api/test-identity/reset' AND HttpMethod = 'POST')
BEGIN
    INSERT INTO EndpointRegistry (HttpMethod, Route, EndpointName, Category, IsActive, Description)
    VALUES ('POST', '/api/test-identity/reset', 'ResetTestIdentity', 'TestIdentity', 1, 'Reset to real identity (DEBUG)');

    DECLARE @ResetId INT = SCOPE_IDENTITY();
    INSERT INTO EndpointRolePermission (EndpointId, RoleName) VALUES (@ResetId, 'SuperUser');
    PRINT 'Added: POST /api/test-identity/reset';
END

-- ExcelExportEndpoints (4 endpoints)
IF NOT EXISTS (SELECT 1 FROM EndpointRegistry WHERE Route = '/api/excel/export/documents' AND HttpMethod = 'POST')
BEGIN
    INSERT INTO EndpointRegistry (HttpMethod, Route, EndpointName, Category, IsActive, Description)
    VALUES ('POST', '/api/excel/export/documents', 'ExportDocumentsToExcel', 'Excel Export', 1, 'Export documents to Excel');

    DECLARE @ExportDocsId INT = SCOPE_IDENTITY();
    INSERT INTO EndpointRolePermission (EndpointId, RoleName) VALUES (@ExportDocsId, 'Publisher'), (@ExportDocsId, 'ADAdmin'), (@ExportDocsId, 'SuperUser');
    PRINT 'Added: POST /api/excel/export/documents';
END

IF NOT EXISTS (SELECT 1 FROM EndpointRegistry WHERE Route = '/api/excel/validate/documents' AND HttpMethod = 'POST')
BEGIN
    INSERT INTO EndpointRegistry (HttpMethod, Route, EndpointName, Category, IsActive, Description)
    VALUES ('POST', '/api/excel/validate/documents', 'ValidateExportSize', 'Excel Export', 1, 'Validate export size before generating');

    DECLARE @ValidateId INT = SCOPE_IDENTITY();
    INSERT INTO EndpointRolePermission (EndpointId, RoleName) VALUES (@ValidateId, 'Publisher'), (@ValidateId, 'ADAdmin'), (@ValidateId, 'SuperUser');
    PRINT 'Added: POST /api/excel/validate/documents';
END

IF NOT EXISTS (SELECT 1 FROM EndpointRegistry WHERE Route = '/api/excel/metadata/documents' AND HttpMethod = 'GET')
BEGIN
    INSERT INTO EndpointRegistry (HttpMethod, Route, EndpointName, Category, IsActive, Description)
    VALUES ('GET', '/api/excel/metadata/documents', 'GetDocumentExportMetadata', 'Excel Export', 1, 'Get metadata for document export');

    DECLARE @MetadataId INT = SCOPE_IDENTITY();
    INSERT INTO EndpointRolePermission (EndpointId, RoleName) VALUES (@MetadataId, 'Publisher'), (@MetadataId, 'ADAdmin'), (@MetadataId, 'SuperUser');
    PRINT 'Added: GET /api/excel/metadata/documents';
END

IF NOT EXISTS (SELECT 1 FROM EndpointRegistry WHERE Route = '/api/excel/export/by-ids' AND HttpMethod = 'POST')
BEGIN
    INSERT INTO EndpointRegistry (HttpMethod, Route, EndpointName, Category, IsActive, Description)
    VALUES ('POST', '/api/excel/export/by-ids', 'ExportDocumentsByIdsToExcel', 'Excel Export', 1, 'Export documents by IDs to Excel');

    DECLARE @ExportByIdsId INT = SCOPE_IDENTITY();
    INSERT INTO EndpointRolePermission (EndpointId, RoleName) VALUES (@ExportByIdsId, 'Publisher'), (@ExportByIdsId, 'ADAdmin'), (@ExportByIdsId, 'SuperUser');
    PRINT 'Added: POST /api/excel/export/by-ids';
END

-- EmailEndpoints (3 endpoints) - Note: Database has different route names
IF NOT EXISTS (SELECT 1 FROM EndpointRegistry WHERE Route = '/api/email/send-with-attachments' AND HttpMethod = 'POST')
BEGIN
    INSERT INTO EndpointRegistry (HttpMethod, Route, EndpointName, Category, IsActive, Description)
    VALUES ('POST', '/api/email/send-with-attachments', 'SendEmailWithAttachments', 'Email', 1, 'Send email with document attachments');

    DECLARE @AttachmentsId INT = SCOPE_IDENTITY();
    INSERT INTO EndpointRolePermission (EndpointId, RoleName) VALUES (@AttachmentsId, 'Publisher'), (@AttachmentsId, 'ADAdmin'), (@AttachmentsId, 'SuperUser');
    PRINT 'Added: POST /api/email/send-with-attachments';
END

IF NOT EXISTS (SELECT 1 FROM EndpointRegistry WHERE Route = '/api/email/send-with-links' AND HttpMethod = 'POST')
BEGIN
    INSERT INTO EndpointRegistry (HttpMethod, Route, EndpointName, Category, IsActive, Description)
    VALUES ('POST', '/api/email/send-with-links', 'SendEmailWithLinks', 'Email', 1, 'Send email with document links');

    DECLARE @LinksId INT = SCOPE_IDENTITY();
    INSERT INTO EndpointRolePermission (EndpointId, RoleName) VALUES (@LinksId, 'Publisher'), (@LinksId, 'ADAdmin'), (@LinksId, 'SuperUser');
    PRINT 'Added: POST /api/email/send-with-links';
END

-- DiagnosticEndpoints (6 endpoints - DEBUG ONLY)
IF NOT EXISTS (SELECT 1 FROM EndpointRegistry WHERE Route = '/api/diagnostic/db-connection' AND HttpMethod = 'GET')
BEGIN
    INSERT INTO EndpointRegistry (HttpMethod, Route, EndpointName, Category, IsActive, Description)
    VALUES ('GET', '/api/diagnostic/db-connection', 'TestDatabaseConnection', 'Diagnostic', 1, 'Test database connectivity (DEBUG)');

    DECLARE @DbConnId INT = SCOPE_IDENTITY();
    INSERT INTO EndpointRolePermission (EndpointId, RoleName) VALUES (@DbConnId, 'SuperUser');
    PRINT 'Added: GET /api/diagnostic/db-connection';
END

IF NOT EXISTS (SELECT 1 FROM EndpointRegistry WHERE Route = '/api/diagnostic/endpoint-registry' AND HttpMethod = 'GET')
BEGIN
    INSERT INTO EndpointRegistry (HttpMethod, Route, EndpointName, Category, IsActive, Description)
    VALUES ('GET', '/api/diagnostic/endpoint-registry', 'TestEndpointRegistryAccess', 'Diagnostic', 1, 'Test EndpointRegistry table access (DEBUG)');

    DECLARE @RegId INT = SCOPE_IDENTITY();
    INSERT INTO EndpointRolePermission (EndpointId, RoleName) VALUES (@RegId, 'SuperUser');
    PRINT 'Added: GET /api/diagnostic/endpoint-registry';
END

IF NOT EXISTS (SELECT 1 FROM EndpointRegistry WHERE Route = '/api/diagnostic/endpoint-role-permission' AND HttpMethod = 'GET')
BEGIN
    INSERT INTO EndpointRegistry (HttpMethod, Route, EndpointName, Category, IsActive, Description)
    VALUES ('GET', '/api/diagnostic/endpoint-role-permission', 'TestEndpointRolePermissionAccess', 'Diagnostic', 1, 'Test EndpointRolePermission table access (DEBUG)');

    DECLARE @PermId INT = SCOPE_IDENTITY();
    INSERT INTO EndpointRolePermission (EndpointId, RoleName) VALUES (@PermId, 'SuperUser');
    PRINT 'Added: GET /api/diagnostic/endpoint-role-permission';
END

IF NOT EXISTS (SELECT 1 FROM EndpointRegistry WHERE Route = '/api/diagnostic/permission-audit-log' AND HttpMethod = 'GET')
BEGIN
    INSERT INTO EndpointRegistry (HttpMethod, Route, EndpointName, Category, IsActive, Description)
    VALUES ('GET', '/api/diagnostic/permission-audit-log', 'TestPermissionAuditLogAccess', 'Diagnostic', 1, 'Test PermissionChangeAuditLog table access (DEBUG)');

    DECLARE @AuditId INT = SCOPE_IDENTITY();
    INSERT INTO EndpointRolePermission (EndpointId, RoleName) VALUES (@AuditId, 'SuperUser');
    PRINT 'Added: GET /api/diagnostic/permission-audit-log';
END

IF NOT EXISTS (SELECT 1 FROM EndpointRegistry WHERE Route = '/api/diagnostic/all-tables' AND HttpMethod = 'GET')
BEGIN
    INSERT INTO EndpointRegistry (HttpMethod, Route, EndpointName, Category, IsActive, Description)
    VALUES ('GET', '/api/diagnostic/all-tables', 'TestAllAuthorizationTables', 'Diagnostic', 1, 'Test all authorization tables access (DEBUG)');

    DECLARE @AllTablesId INT = SCOPE_IDENTITY();
    INSERT INTO EndpointRolePermission (EndpointId, RoleName) VALUES (@AllTablesId, 'SuperUser');
    PRINT 'Added: GET /api/diagnostic/all-tables';
END

IF NOT EXISTS (SELECT 1 FROM EndpointRegistry WHERE Route = '/api/diagnostic/test-authorization-service' AND HttpMethod = 'GET')
BEGIN
    INSERT INTO EndpointRegistry (HttpMethod, Route, EndpointName, Category, IsActive, Description)
    VALUES ('GET', '/api/diagnostic/test-authorization-service', 'TestEndpointAuthorizationService', 'Diagnostic', 1, 'Test EndpointAuthorizationService functionality (DEBUG)');

    DECLARE @AuthSvcId INT = SCOPE_IDENTITY();
    INSERT INTO EndpointRolePermission (EndpointId, RoleName) VALUES (@AuthSvcId, 'SuperUser');
    PRINT 'Added: GET /api/diagnostic/test-authorization-service';
END

-- AuditTrailEndpoints - Note: We migrated /api/audittrail/ but database has /api/audit-trail/
-- Need to check if these should be updated or if there's a route mismatch

PRINT 'Endpoint synchronization complete!';
PRINT 'Note: Some endpoints may have route mismatches between code and database.';
PRINT 'Please verify /api/audittrail/ vs /api/audit-trail/ routes.';
GO
