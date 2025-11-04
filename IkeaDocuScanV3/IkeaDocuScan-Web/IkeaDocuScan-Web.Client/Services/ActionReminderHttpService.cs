using System.Net.Http.Json;
using System.Text;
using IkeaDocuScan.Shared.DTOs.ActionReminders;
using IkeaDocuScan.Shared.Interfaces;

namespace IkeaDocuScan_Web.Client.Services;

/// <summary>
/// Client-side HTTP service for action reminders
/// </summary>
public class ActionReminderHttpService : IActionReminderService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ActionReminderHttpService> _logger;

    public ActionReminderHttpService(
        HttpClient httpClient,
        ILogger<ActionReminderHttpService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<List<ActionReminderDto>> GetDueActionsAsync(ActionReminderSearchRequestDto? request = null)
    {
        try
        {
            var queryString = BuildQueryString(request);
            var url = $"/api/action-reminders{queryString}";

            _logger.LogInformation("Fetching due actions from {Url}", url);

            var response = await _httpClient.GetFromJsonAsync<List<ActionReminderDto>>(url);

            return response ?? new List<ActionReminderDto>();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error fetching due actions");
            return new List<ActionReminderDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching due actions");
            return new List<ActionReminderDto>();
        }
    }

    public async Task<List<ActionReminderDto>> GetActionsDueOnDateAsync(DateTime date)
    {
        try
        {
            var url = $"/api/action-reminders/date/{date:yyyy-MM-dd}";

            _logger.LogInformation("Fetching actions due on {Date} from {Url}", date, url);

            var response = await _httpClient.GetFromJsonAsync<List<ActionReminderDto>>(url);

            return response ?? new List<ActionReminderDto>();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error fetching actions due on date");
            return new List<ActionReminderDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching actions due on date");
            return new List<ActionReminderDto>();
        }
    }

    public async Task<int> GetDueActionsCountAsync()
    {
        try
        {
            var url = "/api/action-reminders/count";

            _logger.LogInformation("Fetching due actions count from {Url}", url);

            var count = await _httpClient.GetFromJsonAsync<int>(url);

            return count;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error fetching due actions count");
            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching due actions count");
            return 0;
        }
    }

    private string BuildQueryString(ActionReminderSearchRequestDto? request)
    {
        if (request == null)
            return string.Empty;

        var queryParams = new List<string>();

        if (request.DateFrom.HasValue)
            queryParams.Add($"DateFrom={Uri.EscapeDataString(request.DateFrom.Value.ToString("yyyy-MM-dd"))}");

        if (request.DateTo.HasValue)
            queryParams.Add($"DateTo={Uri.EscapeDataString(request.DateTo.Value.ToString("yyyy-MM-dd"))}");

        if (request.DocumentTypeIds != null && request.DocumentTypeIds.Any())
        {
            foreach (var id in request.DocumentTypeIds)
            {
                queryParams.Add($"DocumentTypeIds={id}");
            }
        }

        if (request.CounterPartyIds != null && request.CounterPartyIds.Any())
        {
            foreach (var id in request.CounterPartyIds)
            {
                queryParams.Add($"CounterPartyIds={id}");
            }
        }

        if (!string.IsNullOrWhiteSpace(request.CounterPartySearch))
            queryParams.Add($"CounterPartySearch={Uri.EscapeDataString(request.CounterPartySearch)}");

        if (!string.IsNullOrWhiteSpace(request.SearchString))
            queryParams.Add($"SearchString={Uri.EscapeDataString(request.SearchString)}");

        if (!request.IncludeFutureActions)
            queryParams.Add("IncludeFutureActions=false");

        if (request.IncludeOverdueOnly)
            queryParams.Add("IncludeOverdueOnly=true");

        return queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : string.Empty;
    }
}
