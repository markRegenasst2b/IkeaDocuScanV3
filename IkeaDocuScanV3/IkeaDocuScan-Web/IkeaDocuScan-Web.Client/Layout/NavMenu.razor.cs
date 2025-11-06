using IkeaDocuScan.Shared.Interfaces;
using IkeaDocuScan_Web.Client.Services;
using Microsoft.AspNetCore.Components;

namespace IkeaDocuScan_Web.Client.Layout;

public partial class NavMenu : ComponentBase
{
    [Inject] private ExcelPreviewDataService PreviewDataService { get; set; } = null!;
    [Inject] private IReportService ReportService { get; set; } = null!;
    [Inject] private NavigationManager NavigationManager { get; set; } = null!;

    private bool isLoadingReport = false;
    private string? reportError;

    private async Task ShowBarcodeGapsReport()
    {
        await LoadAndShowReport(
            "barcode-gaps",
            "Barcode Gaps Report",
            async () => await ReportService.GetBarcodeGapsReportAsync()
        );
    }

    private async Task ShowDuplicateDocumentsReport()
    {
        await LoadAndShowReport(
            "duplicate-documents",
            "Duplicate Documents Report",
            async () => await ReportService.GetDuplicateDocumentsReportAsync()
        );
    }

    private async Task ShowUnlinkedRegistrationsReport()
    {
        await LoadAndShowReport(
            "unlinked-registrations",
            "Unlinked Registrations Report",
            async () => await ReportService.GetUnlinkedRegistrationsReportAsync()
        );
    }

    private async Task ShowScanCopiesReport()
    {
        await LoadAndShowReport(
            "scan-copies",
            "Scan Copies Report",
            async () => await ReportService.GetScanCopiesReportAsync()
        );
    }

    private async Task ShowSuppliersReport()
    {
        await LoadAndShowReport(
            "suppliers",
            "Suppliers Report",
            async () => await ReportService.GetSuppliersReportAsync()
        );
    }

    private async Task ShowAllDocumentsReport()
    {
        await LoadAndShowReport(
            "all-documents",
            "All Documents Report",
            async () => await ReportService.GetAllDocumentsReportAsync()
        );
    }

    private async Task LoadAndShowReport<T>(string reportType, string title, Func<Task<List<T>>> loadDataFunc)
        where T : ExcelReporting.Models.ExportableBase
    {
        try
        {
            isLoadingReport = true;
            reportError = null;
            StateHasChanged();

            var reportData = await loadDataFunc();

            if (reportData == null || !reportData.Any())
            {
                reportError = $"No data found for {title}";
                return;
            }

            var context = new Dictionary<string, string>
            {
                ["Report Type"] = title,
                ["Generated"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                ["Record Count"] = reportData.Count.ToString()
            };

            PreviewDataService.SetData(reportData, title, context);
            NavigationManager.NavigateTo($"/excel-preview?source={reportType}");
        }
        catch (NotImplementedException)
        {
            reportError = $"{title} is not yet implemented";
        }
        catch (Exception ex)
        {
            reportError = $"Failed to load {title}: {ex.Message}";
        }
        finally
        {
            isLoadingReport = false;
            StateHasChanged();
        }
    }
}
