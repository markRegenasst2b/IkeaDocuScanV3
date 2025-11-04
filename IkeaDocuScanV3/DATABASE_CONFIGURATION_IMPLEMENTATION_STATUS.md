# Database Configuration Implementation Status

## Overview

Moving all application settings (email recipients, templates, SMTP config, etc.) from appsettings.json to database with:
- Full email templating support with placeholders
- Automatic rollback on errors
- SuperUser-only access
- Backward compatibility with appsettings.json
- No change notifications (as per requirements)

---

## Phase 1: Database Schema ✅ COMPLETED

### Files Created:
1. ✅ `IkeaDocuScan.Infrastructure/Entities/Configuration/SystemConfiguration.cs`
2. ✅ `IkeaDocuScan.Infrastructure/Entities/Configuration/SystemConfigurationAudit.cs`
3. ✅ `IkeaDocuScan.Infrastructure/Entities/Configuration/EmailTemplate.cs`
4. ✅ `IkeaDocuScan.Infrastructure/Entities/Configuration/EmailRecipientGroup.cs`
5. ✅ `IkeaDocuScan.Infrastructure/Entities/Configuration/EmailRecipient.cs`

### Files Modified:
1. ✅ `IkeaDocuScan.Infrastructure/Data/AppDbContext.cs`
   - Added 5 new DbSets
   - Added complete EF Core configuration for all entities
   - Added indexes, constraints, and relationships

### Database Tables Created:
- `SystemConfiguration` - Master configuration table
- `SystemConfigurationAudit` - Audit trail for config changes
- `EmailTemplate` - Email templates with placeholder support
- `EmailRecipientGroup` - Recipient distribution lists
- `EmailRecipient` - Individual recipients in groups

---

## Phase 2: DTOs and Interfaces ✅ PARTIALLY COMPLETED

### Files Created:
1. ✅ `IkeaDocuScan.Shared/DTOs/Configuration/EmailTemplateDto.cs`
2. ✅ `IkeaDocuScan.Shared/DTOs/Configuration/EmailRecipientGroupDto.cs`
3. ✅ `IkeaDocuScan.Shared/Interfaces/IConfigurationManager.cs`

### Still Needed:
- SystemConfigurationDto.cs (for UI display)
- ConfigurationSectionDto.cs (for grouping configs)

---

## Phase 3: Configuration Manager Service ⏳ IN PROGRESS

### Next Files to Create:

#### 1. ConfigurationManagerService.cs
**Location**: `IkeaDocuScan-Web/Services/ConfigurationManagerService.cs`

**Key Features**:
- Database-first, file-fallback pattern
- 5-minute cache TTL
- Automatic rollback using database transactions
- Error handling with graceful degradation
- Supports all configuration types (string, string[], int, bool, JSON)

**Pseudo-code**:
```csharp
public class ConfigurationManagerService : IConfigurationManager
{
    // Dependencies
    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    private readonly IConfiguration _fileConfiguration;
    private readonly ILogger _logger;
    private readonly ConcurrentDictionary<string, CacheEntry> _cache;

    // GetConfigurationAsync with database-first, file-fallback
    // SetConfigurationAsync with transaction and rollback
    // GetEmailRecipientsAsync with caching
    // SetEmailRecipientsAsync with rollback
    // GetEmailTemplateAsync
    // SaveEmailTemplateAsync with rollback
}
```

#### 2. EmailTemplateService.cs
**Location**: `IkeaDocuScan-Web/Services/EmailTemplateService.cs`

