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
    private bool showViewPropertiesModal = false;
    private bool showDeleteConfirmationModal = false;
    private DocumentDto? selectedDocumentForModal;
    private string? deleteDocumentIdentifier;
    private int deleteDocumentId;
    private bool isDeleting = false;

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
        Logger.LogInformation("Opening PDF for document ID: {DocumentId}", documentId);

        // Open in new tab using download endpoint
        var url = $"/api/documents/{documentId}/download";
        NavigationManager.NavigateTo(url, true); // forceLoad = true opens in new context
    }

    /// <summary>
    /// Shows the View Properties modal
    /// </summary>
    private async Task ViewProperties(int documentId)
    {
        Logger.LogInformation("Viewing properties for document ID: {DocumentId}", documentId);

        try
        {
            // Fetch full document details
            selectedDocumentForModal = await DocumentService.GetByIdAsync(documentId);

            if (selectedDocumentForModal != null)
            {
                showViewPropertiesModal = true;
                StateHasChanged();
            }
            else
            {
                Logger.LogWarning("Document {DocumentId} not found", documentId);
                errorMessage = "Document not found.";
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading document properties");
            errorMessage = $"Failed to load document properties: {ex.Message}";
        }
    }

    /// <summary>
    /// Closes the View Properties modal
    /// </summary>
    private void CloseViewPropertiesModal()
    {
        showViewPropertiesModal = false;
        selectedDocumentForModal = null;
        StateHasChanged();
    }

    /// <summary>
    /// Generates mailto: link for sending document as email attachment
    /// </summary>
    private void SendEmailAttach(int documentId)
    {
        Logger.LogInformation("Generating email (attach) link for document ID: {DocumentId}", documentId);

        var doc = searchResults?.Items.FirstOrDefault(d => d.Id == documentId);
        if (doc == null) return;

        var emailConfig = EmailOptions?.Value;
        var recipient = emailConfig?.DefaultRecipient ?? "legal@ikea.com";

        var subjectText = emailConfig?.FormatAttachSubject(1) ?? $"IKEA Document: {doc.BarCode}";
        var bodyText = emailConfig?.FormatAttachBody(1, doc.BarCode.ToString())
            ?? $"Please find attached document with barcode: {doc.BarCode}\n\nDocument Name: {doc.DocumentName}\nCounterparty: {doc.Counterparty}";

        var subject = Uri.EscapeDataString(subjectText);
        var body = Uri.EscapeDataString(bodyText);

        var mailtoLink = $"mailto:{recipient}?subject={subject}&body={body}";
        NavigationManager.NavigateTo(mailtoLink, true);
    }

    /// <summary>
    /// Generates mailto: link for sending document link via email
    /// </summary>
    private void SendEmailLink(int documentId)
    {
        Logger.LogInformation("Generating email (link) link for document ID: {DocumentId}", documentId);

        var doc = searchResults?.Items.FirstOrDefault(d => d.Id == documentId);
        if (doc == null) return;

        var emailConfig = EmailOptions?.Value;
        var recipient = emailConfig?.DefaultRecipient ?? "legal@ikea.com";
        var downloadUrl = $"{NavigationManager.BaseUri}api/documents/{documentId}/download";

        var subjectText = emailConfig?.FormatLinkSubject(1) ?? $"IKEA Document Link: {doc.BarCode}";
        var links = $"Document {doc.BarCode}: {downloadUrl}";
        var bodyText = emailConfig?.FormatLinkBody(1, links)
            ?? $"You can access the document using the following link:\n\n{downloadUrl}\n\nDocument Details:\nBarcode: {doc.BarCode}\nDocument Name: {doc.DocumentName}\nCounterparty: {doc.Counterparty}";

        var subject = Uri.EscapeDataString(subjectText);
        var body = Uri.EscapeDataString(bodyText);

        var mailtoLink = $"mailto:{recipient}?subject={subject}&body={body}";
        NavigationManager.NavigateTo(mailtoLink, true);
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
}
