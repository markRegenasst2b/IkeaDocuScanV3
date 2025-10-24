using IkeaDocuScan.Shared.Interfaces;
using IkeaDocuScan.Shared.DTOs.Documents;

namespace IkeaDocuScan_Web.Endpoints;

public static class DocumentEndpoints
{
    public static void MapDocumentEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/documents")
            .RequireAuthorization()
            .WithTags("Documents");

        group.MapGet("/", async (IDocumentService service) =>
        {
            var documents = await service.GetAllAsync();
            return Results.Ok(documents);
        })
        .WithName("GetAllDocuments")
        .Produces<List<DocumentDto>>(200);

        group.MapGet("/{id}", async (int id, IDocumentService service) =>
        {
            var document = await service.GetByIdAsync(id);
            return Results.Ok(document);
        })
        .WithName("GetDocumentById")
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
        .Produces<DocumentDto>(200)
        .Produces(404);

        group.MapPost("/", async (CreateDocumentDto dto, IDocumentService service) =>
        {
            var created = await service.CreateAsync(dto);
            return Results.Created($"/api/documents/{created.Id}", created);
        })
        .WithName("CreateDocument")
        .Produces<DocumentDto>(201)
        .Produces(400);

        group.MapPut("/{id}", async (int id, UpdateDocumentDto dto, IDocumentService service) =>
        {
            if (id != dto.Id)
                return Results.BadRequest(new { error = "ID mismatch between URL and body" });

            var updated = await service.UpdateAsync(dto);
            return Results.Ok(updated);
        })
        .WithName("UpdateDocument")
        .Produces<DocumentDto>(200)
        .Produces(400)
        .Produces(404);

        group.MapDelete("/{id}", async (int id, IDocumentService service) =>
        {
            await service.DeleteAsync(id);
            return Results.NoContent();
        })
        .WithName("DeleteDocument")
        .Produces(204)
        .Produces(404);
    }
}