**Key Features**:
- Placeholder replacement ({{Username}}, {{Count}}, {{Date}}, etc.)
- Support for loops ({{#ActionRows}}, {{/ActionRows}})
- HTML and plain text rendering
- Template validation
- Default templates if database empty

**Key Methods**:
```csharp
- Task<string> RenderTemplateAsync(string templateKey, Dictionary<string, object> data)
- string ReplacePlaceholders(string template, Dictionary<string, object> data)
- string RenderLoop(string template, string loopKey, List<object> items)
- Task<EmailTemplateDto> ValidateTemplateAsync(EmailTemplateDto template)
```

---

## Phase 4: Service Integration ⏳ PENDING

### Files to Modify:

#### 1. EmailService.cs
**Changes**:
- Inject `IConfigurationManager`
- Get recipients from database via `GetEmailRecipientsAsync("AdminEmails")`
- Get templates from database via `GetEmailTemplateAsync("AccessRequestNotification")`
- Use EmailTemplateService to render templates
- Fallback to hard-coded templates if database empty

#### 2. ActionReminderEmailService.cs
**Changes**:
- Inject `IConfigurationManager`
- Get recipients from database via `GetEmailRecipientsAsync("ActionReminderRecipients")`
- Get template from database via `GetEmailTemplateAsync("ActionReminderDaily")`
- Use EmailTemplateService to render with action rows
- Fallback to hard-coded templates

---

## Phase 5: API Endpoints ⏳ PENDING

### Files to Create:

#### 1. ConfigurationEndpoints.cs
**Location**: `IkeaDocuScan-Web/Endpoints/ConfigurationEndpoints.cs`

**Endpoints**:
```csharp
GET  /api/configuration/sections          // Get all configuration sections
GET  /api/configuration/{section}/{key}   // Get specific config value
POST /api/configuration/{section}/{key}   // Set config value
GET  /api/configuration/test-smtp         // Test SMTP connection

GET  /api/configuration/email-recipients/{groupKey}  // Get recipients
POST /api/configuration/email-recipients/{groupKey}  // Update recipients

GET  /api/configuration/email-templates              // Get all templates
GET  /api/configuration/email-templates/{key}        // Get specific template
POST /api/configuration/email-templates              // Create template
PUT  /api/configuration/email-templates/{id}         // Update template
DELETE /api/configuration/email-templates/{id}       // Delete template

POST /api/configuration/reload                       // Reload cache
POST /api/configuration/migrate                      // Migrate from appsettings

GET  /api/configuration/audit/{configKey}            // Get audit trail
```

**Authorization**: All endpoints require SuperUser policy

---

## Phase 6: Management UI ⏳ PENDING

### Files to Create:

#### 1. ConfigurationManagement.razor
**Location**: `IkeaDocuScan-Web.Client/Pages/ConfigurationManagement.razor`

**Sections**:
- SMTP Configuration (SmtpHost, SmtpPort, credentials)
- Email Recipients Management (ActionReminderRecipients, AdminEmails, etc.)
- Email Templates List (with edit/create/delete)
- Cache Management (reload button)
- Test SMTP Connection button
- Configuration Audit Log viewer

#### 2. EmailTemplateEditor.razor
**Location**: `IkeaDocuScan-Web.Client/Components/Configuration/EmailTemplateEditor.razor`

**Features**:
- Rich text editor for HTML body
- Plain text editor for text version
- Subject line with placeholder autocomplete
- Placeholder documentation panel
- Live preview with sample data
- Validation (required fields, valid placeholders)
- Save/Cancel buttons
- Template testing functionality

#### 3. EmailRecipientsEditor.razor
**Location**: `IkeaDocuScan-Web.Client/Components/Configuration/EmailRecipientsEditor.razor`

**Features**:
- List of current recipients
- Add/Remove buttons
- Email validation
- Sort order management
- Group description
- Active/Inactive toggle

---

## Phase 7: Migration Tool ⏳ PENDING

### Files to Create:

#### 1. ConfigurationMigrationService.cs
**Location**: `IkeaDocuScan-Web/Services/ConfigurationMigrationService.cs`

**Methods**:
```csharp
- Task MigrateAllToDatabase(string migratedBy)
- Task MigrateEmailConfiguration()
- Task MigrateActionReminderConfiguration()
- Task MigrateEmailRecipients()
- Task CreateDefaultEmailTemplates()
```

**Default Templates to Create**:
1. AccessRequestNotification
2. AccessRequestConfirmation
3. ActionReminderDaily
4. DocumentLink
5. DocumentAttachment

---

## Phase 8: Service Registration ⏳ PENDING

### Files to Modify:

#### 1. IkeaDocuScan-Web/Program.cs
```csharp
// Register ConfigurationManager
builder.Services.AddScoped<IConfigurationManager, ConfigurationManagerService>();

// Register EmailTemplateService
builder.Services.AddScoped<IEmailTemplateService, EmailTemplateService>();

// Register migration service (if needed)
builder.Services.AddScoped<ConfigurationMigrationService>();

// Map configuration endpoints
app.MapConfigurationEndpoints();
```

#### 2. IkeaDocuScan.ActionReminderService/Program.cs
```csharp
// Register ConfigurationManager
builder.Services.AddScoped<IConfigurationManager, ConfigurationManagerService>();
```

---

## Phase 9: Testing ⏳ PENDING

### Test Scenarios:

#### 1. Database-First, File-Fallback
- [ ] Configuration exists in database → use database value
- [ ] Configuration not in database → use appsettings.json value
- [ ] Database unavailable → fallback to appsettings.json gracefully
- [ ] Cache expires → reload from database

#### 2. Automatic Rollback
- [ ] SMTP test fails → rollback configuration changes
- [ ] Invalid email template → rollback template changes
- [ ] Database save fails → rollback all changes
- [ ] Validation fails → rollback

#### 3. Email Templates
- [ ] Template with placeholders renders correctly
- [ ] Template with loops renders correctly
- [ ] Missing placeholders handled gracefully
- [ ] HTML and plain text versions both work
- [ ] Default templates used when database empty

#### 4. Recipient Management
- [ ] Add recipients → saves to database
- [ ] Remove recipients → deletes from database
- [ ] Change order → updates sort order
- [ ] Deactivate recipient → keeps in database but not used

#### 5. Audit Trail
- [ ] All config changes logged
- [ ] Audit log shows old and new values
- [ ] Audit log includes username and timestamp
- [ ] Audit log filterable by config key

#### 6. Security
- [ ] Only SuperUser can access configuration UI
- [ ] Only SuperUser can call configuration endpoints
- [ ] Configuration API requires authentication
- [ ] Sensitive values (passwords) not logged in audit

---

## Rollback Strategy

### Automatic Rollback Implementation:

```csharp
public async Task SetConfigurationAsync<T>(string configKey, string section, T value, string changedBy, string? reason = null)
{
    await using var context = await _contextFactory.CreateDbContextAsync();
    await using var transaction = await context.Database.BeginTransactionAsync();

    try
    {
        var existing = await context.SystemConfigurations
            .FirstOrDefaultAsync(c => c.ConfigKey == configKey && c.ConfigSection == section);

        string? oldValue = existing?.ConfigValue;

        // Save new configuration
        if (existing != null)
        {
            existing.ConfigValue = SerializeValue(value);
            existing.ModifiedBy = changedBy;
            existing.ModifiedDate = DateTime.UtcNow;
        }
        else
        {
            // Create new
        }

        // Create audit entry
        var audit = new SystemConfigurationAudit
        {
            ConfigurationId = existing.ConfigurationId,
            ConfigKey = configKey,
            OldValue = oldValue,
            NewValue = SerializeValue(value),
            ChangedBy = changedBy,
            ChangedDate = DateTime.UtcNow,
            ChangeReason = reason
        };
        context.SystemConfigurationAudits.Add(audit);

        await context.SaveChangesAsync();

        // Validate the configuration works
        if (section == "Email" && configKey == "SmtpHost")
        {
            var testResult = await TestSmtpConnectionAsync();
            if (!testResult)
            {
                _logger.LogWarning("SMTP test failed. Rolling back configuration change.");
                throw new InvalidOperationException("SMTP configuration test failed");
            }
        }

        // Commit transaction
        await transaction.CommitAsync();

        // Invalidate cache
        _cache.TryRemove($"{section}:{configKey}", out _);

        _logger.LogInformation("Configuration updated: {ConfigKey} by {User}", configKey, changedBy);
    }
    catch (Exception ex)
    {
        // Rollback transaction
        await transaction.RollbackAsync();
        _logger.LogError(ex, "Error updating configuration. Rolled back changes.");
        throw;
    }
}
```

---

## Next Steps

### Immediate Priority (in order):

1. **Create ConfigurationManagerService.cs** ← START HERE
   - Implement all methods with caching
   - Add automatic rollback logic
   - Test database-first, file-fallback pattern

2. **Create EmailTemplateService.cs**
   - Implement placeholder replacement
   - Add loop rendering for action rows
   - Create default templates

3. **Update EmailService.cs and ActionReminderEmailService.cs**
   - Integrate with ConfigurationManager
   - Use EmailTemplateService for rendering

4. **Create ConfigurationEndpoints.cs**
   - Add all REST API endpoints
   - Require SuperUser authorization

5. **Create ConfigurationManagement.razor UI**
   - Email recipients management
   - Email templates CRUD
   - SMTP testing

6. **Create ConfigurationMigrationService.cs**
   - Migrate existing appsettings to database
   - Create default email templates
   - One-time migration endpoint

7. **Test everything**
   - Manual testing of all scenarios
   - Test automatic rollback
   - Test fallback to appsettings

---

## Database Migration Command

Once services are implemented, run:

```bash
# Create EF Core migration
cd IkeaDocuScan.Infrastructure
dotnet ef migrations add AddConfigurationManagement --startup-project ../IkeaDocuScan-Web/IkeaDocuScan-Web

# Apply migration to database
dotnet ef database update --startup-project ../IkeaDocuScan-Web/IkeaDocuScan-Web
```

---

## Deployment Checklist

- [ ] Run database migration
- [ ] Deploy application (backward compatible)
- [ ] Run migration tool to populate database
- [ ] Verify email sending still works
- [ ] Test configuration changes via UI
- [ ] Monitor logs for errors
- [ ] Verify automatic rollback works
- [ ] Test fallback to appsettings if database fails

---

## Estimated Remaining Work

- **ConfigurationManagerService**: 4-6 hours
- **EmailTemplateService**: 3-4 hours
- **Service Integration**: 2-3 hours
- **API Endpoints**: 2-3 hours
- **Management UI**: 6-8 hours
- **Migration Tool**: 2-3 hours
- **Testing**: 4-6 hours

**Total**: 23-33 hours of development time

---

## Current Status

**Phase 1**: ✅ COMPLETE (Database schema, entities, DbContext)
**Phase 2**: ✅ PARTIAL (DTOs and interfaces created)
**Phase 3-9**: ⏳ PENDING (Services, endpoints, UI, testing)

**Ready for**: Creating ConfigurationManagerService.cs as the next critical component.

---

*Last Updated: 2024-11-04*
*Status: Database schema complete, ready for service implementation*
