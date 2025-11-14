-- ============================================================================
-- IkeaDocuScan V3 - Test Data Setup Script
-- ============================================================================
-- Purpose: Create test data for business acceptance testing
-- Version: 1.0
-- Date: 2025-11-14
--
-- WARNING: This script is for TEST ENVIRONMENTS ONLY
--          Do NOT run in production!
-- ============================================================================

USE IkeaDocuScan;
GO

PRINT 'Starting Test Data Setup...';
PRINT '';

-- ============================================================================
-- 1. CREATE TEST USERS
-- ============================================================================
PRINT '1. Creating Test Users...';

-- Reader User
IF NOT EXISTS (SELECT 1 FROM DocuScanUser WHERE AccountName = 'DOMAIN\test_reader')
BEGIN
    INSERT INTO DocuScanUser (AccountName, IsSuperUser, CreatedDate)
    VALUES ('DOMAIN\test_reader', 0, GETUTCDATE());
    PRINT '  - Created test_reader';
END

-- Publisher User
IF NOT EXISTS (SELECT 1 FROM DocuScanUser WHERE AccountName = 'DOMAIN\test_publisher')
BEGIN
    INSERT INTO DocuScanUser (AccountName, IsSuperUser, CreatedDate)
    VALUES ('DOMAIN\test_publisher', 0, GETUTCDATE());
    PRINT '  - Created test_publisher';
END

-- SuperUser
IF NOT EXISTS (SELECT 1 FROM DocuScanUser WHERE AccountName = 'DOMAIN\test_superuser')
BEGIN
    INSERT INTO DocuScanUser (AccountName, IsSuperUser, CreatedDate)
    VALUES ('DOMAIN\test_superuser', 1, GETUTCDATE());
    PRINT '  - Created test_superuser';
END

PRINT '';

-- ============================================================================
-- 2. CREATE TEST REFERENCE DATA
-- ============================================================================
PRINT '2. Creating Test Reference Data...';

-- Test Countries
INSERT INTO Countries (CountryCode, CountryName, CreatedBy, CreatedDate)
SELECT * FROM (VALUES
    ('SE', 'Sweden', 'test_setup', GETUTCDATE()),
    ('DK', 'Denmark', 'test_setup', GETUTCDATE()),
    ('NO', 'Norway', 'test_setup', GETUTCDATE()),
    ('DE', 'Germany', 'test_setup', GETUTCDATE()),
    ('FI', 'Finland', 'test_setup', GETUTCDATE())
) AS Source(CountryCode, CountryName, CreatedBy, CreatedDate)
WHERE NOT EXISTS (SELECT 1 FROM Countries WHERE Countries.CountryCode = Source.CountryCode);
PRINT '  - Created test countries';

-- Test Currencies
INSERT INTO Currencies (CurrencyCode, CurrencyName, Symbol, CreatedBy, CreatedDate)
SELECT * FROM (VALUES
    ('SEK', 'Swedish Krona', 'kr', 'test_setup', GETUTCDATE()),
    ('DKK', 'Danish Krone', 'kr', 'test_setup', GETUTCDATE()),
    ('NOK', 'Norwegian Krone', 'kr', 'test_setup', GETUTCDATE()),
    ('EUR', 'Euro', '€', 'test_setup', GETUTCDATE()),
    ('USD', 'US Dollar', '$', 'test_setup', GETUTCDATE())
) AS Source(CurrencyCode, CurrencyName, Symbol, CreatedBy, CreatedDate)
WHERE NOT EXISTS (SELECT 1 FROM Currencies WHERE Currencies.CurrencyCode = Source.CurrencyCode);
PRINT '  - Created test currencies';

-- Test Document Types
DECLARE @InvoiceTypeId INT, @ContractTypeId INT, @POTypeId INT, @AgreementTypeId INT;

