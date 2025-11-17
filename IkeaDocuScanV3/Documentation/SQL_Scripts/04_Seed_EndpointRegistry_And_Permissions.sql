-- =============================================
-- Script: 04_Seed_EndpointRegistry_And_Permissions.sql
-- Description: Seeds EndpointRegistry and EndpointRolePermission tables with all 126 API endpoints
--              Based on ROLE_EXTENSION_IMPLEMENTATION_PLAN.md Section 3.4 (Full Endpoint Matrix)
-- Date: 2025-11-17
-- Author: Role Extension Implementation
-- =============================================

USE [IkeaDocuScan]
GO

SET NOCOUNT ON;
GO

PRINT 'Starting endpoint registry and permissions seed...'
GO

-- =============================================
-- DOCUMENTS (10 endpoints) - All roles for GET, Publisher+ for POST/PUT, SuperUser for DELETE
-- =============================================
PRINT 'Seeding Documents endpoints...'
GO

DECLARE @EndpointId INT;

-- GET /api/documents/
INSERT INTO [dbo].[EndpointRegistry] ([HttpMethod], [Route], [EndpointName], [Description], [Category], [IsActive])
VALUES ('GET', '/api/documents/', 'GetDocuments', 'Get all documents', 'Documents', 1);
SET @EndpointId = SCOPE_IDENTITY();
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName])
VALUES (@EndpointId, 'Reader'), (@EndpointId, 'Publisher'), (@EndpointId, 'ADAdmin'), (@EndpointId, 'SuperUser');

-- GET /api/documents/{id}
INSERT INTO [dbo].[EndpointRegistry] ([HttpMethod], [Route], [EndpointName], [Description], [Category], [IsActive])
VALUES ('GET', '/api/documents/{id}', 'GetDocument', 'Get document by ID', 'Documents', 1);
SET @EndpointId = SCOPE_IDENTITY();
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName])
VALUES (@EndpointId, 'Reader'), (@EndpointId, 'Publisher'), (@EndpointId, 'ADAdmin'), (@EndpointId, 'SuperUser');

-- GET /api/documents/barcode/{barCode}
INSERT INTO [dbo].[EndpointRegistry] ([HttpMethod], [Route], [EndpointName], [Description], [Category], [IsActive])
VALUES ('GET', '/api/documents/barcode/{barCode}', 'GetDocumentByBarcode', 'Get document by barcode', 'Documents', 1);
SET @EndpointId = SCOPE_IDENTITY();
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName])
VALUES (@EndpointId, 'Reader'), (@EndpointId, 'Publisher'), (@EndpointId, 'ADAdmin'), (@EndpointId, 'SuperUser');

-- POST /api/documents/by-ids
INSERT INTO [dbo].[EndpointRegistry] ([HttpMethod], [Route], [EndpointName], [Description], [Category], [IsActive])
VALUES ('POST', '/api/documents/by-ids', 'GetDocumentsByIds', 'Get documents by IDs', 'Documents', 1);
SET @EndpointId = SCOPE_IDENTITY();
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName])
VALUES (@EndpointId, 'Reader'), (@EndpointId, 'Publisher'), (@EndpointId, 'ADAdmin'), (@EndpointId, 'SuperUser');

-- POST /api/documents/
INSERT INTO [dbo].[EndpointRegistry] ([HttpMethod], [Route], [EndpointName], [Description], [Category], [IsActive])
VALUES ('POST', '/api/documents/', 'CreateDocument', 'Create new document', 'Documents', 1);
SET @EndpointId = SCOPE_IDENTITY();
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName])
VALUES (@EndpointId, 'Publisher'), (@EndpointId, 'ADAdmin'), (@EndpointId, 'SuperUser');

-- PUT /api/documents/{id}
INSERT INTO [dbo].[EndpointRegistry] ([HttpMethod], [Route], [EndpointName], [Description], [Category], [IsActive])
VALUES ('PUT', '/api/documents/{id}', 'UpdateDocument', 'Update document', 'Documents', 1);
SET @EndpointId = SCOPE_IDENTITY();
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName])
VALUES (@EndpointId, 'Publisher'), (@EndpointId, 'ADAdmin'), (@EndpointId, 'SuperUser');

-- DELETE /api/documents/{id}
INSERT INTO [dbo].[EndpointRegistry] ([HttpMethod], [Route], [EndpointName], [Description], [Category], [IsActive])
VALUES ('DELETE', '/api/documents/{id}', 'DeleteDocument', 'Delete document', 'Documents', 1);
SET @EndpointId = SCOPE_IDENTITY();
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName])
VALUES (@EndpointId, 'SuperUser');

-- POST /api/documents/search
INSERT INTO [dbo].[EndpointRegistry] ([HttpMethod], [Route], [EndpointName], [Description], [Category], [IsActive])
VALUES ('POST', '/api/documents/search', 'SearchDocuments', 'Search documents', 'Documents', 1);
SET @EndpointId = SCOPE_IDENTITY();
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName])
VALUES (@EndpointId, 'Reader'), (@EndpointId, 'Publisher'), (@EndpointId, 'ADAdmin'), (@EndpointId, 'SuperUser');

-- GET /api/documents/{id}/stream
INSERT INTO [dbo].[EndpointRegistry] ([HttpMethod], [Route], [EndpointName], [Description], [Category], [IsActive])
VALUES ('GET', '/api/documents/{id}/stream', 'StreamDocument', 'Stream document file', 'Documents', 1);
SET @EndpointId = SCOPE_IDENTITY();
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName])
VALUES (@EndpointId, 'Reader'), (@EndpointId, 'Publisher'), (@EndpointId, 'ADAdmin'), (@EndpointId, 'SuperUser');

-- GET /api/documents/{id}/download
INSERT INTO [dbo].[EndpointRegistry] ([HttpMethod], [Route], [EndpointName], [Description], [Category], [IsActive])
VALUES ('GET', '/api/documents/{id}/download', 'DownloadDocument', 'Download document file', 'Documents', 1);
SET @EndpointId = SCOPE_IDENTITY();
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName])
VALUES (@EndpointId, 'Reader'), (@EndpointId, 'Publisher'), (@EndpointId, 'ADAdmin'), (@EndpointId, 'SuperUser');

PRINT 'Documents endpoints seeded (10 endpoints)'
GO

-- =============================================
-- COUNTER PARTIES (7 endpoints) - Publisher+ only (Reader access REMOVED)
-- =============================================
PRINT 'Seeding Counter Parties endpoints...'
GO

DECLARE @EndpointId INT;

-- GET /api/counterparties/
INSERT INTO [dbo].[EndpointRegistry] ([HttpMethod], [Route], [EndpointName], [Description], [Category], [IsActive])
VALUES ('GET', '/api/counterparties/', 'GetCounterParties', 'Get all counter parties', 'CounterParties', 1);
SET @EndpointId = SCOPE_IDENTITY();
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName])
VALUES (@EndpointId, 'Publisher'), (@EndpointId, 'ADAdmin'), (@EndpointId, 'SuperUser');

-- GET /api/counterparties/search
INSERT INTO [dbo].[EndpointRegistry] ([HttpMethod], [Route], [EndpointName], [Description], [Category], [IsActive])
VALUES ('GET', '/api/counterparties/search', 'SearchCounterParties', 'Search counter parties', 'CounterParties', 1);
SET @EndpointId = SCOPE_IDENTITY();
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName])
VALUES (@EndpointId, 'Publisher'), (@EndpointId, 'ADAdmin'), (@EndpointId, 'SuperUser');

-- GET /api/counterparties/{id}
INSERT INTO [dbo].[EndpointRegistry] ([HttpMethod], [Route], [EndpointName], [Description], [Category], [IsActive])
VALUES ('GET', '/api/counterparties/{id}', 'GetCounterParty', 'Get counter party by ID', 'CounterParties', 1);
SET @EndpointId = SCOPE_IDENTITY();
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName])
VALUES (@EndpointId, 'Publisher'), (@EndpointId, 'ADAdmin'), (@EndpointId, 'SuperUser');

-- POST /api/counterparties/
INSERT INTO [dbo].[EndpointRegistry] ([HttpMethod], [Route], [EndpointName], [Description], [Category], [IsActive])
VALUES ('POST', '/api/counterparties/', 'CreateCounterParty', 'Create counter party', 'CounterParties', 1);
SET @EndpointId = SCOPE_IDENTITY();
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName])
VALUES (@EndpointId, 'SuperUser');

-- PUT /api/counterparties/{id}
INSERT INTO [dbo].[EndpointRegistry] ([HttpMethod], [Route], [EndpointName], [Description], [Category], [IsActive])
VALUES ('PUT', '/api/counterparties/{id}', 'UpdateCounterParty', 'Update counter party', 'CounterParties', 1);
SET @EndpointId = SCOPE_IDENTITY();
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName])
VALUES (@EndpointId, 'SuperUser');

