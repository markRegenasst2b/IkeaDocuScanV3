# Database Configuration - Phase 3 Complete

## Summary

Phase 3 (Configuration Manager Services) is now **COMPLETE**. The core infrastructure for database-driven configuration with automatic rollback is fully implemented.

---

## ✅ Completed (Phases 1-3)

### Phase 1: Database Schema ✅
- 5 Entity classes created
- DbContext updated with full EF Core configuration
- Indexes, constraints, and relationships configured

### Phase 2: DTOs & Interfaces ✅
- EmailTemplateDto and variants
- EmailRecipientGroupDto and EmailRecipientDto
- IConfigurationManager interface
- IEmailTemplateService interface

### Phase 3: Core Services ✅

#### ConfigurationManagerService.cs (635 lines)
**Location**: `IkeaDocuScan-Web/Services/Configuration/ConfigurationManagerService.cs`

**Key Features Implemented**:
✅ **Database-First, File-Fallback Pattern**
- Tries database first
- Falls back to appsettings.json if database unavailable
- Graceful degradation on errors

✅ **5-Minute Cache with TTL**
- `ConcurrentDictionary` for thread-safe caching
- Automatic expiration after 5 minutes
- Cache invalidation on updates

✅ **Automatic Rollback on Errors**
- Uses database transactions (`BeginTransactionAsync`)
- Tests SMTP configuration before committing SMTP changes
- Rolls back on validation failures
- Comprehensive error logging

✅ **Configuration CRUD**
- `GetConfigurationAsync<T>` - with caching
- `GetConfiguration<T>` - synchronous, cache-only
- `SetConfigurationAsync<T>` - with rollback

✅ **Email Recipients Management**
- `GetEmailRecipientsAsync` - with caching
- `SetEmailRecipientsAsync` - with rollback
- `GetAllEmailRecipientGroupsAsync`
- Auto-creates groups if not exist

✅ **Email Templates Management**
- `GetEmailTemplateAsync` - with caching
- `SaveEmailTemplateAsync` - with validation and rollback
- `GetAllEmailTemplatesAsync`
- Template validation before save

✅ **SMTP Testing**
- `TestSmtpConnectionAsync` - validates SMTP config
- Automatic testing before committing SMTP changes
- 10-second timeout for tests

✅ **Helper Methods**
- Value serialization/deserialization (String, StringArray, Int, Bool, Json)
- Admin emails extraction from config
- Group name formatting
- Template validation

#### EmailTemplateService.cs (280 lines)
**Location**: `IkeaDocuScan-Web/Services/Configuration/EmailTemplateService.cs`

**Key Features Implemented**:
✅ **Placeholder Replacement**
- Supports `{{Username}}`, `{{Count}}`, `{{Date}}`, etc.
- Regex-based replacement
- Keeps original placeholder if data not found

✅ **Loop Rendering**
- Supports `{{#ActionRows}}...{{/ActionRows}}`
- Nested data support
- Multiple loops per template

✅ **Template Validation**
- Balanced braces check
- Balanced loop tags check
- Placeholder extraction
- Missing placeholder warnings

✅ **Value Formatting**
- DateTime: `dd/MM/yyyy HH:mm`
- Numbers: Thousand separators
- Decimals: 2 decimal places
- Booleans: Yes/No
- Null-safe handling

---

## 🔧 How It Works

### Example 1: Get Configuration with Fallback

```csharp
// Inject service
private readonly IConfigurationManager _configManager;

// Get value - tries database first, falls back to appsettings.json
var smtpHost = await _configManager.GetConfigurationAsync<string>(
    "SmtpHost",
    "Email",
    "smtp.company.com" // default
);
```

**Flow**:
1. Check cache → Hit? Return cached value
2. Query database → Found active override? Return & cache
3. Fallback to `appsettings.json` → `Email:SmtpHost`
4. Use default value if all else fails

### Example 2: Set Configuration with Automatic Rollback

