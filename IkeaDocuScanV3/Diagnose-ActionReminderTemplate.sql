-- Diagnostic SQL to check ActionReminder email template
-- Run this against your IkeaDocuScan database

-- 1. Check if ActionReminderDaily template exists
SELECT
    TemplateId,
    TemplateName,
    TemplateKey,
    Subject,
    LEN(HtmlBody) as HtmlBodyLength,
    LEN(PlainTextBody) as PlainTextBodyLength,
    IsActive,
    IsDefault,
    CreatedDate,
    ModifiedDate
FROM EmailTemplate
WHERE TemplateKey = 'ActionReminderDaily';

-- 2. Show the actual HtmlBody content (first 500 chars)
SELECT
    TemplateKey,
    'HTML Preview' as ContentType,
    LEFT(HtmlBody, 500) as ContentPreview
FROM EmailTemplate
WHERE TemplateKey = 'ActionReminderDaily'
AND IsActive = 1;

-- 3. Show the actual Subject
SELECT
    TemplateKey,
    'Subject' as ContentType,
    Subject as ContentPreview
FROM EmailTemplate
WHERE TemplateKey = 'ActionReminderDaily'
AND IsActive = 1;

-- 4. Check for placeholder format in template
SELECT
    TemplateKey,
    CASE
        WHEN HtmlBody LIKE '%{{#ActionRows}}%' THEN 'YES - Has loop start marker'
        ELSE 'NO - Missing loop start marker'
    END as HasLoopStart,
    CASE
        WHEN HtmlBody LIKE '%{{/ActionRows}}%' THEN 'YES - Has loop end marker'
        ELSE 'NO - Missing loop end marker'
    END as HasLoopEnd,
    CASE
        WHEN HtmlBody LIKE '%{{BarCode}}%' THEN 'YES - Has BarCode placeholder'
        ELSE 'NO - Missing BarCode placeholder'
    END as HasBarCodePlaceholder,
    CASE
        WHEN Subject LIKE '%{{Count}}%' THEN 'YES - Has Count placeholder'
        ELSE 'NO - Missing Count placeholder'
    END as HasCountInSubject
FROM EmailTemplate
WHERE TemplateKey = 'ActionReminderDaily'
AND IsActive = 1;

-- 5. Count all placeholders in the template
SELECT
    TemplateKey,
    (LEN(HtmlBody) - LEN(REPLACE(HtmlBody, '{{', ''))) / 2 as PlaceholderCount,
    (LEN(Subject) - LEN(REPLACE(Subject, '{{', ''))) / 2 as SubjectPlaceholderCount
FROM EmailTemplate
WHERE TemplateKey = 'ActionReminderDaily'
AND IsActive = 1;

-- 6. Show FULL template content (be careful - could be large)
-- Uncomment if you need to see the complete template
/*
SELECT
    TemplateKey,
    Subject,
    HtmlBody,
    PlainTextBody
FROM EmailTemplates
WHERE TemplateKey = 'ActionReminderDaily'
AND IsActive = 1;
*/
