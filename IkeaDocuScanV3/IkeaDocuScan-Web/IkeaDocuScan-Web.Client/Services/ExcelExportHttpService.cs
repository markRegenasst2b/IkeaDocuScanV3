using IkeaDocuScan.Shared.DTOs.Documents;
using IkeaDocuScan.Shared.DTOs.Excel;
using System.Net.Http.Json;

namespace IkeaDocuScan_Web.Client.Services;

/// <summary>
/// HTTP service for Excel export operations
/// </summary>
public class ExcelExportHttpService
{
    private readonly HttpClient _httpClient;

    public ExcelExportHttpService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// Get export metadata (column definitions)
    /// </summary>
    public async Task<List<ExcelColumnMetadataDto>> GetMetadataAsync()
    {
        var response = await _httpClient.GetFromJsonAsync<List<ExcelColumnMetadataDto>>("/api/excel/metadata/documents");
        return response ?? new List<ExcelColumnMetadataDto>();
    }

    /// <summary>
    /// Validate export size before generating
    /// </summary>
    public async Task<ExcelExportValidationResult> ValidateExportAsync(DocumentSearchRequestDto searchCriteria)
    {
        var request = new ExcelExportRequest
        {
            SearchCriteria = searchCriteria
        };

        var response = await _httpClient.PostAsJsonAsync("/api/excel/validate/documents", request);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<ExcelExportValidationResult>()
            ?? new ExcelExportValidationResult { IsValid = false, Message = "Validation failed" };
    }

    /// <summary>
    /// Get the download URL for Excel export
    /// </summary>
    public string GetExportUrl(DocumentSearchRequestDto searchCriteria, Dictionary<string, string>? filterContext = null)
    {
        // For client-side, we'll use a different approach - POST via form or fetch
        return "/api/excel/export/documents";
    }

    /// <summary>
    /// Export data and get the file bytes
    /// </summary>
    public async Task<byte[]> ExportToExcelAsync(DocumentSearchRequestDto searchCriteria, Dictionary<string, string>? filterContext = null)
    {
        var request = new ExcelExportRequest
        {
            SearchCriteria = searchCriteria,
            FilterContext = filterContext
        };

        var response = await _httpClient.PostAsJsonAsync("/api/excel/export/documents", request);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsByteArrayAsync();
    }
}

/// <summary>
/// Request model for Excel export
/// </summary>
public class ExcelExportRequest
{
    public DocumentSearchRequestDto SearchCriteria { get; set; } = new();
    public Dictionary<string, string>? FilterContext { get; set; }
}

/// <summary>
/// Validation result for export size
/// </summary>
public class ExcelExportValidationResult
{
    public bool IsValid { get; set; }
    public bool HasWarning { get; set; }
    public string? Message { get; set; }
    public int RowCount { get; set; }
}
