using Microsoft.AspNetCore.Components;
using IkeaDocuScan.Shared.DTOs.Documents;
using IkeaDocuScan.Shared.DTOs.DocumentTypes;
using IkeaDocuScan.Shared.DTOs.DocumentNames;
using IkeaDocuScan.Shared.DTOs.CounterParties;
using IkeaDocuScan.Shared.DTOs.Currencies;
using IkeaDocuScan.Shared.DTOs.Countries;
using IkeaDocuScan.Shared.Interfaces;

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
}
