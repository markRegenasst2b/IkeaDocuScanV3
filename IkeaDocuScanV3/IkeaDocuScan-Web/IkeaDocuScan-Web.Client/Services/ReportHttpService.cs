using System.Net.Http.Json;
using IkeaDocuScan.Shared.DTOs.Reports;
using IkeaDocuScan.Shared.Interfaces;

namespace IkeaDocuScan_Web.Client.Services;

/// <summary>
/// Client-side HTTP service for special reports
/// </summary>
public class ReportHttpService : IReportService
{
    private readonly HttpClient _http;
    private readonly ILogger<ReportHttpService> _logger;

    public ReportHttpService(HttpClient http, ILogger<ReportHttpService> logger)
    {
        _http = http;
        _logger = logger;
    }

    /// <summary>
    /// Get Barcode Gaps report
    /// </summary>
    public async Task<List<BarcodeGapReportDto>> GetBarcodeGapsReportAsync()
    {
        try
        {
            _logger.LogInformation("Fetching Barcode Gaps report from API");
            var report = await _http.GetFromJsonAsync<List<BarcodeGapReportDto>>("/api/reports/barcode-gaps");
            return report ?? new List<BarcodeGapReportDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching Barcode Gaps report");
            throw;
        }
    }

    /// <summary>
    /// Get Duplicate Documents report
    /// </summary>
    public async Task<List<DuplicateDocumentsReportDto>> GetDuplicateDocumentsReportAsync()
    {
        try
        {
            _logger.LogInformation("Fetching Duplicate Documents report from API");
            var report = await _http.GetFromJsonAsync<List<DuplicateDocumentsReportDto>>("/api/reports/duplicate-documents");
            return report ?? new List<DuplicateDocumentsReportDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching Duplicate Documents report");
            throw;
        }
    }

    /// <summary>
    /// Get Unlinked Registrations report
    /// </summary>
    public async Task<List<UnlinkedRegistrationsReportDto>> GetUnlinkedRegistrationsReportAsync()
    {
        try
        {
            _logger.LogInformation("Fetching Unlinked Registrations report from API");
            var report = await _http.GetFromJsonAsync<List<UnlinkedRegistrationsReportDto>>("/api/reports/unlinked-registrations");
            return report ?? new List<UnlinkedRegistrationsReportDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching Unlinked Registrations report");
            throw;
        }
    }

    /// <summary>
    /// Get Scan Copies report
    /// </summary>
    public async Task<List<ScanCopiesReportDto>> GetScanCopiesReportAsync()
    {
        try
        {
            _logger.LogInformation("Fetching Scan Copies report from API");
            var report = await _http.GetFromJsonAsync<List<ScanCopiesReportDto>>("/api/reports/scan-copies");
            return report ?? new List<ScanCopiesReportDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching Scan Copies report");
            throw;
        }
    }

    /// <summary>
    /// Get Suppliers report
    /// </summary>
    public async Task<List<SuppliersReportDto>> GetSuppliersReportAsync()
    {
        try
        {
            _logger.LogInformation("Fetching Suppliers report from API");
            var report = await _http.GetFromJsonAsync<List<SuppliersReportDto>>("/api/reports/suppliers");
            return report ?? new List<SuppliersReportDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching Suppliers report");
            throw;
        }
    }

    /// <summary>
    /// Get All Documents report
    /// </summary>
    public async Task<List<AllDocumentsReportDto>> GetAllDocumentsReportAsync()
    {
        try
        {
            _logger.LogInformation("Fetching All Documents report from API");
            var report = await _http.GetFromJsonAsync<List<AllDocumentsReportDto>>("/api/reports/all-documents");
            return report ?? new List<AllDocumentsReportDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching All Documents report");
            throw;
        }
    }
}
