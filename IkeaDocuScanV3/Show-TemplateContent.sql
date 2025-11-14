-- Show the FULL template content for ActionReminderDaily

SELECT
    TemplateKey,
    Subject,
    HtmlBody
FROM EmailTemplate
WHERE TemplateKey = 'ActionReminderDaily'
AND IsActive = 1;

-- Also check placeholder format
SELECT
    'Placeholder Check' as TestType,
    CASE
        WHEN HtmlBody LIKE '%{{#ActionRows}}%' THEN 'FOUND {{#ActionRows}}'
        ELSE 'NOT FOUND {{#ActionRows}}'
    END as LoopStartMarker,
    CASE
        WHEN HtmlBody LIKE '%{{/ActionRows}}%' THEN 'FOUND {{/ActionRows}}'
        ELSE 'NOT FOUND {{/ActionRows}}'
    END as LoopEndMarker,
    CASE
        WHEN HtmlBody LIKE '%{{BarCode}}%' THEN 'FOUND {{BarCode}}'
        ELSE 'NOT FOUND {{BarCode}}'
    END as BarCodePlaceholder,
    CASE
        WHEN HtmlBody LIKE '%{{DocumentType}}%' THEN 'FOUND {{DocumentType}}'
        ELSE 'NOT FOUND {{DocumentType}}'
    END as DocumentTypePlaceholder
FROM EmailTemplates
WHERE TemplateKey = 'ActionReminderDaily'
AND IsActive = 1;
