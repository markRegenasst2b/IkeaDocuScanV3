using IkeaDocuScan.Shared.Exceptions;
using IkeaDocuScan.Shared.Interfaces;
using IkeaDocuScan.Shared.DTOs.DocumentTypes;

namespace IkeaDocuScan_Web.Endpoints;

public static class DocumentTypeEndpoints
{
    public static void MapDocumentTypeEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/documenttypes")
            .RequireAuthorization("HasAccess")
            .WithTags("DocumentTypes");

        // ========================================
        // READ OPERATIONS - All authenticated users with HasAccess
        // ========================================

        group.MapGet("/", async (IDocumentTypeService service) =>
        {
            var documentTypes = await service.GetAllAsync();
            return Results.Ok(documentTypes);
        })
        .WithName("GetAllDocumentTypes")
        .Produces<List<DocumentTypeDto>>(200);

        group.MapGet("/all", async (IDocumentTypeService service) =>
        {
            var documentTypes = await service.GetAllIncludingDisabledAsync();
            return Results.Ok(documentTypes);
        })
        .WithName("GetAllDocumentTypesIncludingDisabled")
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

        // ========================================
        // WRITE OPERATIONS - SuperUser only (system configuration)
        // ========================================

        group.MapPost("/", async (CreateDocumentTypeDto dto, IDocumentTypeService service) =>
        {
            try
            {
                var documentType = await service.CreateAsync(dto);
                return Results.Created($"/api/documenttypes/{documentType.DtId}", documentType);
            }
            catch (ValidationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        })
        .WithName("CreateDocumentType")
        .RequireAuthorization(policy => policy.RequireRole("SuperUser"))
        .Produces<DocumentTypeDto>(201)
        .Produces(400)
        .Produces(403);

        group.MapPut("/{id}", async (int id, UpdateDocumentTypeDto dto, IDocumentTypeService service) =>
        {
            try
            {
                var documentType = await service.UpdateAsync(id, dto);
                return Results.Ok(documentType);
            }
            catch (ValidationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        })
        .WithName("UpdateDocumentType")
        .RequireAuthorization(policy => policy.RequireRole("SuperUser"))
        .Produces<DocumentTypeDto>(200)
        .Produces(400)
        .Produces(403);

        group.MapDelete("/{id}", async (int id, IDocumentTypeService service) =>
        {
            try
            {
                await service.DeleteAsync(id);
                return Results.NoContent();
            }
            catch (ValidationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        })
        .WithName("DeleteDocumentType")
        .RequireAuthorization(policy => policy.RequireRole("SuperUser"))
        .Produces(204)
        .Produces(400)
        .Produces(403);

        group.MapGet("/{id}/usage", async (int id, IDocumentTypeService service) =>
        {
            var (documentCount, documentNameCount, userPermissionCount) = await service.GetUsageCountAsync(id);
            var isInUse = documentCount > 0 || documentNameCount > 0 || userPermissionCount > 0;

            return Results.Ok(new
            {
                dtId = id,
                isInUse,
                documentCount,
                documentNameCount,
                userPermissionCount,
                totalUsage = documentCount + documentNameCount + userPermissionCount
            });
        })
        .WithName("GetDocumentTypeUsage")
        .Produces(200);
    }
}
