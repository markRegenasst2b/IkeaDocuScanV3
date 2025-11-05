-- Verify-ConfigurationDatabase.sql
-- SQL script to verify configuration database state after migration

-- ============================================================
-- 1. Check System Configurations
-- ============================================================
PRINT '=== System Configurations ==='
SELECT
    ConfigSection,
    ConfigKey,
    ConfigValue,
    CreatedBy,
    CreatedDate,
    ModifiedBy,
    ModifiedDate
FROM SystemConfigurations
ORDER BY ConfigSection, ConfigKey;

PRINT ''
PRINT 'Total System Configurations: ' + CAST((SELECT COUNT(*) FROM SystemConfigurations) AS VARCHAR)
PRINT ''

-- ============================================================
-- 2. Check SMTP Settings Specifically
-- ============================================================
PRINT '=== SMTP Settings ==='
SELECT
    ConfigKey,
    ConfigValue,
    ModifiedBy,
    ModifiedDate
FROM SystemConfigurations
WHERE ConfigSection = 'Email' AND ConfigKey LIKE '%Smtp%'
ORDER BY ConfigKey;

PRINT ''

-- ============================================================
-- 3. Check Email Recipient Groups
-- ============================================================
PRINT '=== Email Recipient Groups ==='
SELECT
    GroupKey,
    GroupName,
    CreatedBy,
    CreatedDate
FROM EmailRecipientGroups
ORDER BY GroupKey;

PRINT ''
PRINT 'Total Email Recipient Groups: ' + CAST((SELECT COUNT(*) FROM EmailRecipientGroups) AS VARCHAR)
PRINT ''

-- ============================================================
-- 4. Check Email Recipients (with group names)
-- ============================================================
PRINT '=== Email Recipients ==='
SELECT
    erg.GroupKey,
    erg.GroupName,
    er.EmailAddress,
    er.SortOrder,
    er.CreatedBy,
    er.CreatedDate
FROM EmailRecipients er
INNER JOIN EmailRecipientGroups erg ON er.GroupId = erg.GroupId
ORDER BY erg.GroupKey, er.SortOrder;

PRINT ''
PRINT 'Total Email Recipients: ' + CAST((SELECT COUNT(*) FROM EmailRecipients) AS VARCHAR)
PRINT ''

-- ============================================================
-- 5. Check Email Templates
-- ============================================================
PRINT '=== Email Templates ==='
SELECT
    TemplateId,
    TemplateName,
    TemplateKey,
    Category,
    Subject,
    IsActive,
    IsDefault,
    CreatedBy,
    CreatedDate,
    ModifiedBy,
    ModifiedDate,
    LEN(HtmlBody) AS HtmlBodyLength,
    LEN(PlainTextBody) AS PlainTextBodyLength
FROM EmailTemplates
ORDER BY Category, TemplateName;

PRINT ''
PRINT 'Total Email Templates: ' + CAST((SELECT COUNT(*) FROM EmailTemplates) AS VARCHAR)
PRINT 'Active Templates: ' + CAST((SELECT COUNT(*) FROM EmailTemplates WHERE IsActive = 1) AS VARCHAR)
PRINT 'Default Templates: ' + CAST((SELECT COUNT(*) FROM EmailTemplates WHERE IsDefault = 1) AS VARCHAR)
PRINT ''

-- ============================================================
-- 6. Check Configuration Audit Trail
-- ============================================================
PRINT '=== Recent Configuration Changes (Last 20) ==='
SELECT TOP 20
    AuditId,
    ConfigKey,
    OldValue,
    NewValue,
    ChangedBy,
    ChangedDate,
    ChangeReason
FROM SystemConfigurationAudits
ORDER BY ChangedDate DESC;

PRINT ''
PRINT 'Total Audit Entries: ' + CAST((SELECT COUNT(*) FROM SystemConfigurationAudits) AS VARCHAR)
PRINT ''

-- ============================================================
-- 7. Check for Missing Required Configuration
-- ============================================================
PRINT '=== Configuration Validation ==='

