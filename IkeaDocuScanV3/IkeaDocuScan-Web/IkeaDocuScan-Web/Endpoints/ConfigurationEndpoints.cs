using IkeaDocuScan.Shared.DTOs.Configuration;
using IkeaDocuScan.Shared.Interfaces;
using IkeaDocuScan_Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IkeaDocuScan_Web.Endpoints;

/// <summary>
/// API endpoints for system configuration management
/// All endpoints require SuperUser authorization
/// </summary>
public static class ConfigurationEndpoints
{
    public static void MapConfigurationEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/configuration")
            .RequireAuthorization("SuperUser")
            .WithTags("Configuration");

        // ===== Email Recipients Endpoints =====

        group.MapGet("/email-recipients", async (ISystemConfigurationManager service) =>
        {
            var groups = await service.GetAllEmailRecipientGroupsAsync();
            return Results.Ok(groups);
        })
        .WithName("GetAllEmailRecipientGroups")
        .Produces<List<EmailRecipientGroupDto>>(200)
        .WithDescription("Get all email recipient groups");

        group.MapGet("/email-recipients/{groupKey}", async (string groupKey, ISystemConfigurationManager service) =>
        {
            var recipients = await service.GetEmailRecipientsAsync(groupKey);

            if (recipients.Length == 0)
                return Results.NotFound(new { error = $"Email recipient group '{groupKey}' not found or has no recipients" });

            return Results.Ok(new { groupKey, recipients });
        })
        .WithName("GetEmailRecipientGroup")
        .Produces(200)
        .Produces(404)
        .WithDescription("Get specific email recipient group");

        group.MapPost("/email-recipients/{groupKey}", async (
            string groupKey,
            [FromBody] SetEmailRecipientsRequest request,
            ISystemConfigurationManager service,
            HttpContext httpContext) =>
        {
            var username = httpContext.User.Identity?.Name ?? "Unknown";

            await service.SetEmailRecipientsAsync(
                groupKey,
                request.EmailAddresses,
                username,
                request.Reason);

            return Results.Ok(new { message = $"Email recipient group '{groupKey}' updated successfully" });
        })
        .WithName("UpdateEmailRecipientGroup")
        .Produces(200)
        .Produces(400)
        .WithDescription("Update email recipient group (with automatic rollback on errors)");

        // ===== Email Templates Endpoints =====

        group.MapGet("/email-templates", async (ISystemConfigurationManager service) =>
        {
            var templates = await service.GetAllEmailTemplatesAsync();
            return Results.Ok(templates);
        })
        .WithName("GetAllEmailTemplates")
        .Produces<List<EmailTemplateDto>>(200)
        .WithDescription("Get all email templates");

        group.MapGet("/email-templates/{key}", async (string key, ISystemConfigurationManager service) =>
        {
            var template = await service.GetEmailTemplateAsync(key);

            if (template == null)
                return Results.NotFound(new { error = $"Email template '{key}' not found" });

            return Results.Ok(template);
        })
        .WithName("GetEmailTemplateByKey")
        .Produces<EmailTemplateDto>(200)
        .Produces(404)
        .WithDescription("Get specific email template by key");

        group.MapPost("/email-templates", async (
            [FromBody] CreateEmailTemplateDto dto,
            ISystemConfigurationManager service,
            IEmailTemplateService templateService,
            HttpContext httpContext) =>
        {
            var username = httpContext.User.Identity?.Name ?? "Unknown";

            // Validate template before saving
            var sampleData = new Dictionary<string, object>
            {
                { "Username", "SampleUser" },
                { "Count", 1 },
                { "Date", DateTime.Now },
                { "BarCode", "12345" }
            };

            if (!templateService.ValidateTemplate(dto.HtmlBody, sampleData))
            {
                return Results.BadRequest(new { error = "Template validation failed. Check for balanced braces and loop tags." });
            }

            var template = new EmailTemplateDto
            {
                TemplateName = dto.TemplateName,
                TemplateKey = dto.TemplateKey,
                Subject = dto.Subject,
                HtmlBody = dto.HtmlBody,
                PlainTextBody = dto.PlainTextBody,
                PlaceholderDefinitions = dto.PlaceholderDefinitions,
                Category = dto.Category,
                IsActive = dto.IsActive,
                IsDefault = dto.IsDefault
            };

            var saved = await service.SaveEmailTemplateAsync(template, username);
            return Results.Created($"/api/configuration/email-templates/{saved.TemplateKey}", saved);
        })
        .WithName("CreateEmailTemplate")
        .Produces<EmailTemplateDto>(201)
        .Produces(400)
        .WithDescription("Create email template (with validation and rollback on errors)");

