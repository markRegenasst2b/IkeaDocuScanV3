using IkeaDocuScan.Shared.Interfaces;
using IkeaDocuScan.Shared.DTOs.Documents;

namespace IkeaDocuScan_Web.Endpoints;

/// <summary>
/// API endpoints for document management
/// Uses dynamic database-driven authorization
/// </summary>
public static class DocumentEndpoints
{
    public static void MapDocumentEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/documents")
            .RequireAuthorization()  // Base authentication required
            .WithTags("Documents");

        // ========================================
        // READ OPERATIONS
        // ========================================

        group.MapGet("/", async (IDocumentService service) =>
        {
            var documents = await service.GetAllAsync();
            return Results.Ok(documents);
        })
        .WithName("GetAllDocuments")
        .RequireAuthorization("Endpoint:GET:/api/documents/")
        .Produces<List<DocumentDto>>(200);

        group.MapGet("/{id}", async (int id, IDocumentService service) =>
        {
            var document = await service.GetByIdAsync(id);
            return Results.Ok(document);
        })
        .WithName("GetDocumentById")
        .RequireAuthorization("Endpoint:GET:/api/documents/{id}")
        .Produces<DocumentDto>(200)
        .Produces(404);

        group.MapGet("/barcode/{barCode}", async (string barCode, IDocumentService service) =>
        {
            var document = await service.GetByBarCodeAsync(barCode);
            if (document == null)
                return Results.NotFound(new { error = $"Document with barcode '{barCode}' not found" });

            return Results.Ok(document);
        })
        .WithName("GetDocumentByBarCode")
        .RequireAuthorization("Endpoint:GET:/api/documents/barcode/{barCode}")
        .Produces<DocumentDto>(200)
        .Produces(404);

        group.MapPost("/by-ids", async (List<int> ids, IDocumentService service) =>
        {
            if (ids == null || !ids.Any())
                return Results.BadRequest(new { error = "No document IDs provided" });

            var documents = await service.GetByIdsAsync(ids);
            return Results.Ok(documents);
        })
        .WithName("GetDocumentsByIds")
        .RequireAuthorization("Endpoint:POST:/api/documents/by-ids")
        .Produces<List<DocumentDto>>(200)
        .Produces(400);

        // ========================================
        // WRITE OPERATIONS
        // ========================================

        group.MapPost("/", async (CreateDocumentDto dto, IDocumentService service) =>
        {
            var created = await service.CreateAsync(dto);
            return Results.Created($"/api/documents/{created.Id}", created);
        })
        .WithName("CreateDocument")
        .RequireAuthorization("Endpoint:POST:/api/documents/")
        .Produces<DocumentDto>(201)
        .Produces(400)
        .Produces(403);

        group.MapPut("/{id}", async (int id, UpdateDocumentDto dto, IDocumentService service) =>
        {
            if (id != dto.Id)
                return Results.BadRequest(new { error = "ID mismatch between URL and body" });

            var updated = await service.UpdateAsync(dto);
            return Results.Ok(updated);
        })
        .WithName("UpdateDocument")
        .RequireAuthorization("Endpoint:PUT:/api/documents/{id}")
        .Produces<DocumentDto>(200)
        .Produces(400)
        .Produces(403)
        .Produces(404);

        // ========================================
        // DELETE OPERATIONS
        // ========================================

        group.MapDelete("/{id}", async (int id, IDocumentService service) =>
        {
            await service.DeleteAsync(id);
            return Results.NoContent();
        })
        .WithName("DeleteDocument")
        .RequireAuthorization("Endpoint:DELETE:/api/documents/{id}")
        .Produces(204)
        .Produces(403)
        .Produces(404);

        group.MapPost("/search", async (DocumentSearchRequestDto request, IDocumentService service) =>
        {
            var results = await service.SearchAsync(request);
            return Results.Ok(results);
        })
        .WithName("SearchDocuments")
        .RequireAuthorization("Endpoint:POST:/api/documents/search")
        .Produces<DocumentSearchResultDto>(200)
        .Produces(400);

        group.MapGet("/{id}/stream", async (int id, IDocumentService service) =>
        {
            var fileData = await service.GetDocumentFileAsync(id);
            if (fileData == null)
                return Results.NotFound(new { error = $"Document file not found for document ID {id}" });

            var contentType = GetContentType(fileData.FileName);

            // Return file without filename parameter to enable inline display
            return Results.File(fileData.FileBytes, contentType);
        })
        .WithName("StreamDocumentFile")
        .RequireAuthorization("Endpoint:GET:/api/documents/{id}/stream")
        .Produces(200)
        .Produces(404);

        group.MapGet("/{id}/download", async (int id, IDocumentService service) =>
        {
            var fileData = await service.GetDocumentFileAsync(id);
            if (fileData == null)
                return Results.NotFound(new { error = $"Document file not found for document ID {id}" });

            var contentType = GetContentType(fileData.FileName);

            // Return file with filename parameter to force download
            return Results.File(fileData.FileBytes, contentType, fileData.FileName);
        })
        .WithName("DownloadDocumentFile")
        .RequireAuthorization("Endpoint:GET:/api/documents/{id}/download")
        .Produces(200)
        .Produces(404);
    }

    private static string GetContentType(string? fileName)
    {
        if (string.IsNullOrEmpty(fileName))
            return "application/octet-stream";

        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".xls" => "application/vnd.ms-excel",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".tif" or ".tiff" => "image/tiff",
            _ => "application/octet-stream"
        };
    }
}
