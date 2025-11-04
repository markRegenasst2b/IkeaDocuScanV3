using IkeaDocuScan.Shared.DTOs.Documents;
using IkeaDocuScan.Shared.DTOs.Excel;
using IkeaDocuScan_Web.Client.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace IkeaDocuScan_Web.Client.Pages;

public partial class ExcelPreview : ComponentBase
{
    [Inject] private ExcelExportHttpService ExcelService { get; set; } = null!;
    [Inject] private DocumentHttpService DocumentService { get; set; } = null!;
    [Inject] private IJSRuntime JSRuntime { get; set; } = null!;
    [Inject] private NavigationManager NavigationManager { get; set; } = null!;

    [SupplyParameterFromQuery(Name = "searchString")]
    public string? SearchString { get; set; }

    [SupplyParameterFromQuery(Name = "documentTypeIds")]
    public string? DocumentTypeIdsParam { get; set; }

    [SupplyParameterFromQuery(Name = "pageSize")]
    public int? PageSizeParam { get; set; }

    // State
    private DocumentSearchResultDto? searchResults;
    private List<ExcelColumnMetadataDto> columnMetadata = new();
    private Dictionary<string, string>? filterContext;
    private ExcelExportValidationResult? validationResult;

    // Paging
    private int currentPage = 1;
    private int rowsPerPage = 25;
    private int totalPages => searchResults != null && searchResults.Items.Any()
        ? (int)Math.Ceiling((double)searchResults.Items.Count / rowsPerPage)
        : 0;

    // UI State
    private bool isLoading = true;
    private bool isExporting = false;
    private bool canExport => searchResults != null && searchResults.Items.Any() && !isExporting;
    private string? errorMessage;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            isLoading = true;

            // Get column metadata
            columnMetadata = await ExcelService.GetMetadataAsync();

            // Build search criteria from query parameters
            var searchCriteria = new DocumentSearchRequestDto
            {
                SearchString = SearchString,
                PageNumber = 1,
                PageSize = PageSizeParam ?? 1000, // Get up to 1000 for preview
                DocumentTypeIds = ParseDocumentTypeIds(DocumentTypeIdsParam)
            };

            // Search for documents
            searchResults = await DocumentService.SearchAsync(searchCriteria);

            // Build filter context for display
            BuildFilterContext(searchCriteria);

            // Validate export size
            validationResult = await ExcelService.ValidateExportAsync(searchCriteria);

            // Set initial page size from parameter
            if (PageSizeParam.HasValue)
            {
                rowsPerPage = PageSizeParam.Value switch
                {
                    <= 10 => 10,
                    <= 25 => 25,
                    _ => 100
                };
            }
        }
        catch (Exception ex)
        {
            errorMessage = $"Failed to load data: {ex.Message}";
        }
        finally
        {
            isLoading = false;
        }
    }

    private void BuildFilterContext(DocumentSearchRequestDto criteria)
    {
        filterContext = new Dictionary<string, string>();

        if (!string.IsNullOrEmpty(criteria.SearchString))
        {
            filterContext["Search Text"] = criteria.SearchString;
        }

        if (criteria.DocumentTypeIds.Any())
        {
            filterContext["Document Type IDs"] = string.Join(", ", criteria.DocumentTypeIds);
        }

        if (criteria.DateOfContractFrom.HasValue)
        {
            filterContext["Contract Date From"] = criteria.DateOfContractFrom.Value.ToString("yyyy-MM-dd");
        }

        if (criteria.DateOfContractTo.HasValue)
        {
            filterContext["Contract Date To"] = criteria.DateOfContractTo.Value.ToString("yyyy-MM-dd");
        }

        filterContext["Export Date"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    }

    private List<int> ParseDocumentTypeIds(string? documentTypeIdsParam)
    {
        if (string.IsNullOrEmpty(documentTypeIdsParam))
            return new List<int>();

        return documentTypeIdsParam
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(id => int.TryParse(id.Trim(), out var result) ? result : 0)
            .Where(id => id > 0)
            .ToList();
    }

    private IEnumerable<DocumentSearchItemDto> GetPagedData()
    {
        if (searchResults == null || !searchResults.Items.Any())
            return Enumerable.Empty<DocumentSearchItemDto>();

        var skip = (currentPage - 1) * rowsPerPage;
        return searchResults.Items.Skip(skip).Take(rowsPerPage);
    }

    private int GetStartRow()
    {
        if (searchResults == null || !searchResults.Items.Any())
            return 0;

        return ((currentPage - 1) * rowsPerPage) + 1;
    }

    private int GetEndRow()
    {
        if (searchResults == null || !searchResults.Items.Any())
            return 0;

        return Math.Min(currentPage * rowsPerPage, searchResults.Items.Count);
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

    private string GetCellClass(string dataType)
    {
        return dataType switch
        {
            "Number" => "text-end",
            "Currency" => "text-end",
            "Percentage" => "text-end",
            "Date" => "text-nowrap",
            _ => ""
        };
    }

    private string GetCellValue(DocumentSearchItemDto item, string propertyName)
    {
        try
        {
            var property = typeof(DocumentSearchItemDto).GetProperty(propertyName);
            if (property == null)
                return string.Empty;

            var value = property.GetValue(item);
            if (value == null)
                return string.Empty;

            // Format based on type
            return value switch
            {
                DateTime dt => dt.ToString("yyyy-MM-dd"),
                bool b => b ? "Yes" : "No",
                decimal d => d.ToString("N2"),
                double db => db.ToString("N2"),
                _ => value.ToString() ?? string.Empty
            };
        }
        catch
        {
            return string.Empty;
        }
    }

    private async Task ExportToExcel()
    {
        try
        {
            isExporting = true;
            errorMessage = null;

            // Build search criteria
            var searchCriteria = new DocumentSearchRequestDto
            {
                SearchString = SearchString,
                PageNumber = 1,
                PageSize = searchResults?.TotalCount ?? 1000,
                DocumentTypeIds = ParseDocumentTypeIds(DocumentTypeIdsParam)
            };

            // Get Excel file bytes
            var fileBytes = await ExcelService.ExportToExcelAsync(searchCriteria, filterContext);

            // Generate filename
            var fileName = $"Documents_Export_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

            // Trigger download using JavaScript
            await JSRuntime.InvokeVoidAsync("downloadFileFromBytes", fileName, fileBytes);

            // Optional: Show success message
            await JSRuntime.InvokeVoidAsync("alert", $"Excel file '{fileName}' has been downloaded successfully!");
        }
        catch (Exception ex)
        {
            errorMessage = $"Export failed: {ex.Message}";
        }
        finally
        {
            isExporting = false;
        }
    }

    private void Cancel()
    {
        NavigationManager.NavigateTo("/documents/search");
    }
}
