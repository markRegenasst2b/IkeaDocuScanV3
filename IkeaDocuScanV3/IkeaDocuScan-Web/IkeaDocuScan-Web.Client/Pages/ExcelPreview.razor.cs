using IkeaDocuScan.Shared.DTOs.Documents;
using IkeaDocuScan.Shared.DTOs.Excel;
using IkeaDocuScan.Shared.Interfaces;
using IkeaDocuScan_Web.Client.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace IkeaDocuScan_Web.Client.Pages;

public partial class ExcelPreview : ComponentBase
{
    [Inject] private ExcelExportHttpService ExcelService { get; set; } = null!;
    [Inject] private DocumentHttpService DocumentService { get; set; } = null!;
    [Inject] private IReportService ReportService { get; set; } = null!;
    [Inject] private IJSRuntime JSRuntime { get; set; } = null!;
    [Inject] private NavigationManager NavigationManager { get; set; } = null!;

    [SupplyParameterFromQuery(Name = "searchString")]
    public string? SearchString { get; set; }

    [SupplyParameterFromQuery(Name = "documentTypeIds")]
    public string? DocumentTypeIdsParam { get; set; }

    [SupplyParameterFromQuery(Name = "pageSize")]
    public int? PageSizeParam { get; set; }

    [SupplyParameterFromQuery(Name = "fax")]
    public string? FaxParam { get; set; }

    [SupplyParameterFromQuery(Name = "originalReceived")]
    public string? OriginalReceivedParam { get; set; }

    [SupplyParameterFromQuery(Name = "confidential")]
    public string? ConfidentialParam { get; set; }

    [SupplyParameterFromQuery(Name = "bankConfirmation")]
    public string? BankConfirmationParam { get; set; }

    [SupplyParameterFromQuery(Name = "counterpartyName")]
    public string? CounterpartyName { get; set; }

    [SupplyParameterFromQuery(Name = "documentNumber")]
    public string? DocumentNumber { get; set; }

    [SupplyParameterFromQuery(Name = "associatedToPua")]
    public string? AssociatedToPua { get; set; }

    [SupplyParameterFromQuery(Name = "versionNo")]
    public string? VersionNo { get; set; }

    [SupplyParameterFromQuery(Name = "selectedIds")]
    public string? SelectedIdsParam { get; set; }

    [SupplyParameterFromQuery(Name = "reportType")]
    public string? ReportType { get; set; }

    // State
    private DocumentSearchResultDto? searchResults;
    private bool isSelectionMode => !string.IsNullOrEmpty(SelectedIdsParam);
    private bool isReportMode => !string.IsNullOrEmpty(ReportType);
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

            if (isReportMode)
            {
                // Report mode: Load report data based on report type
                await LoadReportData();
            }
            else if (isSelectionMode)
            {
                // Selection mode: Load specific selected documents by IDs
                await LoadSelectedDocuments();
            }
            else
            {
                // Filter mode: Search with criteria
                await LoadFilteredDocuments();
            }

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

    private async Task LoadReportData()
    {
        if (string.IsNullOrEmpty(ReportType))
        {
            errorMessage = "Report type not specified";
            return;
        }

        try
        {
            switch (ReportType.ToLower())
            {
                case "barcode-gaps":
                    var barcodeGaps = await ReportService.GetBarcodeGapsReportAsync();
                    // Convert to searchResults format for display
                    searchResults = new DocumentSearchResultDto
                    {
                        Items = new List<DocumentSearchItemDto>(),
                        TotalCount = barcodeGaps.Count
                    };
                    // Store report data for export (will be handled by ExcelService)
                    break;

                case "duplicate-documents":
                    var duplicates = await ReportService.GetDuplicateDocumentsReportAsync();
                    searchResults = new DocumentSearchResultDto
                    {
                        Items = new List<DocumentSearchItemDto>(),
                        TotalCount = duplicates.Count
                    };
                    break;

                case "unlinked-registrations":
                    var unlinked = await ReportService.GetUnlinkedRegistrationsReportAsync();
                    searchResults = new DocumentSearchResultDto
                    {
                        Items = new List<DocumentSearchItemDto>(),
                        TotalCount = unlinked.Count
                    };
                    break;

                case "scan-copies":
                    var scanCopies = await ReportService.GetScanCopiesReportAsync();
                    searchResults = new DocumentSearchResultDto
                    {
                        Items = new List<DocumentSearchItemDto>(),
                        TotalCount = scanCopies.Count
                    };
                    break;

                case "suppliers":
                    var suppliers = await ReportService.GetSuppliersReportAsync();
                    searchResults = new DocumentSearchResultDto
                    {
                        Items = new List<DocumentSearchItemDto>(),
                        TotalCount = suppliers.Count
                    };
                    break;

                case "all-documents":
                    var allDocs = await ReportService.GetAllDocumentsReportAsync();
                    searchResults = new DocumentSearchResultDto
                    {
                        Items = new List<DocumentSearchItemDto>(),
                        TotalCount = allDocs.Count
                    };
                    break;

                default:
                    errorMessage = $"Unknown report type: {ReportType}";
                    break;
            }
        }
        catch (NotImplementedException)
        {
            errorMessage = $"Report '{ReportType}' is not yet implemented";
        }
        catch (Exception ex)
        {
            errorMessage = $"Error loading report: {ex.Message}";
            throw;
        }
    }

