using IkeaDocuScan.Shared.Interfaces;
using IkeaDocuScan.Shared.DTOs.DocumentNames;

namespace IkeaDocuScan_Web.Endpoints;

/// <summary>
/// API endpoints for document name management
/// Uses dynamic database-driven authorization
/// </summary>
public static class DocumentNameEndpoints
{
    public static void MapDocumentNameEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/documentnames")
            .RequireAuthorization()  // Base authentication required
            .WithTags("DocumentNames");

        // GET /api/documentnames
        group.MapGet("/", async (IDocumentNameService service) =>
        {
            var documentNames = await service.GetAllAsync();
            return Results.Ok(documentNames);
        })
        .WithName("GetAllDocumentNames")
        .RequireAuthorization("Endpoint:GET:/api/documentnames/")
        .Produces<List<DocumentNameDto>>(200);

        // GET /api/documentnames/bytype/{documentTypeId}
        group.MapGet("/bytype/{documentTypeId:int}", async (int documentTypeId, IDocumentNameService service) =>
        {
            var documentNames = await service.GetByDocumentTypeIdAsync(documentTypeId);
            return Results.Ok(documentNames);
        })
        .WithName("GetDocumentNamesByType")
        .RequireAuthorization("Endpoint:GET:/api/documentnames/bytype/{documentTypeId}")
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
        .RequireAuthorization("Endpoint:GET:/api/documentnames/{id}")
        .Produces<DocumentNameDto>(200)
        .Produces(404);

        // POST /api/documentnames
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
        .WithName("CreateDocumentName")
        .RequireAuthorization("Endpoint:POST:/api/documentnames/")
        .Produces<DocumentNameDto>(201)
        .Produces(400);

        // PUT /api/documentnames/{id}
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
        .WithName("UpdateDocumentName")
        .RequireAuthorization("Endpoint:PUT:/api/documentnames/{id}")
        .Produces<DocumentNameDto>(200)
        .Produces(400);

        // DELETE /api/documentnames/{id}
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
        .WithName("DeleteDocumentName")
        .RequireAuthorization("Endpoint:DELETE:/api/documentnames/{id}")
        .Produces(204)
        .Produces(400);
    }
}
