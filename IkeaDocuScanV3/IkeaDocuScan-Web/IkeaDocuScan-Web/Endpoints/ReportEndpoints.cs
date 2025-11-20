using ExcelReporting.Models;
using ExcelReporting.Services;
using IkeaDocuScan.Shared.DTOs.Reports;
using IkeaDocuScan.Shared.DTOs.Documents;
using IkeaDocuScan.Shared.DTOs.Excel;
using IkeaDocuScan.Shared.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace IkeaDocuScan_Web.Endpoints;

/// <summary>
/// API endpoints for special reports
/// Uses dynamic database-driven authorization
/// </summary>
public static class ReportEndpoints
{
    public static void MapReportEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/reports")
            .RequireAuthorization()  // Base authentication required
            .WithTags("Reports");

        // GET /api/reports/barcode-gaps
        group.MapGet("/barcode-gaps", async (IReportService reportService) =>
        {
            var report = await reportService.GetBarcodeGapsReportAsync();
            return Results.Ok(report);
        })
        .WithName("GetBarcodeGapsReport")
        .RequireAuthorization("Endpoint:GET:/api/reports/barcode-gaps")
        .Produces<List<BarcodeGapReportDto>>(200)
        .WithDescription("Identifies missing barcodes in the sequence");

        // GET /api/reports/duplicate-documents
        group.MapGet("/duplicate-documents", async (IReportService reportService) =>
        {
            var report = await reportService.GetDuplicateDocumentsReportAsync();
            return Results.Ok(report);
        })
        .WithName("GetDuplicateDocumentsReport")
        .RequireAuthorization("Endpoint:GET:/api/reports/duplicate-documents")
        .Produces<List<DuplicateDocumentsReportDto>>(200)
        .WithDescription("Identifies potential duplicate documents");

        // GET /api/reports/unlinked-registrations
        group.MapGet("/unlinked-registrations", async (IReportService reportService) =>
        {
            var report = await reportService.GetUnlinkedRegistrationsReportAsync();
            return Results.Ok(report);
        })
        .WithName("GetUnlinkedRegistrationsReport")
        .RequireAuthorization("Endpoint:GET:/api/reports/unlinked-registrations")
        .Produces<List<UnlinkedRegistrationsReportDto>>(200)
        .WithDescription("Identifies documents registered but not linked to files");

        // GET /api/reports/scan-copies
        group.MapGet("/scan-copies", async (IReportService reportService) =>
        {
            var report = await reportService.GetScanCopiesReportAsync();
            return Results.Ok(report);
        })
        .WithName("GetScanCopiesReport")
        .RequireAuthorization("Endpoint:GET:/api/reports/scan-copies")
        .Produces<List<ScanCopiesReportDto>>(200)
        .WithDescription("Lists all scanned files and their status");

        // GET /api/reports/suppliers
        group.MapGet("/suppliers", async (IReportService reportService) =>
        {
            var report = await reportService.GetSuppliersReportAsync();
            return Results.Ok(report);
        })
        .WithName("GetSuppliersReport")
        .RequireAuthorization("Endpoint:GET:/api/reports/suppliers")
        .Produces<List<SuppliersReportDto>>(200)
        .WithDescription("Provides counterparty/supplier statistics");

        // GET /api/reports/all-documents
        group.MapGet("/all-documents", async (IReportService reportService) =>
        {
            var report = await reportService.GetAllDocumentsReportAsync();
            return Results.Ok(report);
        })
        .WithName("GetAllDocumentsReport")
        .RequireAuthorization("Endpoint:GET:/api/reports/all-documents")
        .Produces<List<AllDocumentsReportDto>>(200)
        .WithDescription("Exports all documents in the system");

        // Excel Export Endpoints