-- DELETE /api/counterparties/{id}
INSERT INTO [dbo].[EndpointRegistry] ([HttpMethod], [Route], [EndpointName], [Description], [Category], [IsActive])
VALUES ('DELETE', '/api/counterparties/{id}', 'DeleteCounterParty', 'Delete counter party', 'CounterParties', 1);
SET @EndpointId = SCOPE_IDENTITY();
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName])
VALUES (@EndpointId, 'SuperUser');

-- GET /api/counterparties/{id}/usage
INSERT INTO [dbo].[EndpointRegistry] ([HttpMethod], [Route], [EndpointName], [Description], [Category], [IsActive])
VALUES ('GET', '/api/counterparties/{id}/usage', 'GetCounterPartyUsage', 'Get counter party usage count', 'CounterParties', 1);
SET @EndpointId = SCOPE_IDENTITY();
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName])
VALUES (@EndpointId, 'Publisher'), (@EndpointId, 'ADAdmin'), (@EndpointId, 'SuperUser');

PRINT 'Counter Parties endpoints seeded (7 endpoints)'
GO

-- =============================================
-- USER PERMISSIONS (11 endpoints) - Mixed permissions (self-access for all, ADAdmin read-only, SuperUser full)
-- =============================================
PRINT 'Seeding User Permissions endpoints...'
GO

DECLARE @EndpointId INT;

-- GET /api/userpermissions/
INSERT INTO [dbo].[EndpointRegistry] ([HttpMethod], [Route], [EndpointName], [Description], [Category], [IsActive])
VALUES ('GET', '/api/userpermissions/', 'GetUserPermissions', 'Get all user permissions', 'UserPermissions', 1);
SET @EndpointId = SCOPE_IDENTITY();
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName])
VALUES (@EndpointId, 'ADAdmin'), (@EndpointId, 'SuperUser');

-- GET /api/userpermissions/users
INSERT INTO [dbo].[EndpointRegistry] ([HttpMethod], [Route], [EndpointName], [Description], [Category], [IsActive])
VALUES ('GET', '/api/userpermissions/users', 'GetUsers', 'Get all users', 'UserPermissions', 1);
SET @EndpointId = SCOPE_IDENTITY();
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName])
VALUES (@EndpointId, 'ADAdmin'), (@EndpointId, 'SuperUser');

-- GET /api/userpermissions/{id} - All roles (self-access enforced in service layer)
INSERT INTO [dbo].[EndpointRegistry] ([HttpMethod], [Route], [EndpointName], [Description], [Category], [IsActive])
VALUES ('GET', '/api/userpermissions/{id}', 'GetUserPermission', 'Get user permission by ID', 'UserPermissions', 1);
SET @EndpointId = SCOPE_IDENTITY();
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName])
VALUES (@EndpointId, 'Reader'), (@EndpointId, 'Publisher'), (@EndpointId, 'ADAdmin'), (@EndpointId, 'SuperUser');

-- GET /api/userpermissions/user/{userId} - All roles (self-access enforced in service layer)
INSERT INTO [dbo].[EndpointRegistry] ([HttpMethod], [Route], [EndpointName], [Description], [Category], [IsActive])
VALUES ('GET', '/api/userpermissions/user/{userId}', 'GetUserPermissionsByUserId', 'Get user permissions by user ID', 'UserPermissions', 1);
SET @EndpointId = SCOPE_IDENTITY();
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName])
VALUES (@EndpointId, 'Reader'), (@EndpointId, 'Publisher'), (@EndpointId, 'ADAdmin'), (@EndpointId, 'SuperUser');

-- GET /api/userpermissions/me
INSERT INTO [dbo].[EndpointRegistry] ([HttpMethod], [Route], [EndpointName], [Description], [Category], [IsActive])
VALUES ('GET', '/api/userpermissions/me', 'GetMyPermissions', 'Get current user permissions', 'UserPermissions', 1);
SET @EndpointId = SCOPE_IDENTITY();
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName])
VALUES (@EndpointId, 'Reader'), (@EndpointId, 'Publisher'), (@EndpointId, 'ADAdmin'), (@EndpointId, 'SuperUser');

-- POST /api/userpermissions/
INSERT INTO [dbo].[EndpointRegistry] ([HttpMethod], [Route], [EndpointName], [Description], [Category], [IsActive])
VALUES ('POST', '/api/userpermissions/', 'CreateUserPermission', 'Create user permission', 'UserPermissions', 1);
SET @EndpointId = SCOPE_IDENTITY();
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName])
VALUES (@EndpointId, 'SuperUser');

-- PUT /api/userpermissions/{id}
INSERT INTO [dbo].[EndpointRegistry] ([HttpMethod], [Route], [EndpointName], [Description], [Category], [IsActive])
VALUES ('PUT', '/api/userpermissions/{id}', 'UpdateUserPermission', 'Update user permission', 'UserPermissions', 1);
SET @EndpointId = SCOPE_IDENTITY();
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName])
VALUES (@EndpointId, 'SuperUser');

-- DELETE /api/userpermissions/{id}
INSERT INTO [dbo].[EndpointRegistry] ([HttpMethod], [Route], [EndpointName], [Description], [Category], [IsActive])
VALUES ('DELETE', '/api/userpermissions/{id}', 'DeleteUserPermission', 'Delete user permission', 'UserPermissions', 1);
SET @EndpointId = SCOPE_IDENTITY();
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName])
VALUES (@EndpointId, 'SuperUser');

-- DELETE /api/userpermissions/user/{userId}
INSERT INTO [dbo].[EndpointRegistry] ([HttpMethod], [Route], [EndpointName], [Description], [Category], [IsActive])
VALUES ('DELETE', '/api/userpermissions/user/{userId}', 'DeleteUser', 'Delete user', 'UserPermissions', 1);
SET @EndpointId = SCOPE_IDENTITY();
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName])
VALUES (@EndpointId, 'SuperUser');

-- POST /api/userpermissions/user
INSERT INTO [dbo].[EndpointRegistry] ([HttpMethod], [Route], [EndpointName], [Description], [Category], [IsActive])
VALUES ('POST', '/api/userpermissions/user', 'CreateUser', 'Create user', 'UserPermissions', 1);
SET @EndpointId = SCOPE_IDENTITY();
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName])
VALUES (@EndpointId, 'SuperUser');

-- PUT /api/userpermissions/user/{userId}
INSERT INTO [dbo].[EndpointRegistry] ([HttpMethod], [Route], [EndpointName], [Description], [Category], [IsActive])
VALUES ('PUT', '/api/userpermissions/user/{userId}', 'UpdateUser', 'Update user', 'UserPermissions', 1);
SET @EndpointId = SCOPE_IDENTITY();
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName])
VALUES (@EndpointId, 'SuperUser');

PRINT 'User Permissions endpoints seeded (11 endpoints)'
GO

-- =============================================
-- CONFIGURATION (19 endpoints) - ADAdmin read access for 5 GET endpoints, SuperUser for all
-- =============================================
PRINT 'Seeding Configuration endpoints...'
GO

DECLARE @EndpointId INT;

-- GET /api/configuration/email-recipients
INSERT INTO [dbo].[EndpointRegistry] ([HttpMethod], [Route], [EndpointName], [Description], [Category], [IsActive])
VALUES ('GET', '/api/configuration/email-recipients', 'GetEmailRecipients', 'Get email recipients', 'Configuration', 1);
SET @EndpointId = SCOPE_IDENTITY();
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName])
VALUES (@EndpointId, 'ADAdmin'), (@EndpointId, 'SuperUser');

-- GET /api/configuration/email-recipients/{groupKey}
INSERT INTO [dbo].[EndpointRegistry] ([HttpMethod], [Route], [EndpointName], [Description], [Category], [IsActive])
VALUES ('GET', '/api/configuration/email-recipients/{groupKey}', 'GetEmailRecipientsByGroup', 'Get email recipients by group', 'Configuration', 1);
SET @EndpointId = SCOPE_IDENTITY();
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName])
VALUES (@EndpointId, 'ADAdmin'), (@EndpointId, 'SuperUser');

-- POST /api/configuration/email-recipients/{groupKey}
INSERT INTO [dbo].[EndpointRegistry] ([HttpMethod], [Route], [EndpointName], [Description], [Category], [IsActive])
VALUES ('POST', '/api/configuration/email-recipients/{groupKey}', 'SaveEmailRecipients', 'Save email recipients', 'Configuration', 1);
SET @EndpointId = SCOPE_IDENTITY();
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName])
VALUES (@EndpointId, 'SuperUser');

