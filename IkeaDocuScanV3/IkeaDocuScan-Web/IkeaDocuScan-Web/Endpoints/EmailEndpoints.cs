using IkeaDocuScan.Shared.DTOs.Email;
using IkeaDocuScan.Shared.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace IkeaDocuScan_Web.Endpoints;

/// <summary>
/// Email-related API endpoints
/// </summary>
public static class EmailEndpoints
{
    /// <summary>
    /// Maps all email-related endpoints
    /// </summary>
    public static IEndpointRouteBuilder MapEmailEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/email")
            .RequireAuthorization("HasAccess")
            .WithTags("Email");

        // POST /api/email/send - Send custom email with HTML body
        group.MapPost("/send", SendEmailAsync)
            .WithName("SendEmail")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);

        // POST /api/email/send-with-attachments - Send email with document attachments
        group.MapPost("/send-with-attachments", SendEmailWithAttachmentsAsync)
            .WithName("SendEmailWithAttachments")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);

        return app;
    }

    /// <summary>
    /// Send a custom email with HTML body
    /// </summary>
    private static async Task<IResult> SendEmailAsync(
        SendEmailRequest request,
        IEmailService emailService,
        ILogger<IEmailService> logger)
    {
        try
        {
            // Validate request
            if (string.IsNullOrWhiteSpace(request.ToEmail))
            {
                return Results.BadRequest(new { error = "Recipient email is required" });
            }

            if (string.IsNullOrWhiteSpace(request.Subject))
            {
                return Results.BadRequest(new { error = "Subject is required" });
            }

            if (string.IsNullOrWhiteSpace(request.HtmlBody))
            {
                return Results.BadRequest(new { error = "Email body is required" });
            }

            logger.LogInformation("Sending email to {ToEmail} with subject: {Subject}",
                request.ToEmail, request.Subject);

            await emailService.SendEmailAsync(
                request.ToEmail,
                request.Subject,
                request.HtmlBody,
                request.PlainTextBody);

            logger.LogInformation("Email sent successfully to {ToEmail}", request.ToEmail);

            return Results.Ok(new { success = true, message = $"Email sent successfully to {request.ToEmail}" });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error sending email to {ToEmail}", request.ToEmail);
            return Results.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError,
                title: "Failed to send email");
        }
    }

    /// <summary>
    /// Send email with document attachments
    /// </summary>
    private static async Task<IResult> SendEmailWithAttachmentsAsync(
        SendEmailWithAttachmentsRequest request,
        IEmailService emailService,
        IDocumentService documentService,
        ILogger<IEmailService> logger)
    {
        try
        {
            // Validate request
            if (string.IsNullOrWhiteSpace(request.ToEmail))
            {
                return Results.BadRequest(new { error = "Recipient email is required" });
            }

            if (string.IsNullOrWhiteSpace(request.Subject))
            {
                return Results.BadRequest(new { error = "Subject is required" });
            }

            if (string.IsNullOrWhiteSpace(request.HtmlBody))
            {
                return Results.BadRequest(new { error = "Email body is required" });
            }

            if (request.DocumentIds == null || !request.DocumentIds.Any())
            {
                return Results.BadRequest(new { error = "At least one document is required" });
            }

            logger.LogInformation("Sending email with {Count} attachments to {ToEmail}",
                request.DocumentIds.Count, request.ToEmail);

            // Load document files
            var attachments = new List<IkeaDocuScan.Shared.Models.Email.EmailAttachment>();
            foreach (var documentId in request.DocumentIds)
            {
                var fileData = await documentService.GetDocumentFileAsync(documentId);
                if (fileData != null)
                {
                    attachments.Add(new IkeaDocuScan.Shared.Models.Email.EmailAttachment
                    {
                        FileName = fileData.FileName,
                        Content = fileData.FileBytes,
                        ContentType = GetContentType(fileData.FileName)
                    });
                }
                else
                {
                    logger.LogWarning("Document file not found for document ID {DocumentId}", documentId);
                }
            }

            if (!attachments.Any())
            {
                return Results.BadRequest(new { error = "No document files found for the specified document IDs" });
            }

            await emailService.SendEmailAsync(
                request.ToEmail,
                request.Subject,
                request.HtmlBody,
                request.PlainTextBody,
                attachments);

            logger.LogInformation("Email with {Count} attachments sent successfully to {ToEmail}",
                attachments.Count, request.ToEmail);

            return Results.Ok(new
            {
                success = true,
                message = $"Email with {attachments.Count} attachment(s) sent successfully to {request.ToEmail}"
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error sending email with attachments to {ToEmail}", request.ToEmail);
            return Results.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError,
                title: "Failed to send email with attachments");
        }
    }

    /// <summary>
    /// Get MIME content type based on file extension
    /// </summary>
    private static string GetContentType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".pdf" => "application/pdf",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".tif" or ".tiff" => "image/tiff",
            ".bmp" => "image/bmp",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".xls" => "application/vnd.ms-excel",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            _ => "application/octet-stream"
        };
    }
}