-- Check for SMTP settings
DECLARE @SmtpHost VARCHAR(500) = (SELECT ConfigValue FROM SystemConfigurations WHERE ConfigSection = 'Email' AND ConfigKey = 'SmtpHost')
DECLARE @SmtpPort VARCHAR(500) = (SELECT ConfigValue FROM SystemConfigurations WHERE ConfigSection = 'Email' AND ConfigKey = 'SmtpPort')
DECLARE @FromAddress VARCHAR(500) = (SELECT ConfigValue FROM SystemConfigurations WHERE ConfigSection = 'Email' AND ConfigKey = 'FromAddress')

IF @SmtpHost IS NULL
    PRINT 'WARNING: SmtpHost not configured'
ELSE
    PRINT 'SmtpHost: ' + @SmtpHost

IF @SmtpPort IS NULL
    PRINT 'WARNING: SmtpPort not configured'
ELSE
    PRINT 'SmtpPort: ' + @SmtpPort

IF @FromAddress IS NULL
    PRINT 'WARNING: FromAddress not configured'
ELSE
    PRINT 'FromAddress: ' + @FromAddress

PRINT ''

-- Check for default email templates
DECLARE @TemplateCount INT = (SELECT COUNT(*) FROM EmailTemplates)
IF @TemplateCount = 0
    PRINT 'WARNING: No email templates found. Run migration to create defaults.'
ELSE IF @TemplateCount < 5
    PRINT 'WARNING: Expected 5 default templates, found ' + CAST(@TemplateCount AS VARCHAR)
ELSE
    PRINT 'Email templates: OK (' + CAST(@TemplateCount AS VARCHAR) + ' templates)'

PRINT ''

-- Check for required recipient groups
DECLARE @AdminGroupExists BIT = (SELECT CASE WHEN EXISTS(SELECT 1 FROM EmailRecipientGroups WHERE GroupKey = 'AdminNotifications') THEN 1 ELSE 0 END)
DECLARE @AccessGroupExists BIT = (SELECT CASE WHEN EXISTS(SELECT 1 FROM EmailRecipientGroups WHERE GroupKey = 'AccessRequests') THEN 1 ELSE 0 END)

IF @AdminGroupExists = 0
    PRINT 'WARNING: AdminNotifications recipient group not found'
ELSE
    PRINT 'AdminNotifications group: OK'

IF @AccessGroupExists = 0
    PRINT 'WARNING: AccessRequests recipient group not found'
ELSE
    PRINT 'AccessRequests group: OK'

PRINT ''

-- ============================================================
-- 8. Configuration Summary by Section
-- ============================================================
PRINT '=== Configuration Summary by Section ==='
SELECT
    ConfigSection,
    COUNT(*) AS ConfigCount,
    MIN(CreatedDate) AS FirstCreated,
    MAX(ModifiedDate) AS LastModified
FROM SystemConfigurations
GROUP BY ConfigSection
ORDER BY ConfigSection;

PRINT ''

-- ============================================================
-- 9. Template Usage Analysis
-- ============================================================
PRINT '=== Email Templates by Category ==='
SELECT
    Category,
    COUNT(*) AS TemplateCount,
    SUM(CASE WHEN IsActive = 1 THEN 1 ELSE 0 END) AS ActiveCount,
    SUM(CASE WHEN IsDefault = 1 THEN 1 ELSE 0 END) AS DefaultCount
FROM EmailTemplates
GROUP BY Category
ORDER BY Category;

PRINT ''

-- ============================================================
-- 10. Show Template Details (for verification)
-- ============================================================
PRINT '=== Template Details ==='
SELECT
    TemplateKey,
    TemplateName,
    Subject,
    Category,
    IsActive,
    IsDefault,
    SUBSTRING(HtmlBody, 1, 100) + '...' AS HtmlBodyPreview
FROM EmailTemplates
ORDER BY TemplateName;

PRINT ''
PRINT '=== Verification Complete ==='