-- GET /api/configuration/email-templates
INSERT INTO [dbo].[EndpointRegistry] ([HttpMethod], [Route], [EndpointName], [Description], [Category], [IsActive])
VALUES ('GET', '/api/configuration/email-templates', 'GetEmailTemplates', 'Get all email templates', 'Configuration', 1);
SET @EndpointId = SCOPE_IDENTITY();
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName])
VALUES (@EndpointId, 'ADAdmin'), (@EndpointId, 'SuperUser');

-- GET /api/configuration/email-templates/{key}
INSERT INTO [dbo].[EndpointRegistry] ([HttpMethod], [Route], [EndpointName], [Description], [Category], [IsActive])
VALUES ('GET', '/api/configuration/email-templates/{key}', 'GetEmailTemplate', 'Get email template by key', 'Configuration', 1);
SET @EndpointId = SCOPE_IDENTITY();
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName])
VALUES (@EndpointId, 'ADAdmin'), (@EndpointId, 'SuperUser');

-- POST /api/configuration/email-templates
INSERT INTO [dbo].[EndpointRegistry] ([HttpMethod], [Route], [EndpointName], [Description], [Category], [IsActive])
VALUES ('POST', '/api/configuration/email-templates', 'CreateEmailTemplate', 'Create email template', 'Configuration', 1);
SET @EndpointId = SCOPE_IDENTITY();
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName])
VALUES (@EndpointId, 'SuperUser');

-- PUT /api/configuration/email-templates/{id}
INSERT INTO [dbo].[EndpointRegistry] ([HttpMethod], [Route], [EndpointName], [Description], [Category], [IsActive])
VALUES ('PUT', '/api/configuration/email-templates/{id}', 'UpdateEmailTemplate', 'Update email template', 'Configuration', 1);
SET @EndpointId = SCOPE_IDENTITY();
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName])
VALUES (@EndpointId, 'SuperUser');

-- DELETE /api/configuration/email-templates/{id}
INSERT INTO [dbo].[EndpointRegistry] ([HttpMethod], [Route], [EndpointName], [Description], [Category], [IsActive])
VALUES ('DELETE', '/api/configuration/email-templates/{id}', 'DeleteEmailTemplate', 'Delete email template', 'Configuration', 1);
SET @EndpointId = SCOPE_IDENTITY();
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName])
VALUES (@EndpointId, 'SuperUser');

-- GET /api/configuration/sections
INSERT INTO [dbo].[EndpointRegistry] ([HttpMethod], [Route], [EndpointName], [Description], [Category], [IsActive])
VALUES ('GET', '/api/configuration/sections', 'GetConfigSections', 'Get configuration sections', 'Configuration', 1);
SET @EndpointId = SCOPE_IDENTITY();
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName])
VALUES (@EndpointId, 'ADAdmin'), (@EndpointId, 'SuperUser');

-- GET /api/configuration/{section}/{key}
INSERT INTO [dbo].[EndpointRegistry] ([HttpMethod], [Route], [EndpointName], [Description], [Category], [IsActive])
VALUES ('GET', '/api/configuration/{section}/{key}', 'GetConfigValue', 'Get configuration value', 'Configuration', 1);
SET @EndpointId = SCOPE_IDENTITY();
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName])
VALUES (@EndpointId, 'SuperUser');

-- POST /api/configuration/{section}/{key}
INSERT INTO [dbo].[EndpointRegistry] ([HttpMethod], [Route], [EndpointName], [Description], [Category], [IsActive])
VALUES ('POST', '/api/configuration/{section}/{key}', 'SetConfigValue', 'Set configuration value', 'Configuration', 1);
SET @EndpointId = SCOPE_IDENTITY();
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName])
VALUES (@EndpointId, 'SuperUser');

-- POST /api/configuration/smtp
INSERT INTO [dbo].[EndpointRegistry] ([HttpMethod], [Route], [EndpointName], [Description], [Category], [IsActive])
VALUES ('POST', '/api/configuration/smtp', 'SaveSmtpConfig', 'Save SMTP configuration', 'Configuration', 1);
SET @EndpointId = SCOPE_IDENTITY();
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName])
VALUES (@EndpointId, 'SuperUser');

-- POST /api/configuration/test-smtp
INSERT INTO [dbo].[EndpointRegistry] ([HttpMethod], [Route], [EndpointName], [Description], [Category], [IsActive])
VALUES ('POST', '/api/configuration/test-smtp', 'TestSmtpConfig', 'Test SMTP configuration', 'Configuration', 1);
SET @EndpointId = SCOPE_IDENTITY();
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName])
VALUES (@EndpointId, 'SuperUser');

-- POST /api/configuration/reload
INSERT INTO [dbo].[EndpointRegistry] ([HttpMethod], [Route], [EndpointName], [Description], [Category], [IsActive])
VALUES ('POST', '/api/configuration/reload', 'ReloadConfiguration', 'Reload configuration', 'Configuration', 1);
SET @EndpointId = SCOPE_IDENTITY();
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName])
VALUES (@EndpointId, 'SuperUser');

-- POST /api/configuration/migrate
INSERT INTO [dbo].[EndpointRegistry] ([HttpMethod], [Route], [EndpointName], [Description], [Category], [IsActive])
VALUES ('POST', '/api/configuration/migrate', 'MigrateConfiguration', 'Migrate configuration', 'Configuration', 1);
SET @EndpointId = SCOPE_IDENTITY();
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName])
VALUES (@EndpointId, 'SuperUser');

-- POST /api/configuration/email-templates/preview
INSERT INTO [dbo].[EndpointRegistry] ([HttpMethod], [Route], [EndpointName], [Description], [Category], [IsActive])
VALUES ('POST', '/api/configuration/email-templates/preview', 'PreviewEmailTemplate', 'Preview email template', 'Configuration', 1);
SET @EndpointId = SCOPE_IDENTITY();
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName])
VALUES (@EndpointId, 'SuperUser');

-- GET /api/configuration/email-templates/placeholders
INSERT INTO [dbo].[EndpointRegistry] ([HttpMethod], [Route], [EndpointName], [Description], [Category], [IsActive])
VALUES ('GET', '/api/configuration/email-templates/placeholders', 'GetEmailPlaceholders', 'Get email template placeholders', 'Configuration', 1);
SET @EndpointId = SCOPE_IDENTITY();
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName])
VALUES (@EndpointId, 'SuperUser');

-- GET /api/configuration/email-templates/diagnostic/DocumentAttachment
INSERT INTO [dbo].[EndpointRegistry] ([HttpMethod], [Route], [EndpointName], [Description], [Category], [IsActive])
VALUES ('GET', '/api/configuration/email-templates/diagnostic/DocumentAttachment', 'DiagnosticEmailTemplate', 'Diagnostic email template', 'Configuration', 1);
SET @EndpointId = SCOPE_IDENTITY();
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName])
VALUES (@EndpointId, 'SuperUser');

-- Missing endpoint from matrix but likely needed
-- GET /api/configuration/all-sections
INSERT INTO [dbo].[EndpointRegistry] ([HttpMethod], [Route], [EndpointName], [Description], [Category], [IsActive])
VALUES ('GET', '/api/configuration/all-sections', 'GetAllConfigSections', 'Get all configuration sections and values', 'Configuration', 1);
SET @EndpointId = SCOPE_IDENTITY();
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName])
VALUES (@EndpointId, 'SuperUser');

PRINT 'Configuration endpoints seeded (19 endpoints)'
GO

-- =============================================
-- LOG VIEWER (5 endpoints) - ADAdmin + SuperUser
-- =============================================
PRINT 'Seeding Log Viewer endpoints...'
GO

DECLARE @EndpointId INT;

-- POST /api/logs/search
INSERT INTO [dbo].[EndpointRegistry] ([HttpMethod], [Route], [EndpointName], [Description], [Category], [IsActive])
VALUES ('POST', '/api/logs/search', 'SearchLogs', 'Search system logs', 'Logs', 1);
SET @EndpointId = SCOPE_IDENTITY();
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName])
VALUES (@EndpointId, 'ADAdmin'), (@EndpointId, 'SuperUser');

-- GET /api/logs/export
INSERT INTO [dbo].[EndpointRegistry] ([HttpMethod], [Route], [EndpointName], [Description], [Category], [IsActive])
VALUES ('GET', '/api/logs/export', 'ExportLogs', 'Export logs to Excel', 'Logs', 1);
SET @EndpointId = SCOPE_IDENTITY();
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName])
VALUES (@EndpointId, 'ADAdmin'), (@EndpointId, 'SuperUser');