```csharp
// Set SMTP host - automatically tests connection before committing
await _configManager.SetConfigurationAsync(
    "SmtpHost",
    "Email",
    "newsmtp.company.com",
    "CurrentUser",
    "Changing to new SMTP server"
);
```

**Flow**:
1. Begin database transaction
2. Update SystemConfiguration table
3. Create SystemConfigurationAudit entry
4. **Test SMTP connection** (for SMTP-related configs)
5. If test fails → **Rollback** transaction
6. If test passes → Commit transaction
7. Invalidate cache

### Example 3: Email Recipients with Rollback

```csharp
// Update recipients - automatically rolls back on error
await _configManager.SetEmailRecipientsAsync(
    "ActionReminderRecipients",
    new[] { "admin1@company.com", "admin2@company.com" },
    "CurrentUser",
    "Adding new admin"
);
```

**Flow**:
1. Begin database transaction
2. Create/Update EmailRecipientGroup
3. Remove old EmailRecipient entries
4. Add new EmailRecipient entries
5. If any error → **Rollback** all changes
6. If success → Commit transaction
7. Invalidate cache

### Example 4: Render Email Template

```csharp
// Render template with placeholders
var template = await _configManager.GetEmailTemplateAsync("AccessRequestNotification");
var rendered = _emailTemplateService.RenderTemplate(
    template.HtmlBody,
    new Dictionary<string, object>
    {
        { "Username", "john.doe" },
        { "Date", DateTime.Now },
        { "ApplicationUrl", "https://docuscan.company.com" }
    }
);
```

### Example 5: Render Template with Loops

```csharp
// Render action reminder email with rows
var rendered = _emailTemplateService.RenderTemplateWithLoops(
    template.HtmlBody,
    new Dictionary<string, object>
    {
        { "Count", 5 },
        { "Date", DateTime.Now }
    },
    new Dictionary<string, List<Dictionary<string, object>>>
    {
        { "ActionRows", actionReminders.Select(a => new Dictionary<string, object>
            {
                { "BarCode", a.BarCode },
                { "DocumentType", a.DocumentType },
                { "ActionDate", a.ActionDate }
            }).ToList()
        }
    }
);
```

---

## ⏳ Remaining Work (Phases 4-7)

### Phase 4: Service Integration (2-3 hours)

**Files to Modify:**

#### 1. EmailService.cs
- Inject `IConfigurationManager` and `IEmailTemplateService`
- Update `SendAccessRequestNotificationAsync`:
  - Get admin emails from database via `GetEmailRecipientsAsync("AdminEmails")`
  - Get template from database via `GetEmailTemplateAsync("AccessRequestNotification")`
  - Render template with EmailTemplateService
  - Fallback to hard-coded template if database empty

#### 2. ActionReminderEmailService.cs
- Inject `IConfigurationManager` and `IEmailTemplateService`
- Update `SendActionReminderEmailAsync`:
  - Get recipients from database via `GetEmailRecipientsAsync("ActionReminderRecipients")`
  - Get template from database via `GetEmailTemplateAsync("ActionReminderDaily")`
  - Render template with action rows loop
  - Fallback to hard-coded template if database empty

### Phase 5: API Endpoints (2-3 hours)

**File to Create**: `IkeaDocuScan-Web/Endpoints/ConfigurationEndpoints.cs`

