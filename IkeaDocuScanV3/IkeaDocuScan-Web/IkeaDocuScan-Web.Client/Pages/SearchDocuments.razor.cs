using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;
using IkeaDocuScan.Shared.DTOs.Documents;
using IkeaDocuScan.Shared.DTOs.DocumentTypes;
using IkeaDocuScan.Shared.DTOs.DocumentNames;
using IkeaDocuScan.Shared.DTOs.CounterParties;
using IkeaDocuScan.Shared.DTOs.Currencies;
using IkeaDocuScan.Shared.DTOs.Countries;
using IkeaDocuScan.Shared.Interfaces;
using IkeaDocuScan.Shared.Configuration;
using IkeaDocuScan_Web.Client.Models;

namespace IkeaDocuScan_Web.Client.Pages;

public partial class SearchDocuments : ComponentBase
{
    [Inject] private IDocumentService DocumentService { get; set; } = default!;
    [Inject] private IDocumentTypeService DocumentTypeService { get; set; } = default!;
    [Inject] private IDocumentNameService DocumentNameService { get; set; } = default!;
    [Inject] private ICounterPartyService CounterPartyService { get; set; } = default!;
    [Inject] private ICurrencyService CurrencyService { get; set; } = default!;
    [Inject] private ICountryService CountryService { get; set; } = default!;
    [Inject] private ILogger<SearchDocuments> Logger { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;
    [Inject] private IOptions<EmailSearchResultsOptions>? EmailOptions { get; set; }

    // Search request and results
    private DocumentSearchRequestDto searchRequest = new();
    private DocumentSearchResultDto? searchResults;

    // Reference data
    private List<DocumentTypeDto> documentTypes = new();
    private List<DocumentNameDto> allDocumentNames = new();
    private List<CounterPartyDto> counterParties = new();
    private List<CurrencyDto> currencies = new();
    private List<CountryDto> countries = new();

    // UI state
    private bool isLoadingReferenceData = true;
    private bool isSearching = false;
    private string? errorMessage;

    // Selection state
    private HashSet<int> selectedDocumentIds = new();

    // Sorting state
    private string? currentSortColumn;
    private string? currentSortDirection;

    // Modal state
    private bool showDeleteConfirmationModal = false;
    private string? deleteDocumentIdentifier;
    private int deleteDocumentId;
    private bool isDeleting = false;

    // Bulk delete state
    private bool showBulkDeleteConfirmationModal = false;
    private int bulkDeleteDocumentCount = 0;
    private List<string>? bulkDeleteIdentifiers;
    private bool isBulkDeleting = false;
    private string? bulkDeleteErrorMessage;
    private BulkDeleteProgress? bulkDeleteProgress;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            Logger.LogInformation("SearchDocuments page initializing");
            await LoadReferenceData();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error initializing SearchDocuments page");
            errorMessage = $"Failed to load reference data: {ex.Message}";
        }
        finally
        {
            isLoadingReferenceData = false;
        }
    }

    private async Task LoadReferenceData()
    {
        Logger.LogInformation("Loading reference data for search filters");

        // Load all reference data in parallel for better performance
        var documentTypesTask = DocumentTypeService.GetAllAsync();
        var documentNamesTask = DocumentNameService.GetAllAsync();
        var counterPartiesTask = CounterPartyService.GetAllAsync();
        var currenciesTask = CurrencyService.GetAllAsync();
        var countriesTask = CountryService.GetAllAsync();

        await Task.WhenAll(
            documentTypesTask,
            documentNamesTask,
            counterPartiesTask,
            currenciesTask,
            countriesTask
        );

        documentTypes = await documentTypesTask;
        allDocumentNames = await documentNamesTask;
        counterParties = await counterPartiesTask;
        currencies = await currenciesTask;
        countries = await countriesTask;

        Logger.LogInformation(
            "Loaded reference data: {DocTypes} document types, {DocNames} document names, {CounterParties} counter parties, {Currencies} currencies, {Countries} countries",
            documentTypes.Count, allDocumentNames.Count, counterParties.Count, currencies.Count, countries.Count);
    }

    private async Task ExecuteSearch()
    {
        try
        {
            isSearching = true;
            errorMessage = null;
            StateHasChanged();

            Logger.LogInformation("Executing document search");

            searchResults = await DocumentService.SearchAsync(searchRequest);

            Logger.LogInformation(
                "Search completed: {TotalCount} results, Page {CurrentPage}/{TotalPages}",
                searchResults.TotalCount, searchResults.CurrentPage, searchResults.TotalPages);

            if (searchResults.MaxLimitReached)
            {
                Logger.LogWarning("Search hit max limit of {MaxLimit} results", searchResults.MaxLimit);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error executing search");
            errorMessage = $"Search failed: {ex.Message}";
            searchResults = null;
        }
        finally
        {
            isSearching = false;
            StateHasChanged();
        }
    }

    private void ClearFilters()
    {
        Logger.LogInformation("Clearing all search filters");

        searchRequest = new DocumentSearchRequestDto
        {
            PageNumber = 1,
            PageSize = 25
        };

        searchResults = null;
        errorMessage = null;
        StateHasChanged();
    }

    /// <summary>
    /// Navigates to the Excel Preview page with current search criteria
    /// </summary>
    private void NavigateToExcelPreview()
    {
        if (searchResults == null || !searchResults.Items.Any())
        {
            Logger.LogWarning("Cannot navigate to Excel preview: No search results available");
            return;
        }

        Logger.LogInformation("Navigating to Excel preview with current search criteria");

        // Build query string from current search request
        var queryParams = new List<string>();

        if (!string.IsNullOrEmpty(searchRequest.SearchString))
        {
            queryParams.Add($"searchString={Uri.EscapeDataString(searchRequest.SearchString)}");
        }

        if (searchRequest.DocumentTypeIds.Any())
        {
            // Pass all selected document type IDs as comma-separated values
            var documentTypeIds = string.Join(",", searchRequest.DocumentTypeIds);
            queryParams.Add($"documentTypeIds={Uri.EscapeDataString(documentTypeIds)}");
        }

        // Boolean filters
        if (searchRequest.Fax.HasValue)
        {
            queryParams.Add($"fax={searchRequest.Fax.Value}");
        }

        if (searchRequest.OriginalReceived.HasValue)
        {
            queryParams.Add($"originalReceived={searchRequest.OriginalReceived.Value}");
        }

        if (searchRequest.Confidential.HasValue)
        {
            queryParams.Add($"confidential={searchRequest.Confidential.Value}");
        }

        if (searchRequest.BankConfirmation.HasValue)
        {
            queryParams.Add($"bankConfirmation={searchRequest.BankConfirmation.Value}");
        }

        // Text filters
        if (!string.IsNullOrEmpty(searchRequest.CounterpartyName))
        {
            queryParams.Add($"counterpartyName={Uri.EscapeDataString(searchRequest.CounterpartyName)}");
        }

        if (!string.IsNullOrEmpty(searchRequest.DocumentNumber))
        {
            queryParams.Add($"documentNumber={Uri.EscapeDataString(searchRequest.DocumentNumber)}");
        }

        if (!string.IsNullOrEmpty(searchRequest.AssociatedToPua))
        {
            queryParams.Add($"associatedToPua={Uri.EscapeDataString(searchRequest.AssociatedToPua)}");
        }

        if (!string.IsNullOrEmpty(searchRequest.VersionNo))
        {
            queryParams.Add($"versionNo={Uri.EscapeDataString(searchRequest.VersionNo)}");
        }

        // Add page size to show in preview
        var totalCount = searchResults.TotalCount;
        queryParams.Add($"pageSize={Math.Min(totalCount, 1000)}");

        var queryString = queryParams.Any() ? "?" + string.Join("&", queryParams) : "";

        NavigationManager.NavigateTo($"/excel-preview{queryString}");
    }

    // ========== Boolean Filter Change Handlers ==========

    /// <summary>
    /// Handles Fax filter change
    /// </summary>
    private void OnFaxChanged(string? value)
    {
        searchRequest.Fax = ParseNullableBool(value);
        Logger.LogDebug("Fax filter changed to: {Value}", searchRequest.Fax);
    }

    /// <summary>
    /// Handles Original Received filter change
    /// </summary>
    private void OnOriginalReceivedChanged(string? value)
    {
        searchRequest.OriginalReceived = ParseNullableBool(value);
        Logger.LogDebug("Original Received filter changed to: {Value}", searchRequest.OriginalReceived);
    }

    /// <summary>
    /// Handles Confidential filter change
    /// </summary>
    private void OnConfidentialChanged(string? value)
    {
        searchRequest.Confidential = ParseNullableBool(value);
        Logger.LogDebug("Confidential filter changed to: {Value}", searchRequest.Confidential);
    }

    /// <summary>
    /// Handles Bank Confirmation filter change
    /// </summary>
    private void OnBankConfirmationChanged(string? value)
    {
        searchRequest.BankConfirmation = ParseNullableBool(value);
        Logger.LogDebug("Bank Confirmation filter changed to: {Value}", searchRequest.BankConfirmation);
    }

    /// <summary>
    /// Parses a string to nullable boolean
    /// </summary>
    private bool? ParseNullableBool(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        if (bool.TryParse(value, out var result))
            return result;

        return null;
    }

    /// <summary>
    /// Handles Document Types multi-select change
    /// </summary>
    private void OnDocumentTypesChanged(ChangeEventArgs e)
    {
        if (e.Value is string[] selectedValues)
        {
            searchRequest.DocumentTypeIds = selectedValues
                .Select(v => int.TryParse(v, out var id) ? id : 0)
                .Where(id => id > 0)
                .ToList();

            Logger.LogDebug("Document types selection changed: {Count} types selected", searchRequest.DocumentTypeIds.Count);

            // Clear document name if it's no longer valid for the selected types
            if (searchRequest.DocumentNameId.HasValue)
            {
                var filteredNames = GetFilteredDocumentNames();
                if (!filteredNames.Any(dn => dn.Id == searchRequest.DocumentNameId.Value))
                {
                    searchRequest.DocumentNameId = null;
                    Logger.LogDebug("Document name filter cleared due to type selection change");
                }
            }

            StateHasChanged();
        }
    }

    /// <summary>
    /// Gets filtered document names based on selected document types
    /// If no document types selected, returns all document names
    /// </summary>
    private List<DocumentNameDto> GetFilteredDocumentNames()
    {
        if (!searchRequest.DocumentTypeIds.Any())
        {
            return allDocumentNames;
        }

        return allDocumentNames
            .Where(dn => searchRequest.DocumentTypeIds.Contains(dn.DocumentTypeId ?? -1))
            .ToList();
    }

    /// <summary>
    /// Gets hint text for document name filter based on current selection
    /// </summary>
    private string GetDocumentNameFilterHint()
    {
        if (!searchRequest.DocumentTypeIds.Any())
        {
            return "Showing all document names";
        }

        var filteredCount = GetFilteredDocumentNames().Count;
        return $"Filtered by selected document types ({filteredCount} available)";
    }

    // ========== Sorting Methods ==========

    /// <summary>
    /// Toggles sort order for the specified column
    /// Single-column sorting: ascending -> descending -> no sort
    /// </summary>
    private async Task ToggleSort(string column)
    {
        Logger.LogInformation("Toggling sort for column: {Column}", column);

        if (currentSortColumn == column)
        {
            // Same column clicked - cycle through: asc -> desc -> no sort
            currentSortDirection = currentSortDirection == "asc"
                ? "desc"
                : currentSortDirection == "desc"
                    ? null
                    : "asc";

            if (currentSortDirection == null)
            {
                currentSortColumn = null;
                Logger.LogInformation("Sort cleared");
            }
            else
            {
                Logger.LogInformation("Sort direction changed to: {Direction}", currentSortDirection);
            }
        }
        else
        {
            // New column clicked - start with ascending
            currentSortColumn = column;
            currentSortDirection = "asc";
            Logger.LogInformation("New sort column: {Column}, direction: asc", column);
        }

        // Update search request with new sort parameters
        searchRequest.SortColumn = currentSortColumn;
        searchRequest.SortDirection = currentSortDirection;

        // Re-execute search with new sorting
        if (searchResults != null)
        {
            await ExecuteSearch();
        }
    }

    /// <summary>
    /// Gets the sort indicator icon for the specified column
    /// </summary>
    private string GetSortIcon(string column)
    {
        if (currentSortColumn != column)
        {
            return string.Empty;
        }

        return currentSortDirection == "asc" ? "▲" : "▼";
    }

    // ========== Selection Methods ==========

    /// <summary>
    /// Toggles selection state for a specific document
    /// </summary>
    private void ToggleSelectDocument(int id, bool selected)
    {
        if (selected)
        {
            selectedDocumentIds.Add(id);
            Logger.LogDebug("Document {Id} selected", id);
        }
        else
        {
            selectedDocumentIds.Remove(id);
            Logger.LogDebug("Document {Id} deselected", id);
        }

        StateHasChanged();
    }

    /// <summary>
    /// Toggles select all checkbox - selects or deselects all on current page
    /// </summary>
    private void ToggleSelectAll(bool selectAll)
    {
        if (selectAll)
        {
            SelectAllOnPage();
        }
        else
        {
            DeselectAll();
        }
    }

    /// <summary>
    /// Selects all documents on the current page
    /// </summary>
    private void SelectAllOnPage()
    {
        if (searchResults == null) return;

        var previousCount = selectedDocumentIds.Count;

        foreach (var doc in searchResults.Items)
        {
            selectedDocumentIds.Add(doc.Id);
        }

        Logger.LogInformation("Selected all on page: {Count} documents selected (total: {Total})",
            searchResults.Items.Count, selectedDocumentIds.Count);

        StateHasChanged();
    }

    /// <summary>
    /// Deselects all documents (across all pages)
    /// </summary>
    private void DeselectAll()
    {
        var previousCount = selectedDocumentIds.Count;
        selectedDocumentIds.Clear();

        Logger.LogInformation("Deselected all documents ({Count} were selected)", previousCount);

        StateHasChanged();
    }

    /// <summary>
    /// Inverts the selection state for all documents on the current page
    /// </summary>
    private void InvertSelection()
    {
        if (searchResults == null) return;

        foreach (var doc in searchResults.Items)
        {
            if (selectedDocumentIds.Contains(doc.Id))
            {
                selectedDocumentIds.Remove(doc.Id);
            }
            else
            {
                selectedDocumentIds.Add(doc.Id);
            }
        }

        Logger.LogInformation("Inverted selection on current page. Total selected: {Count}", selectedDocumentIds.Count);

        StateHasChanged();
    }

    /// <summary>
    /// Checks if a specific document is selected
    /// </summary>
    private bool IsDocumentSelected(int id)
    {
        return selectedDocumentIds.Contains(id);
    }

    // ========== Pagination Methods ==========

    /// <summary>
    /// Navigates to a specific page
    /// </summary>
    private async Task GoToPage(int page)
    {
        if (searchResults == null) return;

        // Validate page number
        if (page < 1 || page > searchResults.TotalPages)
        {
            Logger.LogWarning("Invalid page number requested: {Page}", page);
            return;
        }

        Logger.LogInformation("Navigating to page {Page}", page);

        searchRequest.PageNumber = page;
        await ExecuteSearch();
    }

    /// <summary>
    /// Handles page size change - resets to first page and re-executes search
    /// </summary>
    private async Task OnPageSizeChanged()
    {
        Logger.LogInformation("Page size changed to {PageSize}", searchRequest.PageSize);

        // Reset to first page when page size changes
        searchRequest.PageNumber = 1;

        // Clear sort state when page size changes (optional, based on requirements)
        // currentSortColumn = null;
        // currentSortDirection = null;

        await ExecuteSearch();
    }

    /// <summary>
    /// Gets the starting page number for pagination display (shows 5 pages at a time)
    /// </summary>
    private int GetStartPage()
    {
        if (searchResults == null) return 1;

        var currentPage = searchResults.CurrentPage;
        var totalPages = searchResults.TotalPages;

        // Show current page ± 2 pages
        var startPage = Math.Max(1, currentPage - 2);

        // Adjust if we're near the end
        if (totalPages <= 5)
        {
            return 1;
        }

        if (currentPage > totalPages - 2)
        {
            return Math.Max(1, totalPages - 4);
        }

        return startPage;
    }

    /// <summary>
    /// Gets the ending page number for pagination display (shows 5 pages at a time)
    /// </summary>
    private int GetEndPage()
    {
        if (searchResults == null) return 1;

        var currentPage = searchResults.CurrentPage;
        var totalPages = searchResults.TotalPages;

        // Show current page ± 2 pages
        var endPage = Math.Min(totalPages, currentPage + 2);

        // Adjust if we're near the beginning
        if (totalPages <= 5)
        {
            return totalPages;
        }

        if (currentPage <= 3)
        {
            return Math.Min(5, totalPages);
        }

        return endPage;
    }

    // ========== Formatting Methods ==========

    /// <summary>
    /// Formats a nullable boolean as Yes/No/-
    /// </summary>
    private string FormatBoolean(bool? value)
    {
        return value.HasValue
            ? (value.Value ? "Yes" : "No")
            : "-";
    }

    /// <summary>
    /// Formats currency amount with currency code
    /// </summary>
    private string FormatAmount(decimal? amount, string? currencyCode)
    {
        if (!amount.HasValue) return "-";

        return string.IsNullOrEmpty(currencyCode)
            ? amount.Value.ToString("N2")
            : $"{currencyCode} {amount.Value:N2}";
    }

    /// <summary>
    /// Formats date as short date string
    /// </summary>
    private string FormatDate(DateTime? date)
    {
        return date?.ToString("yyyy-MM-dd") ?? "-";
    }

    /// <summary>
    /// Formats third party list (semicolon-separated to comma-separated)
    /// </summary>
    private string FormatThirdParty(string? thirdParty)
    {
        if (string.IsNullOrWhiteSpace(thirdParty))
        {
            return "-";
        }

        // Split by semicolon and rejoin with comma
        var parties = thirdParty.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return string.Join(", ", parties);
    }

    // ========== Per-Row Action Methods ==========

    /// <summary>
    /// Opens the PDF file in a new browser tab
    /// </summary>
    private void OpenPdf(int documentId)
    {
        Logger.LogInformation("Opening PDF preview for document ID: {DocumentId}", documentId);

        // Open preview page in new tab
        var url = $"/documents/preview/{documentId}";
        NavigationManager.NavigateTo(url, true); // forceLoad = true opens in new context
    }

    /// <summary>
    /// Navigates to the View Properties page
    /// </summary>
    private void ViewProperties(int documentId)
    {
        Logger.LogInformation("Navigating to properties page for document ID: {DocumentId}", documentId);

        // Navigate to the dedicated properties page
        NavigationManager.NavigateTo($"/documents/properties/{documentId}");
    }

    /// <summary>
    /// Navigates to compose email page for sending document as attachment
    /// </summary>
    private void SendEmailAttach(int documentId)
    {
        Logger.LogInformation("Opening compose email page (attach) for document ID: {DocumentId}", documentId);
        NavigationManager.NavigateTo($"/documents/compose-email?DocumentIds={documentId}&Type=attach");
    }

    /// <summary>
    /// Navigates to compose email page for sending document link
    /// </summary>
    private void SendEmailLink(int documentId)
    {
        Logger.LogInformation("Opening compose email page (link) for document ID: {DocumentId}", documentId);
        NavigationManager.NavigateTo($"/documents/compose-email?DocumentIds={documentId}&Type=link");
    }

    /// <summary>
    /// Shows delete confirmation dialog
    /// </summary>
    private void DeleteDocument(int documentId, int barcode)
    {
        Logger.LogInformation("Delete requested for document ID: {DocumentId}, Barcode: {Barcode}", documentId, barcode);

        deleteDocumentId = documentId;
        deleteDocumentIdentifier = $"Barcode: {barcode}";
        showDeleteConfirmationModal = true;
        StateHasChanged();
    }

    /// <summary>
    /// Confirms and executes document deletion
    /// </summary>
    private async Task ConfirmDelete()
    {
        try
        {
            isDeleting = true;
            StateHasChanged();

            Logger.LogInformation("Deleting document ID: {DocumentId}", deleteDocumentId);

            await DocumentService.DeleteAsync(deleteDocumentId);

            Logger.LogInformation("Document {DocumentId} deleted successfully", deleteDocumentId);

            // Remove from selection if it was selected
            selectedDocumentIds.Remove(deleteDocumentId);

            // Close modal
            showDeleteConfirmationModal = false;

            // Re-execute search to refresh results
            await ExecuteSearch();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error deleting document");
            errorMessage = $"Failed to delete document: {ex.Message}";
        }
        finally
        {
            isDeleting = false;
            StateHasChanged();
        }
    }

    /// <summary>
    /// Cancels document deletion
    /// </summary>
    private void CancelDelete()
    {
        showDeleteConfirmationModal = false;
        deleteDocumentId = 0;
        deleteDocumentIdentifier = null;
        StateHasChanged();
    }

    // ========== Bulk Action Methods ==========

    /// <summary>
    /// Shows bulk delete confirmation dialog
    /// </summary>
    private void DeleteSelected()
    {
        Logger.LogInformation("Bulk delete requested for {Count} documents", selectedDocumentIds.Count);

        if (!selectedDocumentIds.Any()) return;

        var selectedDocs = searchResults?.Items
            .Where(d => selectedDocumentIds.Contains(d.Id))
            .ToList();

        bulkDeleteDocumentCount = selectedDocumentIds.Count;
        bulkDeleteIdentifiers = selectedDocs?
            .Select(d => $"Barcode: {d.BarCode} - {d.DocumentName}")
            .ToList();

        bulkDeleteErrorMessage = null;
        bulkDeleteProgress = null;
        showBulkDeleteConfirmationModal = true;
        StateHasChanged();
    }

    /// <summary>
    /// Confirms and executes bulk deletion
    /// </summary>
    private async Task ConfirmBulkDelete()
    {
        try
        {
            isBulkDeleting = true;
            bulkDeleteErrorMessage = null;

            var idsToDelete = selectedDocumentIds.ToList();
            var total = idsToDelete.Count;

            bulkDeleteProgress = new BulkDeleteProgress
            {
                Total = total,
                Completed = 0,
                Failed = 0
            };

            StateHasChanged();

            Logger.LogInformation("Starting bulk delete of {Count} documents", total);

            foreach (var id in idsToDelete)
            {
                try
                {
                    await DocumentService.DeleteAsync(id);
                    bulkDeleteProgress.Completed++;
                    selectedDocumentIds.Remove(id);
                    Logger.LogDebug("Deleted document ID: {Id}", id);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Failed to delete document ID: {Id}", id);
                    bulkDeleteProgress.Failed++;
                }

                StateHasChanged();
            }

            Logger.LogInformation("Bulk delete completed: {Success} succeeded, {Failed} failed",
                bulkDeleteProgress.Completed, bulkDeleteProgress.Failed);

            if (bulkDeleteProgress.Failed > 0)
            {
                bulkDeleteErrorMessage = $"{bulkDeleteProgress.Failed} document(s) could not be deleted.";
            }
            else
            {
                // Close modal if all succeeded
                await Task.Delay(500); // Brief pause to show completion
                showBulkDeleteConfirmationModal = false;

                // Re-execute search to refresh results
                await ExecuteSearch();
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error during bulk delete");
            bulkDeleteErrorMessage = $"Bulk delete failed: {ex.Message}";
        }
        finally
        {
            isBulkDeleting = false;
            StateHasChanged();
        }
    }

    /// <summary>
    /// Cancels bulk deletion
    /// </summary>
    private void CancelBulkDelete()
    {
        showBulkDeleteConfirmationModal = false;
        bulkDeleteDocumentCount = 0;
        bulkDeleteIdentifiers = null;
        bulkDeleteErrorMessage = null;
        bulkDeleteProgress = null;
        StateHasChanged();
    }

    /// <summary>
    /// Sends multiple documents as email attachments
    /// </summary>
    private void BulkEmailAttach()
    {
        Logger.LogInformation("Bulk email (attach) requested for {Count} documents", selectedDocumentIds.Count);

        if (!selectedDocumentIds.Any()) return;

        var documentIds = string.Join(",", selectedDocumentIds);
        NavigationManager.NavigateTo($"/documents/compose-email?DocumentIds={documentIds}&Type=attach");
    }

    /// <summary>
    /// Sends multiple document links via email
    /// </summary>
    private void BulkEmailLink()
    {
        Logger.LogInformation("Bulk email (link) requested for {Count} documents", selectedDocumentIds.Count);

        if (!selectedDocumentIds.Any()) return;

        var documentIds = string.Join(",", selectedDocumentIds);
        NavigationManager.NavigateTo($"/documents/compose-email?DocumentIds={documentIds}&Type=link");
    }

    /// <summary>
    /// Exports selected documents to Excel preview
    /// </summary>
    private void ExportSelectedToExcel()
    {
        Logger.LogInformation("Export selected to Excel requested for {Count} documents", selectedDocumentIds.Count);

        if (!selectedDocumentIds.Any())
        {
            Logger.LogWarning("Cannot export: No documents selected");
            return;
        }

        if (searchResults == null)
        {
            Logger.LogWarning("Cannot export: No search results available");
            return;
        }

        // Get barcodes for selected documents
        var selectedBarcodes = searchResults.Items
            .Where(d => selectedDocumentIds.Contains(d.Id))
            .Select(d => d.BarCode)
            .ToList();

        if (!selectedBarcodes.Any())
        {
            Logger.LogWarning("Cannot export: No matching documents found");
            errorMessage = "Unable to find barcodes for selected documents.";
            StateHasChanged();
            return;
        }

        Logger.LogInformation("Navigating to Excel preview with {Count} selected barcodes", selectedBarcodes.Count);

        // Build query string with selected barcodes
        var selectedIds = string.Join(",", selectedBarcodes);
        NavigationManager.NavigateTo($"/excel-preview?selectedIds={Uri.EscapeDataString(selectedIds)}");
    }

    /// <summary>
    /// Generates and prints a summary report (placeholder)
    /// </summary>
    private void PrintSummary()
    {
        Logger.LogInformation("Print summary requested for {Count} documents", selectedDocumentIds.Count);

        if (!selectedDocumentIds.Any()) return;

        // TODO: Implement actual print summary functionality
        // For now, just show a message
        Logger.LogWarning("Print summary not yet implemented");
        errorMessage = "Print Summary feature is not yet implemented. This will generate a summary report of selected documents.";
        StateHasChanged();
    }

    /// <summary>
    /// Generates and prints a detailed report (placeholder)
    /// </summary>
    private void PrintDetailed()
    {
        Logger.LogInformation("Print detailed requested for {Count} documents", selectedDocumentIds.Count);

        if (!selectedDocumentIds.Any()) return;

        // TODO: Implement actual print detailed functionality
        // For now, just show a message
        Logger.LogWarning("Print detailed not yet implemented");
        errorMessage = "Print Detailed feature is not yet implemented. This will generate a detailed report of selected documents with all properties.";
        StateHasChanged();
    }
}
