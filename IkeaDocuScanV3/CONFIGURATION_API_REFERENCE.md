# Configuration API Endpoints Reference

Complete reference for all Configuration Management API endpoints.

**Base Path:** `/api/configuration`
**Authorization:** All endpoints require `SuperUser` policy

## Email Recipients Endpoints

### Get All Email Recipient Groups
```http
GET /api/configuration/email-recipients
```

**Response:**
```json
[
  {
    "groupId": 1,
    "groupKey": "AdminNotifications",
    "groupName": "Administrator Notifications",
    "recipients": [
      {
        "recipientId": 1,
        "emailAddress": "admin@company.com",
        "sortOrder": 1
      }
    ]
  }
]
```

### Get Specific Recipient Group
```http
GET /api/configuration/email-recipients/{groupKey}
```

**Parameters:**
- `groupKey` (path) - Unique key for the recipient group (e.g., "AdminNotifications")

**Response:**
```json
{
  "groupKey": "AdminNotifications",
  "recipients": [
    "admin1@company.com",
    "admin2@company.com"
  ]
}
```

### Update Recipient Group
```http
POST /api/configuration/email-recipients/{groupKey}
```

**Parameters:**
- `groupKey` (path) - Unique key for the recipient group

**Request Body:**
```json
{
  "emailAddresses": [
    "admin1@company.com",
    "admin2@company.com"
  ],
  "reason": "Adding new admin email"
}
```

**Features:**
- Automatically creates audit trail entry
- Transactional (rolls back on error)
- Creates group if it doesn't exist

---

## Email Templates Endpoints

### Get All Email Templates
```http
GET /api/configuration/email-templates
```

**Response:**
```json
[
  {
    "templateId": 1,
    "templateName": "Access Request Notification",
    "templateKey": "AccessRequestNotification",
    "subject": "Access Request from {{Username}}",
    "htmlBody": "<html>...</html>",
    "plainTextBody": "...",
    "category": "Notifications",
    "isActive": true,
    "isDefault": true,
    "placeholderDefinitions": "{...}",
    "createdBy": "system",
    "createdDate": "2025-01-01T00:00:00Z",
    "modifiedBy": "admin",
    "modifiedDate": "2025-01-05T10:30:00Z"
  }
]
```

### Get Email Template by Key
```http
GET /api/configuration/email-templates/{key}
```

**Parameters:**
- `key` (path) - Template key (e.g., "AccessRequestNotification")

**Response:** Single `EmailTemplateDto` object

### Create Email Template
```http
POST /api/configuration/email-templates
```

**Request Body:**
```json
{
  "templateName": "Custom Notification",
  "templateKey": "CustomNotification",
  "subject": "Notification for {{Username}}",
  "htmlBody": "<html><body><p>Hello {{Username}}</p></body></html>",
  "plainTextBody": "Hello {{Username}}",
  "placeholderDefinitions": "{\"placeholders\": [...]}",
  "category": "Notifications",
  "isActive": true,
  "isDefault": false
}
```

**Features:**
- Template validation before saving
- Checks for balanced braces and loop tags
- Returns `201 Created` with template details
- Location header: `/api/configuration/email-templates/{key}`

### Update Email Template
```http
PUT /api/configuration/email-templates/{id}
```

**Parameters:**
- `id` (path) - Template ID (numeric)

**Request Body:**
```json
{
  "templateId": 1,
  "templateName": "Updated Template",
  "templateKey": "AccessRequestNotification",
  "subject": "Updated subject {{Username}}",
  "htmlBody": "<html>...</html>",
  "plainTextBody": "...",
  "placeholderDefinitions": "...",
  "category": "Notifications",
  "isActive": true,
  "isDefault": false
}
```

**Features:**
- ID in URL must match ID in body
- Template validation before saving
- Audit trail entry created

### Deactivate Email Template
```http
DELETE /api/configuration/email-templates/{id}
```

**Parameters:**
- `id` (path) - Template ID (numeric)

**Notes:**
- Soft delete (sets `IsActive = false`)
- Does not permanently delete from database
- Returns `204 No Content` on success

