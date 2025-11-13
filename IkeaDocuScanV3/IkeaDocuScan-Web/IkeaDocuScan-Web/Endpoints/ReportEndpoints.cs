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
/// </summary>
public static class ReportEndpoints
{
    /// <summary>
    /// Map report endpoints to the route builder
    /// </summary>
    public static void MapReportEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/reports")
            .RequireAuthorization("HasAccess")
            .WithTags("Reports");

        // GET /api/reports/barcode-gaps
        group.MapGet("/barcode-gaps", async (IReportService reportService) =>
        {
            var report = await reportService.GetBarcodeGapsReportAsync();
            return Results.Ok(report);
        })
        .WithName("GetBarcodeGapsReport")
        .Produces<List<BarcodeGapReportDto>>(200)
        .WithDescription("Identifies missing barcodes in the sequence");

        // GET /api/reports/duplicate-documents
        group.MapGet("/duplicate-documents", async (IReportService reportService) =>
        {
            var report = await reportService.GetDuplicateDocumentsReportAsync();
            return Results.Ok(report);
        })
        .WithName("GetDuplicateDocumentsReport")
        .Produces<List<DuplicateDocumentsReportDto>>(200)
        .WithDescription("Identifies potential duplicate documents");

        // GET /api/reports/unlinked-registrations
        group.MapGet("/unlinked-registrations", async (IReportService reportService) =>
        {
            var report = await reportService.GetUnlinkedRegistrationsReportAsync();
            return Results.Ok(report);
        })
        .WithName("GetUnlinkedRegistrationsReport")
        .Produces<List<UnlinkedRegistrationsReportDto>>(200)
        .WithDescription("Identifies documents registered but not linked to files");

        // GET /api/reports/scan-copies
        group.MapGet("/scan-copies", async (IReportService reportService) =>
        {
            var report = await reportService.GetScanCopiesReportAsync();
            return Results.Ok(report);
        })
        .WithName("GetScanCopiesReport")
        .Produces<List<ScanCopiesReportDto>>(200)
        .WithDescription("Lists all scanned files and their status");

        // GET /api/reports/suppliers
        group.MapGet("/suppliers", async (IReportService reportService) =>
        {
            var report = await reportService.GetSuppliersReportAsync();
            return Results.Ok(report);
        })
        .WithName("GetSuppliersReport")
        .Produces<List<SuppliersReportDto>>(200)
        .WithDescription("Provides counterparty/supplier statistics");

        // GET /api/reports/all-documents
        group.MapGet("/all-documents", async (IReportService reportService) =>
        {
            var report = await reportService.GetAllDocumentsReportAsync();
            return Results.Ok(report);
        })
        .WithName("GetAllDocumentsReport")
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
        .Produces(200, contentType: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
        .WithDescription("Exports selected documents to Excel");
    }
}
