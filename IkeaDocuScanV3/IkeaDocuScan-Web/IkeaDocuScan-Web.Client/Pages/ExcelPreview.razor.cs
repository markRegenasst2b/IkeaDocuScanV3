using ExcelReporting.Models;
using ExcelReporting.Services;
using IkeaDocuScan_Web.Client.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace IkeaDocuScan_Web.Client.Pages;

/// <summary>
/// Generic Excel preview page for any DTO derived from ExportableBase
/// Uses reflection and ExcelExportAttribute to dynamically render data
/// </summary>
public partial class ExcelPreview : ComponentBase
{
    [Inject] private ExcelPreviewDataService PreviewDataService { get; set; } = null!;
    [Inject] private PropertyMetadataExtractor MetadataExtractor { get; set; } = null!;
    [Inject] private IJSRuntime JSRuntime { get; set; } = null!;
    [Inject] private NavigationManager NavigationManager { get; set; } = null!;

    [SupplyParameterFromQuery(Name = "source")]
    public string? Source { get; set; }

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
    private string? errorMessage;

    protected override void OnInitialized()
    {
        try
        {
            isLoading = true;

            // Get data from service
            var (retrievedData, title, context) = PreviewDataService.GetData();

            if (retrievedData == null || !retrievedData.Any())
            {
                errorMessage = "No data available to preview. Please navigate from a report or search page.";
                return;
            }

            data = retrievedData;
            pageTitle = title ?? "Data Preview";
            contextInfo = context;

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
        }
        finally
        {
            isLoading = false;
        }
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
            ExcelDataType.Number => "text-end",
            ExcelDataType.Currency => "text-end",
            ExcelDataType.Date => "text-nowrap",
            _ => ""
        };
    }

    private string GetCellValue(ExportableBase item, ExcelExportMetadata column)
    {
        return column.GetFormattedValue(item);
    }

    private async Task Cancel()
    {
        // Use browser history to go back, preserving state
        await JSRuntime.InvokeVoidAsync("history.back");
    }
}