---

## Configuration CRUD Endpoints

### List Configuration Sections
```http
GET /api/configuration/sections
```

**Response:**
```json
[
  {
    "section": "Email",
    "description": "Email server and notification settings"
  },
  {
    "section": "ActionReminder",
    "description": "Action reminder service settings"
  },
  {
    "section": "Application",
    "description": "General application settings"
  },
  {
    "section": "Security",
    "description": "Security and authentication settings"
  }
]
```

### Get Configuration Value
```http
GET /api/configuration/{section}/{key}
```

**Parameters:**
- `section` (path) - Configuration section (e.g., "Email")
- `key` (path) - Configuration key (e.g., "SmtpHost")

**Example:**
```http
GET /api/configuration/Email/SmtpHost
```

**Response:**
```json
{
  "section": "Email",
  "key": "SmtpHost",
  "value": "smtp.office365.com"
}
```

### Set Configuration Value
```http
POST /api/configuration/{section}/{key}
```

**Parameters:**
- `section` (path) - Configuration section
- `key` (path) - Configuration key

**Request Body:**
```json
{
  "value": "smtp.office365.com",
  "reason": "Updating to Office 365 SMTP"
}
```

**Features:**
- Sets individual configuration value in database
- Audit trail entry created
- Uses SQL Server execution strategy for retry support
- **Note:** For SMTP configuration, use `POST /api/configuration/smtp` instead for atomic bulk updates with testing

**Example:**
```http
POST /api/configuration/Email/SmtpHost
Content-Type: application/json

{
  "value": "smtp.office365.com",
  "reason": "Migrating to Office 365"
}
```

---

## Testing & Management Endpoints

### Update SMTP Configuration (Bulk)
```http
POST /api/configuration/smtp
```

**Request Body:**
```json
{
  "smtpHost": "smtp.office365.com",
  "smtpPort": 587,
  "useSsl": true,
  "smtpUsername": "noreply@company.com",
  "smtpPassword": "YourPassword",
  "fromAddress": "noreply@company.com",
  "fromName": "IkeaDocuScan System"
}
```

**Response (Success):**
```json
{
  "success": true,
  "message": "SMTP configuration updated and tested successfully"
}
```

**Response (Test Failed):**
```json
{
  "success": false,
  "error": "SMTP test failed. Configuration not saved.",
  "details": "SMTP configuration test failed. All changes rolled back..."
}
```

**Features:**
- **Atomic Update:** All SMTP settings are saved in a single transaction
- **Automatic Testing:** After saving all settings, SMTP connection is automatically tested with the complete configuration
- **Automatic Rollback:** If SMTP test fails, ALL changes are rolled back - no partial updates
- **Consistent State:** Ensures SMTP configuration is always in a valid, testable state
- **Audit Trail:** Creates audit entries for all settings changed

**Why This is Important:**
Previously, updating individual settings (e.g., SmtpHost, SmtpPort separately) would test after each change, causing failures with incomplete configuration. This bulk endpoint ensures all settings are saved together and tested as a complete unit.

### Test SMTP Connection
```http
POST /api/configuration/test-smtp
```

**Request Body:** None

**Response (Success):**
```json
{
  "success": true,
  "message": "SMTP connection successful"
}
```

**Response (Failure):**
```json
{
  "success": false,
  "error": "Unable to connect to SMTP server"
}
```

**Notes:**
- Tests connection using current database configuration
- Does not send actual email
- Verifies server connectivity, authentication, and SSL/TLS

### Reload Configuration Cache
```http
POST /api/configuration/reload
```

**Request Body:** None

**Response:**
```json
{
  "message": "Configuration cache reloaded successfully"
}
```

**Features:**
- Clears in-memory 5-minute TTL cache
- Forces reload from database
- No application restart required

### Migrate Configuration
```http
POST /api/configuration/migrate
```

**Request Body:**
```json
{
  "overwriteExisting": false
}
```

**Parameters:**
- `overwriteExisting` (optional, default: false) - If true, overwrites existing database values with appsettings.json values