-- GET /api/logs/dates
INSERT INTO [dbo].[EndpointRegistry] ([HttpMethod], [Route], [EndpointName], [Description], [Category], [IsActive])
VALUES ('GET', '/api/logs/dates', 'GetLogDates', 'Get available log dates', 'Logs', 1);
SET @EndpointId = SCOPE_IDENTITY();
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName])
VALUES (@EndpointId, 'ADAdmin'), (@EndpointId, 'SuperUser');

-- GET /api/logs/sources
INSERT INTO [dbo].[EndpointRegistry] ([HttpMethod], [Route], [EndpointName], [Description], [Category], [IsActive])
VALUES ('GET', '/api/logs/sources', 'GetLogSources', 'Get log sources', 'Logs', 1);
SET @EndpointId = SCOPE_IDENTITY();
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName])
VALUES (@EndpointId, 'ADAdmin'), (@EndpointId, 'SuperUser');

-- GET /api/logs/statistics
INSERT INTO [dbo].[EndpointRegistry] ([HttpMethod], [Route], [EndpointName], [Description], [Category], [IsActive])
VALUES ('GET', '/api/logs/statistics', 'GetLogStatistics', 'Get log statistics', 'Logs', 1);
SET @EndpointId = SCOPE_IDENTITY();
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName])
VALUES (@EndpointId, 'ADAdmin'), (@EndpointId, 'SuperUser');

PRINT 'Log Viewer endpoints seeded (5 endpoints)'
GO

-- =============================================
-- SCANNED FILES (6 endpoints) - Publisher+ only (Reader access REMOVED)
-- =============================================
PRINT 'Seeding Scanned Files endpoints...'
GO

DECLARE @EndpointId INT;

-- GET /api/scannedfiles/
INSERT INTO [dbo].[EndpointRegistry] ([HttpMethod], [Route], [EndpointName], [Description], [Category], [IsActive])
VALUES ('GET', '/api/scannedfiles/', 'GetScannedFiles', 'Get all scanned files', 'ScannedFiles', 1);
SET @EndpointId = SCOPE_IDENTITY();
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName])
VALUES (@EndpointId, 'Publisher'), (@EndpointId, 'ADAdmin'), (@EndpointId, 'SuperUser');

-- GET /api/scannedfiles/{fileName}
INSERT INTO [dbo].[EndpointRegistry] ([HttpMethod], [Route], [EndpointName], [Description], [Category], [IsActive])
VALUES ('GET', '/api/scannedfiles/{fileName}', 'GetScannedFile', 'Get scanned file details', 'ScannedFiles', 1);
SET @EndpointId = SCOPE_IDENTITY();
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName])
VALUES (@EndpointId, 'Publisher'), (@EndpointId, 'ADAdmin'), (@EndpointId, 'SuperUser');

-- GET /api/scannedfiles/{fileName}/content
INSERT INTO [dbo].[EndpointRegistry] ([HttpMethod], [Route], [EndpointName], [Description], [Category], [IsActive])
VALUES ('GET', '/api/scannedfiles/{fileName}/content', 'GetScannedFileContent', 'Get scanned file content', 'ScannedFiles', 1);
SET @EndpointId = SCOPE_IDENTITY();
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName])
VALUES (@EndpointId, 'Publisher'), (@EndpointId, 'ADAdmin'), (@EndpointId, 'SuperUser');

-- GET /api/scannedfiles/{fileName}/exists
INSERT INTO [dbo].[EndpointRegistry] ([HttpMethod], [Route], [EndpointName], [Description], [Category], [IsActive])
VALUES ('GET', '/api/scannedfiles/{fileName}/exists', 'CheckScannedFileExists', 'Check if scanned file exists', 'ScannedFiles', 1);
SET @EndpointId = SCOPE_IDENTITY();
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName])
VALUES (@EndpointId, 'Publisher'), (@EndpointId, 'ADAdmin'), (@EndpointId, 'SuperUser');

-- GET /api/scannedfiles/{fileName}/stream
INSERT INTO [dbo].[EndpointRegistry] ([HttpMethod], [Route], [EndpointName], [Description], [Category], [IsActive])
VALUES ('GET', '/api/scannedfiles/{fileName}/stream', 'StreamScannedFile', 'Stream scanned file', 'ScannedFiles', 1);
SET @EndpointId = SCOPE_IDENTITY();
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName])
VALUES (@EndpointId, 'Publisher'), (@EndpointId, 'ADAdmin'), (@EndpointId, 'SuperUser');

-- DELETE /api/scannedfiles/{fileName}
INSERT INTO [dbo].[EndpointRegistry] ([HttpMethod], [Route], [EndpointName], [Description], [Category], [IsActive])
VALUES ('DELETE', '/api/scannedfiles/{fileName}', 'DeleteScannedFile', 'Delete scanned file', 'ScannedFiles', 1);
SET @EndpointId = SCOPE_IDENTITY();
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName])
VALUES (@EndpointId, 'SuperUser');

PRINT 'Scanned Files endpoints seeded (6 endpoints)'
GO

-- =============================================
-- ACTION REMINDERS (3 endpoints) - Publisher+ only (Reader access REMOVED)
-- =============================================
PRINT 'Seeding Action Reminders endpoints...'
GO

DECLARE @EndpointId INT;

-- GET /api/action-reminders/
INSERT INTO [dbo].[EndpointRegistry] ([HttpMethod], [Route], [EndpointName], [Description], [Category], [IsActive])
VALUES ('GET', '/api/action-reminders/', 'GetActionReminders', 'Get all action reminders', 'ActionReminders', 1);
SET @EndpointId = SCOPE_IDENTITY();
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName])
VALUES (@EndpointId, 'Publisher'), (@EndpointId, 'ADAdmin'), (@EndpointId, 'SuperUser');

-- GET /api/action-reminders/count
INSERT INTO [dbo].[EndpointRegistry] ([HttpMethod], [Route], [EndpointName], [Description], [Category], [IsActive])
VALUES ('GET', '/api/action-reminders/count', 'GetActionRemindersCount', 'Get action reminders count', 'ActionReminders', 1);
SET @EndpointId = SCOPE_IDENTITY();
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName])
VALUES (@EndpointId, 'Publisher'), (@EndpointId, 'ADAdmin'), (@EndpointId, 'SuperUser');

-- GET /api/action-reminders/date/{date}
INSERT INTO [dbo].[EndpointRegistry] ([HttpMethod], [Route], [EndpointName], [Description], [Category], [IsActive])
VALUES ('GET', '/api/action-reminders/date/{date}', 'GetActionRemindersByDate', 'Get action reminders by date', 'ActionReminders', 1);
SET @EndpointId = SCOPE_IDENTITY();
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName])
VALUES (@EndpointId, 'Publisher'), (@EndpointId, 'ADAdmin'), (@EndpointId, 'SuperUser');

PRINT 'Action Reminders endpoints seeded (3 endpoints)'
GO

-- =============================================
-- REPORTS (14 endpoints) - Publisher+ only (Reader access REMOVED)
-- =============================================
PRINT 'Seeding Reports endpoints...'
GO

DECLARE @EndpointId INT;

-- GET /api/reports/barcode-gaps
INSERT INTO [dbo].[EndpointRegistry] ([HttpMethod], [Route], [EndpointName], [Description], [Category], [IsActive])
VALUES ('GET', '/api/reports/barcode-gaps', 'GetBarcodeGaps', 'Get barcode gaps report', 'Reports', 1);
SET @EndpointId = SCOPE_IDENTITY();
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName])
VALUES (@EndpointId, 'Publisher'), (@EndpointId, 'ADAdmin'), (@EndpointId, 'SuperUser');

-- GET /api/reports/duplicate-documents
INSERT INTO [dbo].[EndpointRegistry] ([HttpMethod], [Route], [EndpointName], [Description], [Category], [IsActive])
VALUES ('GET', '/api/reports/duplicate-documents', 'GetDuplicateDocuments', 'Get duplicate documents report', 'Reports', 1);
SET @EndpointId = SCOPE_IDENTITY();
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName])
VALUES (@EndpointId, 'Publisher'), (@EndpointId, 'ADAdmin'), (@EndpointId, 'SuperUser');

-- GET /api/reports/unlinked-registrations
INSERT INTO [dbo].[EndpointRegistry] ([HttpMethod], [Route], [EndpointName], [Description], [Category], [IsActive])
VALUES ('GET', '/api/reports/unlinked-registrations', 'GetUnlinkedRegistrations', 'Get unlinked registrations report', 'Reports', 1);
SET @EndpointId = SCOPE_IDENTITY();
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName])
VALUES (@EndpointId, 'Publisher'), (@EndpointId, 'ADAdmin'), (@EndpointId, 'SuperUser');

