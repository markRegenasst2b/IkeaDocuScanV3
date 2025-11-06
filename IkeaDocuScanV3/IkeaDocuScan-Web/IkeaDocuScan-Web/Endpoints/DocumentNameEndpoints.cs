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

        // POST /api/documentnames (SuperUser only)
        group.MapPost("/", async (CreateDocumentNameDto createDto, IDocumentNameService service) =>
        {
            try
            {
                var created = await service.CreateAsync(createDto);
                return Results.Created($"/api/documentnames/{created.Id}", created);
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        })
        .RequireAuthorization("SuperUser")
        .WithName("CreateDocumentName")
        .Produces<DocumentNameDto>(201)
        .Produces(400);

        // PUT /api/documentnames/{id} (SuperUser only)
        group.MapPut("/{id:int}", async (int id, UpdateDocumentNameDto updateDto, IDocumentNameService service) =>
        {
            if (id != updateDto.Id)
            {
                return Results.BadRequest(new { error = "ID mismatch between route and body" });
            }

            try
            {
                var updated = await service.UpdateAsync(updateDto);
                return Results.Ok(updated);
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        })
        .RequireAuthorization("SuperUser")
        .WithName("UpdateDocumentName")
        .Produces<DocumentNameDto>(200)
        .Produces(400);

        // DELETE /api/documentnames/{id} (SuperUser only)
        group.MapDelete("/{id:int}", async (int id, IDocumentNameService service) =>
        {
            try
            {
                await service.DeleteAsync(id);
                return Results.NoContent();
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        })
        .RequireAuthorization("SuperUser")
        .WithName("DeleteDocumentName")
        .Produces(204)
        .Produces(400);
    }
}
