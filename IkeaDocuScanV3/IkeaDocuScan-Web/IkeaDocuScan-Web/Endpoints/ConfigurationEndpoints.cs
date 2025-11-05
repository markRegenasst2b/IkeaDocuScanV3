using IkeaDocuScan.Shared.DTOs.Configuration;
using IkeaDocuScan.Shared.Interfaces;
using IkeaDocuScan_Web.Services;
using Microsoft.AspNetCore.Mvc;

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
        .WithDescription("Set configuration value (with automatic rollback on errors, SMTP testing for email configs)");

        // ===== Testing & Management Endpoints =====

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