-- GET /api/reports/scan-copies
INSERT INTO [dbo].[EndpointRegistry] ([HttpMethod], [Route], [EndpointName], [Description], [Category], [IsActive])
VALUES ('GET', '/api/reports/scan-copies', 'GetScanCopies', 'Get scan copies report', 'Reports', 1);
SET @EndpointId = SCOPE_IDENTITY();
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName])
VALUES (@EndpointId, 'Publisher'), (@EndpointId, 'ADAdmin'), (@EndpointId, 'SuperUser');

-- GET /api/reports/suppliers
INSERT INTO [dbo].[EndpointRegistry] ([HttpMethod], [Route], [EndpointName], [Description], [Category], [IsActive])
VALUES ('GET', '/api/reports/suppliers', 'GetSuppliers', 'Get suppliers report', 'Reports', 1);
SET @EndpointId = SCOPE_IDENTITY();
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName])
VALUES (@EndpointId, 'Publisher'), (@EndpointId, 'ADAdmin'), (@EndpointId, 'SuperUser');

-- GET /api/reports/all-documents
INSERT INTO [dbo].[EndpointRegistry] ([HttpMethod], [Route], [EndpointName], [Description], [Category], [IsActive])
VALUES ('GET', '/api/reports/all-documents', 'GetAllDocuments', 'Get all documents report', 'Reports', 1);
SET @EndpointId = SCOPE_IDENTITY();
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName])
VALUES (@EndpointId, 'Publisher'), (@EndpointId, 'ADAdmin'), (@EndpointId, 'SuperUser');

-- GET /api/reports/barcode-gaps/excel
INSERT INTO [dbo].[EndpointRegistry] ([HttpMethod], [Route], [EndpointName], [Description], [Category], [IsActive])
VALUES ('GET', '/api/reports/barcode-gaps/excel', 'ExportBarcodeGapsExcel', 'Export barcode gaps to Excel', 'Reports', 1);
SET @EndpointId = SCOPE_IDENTITY();
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName])
VALUES (@EndpointId, 'Publisher'), (@EndpointId, 'ADAdmin'), (@EndpointId, 'SuperUser');

-- GET /api/reports/duplicate-documents/excel
INSERT INTO [dbo].[EndpointRegistry] ([HttpMethod], [Route], [EndpointName], [Description], [Category], [IsActive])
VALUES ('GET', '/api/reports/duplicate-documents/excel', 'ExportDuplicateDocumentsExcel', 'Export duplicate documents to Excel', 'Reports', 1);
SET @EndpointId = SCOPE_IDENTITY();
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName])
VALUES (@EndpointId, 'Publisher'), (@EndpointId, 'ADAdmin'), (@EndpointId, 'SuperUser');

-- GET /api/reports/unlinked-registrations/excel
INSERT INTO [dbo].[EndpointRegistry] ([HttpMethod], [Route], [EndpointName], [Description], [Category], [IsActive])
VALUES ('GET', '/api/reports/unlinked-registrations/excel', 'ExportUnlinkedRegistrationsExcel', 'Export unlinked registrations to Excel', 'Reports', 1);
SET @EndpointId = SCOPE_IDENTITY();
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName])
VALUES (@EndpointId, 'Publisher'), (@EndpointId, 'ADAdmin'), (@EndpointId, 'SuperUser');

-- GET /api/reports/scan-copies/excel
INSERT INTO [dbo].[EndpointRegistry] ([HttpMethod], [Route], [EndpointName], [Description], [Category], [IsActive])
VALUES ('GET', '/api/reports/scan-copies/excel', 'ExportScanCopiesExcel', 'Export scan copies to Excel', 'Reports', 1);
SET @EndpointId = SCOPE_IDENTITY();
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName])
VALUES (@EndpointId, 'Publisher'), (@EndpointId, 'ADAdmin'), (@EndpointId, 'SuperUser');

-- GET /api/reports/suppliers/excel
INSERT INTO [dbo].[EndpointRegistry] ([HttpMethod], [Route], [EndpointName], [Description], [Category], [IsActive])
VALUES ('GET', '/api/reports/suppliers/excel', 'ExportSuppliersExcel', 'Export suppliers to Excel', 'Reports', 1);
SET @EndpointId = SCOPE_IDENTITY();
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName])
VALUES (@EndpointId, 'Publisher'), (@EndpointId, 'ADAdmin'), (@EndpointId, 'SuperUser');

-- GET /api/reports/all-documents/excel
INSERT INTO [dbo].[EndpointRegistry] ([HttpMethod], [Route], [EndpointName], [Description], [Category], [IsActive])
VALUES ('GET', '/api/reports/all-documents/excel', 'ExportAllDocumentsExcel', 'Export all documents to Excel', 'Reports', 1);
SET @EndpointId = SCOPE_IDENTITY();
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName])
VALUES (@EndpointId, 'Publisher'), (@EndpointId, 'ADAdmin'), (@EndpointId, 'SuperUser');

-- POST /api/reports/documents/search/excel
INSERT INTO [dbo].[EndpointRegistry] ([HttpMethod], [Route], [EndpointName], [Description], [Category], [IsActive])
VALUES ('POST', '/api/reports/documents/search/excel', 'ExportSearchResultsExcel', 'Export document search results to Excel', 'Reports', 1);
SET @EndpointId = SCOPE_IDENTITY();
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName])
VALUES (@EndpointId, 'Publisher'), (@EndpointId, 'ADAdmin'), (@EndpointId, 'SuperUser');

-- POST /api/reports/documents/selected/excel
INSERT INTO [dbo].[EndpointRegistry] ([HttpMethod], [Route], [EndpointName], [Description], [Category], [IsActive])
VALUES ('POST', '/api/reports/documents/selected/excel', 'ExportSelectedDocumentsExcel', 'Export selected documents to Excel', 'Reports', 1);
SET @EndpointId = SCOPE_IDENTITY();
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName])
VALUES (@EndpointId, 'Publisher'), (@EndpointId, 'ADAdmin'), (@EndpointId, 'SuperUser');

PRINT 'Reports endpoints seeded (14 endpoints)'
GO

-- =============================================
-- COUNTRIES (6 endpoints) - Publisher+ only (Reader access REMOVED)
-- =============================================
PRINT 'Seeding Countries endpoints...'
GO

DECLARE @EndpointId INT;

-- GET /api/countries/
INSERT INTO [dbo].[EndpointRegistry] ([HttpMethod], [Route], [EndpointName], [Description], [Category], [IsActive])
VALUES ('GET', '/api/countries/', 'GetCountries', 'Get all countries', 'Countries', 1);
SET @EndpointId = SCOPE_IDENTITY();
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName])
VALUES (@EndpointId, 'Publisher'), (@EndpointId, 'ADAdmin'), (@EndpointId, 'SuperUser');

-- GET /api/countries/{code}
INSERT INTO [dbo].[EndpointRegistry] ([HttpMethod], [Route], [EndpointName], [Description], [Category], [IsActive])
VALUES ('GET', '/api/countries/{code}', 'GetCountry', 'Get country by code', 'Countries', 1);
SET @EndpointId = SCOPE_IDENTITY();
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName])
VALUES (@EndpointId, 'Publisher'), (@EndpointId, 'ADAdmin'), (@EndpointId, 'SuperUser');

-- POST /api/countries/
INSERT INTO [dbo].[EndpointRegistry] ([HttpMethod], [Route], [EndpointName], [Description], [Category], [IsActive])
VALUES ('POST', '/api/countries/', 'CreateCountry', 'Create country', 'Countries', 1);
SET @EndpointId = SCOPE_IDENTITY();
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName])
VALUES (@EndpointId, 'SuperUser');

-- PUT /api/countries/{code}
INSERT INTO [dbo].[EndpointRegistry] ([HttpMethod], [Route], [EndpointName], [Description], [Category], [IsActive])
VALUES ('PUT', '/api/countries/{code}', 'UpdateCountry', 'Update country', 'Countries', 1);
SET @EndpointId = SCOPE_IDENTITY();
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName])
VALUES (@EndpointId, 'SuperUser');

-- DELETE /api/countries/{code}
INSERT INTO [dbo].[EndpointRegistry] ([HttpMethod], [Route], [EndpointName], [Description], [Category], [IsActive])
VALUES ('DELETE', '/api/countries/{code}', 'DeleteCountry', 'Delete country', 'Countries', 1);
SET @EndpointId = SCOPE_IDENTITY();
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName])
VALUES (@EndpointId, 'SuperUser');

