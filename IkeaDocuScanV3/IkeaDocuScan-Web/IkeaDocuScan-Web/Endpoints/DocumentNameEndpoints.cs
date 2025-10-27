using IkeaDocuScan.Shared.Interfaces;
using IkeaDocuScan.Shared.DTOs.DocumentNames;

namespace IkeaDocuScan_Web.Endpoints;

/// <summary>
/// API endpoints for document names
/// </summary>
public static class DocumentNameEndpoints
{
    /// <summary>
    /// Map document name endpoints to the route builder
    /// </summary>
    public static void MapDocumentNameEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/documentnames")
            .RequireAuthorization("HasAccess")
            .WithTags("DocumentNames");

        // GET /api/documentnames
        group.MapGet("/", async (IDocumentNameService service) =>
        {
            var documentNames = await service.GetAllAsync();
            return Results.Ok(documentNames);
        })
        .WithName("GetAllDocumentNames")
        .Produces<List<DocumentNameDto>>(200);

        // GET /api/documentnames/bytype/{documentTypeId}
        group.MapGet("/bytype/{documentTypeId:int}", async (int documentTypeId, IDocumentNameService service) =>
        {
            var documentNames = await service.GetByDocumentTypeIdAsync(documentTypeId);
            return Results.Ok(documentNames);
        })
        .WithName("GetDocumentNamesByType")
        .Produces<List<DocumentNameDto>>(200);

        // GET /api/documentnames/{id}
        group.MapGet("/{id:int}", async (int id, IDocumentNameService service) =>
        {
            var documentName = await service.GetByIdAsync(id);
            if (documentName == null)
            {
                return Results.NotFound(new { error = $"Document name with ID {id} not found" });
            }
            return Results.Ok(documentName);
        })
        .WithName("GetDocumentNameById")
        .Produces<DocumentNameDto>(200)
        .Produces(404);
    }
}
