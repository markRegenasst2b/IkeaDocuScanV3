-- =============================================
-- Script: SyncEndpointsFromSourceCode.sql
-- Description: Recreates all endpoint registrations based on source code definitions
--              Deletes database endpoints that have no implementation
--              Uses permissions from ROLE_EXTENSION_IMPLEMENTATION_PLAN.md
-- Date: 2025-11-20
-- =============================================

USE [IkeaDocuScan]
GO

PRINT 'Starting endpoint synchronization from source code...'
GO

-- =============================================
-- STEP 1: Delete all existing endpoint permissions and endpoints
-- =============================================
PRINT 'Step 1: Cleaning existing data...'
GO

DELETE FROM [dbo].[PermissionChangeAuditLog];
PRINT '  - Deleted all permission audit logs'
GO

DELETE FROM [dbo].[EndpointRolePermission];
PRINT '  - Deleted all endpoint role permissions'
GO

DELETE FROM [dbo].[EndpointRegistry];
PRINT '  - Deleted all endpoint registrations'
GO

-- =============================================
-- STEP 2: Insert endpoints from source code
-- =============================================
PRINT 'Step 2: Inserting endpoints from source code...'
GO

SET IDENTITY_INSERT [dbo].[EndpointRegistry] ON
GO

-- Documents (10 endpoints)
INSERT INTO [dbo].[EndpointRegistry] ([EndpointId], [HttpMethod], [Route], [EndpointName], [Description], [Category], [IsActive])
VALUES
(1, 'GET', '/api/documents/', 'GetAllDocuments', 'Get all documents (filtered by permissions)', 'Documents', 1),
(2, 'GET', '/api/documents/{id}', 'GetDocumentById', 'Get document by ID', 'Documents', 1),
(3, 'GET', '/api/documents/barcode/{barCode}', 'GetDocumentByBarCode', 'Get document by barcode', 'Documents', 1),
(4, 'POST', '/api/documents/by-ids', 'GetDocumentsByIds', 'Get documents by list of IDs', 'Documents', 1),
(5, 'POST', '/api/documents/', 'CreateDocument', 'Create new document', 'Documents', 1),
(6, 'PUT', '/api/documents/{id}', 'UpdateDocument', 'Update document', 'Documents', 1),
(7, 'DELETE', '/api/documents/{id}', 'DeleteDocument', 'Delete document', 'Documents', 1),
(8, 'POST', '/api/documents/search', 'SearchDocuments', 'Search documents', 'Documents', 1),
(9, 'GET', '/api/documents/{id}/stream', 'StreamDocumentFile', 'Stream document file (inline)', 'Documents', 1),
(10, 'GET', '/api/documents/{id}/download', 'DownloadDocumentFile', 'Download document file', 'Documents', 1),

-- Counter Parties (7 endpoints)
(11, 'GET', '/api/counterparties/', 'GetAllCounterParties', 'Get all counter parties', 'CounterParties', 1),
(12, 'GET', '/api/counterparties/search', 'SearchCounterParties', 'Search counter parties', 'CounterParties', 1),
(13, 'GET', '/api/counterparties/{id}', 'GetCounterPartyById', 'Get counter party by ID', 'CounterParties', 1),
(14, 'POST', '/api/counterparties/', 'CreateCounterParty', 'Create counter party', 'CounterParties', 1),
(15, 'PUT', '/api/counterparties/{id}', 'UpdateCounterParty', 'Update counter party', 'CounterParties', 1),
(16, 'DELETE', '/api/counterparties/{id}', 'DeleteCounterParty', 'Delete counter party', 'CounterParties', 1),
(17, 'GET', '/api/counterparties/{id}/usage', 'GetCounterPartyUsage', 'Get counter party usage count', 'CounterParties', 1),

-- User Permissions (11 endpoints)
(18, 'GET', '/api/userpermissions/', 'GetAllUserPermissions', 'Get all user permissions', 'UserPermissions', 1),
(19, 'GET', '/api/userpermissions/users', 'GetAllDocuScanUsers', 'Get all DocuScan users', 'UserPermissions', 1),
(20, 'GET', '/api/userpermissions/{id}', 'GetUserPermissionById', 'Get user permission by ID', 'UserPermissions', 1),
(21, 'GET', '/api/userpermissions/user/{userId}', 'GetUserPermissionsByUserId', 'Get user permissions by user ID', 'UserPermissions', 1),
(22, 'GET', '/api/userpermissions/me', 'GetMyPermissions', 'Get current user permissions', 'UserPermissions', 1),
(23, 'POST', '/api/userpermissions/', 'CreateUserPermission', 'Create user permission', 'UserPermissions', 1),
(24, 'PUT', '/api/userpermissions/{id}', 'UpdateUserPermission', 'Update user permission', 'UserPermissions', 1),
(25, 'DELETE', '/api/userpermissions/{id}', 'DeleteUserPermission', 'Delete user permission', 'UserPermissions', 1),
(26, 'DELETE', '/api/userpermissions/user/{userId}', 'DeleteDocuScanUser', 'Delete DocuScan user', 'UserPermissions', 1),
(27, 'POST', '/api/userpermissions/user', 'CreateDocuScanUser', 'Create DocuScan user', 'UserPermissions', 1),
(28, 'PUT', '/api/userpermissions/user/{userId}', 'UpdateDocuScanUser', 'Update DocuScan user', 'UserPermissions', 1),