-- GET /api/countries/{code}/usage
INSERT INTO [dbo].[EndpointRegistry] ([HttpMethod], [Route], [EndpointName], [Description], [Category], [IsActive])
VALUES ('GET', '/api/countries/{code}/usage', 'GetCountryUsage', 'Get country usage count', 'Countries', 1);
SET @EndpointId = SCOPE_IDENTITY();
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName])
VALUES (@EndpointId, 'Publisher'), (@EndpointId, 'ADAdmin'), (@EndpointId, 'SuperUser');

PRINT 'Countries endpoints seeded (6 endpoints)'
GO

-- =============================================
-- CURRENCIES (6 endpoints) - Publisher+ only (Reader access REMOVED)
-- =============================================
PRINT 'Seeding Currencies endpoints...'
GO

DECLARE @EndpointId INT;

-- GET /api/currencies/
INSERT INTO [dbo].[EndpointRegistry] ([HttpMethod], [Route], [EndpointName], [Description], [Category], [IsActive])
VALUES ('GET', '/api/currencies/', 'GetCurrencies', 'Get all currencies', 'Currencies', 1);
SET @EndpointId = SCOPE_IDENTITY();
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName])
VALUES (@EndpointId, 'Publisher'), (@EndpointId, 'ADAdmin'), (@EndpointId, 'SuperUser');

-- GET /api/currencies/{code}
INSERT INTO [dbo].[EndpointRegistry] ([HttpMethod], [Route], [EndpointName], [Description], [Category], [IsActive])
VALUES ('GET', '/api/currencies/{code}', 'GetCurrency', 'Get currency by code', 'Currencies', 1);
SET @EndpointId = SCOPE_IDENTITY();
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName])
VALUES (@EndpointId, 'Publisher'), (@EndpointId, 'ADAdmin'), (@EndpointId, 'SuperUser');

-- POST /api/currencies/
INSERT INTO [dbo].[EndpointRegistry] ([HttpMethod], [Route], [EndpointName], [Description], [Category], [IsActive])
VALUES ('POST', '/api/currencies/', 'CreateCurrency', 'Create currency', 'Currencies', 1);
SET @EndpointId = SCOPE_IDENTITY();
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName])
VALUES (@EndpointId, 'SuperUser');

-- PUT /api/currencies/{code}
INSERT INTO [dbo].[EndpointRegistry] ([HttpMethod], [Route], [EndpointName], [Description], [Category], [IsActive])
VALUES ('PUT', '/api/currencies/{code}', 'UpdateCurrency', 'Update currency', 'Currencies', 1);
SET @EndpointId = SCOPE_IDENTITY();
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName])
VALUES (@EndpointId, 'SuperUser');

-- DELETE /api/currencies/{code}
INSERT INTO [dbo].[EndpointRegistry] ([HttpMethod], [Route], [EndpointName], [Description], [Category], [IsActive])
VALUES ('DELETE', '/api/currencies/{code}', 'DeleteCurrency', 'Delete currency', 'Currencies', 1);
SET @EndpointId = SCOPE_IDENTITY();
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName])
VALUES (@EndpointId, 'SuperUser');

-- GET /api/currencies/{code}/usage
INSERT INTO [dbo].[EndpointRegistry] ([HttpMethod], [Route], [EndpointName], [Description], [Category], [IsActive])
VALUES ('GET', '/api/currencies/{code}/usage', 'GetCurrencyUsage', 'Get currency usage count', 'Currencies', 1);
SET @EndpointId = SCOPE_IDENTITY();
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName])
VALUES (@EndpointId, 'Publisher'), (@EndpointId, 'ADAdmin'), (@EndpointId, 'SuperUser');

PRINT 'Currencies endpoints seeded (6 endpoints)'
GO

-- =============================================
-- DOCUMENT TYPES (7 endpoints) - Publisher+ only (Reader access REMOVED)
-- =============================================
PRINT 'Seeding Document Types endpoints...'
GO

DECLARE @EndpointId INT;

-- GET /api/documenttypes/
INSERT INTO [dbo].[EndpointRegistry] ([HttpMethod], [Route], [EndpointName], [Description], [Category], [IsActive])
VALUES ('GET', '/api/documenttypes/', 'GetDocumentTypes', 'Get all document types', 'DocumentTypes', 1);
SET @EndpointId = SCOPE_IDENTITY();
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName])
VALUES (@EndpointId, 'Publisher'), (@EndpointId, 'ADAdmin'), (@EndpointId, 'SuperUser');

-- GET /api/documenttypes/all
INSERT INTO [dbo].[EndpointRegistry] ([HttpMethod], [Route], [EndpointName], [Description], [Category], [IsActive])
VALUES ('GET', '/api/documenttypes/all', 'GetAllDocumentTypes', 'Get all document types (including inactive)', 'DocumentTypes', 1);
SET @EndpointId = SCOPE_IDENTITY();
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName])
VALUES (@EndpointId, 'Publisher'), (@EndpointId, 'ADAdmin'), (@EndpointId, 'SuperUser');

-- GET /api/documenttypes/{id}
INSERT INTO [dbo].[EndpointRegistry] ([HttpMethod], [Route], [EndpointName], [Description], [Category], [IsActive])
VALUES ('GET', '/api/documenttypes/{id}', 'GetDocumentType', 'Get document type by ID', 'DocumentTypes', 1);
SET @EndpointId = SCOPE_IDENTITY();
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName])
VALUES (@EndpointId, 'Publisher'), (@EndpointId, 'ADAdmin'), (@EndpointId, 'SuperUser');

-- POST /api/documenttypes/
INSERT INTO [dbo].[EndpointRegistry] ([HttpMethod], [Route], [EndpointName], [Description], [Category], [IsActive])
VALUES ('POST', '/api/documenttypes/', 'CreateDocumentType', 'Create document type', 'DocumentTypes', 1);
SET @EndpointId = SCOPE_IDENTITY();
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName])
VALUES (@EndpointId, 'SuperUser');

-- PUT /api/documenttypes/{id}
INSERT INTO [dbo].[EndpointRegistry] ([HttpMethod], [Route], [EndpointName], [Description], [Category], [IsActive])
VALUES ('PUT', '/api/documenttypes/{id}', 'UpdateDocumentType', 'Update document type', 'DocumentTypes', 1);
SET @EndpointId = SCOPE_IDENTITY();
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName])
VALUES (@EndpointId, 'SuperUser');

-- DELETE /api/documenttypes/{id}
INSERT INTO [dbo].[EndpointRegistry] ([HttpMethod], [Route], [EndpointName], [Description], [Category], [IsActive])
VALUES ('DELETE', '/api/documenttypes/{id}', 'DeleteDocumentType', 'Delete document type', 'DocumentTypes', 1);
SET @EndpointId = SCOPE_IDENTITY();
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName])
VALUES (@EndpointId, 'SuperUser');

-- GET /api/documenttypes/{id}/usage
INSERT INTO [dbo].[EndpointRegistry] ([HttpMethod], [Route], [EndpointName], [Description], [Category], [IsActive])
VALUES ('GET', '/api/documenttypes/{id}/usage', 'GetDocumentTypeUsage', 'Get document type usage count', 'DocumentTypes', 1);
SET @EndpointId = SCOPE_IDENTITY();
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName])
VALUES (@EndpointId, 'Publisher'), (@EndpointId, 'ADAdmin'), (@EndpointId, 'SuperUser');

PRINT 'Document Types endpoints seeded (7 endpoints)'
GO

-- =============================================
-- DOCUMENT NAMES (6 endpoints) - Publisher+ only (Reader access REMOVED)
-- =============================================
PRINT 'Seeding Document Names endpoints...'
GO

DECLARE @EndpointId INT;

-- GET /api/documentnames/
INSERT INTO [dbo].[EndpointRegistry] ([HttpMethod], [Route], [EndpointName], [Description], [Category], [IsActive])
VALUES ('GET', '/api/documentnames/', 'GetDocumentNames', 'Get all document names', 'DocumentNames', 1);
SET @EndpointId = SCOPE_IDENTITY();
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName])
VALUES (@EndpointId, 'Publisher'), (@EndpointId, 'ADAdmin'), (@EndpointId, 'SuperUser');

-- GET /api/documentnames/bytype/{documentTypeId}
INSERT INTO [dbo].[EndpointRegistry] ([HttpMethod], [Route], [EndpointName], [Description], [Category], [IsActive])
VALUES ('GET', '/api/documentnames/bytype/{documentTypeId}', 'GetDocumentNamesByType', 'Get document names by type', 'DocumentNames', 1);
SET @EndpointId = SCOPE_IDENTITY();
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName])
VALUES (@EndpointId, 'Publisher'), (@EndpointId, 'ADAdmin'), (@EndpointId, 'SuperUser');