**Endpoints Needed**:
```csharp
// Configuration
GET  /api/configuration/sections                    // List all sections
GET  /api/configuration/{section}/{key}             // Get specific config
POST /api/configuration/{section}/{key}             // Set config (with rollback)

// Email Recipients
GET  /api/configuration/email-recipients            // Get all groups
GET  /api/configuration/email-recipients/{groupKey} // Get specific group
POST /api/configuration/email-recipients/{groupKey} // Update group (with rollback)

// Email Templates
GET  /api/configuration/email-templates             // Get all templates
GET  /api/configuration/email-templates/{key}       // Get specific template
POST /api/configuration/email-templates             // Create template (with validation)
PUT  /api/configuration/email-templates/{id}        // Update template (with validation)
DELETE /api/configuration/email-templates/{id}      // Deactivate template

// Testing & Management
POST /api/configuration/test-smtp                   // Test SMTP connection
POST /api/configuration/reload                      // Reload cache
POST /api/configuration/migrate                     // Migrate from appsettings

// Audit
GET  /api/configuration/audit/{configKey}           // Get audit trail
```

**Authorization**: All endpoints require `[Authorize(Policy = "SuperUser")]`

### Phase 6: Management UI (6-8 hours)

**Files to Create:**

#### 1. ConfigurationManagement.razor
**Location**: `IkeaDocuScan-Web.Client/Pages/ConfigurationManagement.razor`

**Sections**:
- SMTP Configuration card (host, port, credentials)
- Email Recipients card (multiple groups)
- Email Templates list (with add/edit/delete)
- Cache Management card (reload button)
- SMTP Test button
- Audit Log viewer (filter by key)

#### 2. EmailTemplateEditor.razor
**Location**: `IkeaDocuScan-Web.Client/Components/Configuration/EmailTemplateEditor.razor`

**Features**:
- Rich text editor for HTML body
- Plain text editor
- Subject line input
- Placeholder documentation panel
- Live preview with sample data
- Save/Cancel buttons
- Template key dropdown (if editing)

#### 3. EmailRecipientsEditor.razor
**Location**: `IkeaDocuScan-Web.Client/Components/Configuration/EmailRecipientsEditor.razor`

**Features**:
- Current recipients list
- Add email input with validation
- Remove buttons
- Sort order drag-and-drop
- Group description display

### Phase 7: Migration Tool (2-3 hours)

**File to Create**: `IkeaDocuScan-Web/Services/Configuration/ConfigurationMigrationService.cs`

**Methods Needed**:
```csharp
Task MigrateAllToDatabase(string migratedBy)
Task MigrateEmailConfiguration()
Task MigrateActionReminderConfiguration()
Task MigrateEmailRecipients()
Task CreateDefaultEmailTemplates()
```

**Default Templates to Create**:
1. **AccessRequestNotification**
   - Subject: `New Access Request - {{Username}}`
   - Placeholders: {{Username}}, {{Reason}}, {{ApplicationUrl}}, {{Date}}

2. **AccessRequestConfirmation**
   - Subject: `Your Access Request Has Been Received`
   - Placeholders: {{Username}}, {{AdminEmail}}, {{Date}}