IF NOT EXISTS (SELECT 1 FROM DocumentTypes WHERE DtName = 'Invoice')
BEGIN
    INSERT INTO DocumentTypes (DtName, RequireAmount, RequireCurrency, IsEnabled, CreatedBy, CreatedDate)
    VALUES ('Invoice', 1, 1, 1, 'test_setup', GETUTCDATE());
    SET @InvoiceTypeId = SCOPE_IDENTITY();
    PRINT '  - Created Invoice document type';
END
ELSE
    SET @InvoiceTypeId = (SELECT DocumentTypeId FROM DocumentTypes WHERE DtName = 'Invoice');

IF NOT EXISTS (SELECT 1 FROM DocumentTypes WHERE DtName = 'Contract')
BEGIN
    INSERT INTO DocumentTypes (DtName, RequireAmount, RequireCurrency, IsEnabled, CreatedBy, CreatedDate)
    VALUES ('Contract', 0, 0, 1, 'test_setup', GETUTCDATE());
    SET @ContractTypeId = SCOPE_IDENTITY();
    PRINT '  - Created Contract document type';
END
ELSE
    SET @ContractTypeId = (SELECT DocumentTypeId FROM DocumentTypes WHERE DtName = 'Contract');

IF NOT EXISTS (SELECT 1 FROM DocumentTypes WHERE DtName = 'Purchase Order')
BEGIN
    INSERT INTO DocumentTypes (DtName, RequireAmount, RequireCurrency, IsEnabled, CreatedBy, CreatedDate)
    VALUES ('Purchase Order', 1, 1, 1, 'test_setup', GETUTCDATE());
    SET @POTypeId = SCOPE_IDENTITY();
    PRINT '  - Created Purchase Order document type';
END
ELSE
    SET @POTypeId = (SELECT DocumentTypeId FROM DocumentTypes WHERE DtName = 'Purchase Order');

