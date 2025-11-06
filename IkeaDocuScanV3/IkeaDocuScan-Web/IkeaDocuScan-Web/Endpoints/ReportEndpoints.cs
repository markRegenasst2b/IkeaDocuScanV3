using IkeaDocuScan.Shared.DTOs.Reports;
using IkeaDocuScan.Shared.Interfaces;

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
    }
}