    private async Task LoadFilteredDocuments()
    {
        // Build search criteria from query parameters
        var searchCriteria = new DocumentSearchRequestDto
        {
            SearchString = SearchString,
            PageNumber = 1,
            PageSize = PageSizeParam ?? 1000, // Get up to 1000 for preview
            DocumentTypeIds = ParseDocumentTypeIds(DocumentTypeIdsParam),
            Fax = ParseNullableBool(FaxParam),
            OriginalReceived = ParseNullableBool(OriginalReceivedParam),
            Confidential = ParseNullableBool(ConfidentialParam),
            BankConfirmation = ParseNullableBool(BankConfirmationParam),
            CounterpartyName = CounterpartyName,
            DocumentNumber = DocumentNumber,
            AssociatedToPua = AssociatedToPua,
            VersionNo = VersionNo
        };

        // Search for documents
        searchResults = await DocumentService.SearchAsync(searchCriteria);

        // Build filter context for display
        BuildFilterContext(searchCriteria);

        // Validate export size
        validationResult = await ExcelService.ValidateExportAsync(searchCriteria);
    }

    private async Task LoadSelectedDocuments()
    {
        var selectedIds = ParseSelectedIds(SelectedIdsParam);

        if (!selectedIds.Any())
        {
            errorMessage = "No documents selected for export.";
            return;
        }

        // Build search criteria with selected barcodes
        var searchCriteria = new DocumentSearchRequestDto
        {
            Barcodes = string.Join(",", selectedIds),
            PageNumber = 1,
            PageSize = selectedIds.Count
        };

        // Search for the selected documents
        searchResults = await DocumentService.SearchAsync(searchCriteria);

        // Build selection context for display
        BuildSelectionContext(selectedIds.Count);

        // Validate export size (though selection is already limited)
        validationResult = await ExcelService.ValidateExportAsync(searchCriteria);
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

        if (criteria.Fax.HasValue)
        {
            filterContext["Fax"] = criteria.Fax.Value ? "Yes" : "No";
        }

        if (criteria.OriginalReceived.HasValue)
        {
            filterContext["Original Received"] = criteria.OriginalReceived.Value ? "Yes" : "No";
        }

        if (criteria.Confidential.HasValue)
        {
            filterContext["Confidential"] = criteria.Confidential.Value ? "Yes" : "No";
        }

        if (criteria.BankConfirmation.HasValue)
        {
            filterContext["Bank Confirmation"] = criteria.BankConfirmation.Value ? "Yes" : "No";
        }

        if (!string.IsNullOrEmpty(criteria.CounterpartyName))
        {
            filterContext["Counterparty Name"] = criteria.CounterpartyName;
        }

        if (!string.IsNullOrEmpty(criteria.DocumentNumber))
        {
            filterContext["Document No"] = criteria.DocumentNumber;
        }

        if (!string.IsNullOrEmpty(criteria.AssociatedToPua))
        {
            filterContext["Assoc to PUA/Agr No"] = criteria.AssociatedToPua;
        }

        if (!string.IsNullOrEmpty(criteria.VersionNo))
        {
            filterContext["Version No"] = criteria.VersionNo;
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

    private void BuildSelectionContext(int selectedCount)
    {
        filterContext = new Dictionary<string, string>
        {
            ["Export Type"] = "Selected Documents",
            ["Documents Selected"] = selectedCount.ToString(),
            ["Export Date"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        };
    }

    private List<int> ParseSelectedIds(string? selectedIdsParam)
    {
        if (string.IsNullOrEmpty(selectedIdsParam))
            return new List<int>();

        return selectedIdsParam
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(id => int.TryParse(id.Trim(), out var result) ? result : 0)
            .Where(id => id > 0)
            .ToList();
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

    private bool? ParseNullableBool(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        if (bool.TryParse(value, out var result))
            return result;

        return null;
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

            DocumentSearchRequestDto searchCriteria;

            if (isSelectionMode)
            {
                // Export selected documents by IDs
                var selectedIds = ParseSelectedIds(SelectedIdsParam);
                searchCriteria = new DocumentSearchRequestDto
                {
                    Barcodes = string.Join(",", selectedIds),
                    PageNumber = 1,
                    PageSize = selectedIds.Count
                };
            }
            else
            {
                // Export filtered documents
                searchCriteria = new DocumentSearchRequestDto
                {
                    SearchString = SearchString,
                    PageNumber = 1,
                    PageSize = searchResults?.TotalCount ?? 1000,
                    DocumentTypeIds = ParseDocumentTypeIds(DocumentTypeIdsParam),
                    Fax = ParseNullableBool(FaxParam),
                    OriginalReceived = ParseNullableBool(OriginalReceivedParam),
                    Confidential = ParseNullableBool(ConfidentialParam),
                    BankConfirmation = ParseNullableBool(BankConfirmationParam),
                    CounterpartyName = CounterpartyName,
                    DocumentNumber = DocumentNumber,
                    AssociatedToPua = AssociatedToPua,
                    VersionNo = VersionNo
                };
            }

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

    private async Task Cancel()
    {
        // Use browser history to go back, preserving search state
        await JSRuntime.InvokeVoidAsync("history.back");
    }
}