IF NOT EXISTS (SELECT 1 FROM DocumentTypes WHERE DtName = 'Agreement')
BEGIN
    INSERT INTO DocumentTypes (DtName, RequireAmount, RequireCurrency, IsEnabled, CreatedBy, CreatedDate)
    VALUES ('Agreement', 0, 0, 1, 'test_setup', GETUTCDATE');
    SET @AgreementTypeId = SCOPE_IDENTITY();
    PRINT '  - Created Agreement document type';
END
ELSE
    SET @AgreementTypeId = (SELECT DocumentTypeId FROM DocumentTypes WHERE DtName = 'Agreement');

-- Test Counter Parties
INSERT INTO CounterParties (Name, CountryCode, IsActive, CreatedBy, CreatedDate)
SELECT * FROM (VALUES
    ('ACME Corporation', 'SE', 1, 'test_setup', GETUTCDATE()),
    ('TechCo AB', 'DK', 1, 'test_setup', GETUTCDATE()),
    ('Nordic Supplies', 'NO', 1, 'test_setup', GETUTCDATE()),
    ('European Partners GmbH', 'DE', 1, 'test_setup', GETUTCDATE()),
    ('Scandinavian Industries', 'SE', 1, 'test_setup', GETUTCDATE()),
    ('Copenhagen Trading', 'DK', 1, 'test_setup', GETUTCDATE()),
    ('Oslo Logistics', 'NO', 1, 'test_setup', GETUTCDATE()),
    ('Finnish Exports Oy', 'FI', 1, 'test_setup', GETUTCDATE()),
    ('Test Supplier Outside Permissions', 'DE', 1, 'test_setup', GETUTCDATE()),
    ('Global Services Inc', 'SE', 1, 'test_setup', GETUTCDATE())
) AS Source(Name, CountryCode, IsActive, CreatedBy, CreatedDate)
WHERE NOT EXISTS (SELECT 1 FROM CounterParties cp WHERE cp.Name = Source.Name);
PRINT '  - Created test counter parties';

PRINT '';

-- ============================================================================
-- 3. ASSIGN USER PERMISSIONS
-- ============================================================================
PRINT '3. Assigning User Permissions...';

DECLARE @ReaderUserId INT = (SELECT UserId FROM DocuScanUser WHERE AccountName = 'DOMAIN\test_reader');
DECLARE @PublisherUserId INT = (SELECT UserId FROM DocuScanUser WHERE AccountName = 'DOMAIN\test_publisher');
DECLARE @ACMEId INT = (SELECT CounterPartyId FROM CounterParties WHERE Name = 'ACME Corporation');
DECLARE @TechCoId INT = (SELECT CounterPartyId FROM CounterParties WHERE Name = 'TechCo AB');

-- Reader Permissions (Limited)
DELETE FROM UserPermissions WHERE UserId = @ReaderUserId;
INSERT INTO UserPermissions (UserId, DocumentTypeId, CountryCode, CounterPartyId, CreatedBy, CreatedDate)
VALUES
    -- Can see Invoices for ACME in Sweden
    (@ReaderUserId, @InvoiceTypeId, 'SE', @ACMEId, 'test_setup', GETUTCDATE()),
    -- Can see Contracts for TechCo in Denmark
    (@ReaderUserId, @ContractTypeId, 'DK', @TechCoId, 'test_setup', GETUTCDATE()),
    -- Can see all document types in Norway
    (@ReaderUserId, NULL, 'NO', NULL, 'test_setup', GETUTCDATE());
PRINT '  - Assigned Reader permissions (limited access)';

-- Publisher Permissions (Broader)
DELETE FROM UserPermissions WHERE UserId = @PublisherUserId;
INSERT INTO UserPermissions (UserId, DocumentTypeId, CountryCode, CounterPartyId, CreatedBy, CreatedDate)
VALUES
    -- Can see all Invoices in Sweden
    (@PublisherUserId, @InvoiceTypeId, 'SE', NULL, 'test_setup', GETUTCDATE()),
    -- Can see all Contracts everywhere
    (@PublisherUserId, @ContractTypeId, NULL, NULL, 'test_setup', GETUTCDATE()),
    -- Can see all documents in Denmark
    (@PublisherUserId, NULL, 'DK', NULL, 'test_setup', GETUTCDATE());
PRINT '  - Assigned Publisher permissions (broad access)';

-- SuperUser has NO permissions (IsSuperUser=true bypasses filtering)
PRINT '  - SuperUser needs no permissions (sees all)';

PRINT '';

-- ============================================================================
-- 4. CREATE TEST DOCUMENTS
-- ============================================================================
PRINT '4. Creating Test Documents...';

-- Documents within Reader's permissions
INSERT INTO Documents (BarCode, DocumentTypeId, DocumentNo, CounterPartyId, CountryCode, CurrencyCode, Amount, ReceivingDate, CreatedBy, CreatedDate, Comment)
VALUES
    (2025001001, @InvoiceTypeId, 'INV-2025-001', @ACMEId, 'SE', 'SEK', 15000.00, '2025-01-15', 'test_setup', GETUTCDATE(), 'Test invoice - Reader can see'),
    (2025001002, @InvoiceTypeId, 'INV-2025-002', @ACMEId, 'SE', 'SEK', 22500.50, '2025-01-20', 'test_setup', GETUTCDATE(), 'Test invoice - Reader can see'),
    (2025001003, @ContractTypeId, 'CONT-2025-001', @TechCoId, 'DK', 'DKK', NULL, '2025-01-10', 'test_setup', GETUTCDATE(), 'Test contract - Reader can see');

-- Documents within Publisher's permissions but NOT Reader's
INSERT INTO Documents (BarCode, DocumentTypeId, DocumentNo, CounterPartyId, CountryCode, CurrencyCode, Amount, ReceivingDate, CreatedBy, CreatedDate, Comment)
VALUES
    (2025002001, @InvoiceTypeId, 'INV-2025-100', (SELECT CounterPartyId FROM CounterParties WHERE Name = 'Nordic Supplies'), 'SE', 'SEK', 50000.00, '2025-01-25', 'test_setup', GETUTCDATE(), 'Publisher can see, Reader cannot'),
    (2025002002, @ContractTypeId, 'CONT-2025-050', (SELECT CounterPartyId FROM CounterParties WHERE Name = 'Copenhagen Trading'), 'DK', 'DKK', NULL, '2025-01-18', 'test_setup', GETUTCDATE(), 'Publisher can see, Reader cannot');

-- Documents outside both Reader and Publisher permissions (only SuperUser)
INSERT INTO Documents (BarCode, DocumentTypeId, DocumentNo, CounterPartyId, CountryCode, CurrencyCode, Amount, ReceivingDate, CreatedBy, CreatedDate, Comment)
VALUES
    (2025009001, @POTypeId, 'PO-2025-001', (SELECT CounterPartyId FROM CounterParties WHERE Name = 'European Partners GmbH'), 'DE', 'EUR', 75000.00, '2025-01-12', 'test_setup', GETUTCDATE(), 'Only SuperUser can see - Germany PO'),
    (2025009002, @POTypeId, 'PO-2025-002', (SELECT CounterPartyId FROM CounterParties WHERE Name = 'Test Supplier Outside Permissions'), 'DE', 'EUR', 30000.00, '2025-01-14', 'test_setup', GETUTCDATE(), 'Only SuperUser can see');

-- Document with special characters (for testing)
INSERT INTO Documents (BarCode, DocumentTypeId, DocumentNo, CounterPartyId, CountryCode, CurrencyCode, Amount, ReceivingDate, CreatedBy, CreatedDate, Comment)
VALUES
    (2025000999, @InvoiceTypeId, 'INV/2025-ÅÄÖ', @ACMEId, 'SE', 'SEK', 1234.56, '2025-01-01', 'test_setup', GETUTCDATE(), 'Special chars: O''Brien & Associates €1,000.50');

-- Documents with ActionDate for testing Action Reminders
INSERT INTO Documents (BarCode, DocumentTypeId, DocumentNo, CounterPartyId, CountryCode, CurrencyCode, Amount, ReceivingDate, ActionDate, ActionDescription, CreatedBy, CreatedDate, Comment)
VALUES
    (2025003001, @ContractTypeId, 'CONT-ACT-001', @ACMEId, 'SE', NULL, NULL, '2025-01-01', CAST(GETDATE() AS DATE), 'Review and approve contract', 'test_setup', GETUTCDATE(), 'Action due TODAY - test reminder'),
    (2025003002, @InvoiceTypeId, 'INV-ACT-001', @ACMEId, 'SE', 'SEK', 10000, '2025-01-01', DATEADD(DAY, -1, CAST(GETDATE() AS DATE)), 'Process payment URGENT', 'test_setup', GETUTCDATE(), 'Action OVERDUE - test reminder'),
    (2025003003, @ContractTypeId, 'CONT-ACT-002', @TechCoId, 'DK', NULL, NULL, '2025-01-01', DATEADD(DAY, 1, CAST(GETDATE() AS DATE)), 'Send to legal', 'test_setup', GETUTCDATE(), 'Action due TOMORROW');

PRINT '  - Created 13 test documents';
PRINT '    - 3 visible to Reader';
PRINT '    - 5 visible to Publisher';
PRINT '    - 13 visible to SuperUser (all)';
PRINT '    - 3 with ActionDate for reminder testing';

PRINT '';

-- ============================================================================
-- 5. CREATE TEST EMAIL TEMPLATE (for Action Reminders)
-- ============================================================================
PRINT '5. Creating Test Email Template...';

IF NOT EXISTS (SELECT 1 FROM EmailTemplates WHERE TemplateKey = 'ActionReminderDaily')
BEGIN
    INSERT INTO EmailTemplates (
        TemplateName, TemplateKey, Subject, HtmlBody, PlainTextBody,
        PlaceholderDefinitions, Category, IsActive, IsDefault, CreatedBy, CreatedDate
    )
    VALUES (
        'Daily Action Reminder',
        'ActionReminderDaily',
        'Action Reminders Due - {{Count}} Items',
        '<!DOCTYPE html><html><body><h1>Action Reminders Due Today</h1><p>You have {{Count}} action reminder(s) due on {{Date}}.</p><table><thead><tr><th>Barcode</th><th>Type</th><th>Action Date</th><th>Description</th></tr></thead><tbody>{{#ActionRows}}<tr><td>{{BarCode}}</td><td>{{DocumentType}}</td><td>{{ActionDate}}</td><td>{{ActionDescription}}</td></tr>{{/ActionRows}}</tbody></table></body></html>',
        'You have {{Count}} action reminders due.\n\n{{#ActionRows}}- {{BarCode}}: {{ActionDescription}}\n{{/ActionRows}}',
        '{"Global":["Count","Date"],"ActionRows":["BarCode","DocumentType","ActionDate","ActionDescription"]}',
        'ActionReminder',
        1,
        1,
        'test_setup',
        GETUTCDATE()
    );
    PRINT '  - Created ActionReminderDaily email template';
END
ELSE
    PRINT '  - ActionReminderDaily template already exists';

PRINT '';

-- ============================================================================
-- 6. VERIFICATION QUERIES
-- ============================================================================
PRINT '6. Verification Summary...';
PRINT '';

PRINT 'Test Users:';
SELECT AccountName, IsSuperUser, CreatedDate FROM DocuScanUser WHERE AccountName LIKE '%test_%';
PRINT '';

PRINT 'User Permissions Summary:';
SELECT
    u.AccountName,
    COUNT(up.PermissionId) as PermissionCount,
    STRING_AGG(CONCAT(ISNULL(dt.DtName, 'ALL'), ' / ', ISNULL(up.CountryCode, 'ALL')), '; ') as Permissions
FROM DocuScanUser u
LEFT JOIN UserPermissions up ON u.UserId = up.UserId
LEFT JOIN DocumentTypes dt ON up.DocumentTypeId = dt.DocumentTypeId
WHERE u.AccountName LIKE '%test_%'
GROUP BY u.AccountName;
PRINT '';

PRINT 'Document Count by Visibility:';
PRINT '  - Total Documents: ' + CAST((SELECT COUNT(*) FROM Documents WHERE BarCode >= 2025000000) AS VARCHAR);
PRINT '  - With ActionDate: ' + CAST((SELECT COUNT(*) FROM Documents WHERE ActionDate IS NOT NULL AND BarCode >= 2025000000) AS VARCHAR);
PRINT '';

PRINT 'Reference Data Counts:';
SELECT 'Countries' as DataType, COUNT(*) as Count FROM Countries
UNION ALL SELECT 'Currencies', COUNT(*) FROM Currencies
UNION ALL SELECT 'DocumentTypes', COUNT(*) FROM DocumentTypes WHERE IsEnabled = 1
UNION ALL SELECT 'CounterParties', COUNT(*) FROM CounterParties WHERE IsActive = 1;
PRINT '';

-- ============================================================================
-- COMPLETION
-- ============================================================================
PRINT '';
PRINT '============================================================================';
PRINT 'Test Data Setup Complete!';
PRINT '============================================================================';
PRINT '';
PRINT 'Next Steps:';
PRINT '1. Login as DOMAIN\test_reader - Should see 3-4 documents';
PRINT '2. Login as DOMAIN\test_publisher - Should see 5-7 documents';
PRINT '3. Login as DOMAIN\test_superuser - Should see all 13+ documents';
PRINT '4. Test Action Reminders (should find 2-3 items due)';
PRINT '5. Run business test plans in BusinessTestSupport folder';
PRINT '';
PRINT 'WARNING: This is TEST DATA ONLY - Do not use in production!';
PRINT '============================================================================';

GO
