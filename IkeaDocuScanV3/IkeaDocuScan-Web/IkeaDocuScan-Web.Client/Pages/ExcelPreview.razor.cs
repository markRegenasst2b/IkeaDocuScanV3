using ExcelReporting.Models;
using ExcelReporting.Services;
using IkeaDocuScan.Shared.Interfaces;
using IkeaDocuScan.Shared.DTOs.Excel;
using IkeaDocuScan_Web.Client.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Net.Http.Json;

namespace IkeaDocuScan_Web.Client.Pages;

/// <summary>
/// Generic Excel preview page for any DTO derived from ExportableBase
/// Uses reflection and ExcelExportAttribute to dynamically render data
/// Supports both direct report loading and data passed via ExcelPreviewDataService
/// </summary>
public partial class ExcelPreview : ComponentBase
{
    [Inject] private ExcelPreviewDataService PreviewDataService { get; set; } = null!;
    [Inject] private PropertyMetadataExtractor MetadataExtractor { get; set; } = null!;
    [Inject] private IReportService ReportService { get; set; } = null!;
    [Inject] private IJSRuntime JSRuntime { get; set; } = null!;
    [Inject] private NavigationManager NavigationManager { get; set; } = null!;
    [Inject] private HttpClient HttpClient { get; set; } = null!;

    [SupplyParameterFromQuery(Name = "source")]
    public string? Source { get; set; }

    [SupplyParameterFromQuery(Name = "reportType")]
    public string? ReportType { get; set; }

    // State
    private List<ExportableBase> data = new();
    private List<ExcelExportMetadata> columnMetadata = new();
    private Dictionary<string, string>? contextInfo;
    private string pageTitle = "Data Preview";

    // Paging
    private int currentPage = 1;
    private int rowsPerPage = 25;
    private int totalPages => data.Any() ? (int)Math.Ceiling((double)data.Count / rowsPerPage) : 0;
    private int totalRecords => data.Count;

    // UI State
    private bool isLoading = true;
    private bool isDownloading = false;
    private string? errorMessage;
    private string? lastSource; // Track last source to detect navigation changes
    private string? lastReportType; // Track last report type to detect navigation changes
    private string? excelDownloadUrl; // URL for Excel download (for reports)
    private List<int>? documentIds; // Document IDs for export by IDs (for search/selection)

    protected override void OnParametersSet()
    {
        // Check if we're navigating to a different report or source
        // This ensures we reload data when switching between reports
        if (lastSource != Source || lastReportType != ReportType)
        {
            lastSource = Source;
            lastReportType = ReportType;
            LoadPreviewData();
        }
    }