**Response (Success):**
```json
{
  "success": true,
  "message": "Migration completed successfully",
  "details": {
    "smtpSettingsMigrated": 8,
    "recipientGroupsMigrated": 2,
    "emailTemplatesCreated": 5
  }
}
```

**Features:**
- Migrates SMTP settings from appsettings.json to database
- Migrates email recipient groups to database
- Creates 5 default email templates:
  1. AccessRequestNotification
  2. AccessRequestConfirmation
  3. ActionReminderDaily
  4. DocumentLink
  5. DocumentAttachment
- Creates audit trail entries for all migrations
- Safe to run multiple times (won't duplicate unless `overwriteExisting: true`)

---

## Template Preview & Documentation

### Preview Email Template
```http
POST /api/configuration/email-templates/preview
```

**Request Body:**
```json
{
  "template": "<html><body>Hello {{Username}}, you have {{Count}} items.</body></html>",
  "data": {
    "Username": "john.doe",
    "Count": 5
  },
  "loops": {
    "ActionRows": [
      {
        "BarCode": "12345",
        "DocumentType": "Invoice"
      },
      {
        "BarCode": "67890",
        "DocumentType": "Receipt"
      }
    ]
  }
}
```

**Response:**
```json
{
  "preview": "<html><body>Hello john.doe, you have 5 items.</body></html>"
}
```

**Notes:**
- Renders template with provided data
- Supports loops (optional)
- Useful for testing templates before saving

### Get Available Placeholders
```http
GET /api/configuration/email-templates/placeholders
```

**Response:**
```json
{
  "placeholders": [
    {
      "name": "Username",
      "description": "User's username",
      "example": "john.doe",
      "templates": ["AccessRequestNotification", "AccessRequestConfirmation"]
    },
    {
      "name": "Count",
      "description": "Number of items",
      "example": "5",
      "templates": ["ActionReminderDaily", "DocumentLinks"]
    }
  ],
  "loops": [
    {
      "name": "ActionRows",
      "description": "Loop for action reminder rows",
      "fields": ["BarCode", "DocumentType", "DocumentName", "CounterParty"],
      "templates": ["ActionReminderDaily"]
    }
  ]
}
```

---

## Common Placeholders

| Placeholder | Description | Example | Templates |
|-------------|-------------|---------|-----------|
| `{{Username}}` | User's username | john.doe | AccessRequest* |
| `{{Reason}}` | Access request reason | Need access for project X | AccessRequestNotification |
| `{{ApplicationUrl}}` | Base app URL | https://docuscan.company.com | All |
| `{{Date}}` | Current date/time | 04/11/2025 14:30 | All |
| `{{Count}}` | Number of items | 5 | ActionReminder, Document* |
| `{{BarCode}}` | Document barcode | 12345 | Document* |
| `{{DocumentLink}}` | Document URL | https://... | DocumentLink |
| `{{FileName}}` | Attachment filename | invoice.pdf | DocumentAttachment |
| `{{Message}}` | Custom message | Please review | Document* |
| `{{AdminEmail}}` | Admin email address | admin@company.com | AccessRequestConfirmation |

## Loop Structures

### ActionRows Loop
```html
{{#ActionRows}}
  <tr>
    <td>{{BarCode}}</td>
    <td>{{DocumentType}}</td>
    <td>{{DocumentName}}</td>
    <td>{{CounterParty}}</td>
    <td>{{ActionDate}}</td>
    <td>{{ReceivingDate}}</td>
    <td>{{ActionDescription}}</td>
    <td>{{IsOverdue}}</td>
  </tr>
{{/ActionRows}}
```

### DocumentRows Loop
```html
{{#DocumentRows}}
  <tr>
    <td>{{BarCode}}</td>
    <td><a href="{{Link}}">View</a></td>
    <td>{{FileName}}</td>
  </tr>
{{/DocumentRows}}
```

---

## Error Responses

All endpoints return standard error responses:

**400 Bad Request:**
```json
{
  "error": "Detailed error message"
}
```

**404 Not Found:**
```json
{
  "error": "Resource not found"
}
```

**401 Unauthorized:**
```json
{
  "error": "Authentication required"
}
```

**403 Forbidden:**
```json
{
  "error": "SuperUser authorization required"
}
```

---

## PowerShell Examples

### Test SMTP Configuration
```powershell
# Test current SMTP settings
Invoke-RestMethod -Uri "https://localhost:44101/api/configuration/test-smtp" `
    -Method POST `
    -UseDefaultCredentials `
    -SkipCertificateCheck
```

### Update SMTP Configuration (Bulk - Recommended)
```powershell
$smtpConfig = @{
    smtpHost = "smtp.office365.com"
    smtpPort = 587
    useSsl = $true
    smtpUsername = "noreply@company.com"
    smtpPassword = "YourPassword"
    fromAddress = "noreply@company.com"
    fromName = "IkeaDocuScan System"
} | ConvertTo-Json

Invoke-RestMethod -Uri "https://localhost:44101/api/configuration/smtp" `
    -Method POST `
    -Body $smtpConfig `
    -ContentType "application/json" `
    -UseDefaultCredentials `
    -SkipCertificateCheck
```

### Update Individual Configuration Setting
```powershell
# Note: For SMTP settings, use the bulk endpoint above instead
$body = @{
    value = "NewValue"
    reason = "Configuration update"
} | ConvertTo-Json

Invoke-RestMethod -Uri "https://localhost:44101/api/configuration/Application/SomeSetting" `
    -Method POST `
    -Body $body `
    -ContentType "application/json" `
    -UseDefaultCredentials `
    -SkipCertificateCheck
```

### Run Migration
```powershell
$body = @{
    overwriteExisting = $false
} | ConvertTo-Json

Invoke-RestMethod -Uri "https://localhost:44101/api/configuration/migrate" `
    -Method POST `
    -Body $body `
    -ContentType "application/json" `
    -UseDefaultCredentials `
    -SkipCertificateCheck
```

### Create Email Template
```powershell
$template = @{
    templateName = "My Custom Template"
    templateKey = "MyCustomTemplate"
    subject = "Hello {{Username}}"
    htmlBody = "<html><body><h1>Hello {{Username}}</h1></body></html>"
    plainTextBody = "Hello {{Username}}"
    category = "Notifications"
    isActive = $true
    isDefault = $false
} | ConvertTo-Json

Invoke-RestMethod -Uri "https://localhost:44101/api/configuration/email-templates" `
    -Method POST `
    -Body $template `
    -ContentType "application/json" `
    -UseDefaultCredentials `
    -SkipCertificateCheck
```

---

## Important Notes

1. **SMTP Bulk Updates (CRITICAL):** Always use `POST /api/configuration/smtp` to update SMTP settings. This endpoint saves ALL settings atomically. Use `?skipTest=true` to save without testing (default behavior in UI).

2. **SMTP Testing is Separate:** SMTP testing is performed separately via `POST /api/configuration/test-smtp`. This tests the SAVED configuration in the database, not pending changes. Users must save first, then test.

3. **Why Save and Test are Separate:**
   - **Save** is fast (< 5 seconds) and doesn't block on network issues
   - **Test** is slow (up to 15 seconds) and may timeout
   - Separation allows users to save incomplete configs and fix incrementally
   - No chicken-and-egg problem when current config is broken

4. **Execution Strategy:** Configuration updates use manual transaction control (no retry strategy) to ensure fast, predictable behavior.

4. **Audit Trail:** All configuration changes are automatically logged to `SystemConfigurationAudits` table with change reason, timestamp, and user.

5. **Cache TTL:** Configuration values are cached for 5 minutes. Use the reload endpoint to force immediate refresh.

6. **Soft Deletes:** Email templates are deactivated (soft delete) rather than permanently deleted to maintain audit trail and prevent breaking references.

7. **Authorization:** All endpoints require the `SuperUser` policy. Regular users with `HasAccess` policy cannot access configuration management.