3. **ActionReminderDaily**
   - Subject: `Action Reminders Due Today - {{Count}} Items`
   - Placeholders: {{Count}}, {{Date}}
   - Loop: {{#ActionRows}} with {{BarCode}}, {{DocumentType}}, etc.

4. **DocumentLink**
   - Subject: `Document Shared: {{BarCode}}`
   - Placeholders: {{BarCode}}, {{DocumentLink}}, {{Message}}

5. **DocumentAttachment**
   - Subject: `Document: {{BarCode}}`
   - Placeholders: {{BarCode}}, {{FileName}}, {{Message}}

---

## 📋 Service Registration

Add to **IkeaDocuScan-Web/Program.cs**:

```csharp
// Register configuration services
builder.Services.AddScoped<IConfigurationManager, ConfigurationManagerService>();
builder.Services.AddScoped<IEmailTemplateService, EmailTemplateService>();

// Map configuration endpoints (after creating)
app.MapConfigurationEndpoints();
```

Add to **IkeaDocuScan.ActionReminderService/Program.cs**:

```csharp
// Register configuration manager
builder.Services.AddScoped<IConfigurationManager, ConfigurationManagerService>();
builder.Services.AddScoped<IEmailTemplateService, EmailTemplateService>();
```

---

## 🗄️ Create EF Core Migration

Once services are registered, create the migration:

```bash
cd IkeaDocuScan.Infrastructure
dotnet ef migrations add AddConfigurationManagement --startup-project ../IkeaDocuScan-Web/IkeaDocuScan-Web

# Apply migration
dotnet ef database update --startup-project ../IkeaDocuScan-Web/IkeaDocuScan-Web
```

---

## 🧪 Testing Scenarios

### Test 1: Database-First, File-Fallback
- Configuration exists in DB → uses DB value
- Configuration not in DB → uses appsettings
- Database unavailable → graceful fallback

### Test 2: Automatic Rollback
- Invalid SMTP config → rolls back changes
- Database save fails → rolls back transaction
- Template validation fails → rolls back changes

### Test 3: Caching
- First access → hits database
- Second access → hits cache
- After 5 minutes → refreshes from database
- After update → cache invalidated

### Test 4: Email Templates
- Template with placeholders → renders correctly
- Template with loops → renders rows
- Missing placeholders → keeps original markers
- Invalid template → rollback

### Test 5: Audit Trail
- Config change → audit entry created
- Old value saved
- New value saved
- Username and reason logged

---

## 📊 Implementation Progress

| Phase | Status | Files | Completion |
|-------|--------|-------|------------|
| 1. Database Schema | ✅ Complete | 6 files | 100% |
| 2. DTOs & Interfaces | ✅ Complete | 3 files | 100% |
| 3. Core Services | ✅ Complete | 2 files | 100% |
| 4. Service Integration | ⏳ Pending | 2 files | 0% |
| 5. API Endpoints | ⏳ Pending | 1 file | 0% |
| 6. Management UI | ⏳ Pending | 3 files | 0% |
| 7. Migration Tool | ⏳ Pending | 1 file | 0% |

**Overall Progress**: 11/18 files complete (61%)
**Estimated Remaining**: 10-14 hours

---

## 🎯 Next Steps

**Immediate Priority**:
1. Register services in Program.cs (both projects)
2. Create EF Core migration
3. Test ConfigurationManagerService manually
4. Update EmailService.cs to use ConfigurationManager
5. Update ActionReminderEmailService.cs
6. Create ConfigurationEndpoints.cs
7. Create ConfigurationManagement.razor UI

---

## 📦 Files Created in Phase 3

### Services (2 files)
1. ✅ `IkeaDocuScan-Web/Services/Configuration/ConfigurationManagerService.cs` (635 lines)
2. ✅ `IkeaDocuScan-Web/Services/Configuration/EmailTemplateService.cs` (280 lines)

### Interfaces (1 file)
3. ✅ `IkeaDocuScan.Shared/Interfaces/IEmailTemplateService.cs`

**Total**: 3 new files, ~915 lines of code

---

## 🔑 Key Features Delivered

✅ **Automatic Rollback**: Database transactions with automatic rollback on validation failures
✅ **SMTP Testing**: Tests SMTP connection before committing SMTP configuration changes
✅ **Caching**: 5-minute TTL cache for performance
✅ **Database-First**: Tries database first, falls back to appsettings.json gracefully
✅ **Placeholder Support**: Full template rendering with placeholders like {{Username}}
✅ **Loop Support**: Render tables with {{#ActionRows}}...{{/ActionRows}}
✅ **Value Formatting**: Automatic formatting for dates, numbers, booleans
✅ **Template Validation**: Validates templates before saving
✅ **Audit Trail**: Complete audit logging with old/new values
✅ **Thread-Safe**: Uses DbContextFactory for safe concurrent access
✅ **Error Handling**: Comprehensive error handling with logging

---

**Phase 3 Status**: ✅ **COMPLETE**
**Ready For**: Phase 4 (Service Integration) or Phase 5 (API Endpoints)

---

*Last Updated: 2024-11-04*
*Next Milestone: Service Integration or API Endpoints*