        group.MapPut("/email-templates/{id}", async (
            int id,
            [FromBody] UpdateEmailTemplateDto dto,
            ISystemConfigurationManager service,
            IEmailTemplateService templateService,
            HttpContext httpContext) =>
        {
            if (id != dto.TemplateId)
                return Results.BadRequest(new { error = "ID mismatch between URL and body" });

            var username = httpContext.User.Identity?.Name ?? "Unknown";

            // Validate template before saving
            var sampleData = new Dictionary<string, object>
            {
                { "Username", "SampleUser" },
                { "Count", 1 },
                { "Date", DateTime.Now },
                { "BarCode", "12345" }
            };

            if (!templateService.ValidateTemplate(dto.HtmlBody, sampleData))
            {
                return Results.BadRequest(new { error = "Template validation failed. Check for balanced braces and loop tags." });
            }

            var template = new EmailTemplateDto
            {
                TemplateId = dto.TemplateId,
                TemplateName = dto.TemplateName,
                TemplateKey = dto.TemplateKey,
                Subject = dto.Subject,
                HtmlBody = dto.HtmlBody,
                PlainTextBody = dto.PlainTextBody,
                PlaceholderDefinitions = dto.PlaceholderDefinitions,
                Category = dto.Category,
                IsActive = dto.IsActive,
                IsDefault = dto.IsDefault
            };

            var updated = await service.SaveEmailTemplateAsync(template, username);
            return Results.Ok(updated);
        })
        .WithName("UpdateEmailTemplate")
        .Produces<EmailTemplateDto>(200)
        .Produces(400)
        .Produces(404)
        .WithDescription("Update email template (with validation and rollback on errors)");

        group.MapDelete("/email-templates/{id}", async (
            int id,
            ISystemConfigurationManager service,
            HttpContext httpContext) =>
        {
            var username = httpContext.User.Identity?.Name ?? "Unknown";

            // Get the template first to deactivate it
            var templates = await service.GetAllEmailTemplatesAsync();
            var template = templates.FirstOrDefault(t => t.TemplateId == id);

            if (template == null)
                return Results.NotFound(new { error = $"Email template with ID {id} not found" });

            // Deactivate instead of delete
            template.IsActive = false;
            await service.SaveEmailTemplateAsync(template, username);

            return Results.NoContent();
        })
        .WithName("DeactivateEmailTemplate")
        .Produces(204)
        .Produces(404)
        .WithDescription("Deactivate email template (soft delete)");

        // ===== Configuration CRUD Endpoints =====

        group.MapGet("/sections", () =>
        {
            var sections = new[]
            {
                new { section = "Email", description = "Email server and notification settings" },
                new { section = "ActionReminder", description = "Action reminder service settings" },
                new { section = "Application", description = "General application settings" },
                new { section = "Security", description = "Security and authentication settings" }
            };
            return Results.Ok(sections);
        })
        .WithName("GetConfigurationSections")
        .Produces(200)
        .WithDescription("List all configuration sections");

        group.MapGet("/{section}/{key}", async (
            string section,
            string key,
            ISystemConfigurationManager service) =>
        {
            var value = await service.GetConfigurationAsync<string>(key, section, null);

            if (value == null)
                return Results.NotFound(new { error = $"Configuration '{section}:{key}' not found" });

            return Results.Ok(new { section, key, value });
        })
        .WithName("GetConfiguration")
        .Produces(200)
        .Produces(404)
        .WithDescription("Get specific configuration value");

        group.MapPost("/{section}/{key}", async (
            string section,
            string key,
            [FromBody] SetConfigurationRequest request,
            ISystemConfigurationManager service,
            HttpContext httpContext) =>
        {
            var username = httpContext.User.Identity?.Name ?? "Unknown";

            await service.SetConfigurationAsync(
                key,
                section,
                request.Value,
                username,
                request.Reason);

            return Results.Ok(new { message = $"Configuration '{section}:{key}' updated successfully" });
        })
        .WithName("SetConfiguration")
        .Produces(200)
        .Produces(400)
        .WithDescription("Set individual configuration value (with automatic rollback on errors). Use POST /smtp for bulk SMTP updates with testing.");