-- Configuration (19 endpoints)
(29, 'GET', '/api/configuration/email-recipients', 'GetAllEmailRecipientGroups', 'Get all email recipient groups', 'Configuration', 1),
(30, 'GET', '/api/configuration/email-recipients/{groupKey}', 'GetEmailRecipientGroup', 'Get email recipient group', 'Configuration', 1),
(31, 'POST', '/api/configuration/email-recipients/{groupKey}', 'UpdateEmailRecipientGroup', 'Update email recipient group', 'Configuration', 1),
(32, 'GET', '/api/configuration/email-templates', 'GetAllEmailTemplates', 'Get all email templates', 'Configuration', 1),
(33, 'GET', '/api/configuration/email-templates/{key}', 'GetEmailTemplateByKey', 'Get email template by key', 'Configuration', 1),
(34, 'POST', '/api/configuration/email-templates', 'CreateEmailTemplate', 'Create email template', 'Configuration', 1),
(35, 'PUT', '/api/configuration/email-templates/{id}', 'UpdateEmailTemplate', 'Update email template', 'Configuration', 1),
(36, 'DELETE', '/api/configuration/email-templates/{id}', 'DeactivateEmailTemplate', 'Deactivate email template', 'Configuration', 1),
(37, 'GET', '/api/configuration/sections', 'GetConfigurationSections', 'Get configuration sections', 'Configuration', 1),
(38, 'GET', '/api/configuration/{section}/{key}', 'GetConfiguration', 'Get configuration value', 'Configuration', 1),
(39, 'POST', '/api/configuration/{section}/{key}', 'SetConfiguration', 'Set configuration value', 'Configuration', 1),
(40, 'POST', '/api/configuration/smtp', 'UpdateSmtpConfiguration', 'Update SMTP configuration', 'Configuration', 1),
(41, 'POST', '/api/configuration/test-smtp', 'TestSmtpConnection', 'Test SMTP connection', 'Configuration', 1),
(42, 'POST', '/api/configuration/reload', 'ReloadConfigurationCache', 'Reload configuration cache', 'Configuration', 1),
(43, 'POST', '/api/configuration/migrate', 'MigrateConfiguration', 'Migrate configuration', 'Configuration', 1),
(44, 'POST', '/api/configuration/email-templates/preview', 'PreviewEmailTemplate', 'Preview email template', 'Configuration', 1),
(45, 'GET', '/api/configuration/email-templates/placeholders', 'GetEmailTemplatePlaceholders', 'Get email template placeholders', 'Configuration', 1),
(46, 'GET', '/api/configuration/email-templates/diagnostic/DocumentAttachment', 'DiagnoseDocumentAttachmentTemplate', 'Diagnose document attachment template', 'Configuration', 1),

-- Log Viewer (5 endpoints)
(47, 'POST', '/api/logs/search', 'SearchLogs', 'Search logs', 'LogViewer', 1),
(48, 'GET', '/api/logs/export', 'ExportLogs', 'Export logs', 'LogViewer', 1),
(49, 'GET', '/api/logs/dates', 'GetLogDates', 'Get log dates', 'LogViewer', 1),
(50, 'GET', '/api/logs/sources', 'GetLogSources', 'Get log sources', 'LogViewer', 1),
(51, 'GET', '/api/logs/statistics', 'GetLogStatistics', 'Get log statistics', 'LogViewer', 1),