-- GET /api/documentnames/{id}
INSERT INTO [dbo].[EndpointRegistry] ([HttpMethod], [Route], [EndpointName], [Description], [Category], [IsActive])
VALUES ('GET', '/api/documentnames/{id}', 'GetDocumentName', 'Get document name by ID', 'DocumentNames', 1);
SET @EndpointId = SCOPE_IDENTITY();
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName])
VALUES (@EndpointId, 'Publisher'), (@EndpointId, 'ADAdmin'), (@EndpointId, 'SuperUser');

-- POST /api/documentnames/
INSERT INTO [dbo].[EndpointRegistry] ([HttpMethod], [Route], [EndpointName], [Description], [Category], [IsActive])
VALUES ('POST', '/api/documentnames/', 'CreateDocumentName', 'Create document name', 'DocumentNames', 1);
SET @EndpointId = SCOPE_IDENTITY();
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName])
VALUES (@EndpointId, 'SuperUser');

-- PUT /api/documentnames/{id}
INSERT INTO [dbo].[EndpointRegistry] ([HttpMethod], [Route], [EndpointName], [Description], [Category], [IsActive])
VALUES ('PUT', '/api/documentnames/{id}', 'UpdateDocumentName', 'Update document name', 'DocumentNames', 1);
SET @EndpointId = SCOPE_IDENTITY();
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName])
VALUES (@EndpointId, 'SuperUser');

-- DELETE /api/documentnames/{id}
INSERT INTO [dbo].[EndpointRegistry] ([HttpMethod], [Route], [EndpointName], [Description], [Category], [IsActive])
VALUES ('DELETE', '/api/documentnames/{id}', 'DeleteDocumentName', 'Delete document name', 'DocumentNames', 1);
SET @EndpointId = SCOPE_IDENTITY();
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName])
VALUES (@EndpointId, 'SuperUser');

PRINT 'Document Names endpoints seeded (6 endpoints)'
GO

-- =============================================
-- ENDPOINT AUTHORIZATION (10 endpoints - NEW) - SuperUser only, except /check for all
-- =============================================
PRINT 'Seeding Endpoint Authorization endpoints...'
GO

DECLARE @EndpointId INT;

-- GET /api/endpoint-authorization/endpoints
INSERT INTO [dbo].[EndpointRegistry] ([HttpMethod], [Route], [EndpointName], [Description], [Category], [IsActive])
VALUES ('GET', '/api/endpoint-authorization/endpoints', 'GetEndpoints', 'Get all endpoints in registry', 'EndpointAuthorization', 1);
SET @EndpointId = SCOPE_IDENTITY();
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName])
VALUES (@EndpointId, 'SuperUser');

-- GET /api/endpoint-authorization/endpoints/{id}
INSERT INTO [dbo].[EndpointRegistry] ([HttpMethod], [Route], [EndpointName], [Description], [Category], [IsActive])
VALUES ('GET', '/api/endpoint-authorization/endpoints/{id}', 'GetEndpoint', 'Get endpoint details', 'EndpointAuthorization', 1);
SET @EndpointId = SCOPE_IDENTITY();
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName])
VALUES (@EndpointId, 'SuperUser');

-- GET /api/endpoint-authorization/endpoints/{id}/roles
INSERT INTO [dbo].[EndpointRegistry] ([HttpMethod], [Route], [EndpointName], [Description], [Category], [IsActive])
VALUES ('GET', '/api/endpoint-authorization/endpoints/{id}/roles', 'GetEndpointRoles', 'Get roles for endpoint', 'EndpointAuthorization', 1);
SET @EndpointId = SCOPE_IDENTITY();
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName])
VALUES (@EndpointId, 'SuperUser');

-- POST /api/endpoint-authorization/endpoints/{id}/roles
INSERT INTO [dbo].[EndpointRegistry] ([HttpMethod], [Route], [EndpointName], [Description], [Category], [IsActive])
VALUES ('POST', '/api/endpoint-authorization/endpoints/{id}/roles', 'UpdateEndpointRoles', 'Update roles for endpoint', 'EndpointAuthorization', 1);
SET @EndpointId = SCOPE_IDENTITY();
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName])
VALUES (@EndpointId, 'SuperUser');

-- GET /api/endpoint-authorization/roles
INSERT INTO [dbo].[EndpointRegistry] ([HttpMethod], [Route], [EndpointName], [Description], [Category], [IsActive])
VALUES ('GET', '/api/endpoint-authorization/roles', 'GetRoles', 'Get all available roles', 'EndpointAuthorization', 1);
SET @EndpointId = SCOPE_IDENTITY();
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName])
VALUES (@EndpointId, 'SuperUser');

-- GET /api/endpoint-authorization/audit
INSERT INTO [dbo].[EndpointRegistry] ([HttpMethod], [Route], [EndpointName], [Description], [Category], [IsActive])
VALUES ('GET', '/api/endpoint-authorization/audit', 'GetAuditLog', 'Get permission change audit log', 'EndpointAuthorization', 1);
SET @EndpointId = SCOPE_IDENTITY();
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName])
VALUES (@EndpointId, 'SuperUser');

-- POST /api/endpoint-authorization/cache/invalidate
INSERT INTO [dbo].[EndpointRegistry] ([HttpMethod], [Route], [EndpointName], [Description], [Category], [IsActive])
VALUES ('POST', '/api/endpoint-authorization/cache/invalidate', 'InvalidateCache', 'Invalidate authorization cache', 'EndpointAuthorization', 1);
SET @EndpointId = SCOPE_IDENTITY();
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName])
VALUES (@EndpointId, 'SuperUser');

-- POST /api/endpoint-authorization/sync
INSERT INTO [dbo].[EndpointRegistry] ([HttpMethod], [Route], [EndpointName], [Description], [Category], [IsActive])
VALUES ('POST', '/api/endpoint-authorization/sync', 'SyncEndpoints', 'Sync endpoints from code to database', 'EndpointAuthorization', 1);
SET @EndpointId = SCOPE_IDENTITY();
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName])
VALUES (@EndpointId, 'SuperUser');

-- GET /api/endpoint-authorization/check - ALL ROLES (for NavMenu visibility)
INSERT INTO [dbo].[EndpointRegistry] ([HttpMethod], [Route], [EndpointName], [Description], [Category], [IsActive])
VALUES ('GET', '/api/endpoint-authorization/check', 'CheckEndpointAccess', 'Check if user can access specific endpoint', 'EndpointAuthorization', 1);
SET @EndpointId = SCOPE_IDENTITY();
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName])
VALUES (@EndpointId, 'Reader'), (@EndpointId, 'Publisher'), (@EndpointId, 'ADAdmin'), (@EndpointId, 'SuperUser');

-- POST /api/endpoint-authorization/validate
INSERT INTO [dbo].[EndpointRegistry] ([HttpMethod], [Route], [EndpointName], [Description], [Category], [IsActive])
VALUES ('POST', '/api/endpoint-authorization/validate', 'ValidatePermissions', 'Validate permission changes before applying', 'EndpointAuthorization', 1);
SET @EndpointId = SCOPE_IDENTITY();
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName])
VALUES (@EndpointId, 'SuperUser');

PRINT 'Endpoint Authorization endpoints seeded (10 endpoints)'
GO

-- =============================================
-- AUDIT TRAIL (7 endpoints) - Publisher+ (unchanged from current)
-- =============================================
PRINT 'Seeding Audit Trail endpoints...'
GO

DECLARE @EndpointId INT;

-- GET /api/audit-trail/
INSERT INTO [dbo].[EndpointRegistry] ([HttpMethod], [Route], [EndpointName], [Description], [Category], [IsActive])
VALUES ('GET', '/api/audit-trail/', 'GetAuditTrail', 'Get audit trail', 'AuditTrail', 1);
SET @EndpointId = SCOPE_IDENTITY();
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName])
VALUES (@EndpointId, 'Publisher'), (@EndpointId, 'ADAdmin'), (@EndpointId, 'SuperUser');

-- POST /api/audit-trail/search
INSERT INTO [dbo].[EndpointRegistry] ([HttpMethod], [Route], [EndpointName], [Description], [Category], [IsActive])
VALUES ('POST', '/api/audit-trail/search', 'SearchAuditTrail', 'Search audit trail', 'AuditTrail', 1);
SET @EndpointId = SCOPE_IDENTITY();
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName])
VALUES (@EndpointId, 'Publisher'), (@EndpointId, 'ADAdmin'), (@EndpointId, 'SuperUser');