    private async void LoadPreviewData()
    {
        try
        {
            isLoading = true;
            errorMessage = null;

            List<ExportableBase>? retrievedData = null;
            string? title = null;
            Dictionary<string, string>? context = null;

            // Check if we're loading a report directly
            if (!string.IsNullOrEmpty(ReportType))
            {
                // Load report data directly from ReportService
                (retrievedData, title) = await LoadReportDataAsync(ReportType);

                // Build context information
                context = new Dictionary<string, string>
                {
                    ["Report Type"] = title ?? ReportType,
                    ["Generated"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    ["Record Count"] = (retrievedData?.Count ?? 0).ToString()
                };

                // Set Excel download URL for reports
                excelDownloadUrl = $"/api/reports/{ReportType}/excel";
            }
            else
            {
                // Get data from service (for search results, etc.)
                (retrievedData, title, context) = PreviewDataService.GetData();

                // For search/selection, extract document IDs for POST-based download
                // This allows us to download without resending full DTOs (security + efficiency)
                excelDownloadUrl = null; // No GET URL for search results

                // Extract document IDs from the data for the download endpoint
                if (retrievedData != null && retrievedData.Any())
                {
                    documentIds = retrievedData
                        .Select(d => d.GetType().GetProperty("Id")?.GetValue(d))
                        .Where(id => id != null)
                        .Cast<int>()
                        .ToList();
                }
                else
                {
                    documentIds = null;
                }
            }

            pageTitle = title ?? "Data Preview";
            contextInfo = context;

            // Reset paging
            currentPage = 1;

            // Handle no data scenario with contextual message
            if (retrievedData == null || !retrievedData.Any())
            {
                if (!string.IsNullOrEmpty(title))
                {
                    errorMessage = $"No data found for {title}. The report completed successfully but returned no records matching the criteria.";
                }
                else
                {
                    errorMessage = "No data available to preview. Please navigate from a report or search page.";
                }
                data = new List<ExportableBase>();
                columnMetadata = new List<ExcelExportMetadata>();
                return;
            }

            data = retrievedData;

            // Extract metadata using reflection and ExcelExportAttribute
            var dataType = data.First().GetType();
            columnMetadata = MetadataExtractor.ExtractMetadata(dataType);

            if (!columnMetadata.Any())
            {
                errorMessage = $"No exportable properties found on type {dataType.Name}. Ensure properties have [ExcelExport] attribute.";
            }
        }
        catch (Exception ex)
        {
            errorMessage = $"Failed to load preview: {ex.Message}";
            data = new List<ExportableBase>();
            columnMetadata = new List<ExcelExportMetadata>();
        }
        finally
        {
            isLoading = false;
            StateHasChanged();
        }
    }

    private async Task<(List<ExportableBase>? Data, string Title)> LoadReportDataAsync(string reportType)
    {
        return reportType.ToLowerInvariant() switch
        {
            "barcode-gaps" => (
                (await ReportService.GetBarcodeGapsReportAsync()).Cast<ExportableBase>().ToList(),
                "Barcode Gaps Report"
            ),
            "duplicate-documents" => (
                (await ReportService.GetDuplicateDocumentsReportAsync()).Cast<ExportableBase>().ToList(),
                "Duplicate Documents Report"
            ),
            "unlinked-registrations" => (
                (await ReportService.GetUnlinkedRegistrationsReportAsync()).Cast<ExportableBase>().ToList(),
                "Unlinked Registrations Report"
            ),
            "scan-copies" => (
                (await ReportService.GetScanCopiesReportAsync()).Cast<ExportableBase>().ToList(),
                "Scan Copies Report"
            ),
            "suppliers" => (
                (await ReportService.GetSuppliersReportAsync()).Cast<ExportableBase>().ToList(),
                "Suppliers Report"
            ),
            "all-documents" => (
                (await ReportService.GetAllDocumentsReportAsync()).Cast<ExportableBase>().ToList(),
                "All Documents Report"
            ),
            _ => throw new ArgumentException($"Unknown report type: {reportType}")
        };
    }

    private bool HasData() => data.Any();

    private IEnumerable<ExportableBase> GetPagedData()
    {
        if (!HasData())
            return Enumerable.Empty<ExportableBase>();

        var skip = (currentPage - 1) * rowsPerPage;
        return data.Skip(skip).Take(rowsPerPage);
    }

    private int GetStartRow()
    {
        if (!HasData())
            return 0;

        return ((currentPage - 1) * rowsPerPage) + 1;
    }

    private int GetEndRow()
    {
        if (!HasData())
            return 0;

        return Math.Min(currentPage * rowsPerPage, data.Count);
    }

    private void FirstPage()
    {
        currentPage = 1;
    }

    private void PreviousPage()
    {
        if (currentPage > 1)
        {
            currentPage--;
        }
    }

    private void NextPage()
    {
        if (currentPage < totalPages)
        {
            currentPage++;
        }
    }

    private void LastPage()
    {
        currentPage = totalPages;
    }

    private void OnRowsPerPageChanged()
    {
        currentPage = 1; // Reset to first page when changing rows per page
    }

    private string GetCellClass(ExcelDataType dataType)
    {
        return dataType switch
        {
            ExcelDataType.Number => "text-nowrap",
            ExcelDataType.Currency => "text-nowrap",
            ExcelDataType.Date => "text-nowrap",
            _ => ""
        };
    }

    private string GetCellValue(ExportableBase item, ExcelExportMetadata column)
    {
        return column.GetFormattedValue(item);
    }

    /// <summary>
    /// Downloads Excel file by POSTing document IDs to the server
    /// Used for search results and selected documents where GET URLs don't work
    /// </summary>
    private async Task DownloadExcelByIds()
    {
        if (documentIds == null || !documentIds.Any())
        {
            errorMessage = "No document IDs available for export";
            return;
        }

        try
        {
            isDownloading = true;
            errorMessage = null;
            StateHasChanged();

            // Create request with document IDs
            var request = new ExcelExportByIdsRequestDto
            {
                DocumentIds = documentIds,
                Title = pageTitle,
                Context = contextInfo
            };

            // POST to the export by IDs endpoint
            var response = await HttpClient.PostAsJsonAsync("/api/excel/export/by-ids", request);

            if (response.IsSuccessStatusCode)
            {
                // Get the file bytes
                var fileBytes = await response.Content.ReadAsByteArrayAsync();

                // Generate filename
                var title = pageTitle?.Replace(" ", "_") ?? "Documents";
                var fileName = $"{title}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

                // Trigger download using JavaScript
                await JSRuntime.InvokeVoidAsync("downloadFileFromBytes", fileName, fileBytes);
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                errorMessage = $"Export failed: {response.StatusCode} - {errorContent}";
            }
        }
        catch (Exception ex)
        {
            errorMessage = $"Failed to download Excel: {ex.Message}";
        }
        finally
        {
            isDownloading = false;
            StateHasChanged();
        }
    }

    private async Task Cancel()
    {
        // Use browser history to go back, preserving state
        await JSRuntime.InvokeVoidAsync("history.back");
    }
}
