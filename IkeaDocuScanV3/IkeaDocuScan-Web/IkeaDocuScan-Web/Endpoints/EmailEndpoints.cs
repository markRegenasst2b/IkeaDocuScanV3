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

        // POST /api/email/send-with-links - Send email with document links
        group.MapPost("/send-with-links", SendEmailWithLinksAsync)
            .WithName("SendEmailWithLinks")
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
    /// Send email with document attachments using database template
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

            if (request.DocumentIds == null || !request.DocumentIds.Any())
            {
                return Results.BadRequest(new { error = "At least one document is required" });
            }

            logger.LogInformation("Sending email with {Count} attachments to {ToEmail}",
                request.DocumentIds.Count, request.ToEmail);

            // Load all documents and their files
            var documents = new List<(string BarCode, byte[] Data, string FileName)>();

            foreach (var documentId in request.DocumentIds)
            {
                var document = await documentService.GetByIdAsync(documentId);
                if (document == null)
                {
                    logger.LogWarning("Document not found for ID {DocumentId}", documentId);
                    continue;
                }

                var fileData = await documentService.GetDocumentFileAsync(documentId);
                if (fileData == null)
                {
                    logger.LogWarning("Document file not found for document ID {DocumentId}", documentId);
                    continue;
                }

                documents.Add((document.BarCode.ToString(), fileData.FileBytes, fileData.FileName));
            }

            if (!documents.Any())
            {
                return Results.BadRequest(new { error = "No document files found for the specified document IDs" });
            }

            // Send ONE email with all attachments using the DocumentAttachments template
            await emailService.SendDocumentAttachmentsAsync(
                request.ToEmail,
                documents,
                request.AdditionalMessage);

            logger.LogInformation("Email with {Count} attachments sent successfully to {ToEmail}",
                documents.Count, request.ToEmail);

            return Results.Ok(new
            {
                success = true,
                message = $"Email with {request.DocumentIds.Count} attachment(s) sent successfully to {request.ToEmail}"
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
    /// Send email with document links using database template
    /// </summary>
    private static async Task<IResult> SendEmailWithLinksAsync(
        SendEmailWithLinksRequest request,
        IEmailService emailService,
        IDocumentService documentService,
        IConfiguration configuration,
        ILogger<IEmailService> logger)
    {
        try
        {
            // Validate request
            if (string.IsNullOrWhiteSpace(request.ToEmail))
            {
                return Results.BadRequest(new { error = "Recipient email is required" });
            }

            if (request.DocumentIds == null || !request.DocumentIds.Any())
            {
                return Results.BadRequest(new { error = "At least one document is required" });
            }

            logger.LogInformation("Sending email with {Count} document links to {ToEmail}",
                request.DocumentIds.Count, request.ToEmail);

            // Load all documents and generate links
            var documents = new List<(string BarCode, string Link)>();
            var baseUrl = configuration.GetValue<string>("ApplicationUrl") ?? "https://localhost:44101";

            foreach (var documentId in request.DocumentIds)
            {
                var document = await documentService.GetByIdAsync(documentId);
                if (document == null)
                {
                    logger.LogWarning("Document not found for ID {DocumentId}", documentId);
                    continue;
                }

                var link = $"{baseUrl}/documents/preview/{documentId}";
                documents.Add((document.BarCode.ToString(), link));
            }

            if (!documents.Any())
            {
                return Results.BadRequest(new { error = "No documents found for the specified document IDs" });
            }

            // Send ONE email with all document links using the DocumentLinks template
            await emailService.SendDocumentLinksAsync(
                request.ToEmail,
                documents,
                request.AdditionalMessage);

            logger.LogInformation("Email with {Count} document links sent successfully to {ToEmail}",
                documents.Count, request.ToEmail);

            return Results.Ok(new
            {
                success = true,
                message = $"Email with {documents.Count} document link(s) sent successfully to {request.ToEmail}"
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error sending email with document links to {ToEmail}", request.ToEmail);
            return Results.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError,
                title: "Failed to send email with document links");
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