        // ===== Testing & Management Endpoints =====

        group.MapPost("/smtp", async (
            [FromBody] SmtpConfigurationDto config,
            [FromQuery] bool? skipTest,
            ISystemConfigurationManager service,
            HttpContext httpContext) =>
        {
            try
            {
                var username = httpContext.User.Identity?.Name ?? "Unknown";
                var skip = skipTest ?? false;

                await service.SetSmtpConfigurationAsync(config, username, "SMTP configuration update", skip);

                var message = skip
                    ? "SMTP configuration saved without testing (validation skipped)"
                    : "SMTP configuration updated and tested successfully";

                return Results.Ok(new
                {
                    success = true,
                    message,
                    tested = !skip
                });
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("SMTP configuration test failed"))
            {
                return Results.BadRequest(new
                {
                    success = false,
                    error = "SMTP test failed. Configuration not saved.",
                    details = ex.Message
                });
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new
                {
                    success = false,
                    error = ex.Message
                });
            }
        })
        .WithName("UpdateSmtpConfiguration")
        .Produces(200)
        .Produces(400)
        .WithDescription("Update all SMTP settings atomically. Add ?skipTest=true query parameter to save without testing (not recommended).");

        group.MapPost("/test-smtp", async (ISystemConfigurationManager service) =>
        {
            try
            {
                var result = await service.TestSmtpConnectionAsync();

                if (result)
                    return Results.Ok(new { success = true, message = "SMTP connection successful" });

                return Results.BadRequest(new { success = false, error = "SMTP connection failed" });
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { success = false, error = ex.Message });
            }
        })
        .WithName("TestSmtpConnection")
        .Produces(200)
        .Produces(400)
        .WithDescription("Test SMTP server connection with current configuration");

        group.MapPost("/reload", async (ISystemConfigurationManager service) =>
        {
            await service.ReloadAsync();
            return Results.Ok(new { message = "Configuration cache reloaded successfully" });
        })
        .WithName("ReloadConfigurationCache")
        .Produces(200)
        .WithDescription("Reload configuration cache (clears 5-minute TTL cache)");

        group.MapPost("/migrate", async (
            [FromBody] MigrateConfigurationRequest? request,
            ConfigurationMigrationService migrationService,
            HttpContext httpContext) =>
        {
            var username = httpContext.User.Identity?.Name ?? "Unknown";
            var overwriteExisting = request?.OverwriteExisting ?? false;

            try
            {
                var result = await migrationService.MigrateAllAsync(username, overwriteExisting);

                if (result.Success)
                {
                    return Results.Ok(new
                    {
                        success = true,
                        message = result.Message,
                        details = new
                        {
                            smtpSettingsMigrated = result.SmtpSettingsMigrated,
                            recipientGroupsMigrated = result.RecipientGroupsMigrated,
                            emailTemplatesCreated = result.EmailTemplatesCreated
                        }
                    });
                }

                return Results.BadRequest(new { success = false, error = result.Message });
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { success = false, error = ex.Message });
            }
        })
        .WithName("MigrateConfiguration")
        .Produces(200)
        .Produces(400)
        .WithDescription("Migrate configuration from appsettings.json to database (with optional overwrite)");

        // ===== Template Preview Endpoint =====

        group.MapPost("/email-templates/preview", async (
            [FromBody] PreviewTemplateRequest request,
            IEmailTemplateService templateService) =>
        {
            try
            {
                string rendered;

                if (request.Loops != null && request.Loops.Count > 0)
                {
                    rendered = templateService.RenderTemplateWithLoops(
                        request.Template,
                        request.Data,
                        request.Loops);
                }
                else
                {
                    rendered = templateService.RenderTemplate(request.Template, request.Data);
                }

                return Results.Ok(new { preview = rendered });
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = $"Template rendering failed: {ex.Message}" });
            }
        })
        .WithName("PreviewEmailTemplate")
        .Produces(200)
        .Produces(400)
        .WithDescription("Preview email template with sample data");

        // ===== Placeholder Documentation Endpoint =====

        group.MapGet("/email-templates/placeholders", () =>
        {
            var placeholders = new[]
            {
                new { name = "Username", description = "User's username", example = "john.doe", templates = new[] { "AccessRequestNotification", "AccessRequestConfirmation" } },
                new { name = "Reason", description = "Access request reason", example = "Need access for project X", templates = new[] { "AccessRequestNotification" } },
                new { name = "ApplicationUrl", description = "Base application URL", example = "https://docuscan.company.com", templates = new[] { "AccessRequestNotification", "AccessRequestConfirmation" } },
                new { name = "Date", description = "Current date/time", example = "04/11/2025 14:30", templates = new[] { "All" } },
                new { name = "Count", description = "Number of items", example = "5", templates = new[] { "ActionReminderDaily", "DocumentLinks", "DocumentAttachments" } },
                new { name = "BarCode", description = "Document barcode", example = "12345", templates = new[] { "DocumentLink", "DocumentAttachment" } },
                new { name = "DocumentLink", description = "Link to document", example = "https://...", templates = new[] { "DocumentLink" } },
                new { name = "FileName", description = "Attachment file name", example = "invoice.pdf", templates = new[] { "DocumentAttachment" } },
                new { name = "Message", description = "Optional custom message", example = "Please review", templates = new[] { "DocumentLink", "DocumentAttachment" } },
                new { name = "AdminEmail", description = "Administrator email", example = "admin@company.com", templates = new[] { "AccessRequestConfirmation" } }
            };

            var loops = new[]
            {
                new { name = "ActionRows", description = "Loop for action reminder rows", fields = new[] { "BarCode", "DocumentType", "DocumentName", "CounterParty", "ActionDate", "ReceivingDate", "ActionDescription", "IsOverdue" }, templates = new[] { "ActionReminderDaily" } },
                new { name = "DocumentRows", description = "Loop for document rows", fields = new[] { "BarCode", "Link", "FileName" }, templates = new[] { "DocumentLinks", "DocumentAttachments" } }
            };

            return Results.Ok(new { placeholders, loops });
        })
        .WithName("GetEmailTemplatePlaceholders")
        .Produces(200)
        .WithDescription("Get available placeholders and loops for email templates");

        // ===== Diagnostic Endpoint for DocumentAttachment Template =====

        group.MapGet("/email-templates/diagnostic/DocumentAttachment", async (
            ISystemConfigurationManager service,
            Microsoft.EntityFrameworkCore.IDbContextFactory<IkeaDocuScan.Infrastructure.Data.AppDbContext> contextFactory) =>
        {
            var diagnosticResult = new
            {
                timestamp = DateTime.UtcNow,
                checks = new List<object>()
            };

            try
            {
                // Check 1: Query database directly without cache
                await using var context = await contextFactory.CreateDbContextAsync();
                var allTemplatesRaw = await context.EmailTemplates
                    .AsNoTracking()
                    .Where(t => t.TemplateKey.Contains("Document"))
                    .ToListAsync();

                // Convert to output format with hex bytes (client-side evaluation)
                var allTemplates = allTemplatesRaw.Select(t => new
                {
                    t.TemplateId,
                    t.TemplateKey,
                    TemplateKeyLength = t.TemplateKey.Length,
                    TemplateKeyBytes = string.Join("-", System.Text.Encoding.UTF8.GetBytes(t.TemplateKey).Select(b => b.ToString("X2"))),
                    t.TemplateName,
                    t.IsActive,
                    t.IsDefault,
                    t.Category,
                    t.CreatedDate,
                    t.CreatedBy
                }).ToList();

                diagnosticResult.checks.Add(new
                {
                    checkName = "Database Query - Templates with 'Document' in key",
                    status = "success",
                    foundCount = allTemplates.Count,
                    templates = allTemplates
                });

                // Check 2: Query exact "DocumentAttachment" template
                var exactTemplate = await context.EmailTemplates
                    .AsNoTracking()
                    .Where(t => t.TemplateKey == "DocumentAttachment")
                    .Select(t => new
                    {
                        t.TemplateId,
                        t.TemplateKey,
                        t.TemplateName,
                        t.IsActive,
                        t.IsDefault,
                        t.Category,
                        SubjectPreview = t.Subject.Length > 100 ? t.Subject.Substring(0, 100) + "..." : t.Subject,
                        HtmlBodyPreview = t.HtmlBody.Length > 200 ? t.HtmlBody.Substring(0, 200) + "..." : t.HtmlBody,
                        HasPlainText = !string.IsNullOrEmpty(t.PlainTextBody),
                        t.CreatedDate,
                        t.CreatedBy,
                        t.ModifiedDate,
                        t.ModifiedBy
                    })
                    .FirstOrDefaultAsync();

                diagnosticResult.checks.Add(new
                {
                    checkName = "Exact Match Query - TemplateKey == 'DocumentAttachment'",
                    status = exactTemplate != null ? "success" : "not_found",
                    template = exactTemplate
                });

                // Check 3: Query with IsActive filter (mimics what GetEmailTemplateAsync does)
                var activeTemplate = await context.EmailTemplates
                    .AsNoTracking()
                    .Where(t => t.TemplateKey == "DocumentAttachment" && t.IsActive)
                    .FirstOrDefaultAsync();

                diagnosticResult.checks.Add(new
                {
                    checkName = "Active Template Query - TemplateKey == 'DocumentAttachment' && IsActive",
                    status = activeTemplate != null ? "success" : "not_found",
                    found = activeTemplate != null,
                    templateId = activeTemplate?.TemplateId,
                    isActive = activeTemplate?.IsActive
                });

                // Check 4: Test through service layer (uses cache)
                var serviceTemplate = await service.GetEmailTemplateAsync("DocumentAttachment");

                diagnosticResult.checks.Add(new
                {
                    checkName = "Service Layer Retrieval (with cache)",
                    status = serviceTemplate != null ? "success" : "not_found",
                    found = serviceTemplate != null,
                    templateId = serviceTemplate?.TemplateId,
                    templateName = serviceTemplate?.TemplateName
                });

                // Check 5: Character analysis of expected key
                var expectedKey = "DocumentAttachment";
                diagnosticResult.checks.Add(new
                {
                    checkName = "Expected TemplateKey Analysis",
                    status = "info",
                    expectedKey = expectedKey,
                    expectedLength = expectedKey.Length,
                    expectedBytes = System.Text.Encoding.UTF8.GetBytes(expectedKey).Select(b => b.ToString("X2")).ToArray(),
                    expectedBytesAsString = string.Join("-", System.Text.Encoding.UTF8.GetBytes(expectedKey).Select(b => b.ToString("X2")))
                });

                // Summary
                var summary = new
                {
                    templatesInDatabase = allTemplates.Count,
                    exactMatchFound = exactTemplate != null,
                    activeMatchFound = activeTemplate != null,
                    serviceRetrievalSuccessful = serviceTemplate != null,
                    recommendation = activeTemplate != null && serviceTemplate == null
                        ? "Template exists and is active in database, but service retrieval failed. Try clearing cache with POST /api/configuration/reload"
                        : activeTemplate == null && exactTemplate != null
                        ? "Template exists but is INACTIVE. Activate it to use it."
                        : exactTemplate == null
                        ? "Template does not exist in database. Run migration: POST /api/configuration/migrate"
                        : "Template is being retrieved successfully."
                };

                return Results.Ok(new { diagnostic = diagnosticResult, summary });
            }
            catch (Exception ex)
            {
                diagnosticResult.checks.Add(new
                {
                    checkName = "Exception",
                    status = "error",
                    message = ex.Message,
                    stackTrace = ex.StackTrace
                });

                return Results.Ok(diagnosticResult);
            }
        })
        .WithName("DiagnoseDocumentAttachmentTemplate")
        .Produces(200)
        .WithDescription("Comprehensive diagnostic for DocumentAttachment email template");
    }
}

// ===== Request DTOs =====

public record SetEmailRecipientsRequest(string[] EmailAddresses, string? Reason);

public record SetConfigurationRequest(string Value, string? Reason);

public record PreviewTemplateRequest(
    string Template,
    Dictionary<string, object> Data,
    Dictionary<string, List<Dictionary<string, object>>>? Loops = null);

public record MigrateConfigurationRequest(bool OverwriteExisting = false);