-- Scanned Files (6 endpoints)
(52, 'GET', '/api/scannedfiles/', 'GetAllScannedFiles', 'Get all scanned files', 'ScannedFiles', 1),
(53, 'GET', '/api/scannedfiles/{fileName}', 'GetScannedFileByName', 'Get scanned file by name', 'ScannedFiles', 1),
(54, 'GET', '/api/scannedfiles/{fileName}/content', 'GetScannedFileContent', 'Get scanned file content', 'ScannedFiles', 1),
(55, 'GET', '/api/scannedfiles/{fileName}/exists', 'CheckScannedFileExists', 'Check if scanned file exists', 'ScannedFiles', 1),
(56, 'GET', '/api/scannedfiles/{fileName}/stream', 'GetScannedFileStream', 'Stream scanned file', 'ScannedFiles', 1),
(57, 'DELETE', '/api/scannedfiles/{fileName}', 'DeleteScannedFile', 'Delete scanned file', 'ScannedFiles', 1),

-- Action Reminders (3 endpoints)
(58, 'GET', '/api/action-reminders/', 'GetDueActions', 'Get due actions', 'ActionReminders', 1),
(59, 'GET', '/api/action-reminders/count', 'GetDueActionsCount', 'Get due actions count', 'ActionReminders', 1),
(60, 'GET', '/api/action-reminders/date/{date}', 'GetActionsDueOnDate', 'Get actions due on date', 'ActionReminders', 1),

-- Reports (14 endpoints)
(61, 'GET', '/api/reports/barcode-gaps', 'GetBarcodeGapsReport', 'Get barcode gaps report', 'Reports', 1),
(62, 'GET', '/api/reports/duplicate-documents', 'GetDuplicateDocumentsReport', 'Get duplicate documents report', 'Reports', 1),
(63, 'GET', '/api/reports/unlinked-registrations', 'GetUnlinkedRegistrationsReport', 'Get unlinked registrations report', 'Reports', 1),
(64, 'GET', '/api/reports/scan-copies', 'GetScanCopiesReport', 'Get scan copies report', 'Reports', 1),
(65, 'GET', '/api/reports/suppliers', 'GetSuppliersReport', 'Get suppliers report', 'Reports', 1),
(66, 'GET', '/api/reports/all-documents', 'GetAllDocumentsReport', 'Get all documents report', 'Reports', 1),
(67, 'GET', '/api/reports/barcode-gaps/excel', 'ExportBarcodeGapsToExcel', 'Export barcode gaps to Excel', 'Reports', 1),
(68, 'GET', '/api/reports/duplicate-documents/excel', 'ExportDuplicateDocumentsToExcel', 'Export duplicate documents to Excel', 'Reports', 1),
(69, 'GET', '/api/reports/unlinked-registrations/excel', 'ExportUnlinkedRegistrationsToExcel', 'Export unlinked registrations to Excel', 'Reports', 1),
(70, 'GET', '/api/reports/scan-copies/excel', 'ExportScanCopiesToExcel', 'Export scan copies to Excel', 'Reports', 1),
(71, 'GET', '/api/reports/suppliers/excel', 'ExportSuppliersToExcel', 'Export suppliers to Excel', 'Reports', 1),
(72, 'GET', '/api/reports/all-documents/excel', 'ExportAllDocumentsToExcel', 'Export all documents to Excel', 'Reports', 1),
(73, 'POST', '/api/reports/documents/search/excel', 'ExportSearchResultsToExcel', 'Export search results to Excel', 'Reports', 1),
(74, 'POST', '/api/reports/documents/selected/excel', 'ExportSelectedDocumentsToExcel', 'Export selected documents to Excel', 'Reports', 1),

-- Countries (6 endpoints)
(75, 'GET', '/api/countries/', 'GetAllCountries', 'Get all countries', 'Countries', 1),
(76, 'GET', '/api/countries/{code}', 'GetCountryByCode', 'Get country by code', 'Countries', 1),
(77, 'POST', '/api/countries/', 'CreateCountry', 'Create country', 'Countries', 1),
(78, 'PUT', '/api/countries/{code}', 'UpdateCountry', 'Update country', 'Countries', 1),
(79, 'DELETE', '/api/countries/{code}', 'DeleteCountry', 'Delete country', 'Countries', 1),
(80, 'GET', '/api/countries/{code}/usage', 'GetCountryUsage', 'Get country usage', 'Countries', 1),

-- Currencies (6 endpoints)
(81, 'GET', '/api/currencies/', 'GetAllCurrencies', 'Get all currencies', 'Currencies', 1),
(82, 'GET', '/api/currencies/{code}', 'GetCurrencyByCode', 'Get currency by code', 'Currencies', 1),
(83, 'POST', '/api/currencies/', 'CreateCurrency', 'Create currency', 'Currencies', 1),
(84, 'PUT', '/api/currencies/{code}', 'UpdateCurrency', 'Update currency', 'Currencies', 1),
(85, 'DELETE', '/api/currencies/{code}', 'DeleteCurrency', 'Delete currency', 'Currencies', 1),
(86, 'GET', '/api/currencies/{code}/usage', 'GetCurrencyUsage', 'Get currency usage', 'Currencies', 1),

