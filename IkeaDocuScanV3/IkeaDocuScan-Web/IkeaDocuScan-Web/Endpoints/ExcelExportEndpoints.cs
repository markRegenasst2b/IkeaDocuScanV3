using ExcelReporting.Models;
using ExcelReporting.Services;
using IkeaDocuScan.Shared.DTOs.Excel;
using IkeaDocuScan.Shared.DTOs.Documents;
using IkeaDocuScan.Shared.Enums;
using IkeaDocuScan.Shared.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace IkeaDocuScan_Web.Endpoints;

/// <summary>
/// API endpoints for Excel export functionality
/// </summary>
public static class ExcelExportEndpoints
{
    public static void MapExcelExportEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/excel")
            .RequireAuthorization("HasAccess")
            .WithTags("Excel Export");

        // Export documents to Excel
        group.MapPost("/export/documents", async (
            [FromBody] ExcelExportRequestDto request,
            [FromServices] IDocumentService documentService,
            [FromServices] IExcelExportService excelService,
            [FromServices] IAuditTrailService auditService,
            [FromServices] ICurrentUserService currentUserService,
            [FromServices] IOptions<ExcelExportOptions> options) =>
        {
            try
            {
                // Get documents based on filter criteria
                var documents = await documentService.SearchAsync(request.SearchCriteria);

                // Convert to export DTOs
                var exportData = documents.Items
                    .Select(DocumentExportDto.FromSearchItem)
                    .ToList();

                // Validate data size
                var exportOptions = options.Value;
                var validation = excelService.ValidateDataSize(exportData, exportOptions);

                if (!validation.IsValid)
                {
                    return Results.BadRequest(new { error = validation.Message, rowCount = validation.RowCount });
                }

                // Generate Excel file
                var stream = await excelService.GenerateExcelAsync(exportData, exportOptions);

                // Log export to audit trail
                await auditService.LogAsync(
                    AuditAction.ExportExcel,
                    "BULK_EXPORT",
                    $"Exported {exportData.Count} documents to Excel");

                // Return file
                var fileName = $"Documents_Export_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                return Results.File(
                    stream,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    fileName);
            }
            catch (Exception ex)
            {
                return Results.Problem(
                    detail: ex.Message,
                    statusCode: 500,
                    title: "Excel Export Failed");
            }
        })
        .WithName("ExportDocumentsToExcel")
        .Produces(200, contentType: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
        .Produces(400)
        .Produces(500);

        // Validate export size before generating
        group.MapPost("/validate/documents", async (
            [FromBody] ExcelExportRequestDto request,
            [FromServices] IDocumentService documentService,
            [FromServices] IExcelExportService excelService,
            [FromServices] IOptions<ExcelExportOptions> options) =>
        {
            try
            {
                var documents = await documentService.SearchAsync(request.SearchCriteria);
                var exportData = documents.Items
                    .Select(DocumentExportDto.FromSearchItem)
                    .ToList();

                var exportOptions = options.Value;
                var validation = excelService.ValidateDataSize(exportData, exportOptions);

                return Results.Ok(validation);
            }
            catch (Exception ex)
            {
                return Results.Problem(
                    detail: ex.Message,
                    statusCode: 500,
                    title: "Validation Failed");
            }
        })
        .WithName("ValidateExportSize")
        .Produces<ExcelExportValidationResult>(200)
        .Produces(500);

        // Get metadata for document export
        group.MapGet("/metadata/documents", (
            [FromServices] IExcelExportService excelService) =>
        {
            var metadata = excelService.GetMetadata<DocumentExportDto>();
            return Results.Ok(metadata);
        })
        .WithName("GetDocumentExportMetadata")
        .Produces<List<ExcelExportMetadata>>(200);
    }
}

/// <summary>
/// Request DTO for Excel export
/// </summary>
public class ExcelExportRequestDto
{
    /// <summary>
    /// Search criteria for filtering documents
    /// </summary>
    public DocumentSearchRequestDto SearchCriteria { get; set; } = new();

    /// <summary>
    /// Optional filter context for display purposes
    /// </summary>
    public Dictionary<string, string>? FilterContext { get; set; }
}
