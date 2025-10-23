using IkeaDocuScan.Shared.Enums;
using IkeaDocuScan.Shared.Interfaces;
using System.Net.Http.Json;

namespace IkeaDocuScan_Web.Client.Services;

/// <summary>
/// HTTP client service for AuditTrail operations
/// Implements IAuditTrailService interface to call server APIs
/// </summary>
public class AuditTrailHttpService : IAuditTrailService
{
    private readonly HttpClient _http;
    private readonly ILogger<AuditTrailHttpService> _logger;

    public AuditTrailHttpService(HttpClient http, ILogger<AuditTrailHttpService> logger)
    {
        _http = http;
        _logger = logger;
    }

    public async Task LogAsync(AuditAction action, string barCode, string? details = null, string? username = null)
    {
        try
        {
            _logger.LogInformation("Logging audit action {Action} for barcode {BarCode}", action, barCode);
            var request = new
            {
                Action = action.ToString(),
                BarCode = barCode,
                Details = details,
                Username = username
            };

            var response = await _http.PostAsJsonAsync("/api/audittrail", request);
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging audit trail for barcode {BarCode}", barCode);
            throw;
        }
    }

    public async Task LogByDocumentIdAsync(AuditAction action, int documentId, string? details = null, string? username = null)
    {
        try
        {
            _logger.LogInformation("Logging audit action {Action} for document {DocumentId}", action, documentId);
            var request = new
            {
                Action = action.ToString(),
                DocumentId = documentId,
                Details = details,
                Username = username
            };

            var response = await _http.PostAsJsonAsync($"/api/audittrail/document/{documentId}", request);
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging audit trail for document {DocumentId}", documentId);
            throw;
        }
    }

    public async Task LogBatchAsync(AuditAction action, IEnumerable<string> barCodes, string? details = null, string? username = null)
    {
        try
        {
            _logger.LogInformation("Logging batch audit action {Action} for {Count} barcodes", action, barCodes.Count());
            var request = new
            {
                Action = action.ToString(),
                BarCodes = barCodes,
                Details = details,
                Username = username
            };

            var response = await _http.PostAsJsonAsync("/api/audittrail/batch", request);
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging batch audit trail");
            throw;
        }
    }

    public async Task<List<AuditTrailDto>> GetByBarCodeAsync(string barCode, int limit = 100)
    {
        try
        {
            _logger.LogInformation("Fetching audit trail for barcode {BarCode}", barCode);
            var result = await _http.GetFromJsonAsync<List<AuditTrailDto>>($"/api/audittrail/barcode/{Uri.EscapeDataString(barCode)}?limit={limit}");
            return result ?? new List<AuditTrailDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching audit trail for barcode {BarCode}", barCode);
            throw;
        }
    }

    public async Task<List<AuditTrailDto>> GetByUserAsync(string username, int limit = 100)
    {
        try
        {
            _logger.LogInformation("Fetching audit trail for user {Username}", username);
            var result = await _http.GetFromJsonAsync<List<AuditTrailDto>>($"/api/audittrail/user/{Uri.EscapeDataString(username)}?limit={limit}");
            return result ?? new List<AuditTrailDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching audit trail for user {Username}", username);
            throw;
        }
    }

    public async Task<List<AuditTrailDto>> GetRecentAsync(int limit = 100)
    {
        try
        {
            _logger.LogInformation("Fetching recent audit trail entries");
            var result = await _http.GetFromJsonAsync<List<AuditTrailDto>>($"/api/audittrail/recent?limit={limit}");
            return result ?? new List<AuditTrailDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching recent audit trail");
            throw;
        }
    }

    public async Task<List<AuditTrailDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, AuditAction? action = null)
    {
        try
        {
            _logger.LogInformation("Fetching audit trail for date range {StartDate} to {EndDate}", startDate, endDate);
            var url = $"/api/audittrail/daterange?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}";
            if (action.HasValue)
            {
                url += $"&action={action.Value}";
            }

            var result = await _http.GetFromJsonAsync<List<AuditTrailDto>>(url);
            return result ?? new List<AuditTrailDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching audit trail for date range");
            throw;
        }
    }
}