-- Document Types (7 endpoints)
(87, 'GET', '/api/documenttypes/', 'GetAllDocumentTypes', 'Get all document types', 'DocumentTypes', 1),
(88, 'GET', '/api/documenttypes/all', 'GetAllDocumentTypesIncludingDisabled', 'Get all document types including disabled', 'DocumentTypes', 1),
(89, 'GET', '/api/documenttypes/{id}', 'GetDocumentTypeById', 'Get document type by ID', 'DocumentTypes', 1),
(90, 'POST', '/api/documenttypes/', 'CreateDocumentType', 'Create document type', 'DocumentTypes', 1),
(91, 'PUT', '/api/documenttypes/{id}', 'UpdateDocumentType', 'Update document type', 'DocumentTypes', 1),
(92, 'DELETE', '/api/documenttypes/{id}', 'DeleteDocumentType', 'Delete document type', 'DocumentTypes', 1),
(93, 'GET', '/api/documenttypes/{id}/usage', 'GetDocumentTypeUsage', 'Get document type usage', 'DocumentTypes', 1),

-- Document Names (6 endpoints)
(94, 'GET', '/api/documentnames/', 'GetAllDocumentNames', 'Get all document names', 'DocumentNames', 1),
(95, 'GET', '/api/documentnames/bytype/{documentTypeId}', 'GetDocumentNamesByType', 'Get document names by type', 'DocumentNames', 1),
(96, 'GET', '/api/documentnames/{id}', 'GetDocumentNameById', 'Get document name by ID', 'DocumentNames', 1),
(97, 'POST', '/api/documentnames/', 'CreateDocumentName', 'Create document name', 'DocumentNames', 1),
(98, 'PUT', '/api/documentnames/{id}', 'UpdateDocumentName', 'Update document name', 'DocumentNames', 1),
(99, 'DELETE', '/api/documentnames/{id}', 'DeleteDocumentName', 'Delete document name', 'DocumentNames', 1),

-- Audit Trail (7 endpoints) - Using /audittrail as in source code (NOT /audit-trail)
(100, 'POST', '/api/audittrail/', 'LogAuditTrail', 'Log audit trail entry', 'AuditTrail', 1),
(101, 'POST', '/api/audittrail/document/{documentId}', 'LogAuditTrailByDocument', 'Log audit trail by document', 'AuditTrail', 1),
(102, 'POST', '/api/audittrail/batch', 'LogAuditTrailBatch', 'Log batch audit trail entries', 'AuditTrail', 1),
(103, 'GET', '/api/audittrail/barcode/{barCode}', 'GetAuditTrailByBarCode', 'Get audit trail by barcode', 'AuditTrail', 1),
(104, 'GET', '/api/audittrail/user/{username}', 'GetAuditTrailByUser', 'Get audit trail by user', 'AuditTrail', 1),
(105, 'GET', '/api/audittrail/recent', 'GetRecentAuditTrail', 'Get recent audit trail', 'AuditTrail', 1),
(106, 'GET', '/api/audittrail/daterange', 'GetAuditTrailByDateRange', 'Get audit trail by date range', 'AuditTrail', 1),

-- Excel Export (4 endpoints)
(107, 'POST', '/api/excel/export/documents', 'ExportDocumentsToExcel', 'Export documents to Excel', 'Excel', 1),
(108, 'POST', '/api/excel/validate/documents', 'ValidateExportSize', 'Validate export size', 'Excel', 1),
(109, 'GET', '/api/excel/metadata/documents', 'GetDocumentExportMetadata', 'Get document export metadata', 'Excel', 1),
(110, 'POST', '/api/excel/export/by-ids', 'ExportDocumentsByIdsToExcel', 'Export documents by IDs to Excel', 'Excel', 1),

-- Email (3 endpoints)
(111, 'POST', '/api/email/send', 'SendEmail', 'Send email', 'Email', 1),
(112, 'POST', '/api/email/send-with-attachments', 'SendEmailWithAttachments', 'Send email with attachments', 'Email', 1),
(113, 'POST', '/api/email/send-with-links', 'SendEmailWithLinks', 'Send email with links', 'Email', 1),

-- User Identity (1 endpoint)
(114, 'GET', '/api/user/identity', 'GetUserIdentity', 'Get user identity', 'UserIdentity', 1);