-- GET /api/audit-trail/users
INSERT INTO [dbo].[EndpointRegistry] ([HttpMethod], [Route], [EndpointName], [Description], [Category], [IsActive])
VALUES ('GET', '/api/audit-trail/users', 'GetAuditUsers', 'Get users from audit trail', 'AuditTrail', 1);
SET @EndpointId = SCOPE_IDENTITY();
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName])
VALUES (@EndpointId, 'Publisher'), (@EndpointId, 'ADAdmin'), (@EndpointId, 'SuperUser');

-- GET /api/audit-trail/actions
INSERT INTO [dbo].[EndpointRegistry] ([HttpMethod], [Route], [EndpointName], [Description], [Category], [IsActive])
VALUES ('GET', '/api/audit-trail/actions', 'GetAuditActions', 'Get audit actions', 'AuditTrail', 1);
SET @EndpointId = SCOPE_IDENTITY();
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName])
VALUES (@EndpointId, 'Publisher'), (@EndpointId, 'ADAdmin'), (@EndpointId, 'SuperUser');

-- GET /api/audit-trail/export
INSERT INTO [dbo].[EndpointRegistry] ([HttpMethod], [Route], [EndpointName], [Description], [Category], [IsActive])
VALUES ('GET', '/api/audit-trail/export', 'ExportAuditTrail', 'Export audit trail to Excel', 'AuditTrail', 1);
SET @EndpointId = SCOPE_IDENTITY();
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName])
VALUES (@EndpointId, 'Publisher'), (@EndpointId, 'ADAdmin'), (@EndpointId, 'SuperUser');

-- GET /api/audit-trail/{id}
INSERT INTO [dbo].[EndpointRegistry] ([HttpMethod], [Route], [EndpointName], [Description], [Category], [IsActive])
VALUES ('GET', '/api/audit-trail/{id}', 'GetAuditTrailById', 'Get audit trail entry by ID', 'AuditTrail', 1);
SET @EndpointId = SCOPE_IDENTITY();
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName])
VALUES (@EndpointId, 'Publisher'), (@EndpointId, 'ADAdmin'), (@EndpointId, 'SuperUser');

-- POST /api/audit-trail/
INSERT INTO [dbo].[EndpointRegistry] ([HttpMethod], [Route], [EndpointName], [Description], [Category], [IsActive])
VALUES ('POST', '/api/audit-trail/', 'LogAuditTrail', 'Log audit trail entry', 'AuditTrail', 1);
SET @EndpointId = SCOPE_IDENTITY();
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName])
VALUES (@EndpointId, 'Publisher'), (@EndpointId, 'ADAdmin'), (@EndpointId, 'SuperUser');

PRINT 'Audit Trail endpoints seeded (7 endpoints)'
GO

-- =============================================
-- EXCEL EXPORT (4 endpoints) - All roles (unchanged from current)
-- =============================================
PRINT 'Seeding Excel Export endpoints...'
GO

DECLARE @EndpointId INT;

-- GET /api/excel/preview
INSERT INTO [dbo].[EndpointRegistry] ([HttpMethod], [Route], [EndpointName], [Description], [Category], [IsActive])
VALUES ('GET', '/api/excel/preview', 'PreviewExcel', 'Preview Excel data', 'ExcelExport', 1);
SET @EndpointId = SCOPE_IDENTITY();
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName])
VALUES (@EndpointId, 'Reader'), (@EndpointId, 'Publisher'), (@EndpointId, 'ADAdmin'), (@EndpointId, 'SuperUser');

-- POST /api/excel/export
INSERT INTO [dbo].[EndpointRegistry] ([HttpMethod], [Route], [EndpointName], [Description], [Category], [IsActive])
VALUES ('POST', '/api/excel/export', 'ExportExcel', 'Export to Excel', 'ExcelExport', 1);
SET @EndpointId = SCOPE_IDENTITY();
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName])
VALUES (@EndpointId, 'Reader'), (@EndpointId, 'Publisher'), (@EndpointId, 'ADAdmin'), (@EndpointId, 'SuperUser');

-- GET /api/excel/template/{templateName}
INSERT INTO [dbo].[EndpointRegistry] ([HttpMethod], [Route], [EndpointName], [Description], [Category], [IsActive])
VALUES ('GET', '/api/excel/template/{templateName}', 'GetExcelTemplate', 'Get Excel template', 'ExcelExport', 1);
SET @EndpointId = SCOPE_IDENTITY();
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName])
VALUES (@EndpointId, 'Reader'), (@EndpointId, 'Publisher'), (@EndpointId, 'ADAdmin'), (@EndpointId, 'SuperUser');

-- POST /api/excel/import
INSERT INTO [dbo].[EndpointRegistry] ([HttpMethod], [Route], [EndpointName], [Description], [Category], [IsActive])
VALUES ('POST', '/api/excel/import', 'ImportExcel', 'Import from Excel', 'ExcelExport', 1);
SET @EndpointId = SCOPE_IDENTITY();
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName])
VALUES (@EndpointId, 'Reader'), (@EndpointId, 'Publisher'), (@EndpointId, 'ADAdmin'), (@EndpointId, 'SuperUser');

PRINT 'Excel Export endpoints seeded (4 endpoints)'
GO

-- =============================================
-- EMAIL OPERATIONS (3 endpoints) - Publisher+ (unchanged from current)
-- =============================================
PRINT 'Seeding Email Operations endpoints...'
GO

DECLARE @EndpointId INT;

-- POST /api/email/send
INSERT INTO [dbo].[EndpointRegistry] ([HttpMethod], [Route], [EndpointName], [Description], [Category], [IsActive])
VALUES ('POST', '/api/email/send', 'SendEmail', 'Send email', 'Email', 1);
SET @EndpointId = SCOPE_IDENTITY();
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName])
VALUES (@EndpointId, 'Publisher'), (@EndpointId, 'ADAdmin'), (@EndpointId, 'SuperUser');

-- POST /api/email/send-with-attachment
INSERT INTO [dbo].[EndpointRegistry] ([HttpMethod], [Route], [EndpointName], [Description], [Category], [IsActive])
VALUES ('POST', '/api/email/send-with-attachment', 'SendEmailWithAttachment', 'Send email with attachment', 'Email', 1);
SET @EndpointId = SCOPE_IDENTITY();
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName])
VALUES (@EndpointId, 'Publisher'), (@EndpointId, 'ADAdmin'), (@EndpointId, 'SuperUser');

-- GET /api/email/test
INSERT INTO [dbo].[EndpointRegistry] ([HttpMethod], [Route], [EndpointName], [Description], [Category], [IsActive])
VALUES ('GET', '/api/email/test', 'TestEmail', 'Test email configuration', 'Email', 1);
SET @EndpointId = SCOPE_IDENTITY();
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName])
VALUES (@EndpointId, 'Publisher'), (@EndpointId, 'ADAdmin'), (@EndpointId, 'SuperUser');

PRINT 'Email Operations endpoints seeded (3 endpoints)'
GO

-- =============================================
-- USER IDENTITY (1 endpoint) - All authenticated users (unchanged from current)
-- =============================================
PRINT 'Seeding User Identity endpoints...'
GO

DECLARE @EndpointId INT;

-- GET /api/identity/current-user
INSERT INTO [dbo].[EndpointRegistry] ([HttpMethod], [Route], [EndpointName], [Description], [Category], [IsActive])
VALUES ('GET', '/api/identity/current-user', 'GetCurrentUser', 'Get current user information', 'UserIdentity', 1);
SET @EndpointId = SCOPE_IDENTITY();
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName])
VALUES (@EndpointId, 'Reader'), (@EndpointId, 'Publisher'), (@EndpointId, 'ADAdmin'), (@EndpointId, 'SuperUser');

PRINT 'User Identity endpoints seeded (1 endpoint)'
GO

-- =============================================
-- SUMMARY AND VERIFICATION
-- =============================================
PRINT ''
PRINT '==========================================='
PRINT 'SEED DATA SUMMARY'
PRINT '==========================================='
PRINT ''

DECLARE @TotalEndpoints INT, @TotalPermissions INT;

SELECT @TotalEndpoints = COUNT(*) FROM [dbo].[EndpointRegistry];
SELECT @TotalPermissions = COUNT(*) FROM [dbo].[EndpointRolePermission];

PRINT 'Total Endpoints Seeded: ' + CAST(@TotalEndpoints AS VARCHAR(10));
PRINT 'Total Role Permissions Seeded: ' + CAST(@TotalPermissions AS VARCHAR(10));
PRINT ''

-- Count by category
SELECT
    Category,
    COUNT(*) AS EndpointCount
FROM [dbo].[EndpointRegistry]
GROUP BY Category
ORDER BY Category;

PRINT ''
PRINT 'Endpoint registry and permissions seed completed successfully!'
PRINT ''

GO

SET NOCOUNT OFF;
GO
