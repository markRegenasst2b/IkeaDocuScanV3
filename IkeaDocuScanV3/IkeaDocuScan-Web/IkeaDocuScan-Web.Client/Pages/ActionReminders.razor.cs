using IkeaDocuScan.Shared.DTOs.ActionReminders;
using IkeaDocuScan.Shared.DTOs.CounterParties;
using IkeaDocuScan.Shared.DTOs.DocumentTypes;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using ExcelReporting.Services;
using ExcelReporting.Models;

namespace IkeaDocuScan_Web.Client.Pages;

public partial class ActionReminders
{
    // State
    private List<ActionReminderDto>? actionReminders;
    private ActionReminderSearchRequestDto searchRequest = new();
    private List<DocumentTypeDto>? documentTypes;
    private int? selectedDocumentTypeId;
    private int totalCount;
    private bool isLoading;
    private bool showFilters = true;
    private string? errorMessage;
    private DateTime lastUpdated;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            isLoading = true;

            // Load filter data
            documentTypes = await DocumentTypeService.GetAllAsync();

            // Initialize with default filters (show today and overdue)
            await SetQuickFilter(QuickFilterType.Today);

            await LoadActionRemindersAsync();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error initializing action reminders page");
            errorMessage = "Failed to load page data. Please refresh the page.";
        }
        finally
        {
            isLoading = false;
        }
    }

    private async Task LoadActionRemindersAsync()
    {
        isLoading = true;
        errorMessage = null;

        try
        {
            // Update search request with selected document type ID
            searchRequest.DocumentTypeIds = selectedDocumentTypeId.HasValue
                ? new List<int> { selectedDocumentTypeId.Value }
                : null;

            Logger.LogInformation("Loading action reminders with filters: {@SearchRequest}", searchRequest);

            actionReminders = await ActionReminderService.GetDueActionsAsync(searchRequest);
            totalCount = actionReminders?.Count ?? 0;
            lastUpdated = DateTime.Now;

            Logger.LogInformation("Loaded {Count} action reminders", totalCount);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading action reminders");
            errorMessage = "Failed to load action reminders. Please try again.";
            actionReminders = new List<ActionReminderDto>();
            totalCount = 0;
        }
        finally
        {
            isLoading = false;
        }
    }

    private async Task ApplyFiltersAsync()
    {
        await LoadActionRemindersAsync();
    }

    private async Task ClearFiltersAsync()
    {
        searchRequest = new ActionReminderSearchRequestDto
        {
            IncludeFutureActions = true
        };
        selectedDocumentTypeId = null;
        await LoadActionRemindersAsync();
    }

    private async Task OnDocumentTypeChanged(ChangeEventArgs e)
    {
        if (e.Value != null && int.TryParse(e.Value.ToString(), out var id))
        {
            selectedDocumentTypeId = id;
        }
        else
        {
            selectedDocumentTypeId = null;
        }
    }

    private async Task RefreshAsync()
    {
        await LoadActionRemindersAsync();
    }

    private void ToggleFilters()
    {
        showFilters = !showFilters;
    }

    private async Task SetQuickFilter(QuickFilterType filterType)
    {
        var today = DateTime.Today;

        switch (filterType)
        {
            case QuickFilterType.Today:
                searchRequest.DateFrom = today;
                searchRequest.DateTo = today;
                searchRequest.IncludeOverdueOnly = false;
                searchRequest.IncludeFutureActions = false;
                break;

            case QuickFilterType.ThisWeek:
                var startOfWeek = today.AddDays(-(int)today.DayOfWeek);
                var endOfWeek = startOfWeek.AddDays(6);
                searchRequest.DateFrom = startOfWeek;
                searchRequest.DateTo = endOfWeek;
                searchRequest.IncludeOverdueOnly = false;
                searchRequest.IncludeFutureActions = true;
                break;

            case QuickFilterType.ThisMonth:
                var startOfMonth = new DateTime(today.Year, today.Month, 1);
                var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);
                searchRequest.DateFrom = startOfMonth;
                searchRequest.DateTo = endOfMonth;
                searchRequest.IncludeOverdueOnly = false;
                searchRequest.IncludeFutureActions = true;
                break;

            case QuickFilterType.Overdue:
                searchRequest.DateFrom = null;
                searchRequest.DateTo = today.AddDays(-1);
                searchRequest.IncludeOverdueOnly = true;
                searchRequest.IncludeFutureActions = false;
                break;
        }

        await LoadActionRemindersAsync();
    }

    private async Task ExportToExcel()
    {
        if (actionReminders == null || !actionReminders.Any())
        {
            errorMessage = "No data to export.";
            return;
        }

        try
        {
            Logger.LogInformation("Exporting {Count} action reminders to Excel", actionReminders.Count);

            // Use the ExcelReporting service directly
            var excelService = new ExcelExportService(new PropertyMetadataExtractor());
            var options = new ExcelExportOptions
            {
                SheetName = "Action Reminders",
                IncludeHeader = true,
                AutoFitColumns = true,
                ApplyHeaderFormatting = true,
                HeaderBackgroundColor = "#0051BA", // IKEA Blue
                HeaderFontColor = "#FFFFFF",
                FreezeHeaderRow = true,
                EnableFilters = true,
                MaxColumnWidth = 50,
                DateFormat = "dd/MM/yyyy"
            };

            var stream = await excelService.GenerateExcelAsync(actionReminders, options);

            // Trigger download
            var fileName = $"ActionReminders_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

            // Use JS Interop to download the file
            var bytes = stream.ToArray();
            var base64 = Convert.ToBase64String(bytes);
            await JSRuntime.InvokeVoidAsync("downloadFileFromBase64", fileName, base64);

            Logger.LogInformation("Excel export completed: {FileName}", fileName);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error exporting to Excel");
            errorMessage = "Failed to export to Excel. Please try again.";
        }
    }

    private string GetRowClass(ActionReminderDto action)
    {
        if (action.ActionDate.HasValue)
        {
            var today = DateTime.Today;
            if (action.ActionDate.Value < today)
                return "table-danger"; // Overdue
            if (action.ActionDate.Value == today)
                return "table-warning"; // Due today
        }
        return string.Empty;
    }

    private string GetDateBadgeClass(DateTime date)
    {
        var today = DateTime.Today;
        if (date < today)
            return "badge bg-danger"; // Overdue
        if (date == today)
            return "badge bg-warning text-dark"; // Due today
        if (date <= today.AddDays(7))
            return "badge bg-info"; // Due this week
        return "badge bg-secondary"; // Future
    }
}