-- Test Identity (4 endpoints - DEBUG only) - Not included as they're debug-only
-- Endpoint Authorization (10 endpoints) - NOT IMPLEMENTED YET (marking as inactive)
-- Diagnostic (5 endpoints) - DEBUG only - Not included

PRINT '  - Inserted 114 endpoints from source code'
GO

SET IDENTITY_INSERT [dbo].[EndpointRegistry] OFF
GO

-- =============================================
-- STEP 3: Insert role permissions based on ROLE_EXTENSION_IMPLEMENTATION_PLAN.md
-- =============================================
PRINT 'Step 3: Inserting role permissions...'
GO

-- Documents - Read operations (All roles: Reader, Publisher, ADAdmin, SuperUser)
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName], [CreatedBy])
SELECT EndpointId, 'Reader', 'SYSTEM' FROM [dbo].[EndpointRegistry] WHERE EndpointId IN (1,2,3,4,8,9,10);

INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName], [CreatedBy])
SELECT EndpointId, 'Publisher', 'SYSTEM' FROM [dbo].[EndpointRegistry] WHERE EndpointId IN (1,2,3,4,8,9,10);

INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName], [CreatedBy])
SELECT EndpointId, 'ADAdmin', 'SYSTEM' FROM [dbo].[EndpointRegistry] WHERE EndpointId IN (1,2,3,4,8,9,10);

INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName], [CreatedBy])
SELECT EndpointId, 'SuperUser', 'SYSTEM' FROM [dbo].[EndpointRegistry] WHERE EndpointId IN (1,2,3,4,8,9,10);

-- Documents - Create/Update (Publisher, ADAdmin, SuperUser)
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName], [CreatedBy])
SELECT EndpointId, 'Publisher', 'SYSTEM' FROM [dbo].[EndpointRegistry] WHERE EndpointId IN (5,6);

INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName], [CreatedBy])
SELECT EndpointId, 'ADAdmin', 'SYSTEM' FROM [dbo].[EndpointRegistry] WHERE EndpointId IN (5,6);

INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName], [CreatedBy])
SELECT EndpointId, 'SuperUser', 'SYSTEM' FROM [dbo].[EndpointRegistry] WHERE EndpointId IN (5,6);

-- Documents - Delete (SuperUser only)
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName], [CreatedBy])
VALUES (7, 'SuperUser', 'SYSTEM');

-- Counter Parties - GET endpoints (Publisher, ADAdmin, SuperUser)
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName], [CreatedBy])
SELECT EndpointId, 'Reader', 'SYSTEM' FROM [dbo].[EndpointRegistry] WHERE EndpointId IN (11,12,13,17);

INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName], [CreatedBy])
SELECT EndpointId, 'Publisher', 'SYSTEM' FROM [dbo].[EndpointRegistry] WHERE EndpointId IN (11,12,13,17);

INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName], [CreatedBy])
SELECT EndpointId, 'ADAdmin', 'SYSTEM' FROM [dbo].[EndpointRegistry] WHERE EndpointId IN (11,12,13,17);

INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName], [CreatedBy])
SELECT EndpointId, 'SuperUser', 'SYSTEM' FROM [dbo].[EndpointRegistry] WHERE EndpointId IN (11,12,13,17);

-- Counter Parties - CUD operations (SuperUser only)
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName], [CreatedBy])
SELECT EndpointId, 'SuperUser', 'SYSTEM' FROM [dbo].[EndpointRegistry] WHERE EndpointId IN (14,15,16);

-- User Permissions - View all (ADAdmin, SuperUser)
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName], [CreatedBy])
SELECT EndpointId, 'ADAdmin', 'SYSTEM' FROM [dbo].[EndpointRegistry] WHERE EndpointId IN (18,19);

INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName], [CreatedBy])
SELECT EndpointId, 'SuperUser', 'SYSTEM' FROM [dbo].[EndpointRegistry] WHERE EndpointId IN (18,19);

-- User Permissions - Self-access (All roles)
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName], [CreatedBy])
SELECT EndpointId, 'Reader', 'SYSTEM' FROM [dbo].[EndpointRegistry] WHERE EndpointId IN (20,21,22);

INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName], [CreatedBy])
SELECT EndpointId, 'Publisher', 'SYSTEM' FROM [dbo].[EndpointRegistry] WHERE EndpointId IN (20,21,22);

INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName], [CreatedBy])
SELECT EndpointId, 'ADAdmin', 'SYSTEM' FROM [dbo].[EndpointRegistry] WHERE EndpointId IN (20,21,22);

INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName], [CreatedBy])
SELECT EndpointId, 'SuperUser', 'SYSTEM' FROM [dbo].[EndpointRegistry] WHERE EndpointId IN (20,21,22);

-- User Permissions - CUD operations (SuperUser only)
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName], [CreatedBy])
SELECT EndpointId, 'SuperUser', 'SYSTEM' FROM [dbo].[EndpointRegistry] WHERE EndpointId IN (23,24,25,26,27,28);

-- Configuration - Read-only (ADAdmin, SuperUser)
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName], [CreatedBy])
SELECT EndpointId, 'ADAdmin', 'SYSTEM' FROM [dbo].[EndpointRegistry] WHERE EndpointId IN (29,30,32,33,37);

INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName], [CreatedBy])
SELECT EndpointId, 'SuperUser', 'SYSTEM' FROM [dbo].[EndpointRegistry] WHERE EndpointId IN (29,30,32,33,37);

-- Configuration - Write operations (SuperUser only)
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName], [CreatedBy])
SELECT EndpointId, 'SuperUser', 'SYSTEM' FROM [dbo].[EndpointRegistry] WHERE EndpointId IN (31,34,35,36,38,39,40,41,42,43,44,45,46);

-- Log Viewer (ADAdmin, SuperUser)
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName], [CreatedBy])
SELECT EndpointId, 'ADAdmin', 'SYSTEM' FROM [dbo].[EndpointRegistry] WHERE EndpointId IN (47,48,49,50,51);

INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName], [CreatedBy])
SELECT EndpointId, 'SuperUser', 'SYSTEM' FROM [dbo].[EndpointRegistry] WHERE EndpointId IN (47,48,49,50,51);

-- Scanned Files - GET operations (Publisher, ADAdmin, SuperUser) - Reader access REMOVED
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName], [CreatedBy])
SELECT EndpointId, 'Publisher', 'SYSTEM' FROM [dbo].[EndpointRegistry] WHERE EndpointId IN (52,53,54,55,56);

INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName], [CreatedBy])
SELECT EndpointId, 'ADAdmin', 'SYSTEM' FROM [dbo].[EndpointRegistry] WHERE EndpointId IN (52,53,54,55,56);

INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName], [CreatedBy])
SELECT EndpointId, 'SuperUser', 'SYSTEM' FROM [dbo].[EndpointRegistry] WHERE EndpointId IN (52,53,54,55,56);

-- Scanned Files - DELETE (SuperUser only)
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName], [CreatedBy])
VALUES (57, 'SuperUser', 'SYSTEM');

-- Action Reminders (Publisher, ADAdmin, SuperUser) - Reader access REMOVED
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName], [CreatedBy])
SELECT EndpointId, 'Publisher', 'SYSTEM' FROM [dbo].[EndpointRegistry] WHERE EndpointId IN (58,59,60);

INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName], [CreatedBy])
SELECT EndpointId, 'ADAdmin', 'SYSTEM' FROM [dbo].[EndpointRegistry] WHERE EndpointId IN (58,59,60);

INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName], [CreatedBy])
SELECT EndpointId, 'SuperUser', 'SYSTEM' FROM [dbo].[EndpointRegistry] WHERE EndpointId IN (58,59,60);

-- Reports (Publisher, ADAdmin, SuperUser) - Reader access REMOVED
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName], [CreatedBy])
SELECT EndpointId, 'Publisher', 'SYSTEM' FROM [dbo].[EndpointRegistry] WHERE EndpointId IN (61,62,63,64,65,66,67,68,69,70,71,72,73,74);

INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName], [CreatedBy])
SELECT EndpointId, 'ADAdmin', 'SYSTEM' FROM [dbo].[EndpointRegistry] WHERE EndpointId IN (61,62,63,64,65,66,67,68,69,70,71,72,73,74);

INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName], [CreatedBy])
SELECT EndpointId, 'SuperUser', 'SYSTEM' FROM [dbo].[EndpointRegistry] WHERE EndpointId IN (61,62,63,64,65,66,67,68,69,70,71,72,73,74);

-- Countries - GET operations (Publisher, ADAdmin, SuperUser)
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName], [CreatedBy])
SELECT EndpointId, 'Reader', 'SYSTEM' FROM [dbo].[EndpointRegistry] WHERE EndpointId IN (75,76,80);

INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName], [CreatedBy])
SELECT EndpointId, 'Publisher', 'SYSTEM' FROM [dbo].[EndpointRegistry] WHERE EndpointId IN (75,76,80);

INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName], [CreatedBy])
SELECT EndpointId, 'ADAdmin', 'SYSTEM' FROM [dbo].[EndpointRegistry] WHERE EndpointId IN (75,76,80);

INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName], [CreatedBy])
SELECT EndpointId, 'SuperUser', 'SYSTEM' FROM [dbo].[EndpointRegistry] WHERE EndpointId IN (75,76,80);

-- Countries - CUD operations (SuperUser only)
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName], [CreatedBy])
SELECT EndpointId, 'SuperUser', 'SYSTEM' FROM [dbo].[EndpointRegistry] WHERE EndpointId IN (77,78,79);

-- Currencies - GET operations (Publisher, ADAdmin, SuperUser) 
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName], [CreatedBy])
SELECT EndpointId, 'Reader', 'SYSTEM' FROM [dbo].[EndpointRegistry] WHERE EndpointId IN (81,82,86);

INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName], [CreatedBy])
SELECT EndpointId, 'Publisher', 'SYSTEM' FROM [dbo].[EndpointRegistry] WHERE EndpointId IN (81,82,86);

INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName], [CreatedBy])
SELECT EndpointId, 'ADAdmin', 'SYSTEM' FROM [dbo].[EndpointRegistry] WHERE EndpointId IN (81,82,86);

INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName], [CreatedBy])
SELECT EndpointId, 'SuperUser', 'SYSTEM' FROM [dbo].[EndpointRegistry] WHERE EndpointId IN (81,82,86);

-- Currencies - CUD operations (SuperUser only)
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName], [CreatedBy])
SELECT EndpointId, 'SuperUser', 'SYSTEM' FROM [dbo].[EndpointRegistry] WHERE EndpointId IN (83,84,85);

-- Document Types - GET operations (Publisher, ADAdmin, SuperUser) 
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName], [CreatedBy])
SELECT EndpointId, 'Reader', 'SYSTEM' FROM [dbo].[EndpointRegistry] WHERE EndpointId IN (87,88,89,93);

INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName], [CreatedBy])
SELECT EndpointId, 'Publisher', 'SYSTEM' FROM [dbo].[EndpointRegistry] WHERE EndpointId IN (87,88,89,93);

INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName], [CreatedBy])
SELECT EndpointId, 'ADAdmin', 'SYSTEM' FROM [dbo].[EndpointRegistry] WHERE EndpointId IN (87,88,89,93);

INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName], [CreatedBy])
SELECT EndpointId, 'SuperUser', 'SYSTEM' FROM [dbo].[EndpointRegistry] WHERE EndpointId IN (87,88,89,93);

-- Document Types - CUD operations (SuperUser only)
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName], [CreatedBy])
SELECT EndpointId, 'SuperUser', 'SYSTEM' FROM [dbo].[EndpointRegistry] WHERE EndpointId IN (90,91,92);

-- Document Names - GET operations (Publisher, ADAdmin, SuperUser)
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName], [CreatedBy])
SELECT EndpointId, 'Reader', 'SYSTEM' FROM [dbo].[EndpointRegistry] WHERE EndpointId IN (94,95,96);

INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName], [CreatedBy])
SELECT EndpointId, 'Publisher', 'SYSTEM' FROM [dbo].[EndpointRegistry] WHERE EndpointId IN (94,95,96);

INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName], [CreatedBy])
SELECT EndpointId, 'ADAdmin', 'SYSTEM' FROM [dbo].[EndpointRegistry] WHERE EndpointId IN (94,95,96);

INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName], [CreatedBy])
SELECT EndpointId, 'SuperUser', 'SYSTEM' FROM [dbo].[EndpointRegistry] WHERE EndpointId IN (94,95,96);

-- Document Names - CUD operations (SuperUser only)
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName], [CreatedBy])
SELECT EndpointId, 'SuperUser', 'SYSTEM' FROM [dbo].[EndpointRegistry] WHERE EndpointId IN (97,98,99);

-- Audit Trail (Publisher, ADAdmin, SuperUser) - Per plan, unchanged
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName], [CreatedBy])
SELECT EndpointId, 'Publisher', 'SYSTEM' FROM [dbo].[EndpointRegistry] WHERE EndpointId IN (100,101,102,103,104,105,106);

INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName], [CreatedBy])
SELECT EndpointId, 'ADAdmin', 'SYSTEM' FROM [dbo].[EndpointRegistry] WHERE EndpointId IN (100,101,102,103,104,105,106);

INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName], [CreatedBy])
SELECT EndpointId, 'SuperUser', 'SYSTEM' FROM [dbo].[EndpointRegistry] WHERE EndpointId IN (100,101,102,103,104,105,106);

-- Excel Export (All roles) - Per plan, unchanged
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName], [CreatedBy])
SELECT EndpointId, 'Reader', 'SYSTEM' FROM [dbo].[EndpointRegistry] WHERE EndpointId IN (107,108,109,110);

INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName], [CreatedBy])
SELECT EndpointId, 'Publisher', 'SYSTEM' FROM [dbo].[EndpointRegistry] WHERE EndpointId IN (107,108,109,110);

INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName], [CreatedBy])
SELECT EndpointId, 'ADAdmin', 'SYSTEM' FROM [dbo].[EndpointRegistry] WHERE EndpointId IN (107,108,109,110);

INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName], [CreatedBy])
SELECT EndpointId, 'SuperUser', 'SYSTEM' FROM [dbo].[EndpointRegistry] WHERE EndpointId IN (107,108,109,110);

-- Email (Publisher, ADAdmin, SuperUser) - Per plan, unchanged
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName], [CreatedBy])
SELECT EndpointId, 'Publisher', 'SYSTEM' FROM [dbo].[EndpointRegistry] WHERE EndpointId IN (111,112,113);

INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName], [CreatedBy])
SELECT EndpointId, 'ADAdmin', 'SYSTEM' FROM [dbo].[EndpointRegistry] WHERE EndpointId IN (111,112,113);

INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName], [CreatedBy])
SELECT EndpointId, 'SuperUser', 'SYSTEM' FROM [dbo].[EndpointRegistry] WHERE EndpointId IN (111,112,113);

-- User Identity (All roles) - Per plan, unchanged
INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName], [CreatedBy])
SELECT EndpointId, 'Reader', 'SYSTEM' FROM [dbo].[EndpointRegistry] WHERE EndpointId = 114;

INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName], [CreatedBy])
SELECT EndpointId, 'Publisher', 'SYSTEM' FROM [dbo].[EndpointRegistry] WHERE EndpointId = 114;

INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName], [CreatedBy])
SELECT EndpointId, 'ADAdmin', 'SYSTEM' FROM [dbo].[EndpointRegistry] WHERE EndpointId = 114;

INSERT INTO [dbo].[EndpointRolePermission] ([EndpointId], [RoleName], [CreatedBy])
SELECT EndpointId, 'SuperUser', 'SYSTEM' FROM [dbo].[EndpointRegistry] WHERE EndpointId = 114;

PRINT '  - Inserted role permissions for all 114 endpoints'
GO

-- =============================================
-- STEP 4: Verification queries
-- =============================================
PRINT 'Step 4: Verification...'
GO

-- Count endpoints by category
SELECT Category, COUNT(*) AS EndpointCount
FROM [dbo].[EndpointRegistry]
WHERE IsActive = 1
GROUP BY Category
ORDER BY Category;

-- Count permissions by role
SELECT RoleName, COUNT(*) AS PermissionCount
FROM [dbo].[EndpointRolePermission]
GROUP BY RoleName
ORDER BY RoleName;

-- Show sample of endpoints with their roles
SELECT TOP 20
    er.EndpointId,
    er.HttpMethod,
    er.Route,
    er.EndpointName,
    er.Category,
    STRING_AGG(erp.RoleName, ', ') WITHIN GROUP (ORDER BY erp.RoleName) AS AllowedRoles
FROM [dbo].[EndpointRegistry] er
LEFT JOIN [dbo].[EndpointRolePermission] erp ON er.EndpointId = erp.EndpointId
WHERE er.IsActive = 1
GROUP BY er.EndpointId, er.HttpMethod, er.Route, er.EndpointName, er.Category
ORDER BY er.EndpointId;

GO

PRINT '========================================='
PRINT 'Endpoint synchronization completed successfully!'
PRINT '========================================='
PRINT ''
PRINT 'Summary:'
PRINT '  - 114 endpoints registered from source code'
PRINT '  - Permissions assigned based on ROLE_EXTENSION_IMPLEMENTATION_PLAN.md'
PRINT '  - Old database-only endpoints removed'
PRINT ''
PRINT 'Note: The following endpoints are NOT INCLUDED:'
PRINT '  - Endpoint Authorization endpoints (not implemented yet)'
PRINT '  - Diagnostic endpoints (DEBUG-only)'
PRINT '  - Test Identity endpoints (DEBUG-only)'
PRINT '  - Old /audit-trail endpoints (replaced with /audittrail)'
PRINT ''
GO