        // GET /api/reports/barcode-gaps/excel
        group.MapGet("/barcode-gaps/excel", async (
            IReportService reportService,
            IExcelExportService excelService) =>
        {
            var report = await reportService.GetBarcodeGapsReportAsync();
            var options = new ExcelExportOptions { SheetName = "Barcode Gaps" };
            var stream = await excelService.GenerateExcelAsync(report, options);
            var fileName = $"BarcodeGaps_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            return Results.File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        })
        .WithName("ExportBarcodeGapsToExcel")
        .RequireAuthorization("Endpoint:GET:/api/reports/barcode-gaps/excel")
        .Produces(200, contentType: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
        .WithDescription("Exports barcode gaps report to Excel");

        // GET /api/reports/duplicate-documents/excel
        group.MapGet("/duplicate-documents/excel", async (
            IReportService reportService,
            IExcelExportService excelService) =>
        {
            var report = await reportService.GetDuplicateDocumentsReportAsync();
            var options = new ExcelExportOptions { SheetName = "Duplicate Documents" };
            var stream = await excelService.GenerateExcelAsync(report, options);
            var fileName = $"DuplicateDocuments_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            return Results.File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        })
        .WithName("ExportDuplicateDocumentsToExcel")
        .RequireAuthorization("Endpoint:GET:/api/reports/duplicate-documents/excel")
        .Produces(200, contentType: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
        .WithDescription("Exports duplicate documents report to Excel");

        // GET /api/reports/unlinked-registrations/excel
        group.MapGet("/unlinked-registrations/excel", async (
            IReportService reportService,
            IExcelExportService excelService) =>
        {
            var report = await reportService.GetUnlinkedRegistrationsReportAsync();
            var options = new ExcelExportOptions { SheetName = "Unlinked Registrations" };
            var stream = await excelService.GenerateExcelAsync(report, options);
            var fileName = $"UnlinkedRegistrations_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            return Results.File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        })
        .WithName("ExportUnlinkedRegistrationsToExcel")
        .RequireAuthorization("Endpoint:GET:/api/reports/unlinked-registrations/excel")
        .Produces(200, contentType: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
        .WithDescription("Exports unlinked registrations report to Excel");

        // GET /api/reports/scan-copies/excel
        group.MapGet("/scan-copies/excel", async (
            IReportService reportService,
            IExcelExportService excelService) =>
        {
            var report = await reportService.GetScanCopiesReportAsync();
            var options = new ExcelExportOptions { SheetName = "Scan Copies" };
            var stream = await excelService.GenerateExcelAsync(report, options);
            var fileName = $"ScanCopies_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            return Results.File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        })
        .WithName("ExportScanCopiesToExcel")
        .RequireAuthorization("Endpoint:GET:/api/reports/scan-copies/excel")
        .Produces(200, contentType: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
        .WithDescription("Exports scan copies report to Excel");

        // GET /api/reports/suppliers/excel
        group.MapGet("/suppliers/excel", async (
            IReportService reportService,
            IExcelExportService excelService) =>
        {
            var report = await reportService.GetSuppliersReportAsync();
            var options = new ExcelExportOptions { SheetName = "Suppliers" };
            var stream = await excelService.GenerateExcelAsync(report, options);
            var fileName = $"Suppliers_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            return Results.File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        })
        .WithName("ExportSuppliersToExcel")
        .RequireAuthorization("Endpoint:GET:/api/reports/suppliers/excel")
        .Produces(200, contentType: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
        .WithDescription("Exports suppliers report to Excel");

        // GET /api/reports/all-documents/excel
        group.MapGet("/all-documents/excel", async (
            IReportService reportService,
            IExcelExportService excelService) =>
        {
            var report = await reportService.GetAllDocumentsReportAsync();
            var options = new ExcelExportOptions { SheetName = "All Documents" };
            var stream = await excelService.GenerateExcelAsync(report, options);
            var fileName = $"AllDocuments_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            return Results.File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        })
        .WithName("ExportAllDocumentsToExcel")
        .RequireAuthorization("Endpoint:GET:/api/reports/all-documents/excel")
        .Produces(200, contentType: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
        .WithDescription("Exports all documents report to Excel");

        // POST /api/reports/documents/search/excel
        group.MapPost("/documents/search/excel", async (
            [FromBody] DocumentSearchRequestDto searchRequest,
            IDocumentService documentService,
            IExcelExportService excelService) =>
        {
            var searchResults = await documentService.SearchAsync(searchRequest);
            var exportData = searchResults.Items
                .Select(item => DocumentExportDto.FromSearchItem(item))
                .ToList();

            var options = new ExcelExportOptions { SheetName = "Search Results" };
            var stream = await excelService.GenerateExcelAsync(exportData, options);
            var fileName = $"SearchResults_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            return Results.File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        })
        .WithName("ExportSearchResultsToExcel")
        .RequireAuthorization("Endpoint:POST:/api/reports/documents/search/excel")
        .Produces(200, contentType: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
        .WithDescription("Exports document search results to Excel");

        // POST /api/reports/documents/selected/excel
        group.MapPost("/documents/selected/excel", async (
            [FromBody] int[] documentIds,
            IDocumentService documentService,
            IExcelExportService excelService) =>
        {
            // Load all documents in a single efficient query (fixes N+1 problem)
            var documents = await documentService.GetByIdsAsync(documentIds);

            var exportData = documents
                .Select(DocumentExportDto.FromDocumentDto)
                .ToList();

            var options = new ExcelExportOptions { SheetName = "Selected Documents" };
            var stream = await excelService.GenerateExcelAsync(exportData, options);
            var fileName = $"SelectedDocuments_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            return Results.File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        })
        .WithName("ExportSelectedDocumentsToExcel")
        .RequireAuthorization("Endpoint:POST:/api/reports/documents/selected/excel")
        .Produces(200, contentType: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
        .WithDescription("Exports selected documents to Excel");
    }
}
