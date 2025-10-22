using IkeaDocuScan.Shared.Interfaces;
using IkeaDocuScan.Shared.DTOs.DocumentTypes;

namespace IkeaDocuScan_Web.Endpoints;

public static class DocumentTypeEndpoints
{
    public static void MapDocumentTypeEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/documenttypes")
            .RequireAuthorization()
            .WithTags("DocumentTypes");

        group.MapGet("/", async (IDocumentTypeService service) =>
        {
            var documentTypes = await service.GetAllAsync();
            return Results.Ok(documentTypes);
        })
        .WithName("GetAllDocumentTypes")
        .Produces<List<DocumentTypeDto>>(200);

        group.MapGet("/{id}", async (int id, IDocumentTypeService service) =>
        {
            var documentType = await service.GetByIdAsync(id);
            if (documentType == null)
                return Results.NotFound(new { error = $"DocumentType with ID {id} not found" });

            return Results.Ok(documentType);
        })
        .WithName("GetDocumentTypeById")
        .Produces<DocumentTypeDto>(200)
        .Produces(404);
    }
}
