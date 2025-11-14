using IkeaDocuScan.Shared.DTOs;
using IkeaDocuScan.Shared.Interfaces;
using System.Net.Http.Json;

namespace IkeaDocuScan_Web.Client.Services;

/// <summary>
/// Client-side HTTP service for accessing log viewer API
/// </summary>
public class LogViewerHttpService : ILogViewerService
{
    private readonly HttpClient _httpClient;

    public LogViewerHttpService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<LogSearchResult> SearchLogsAsync(LogSearchRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/logs/search", request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<LogSearchResult>(cancellationToken: cancellationToken);
            return result ?? new LogSearchResult();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error searching logs: {ex.Message}");
            return new LogSearchResult();
        }
    }

    public async Task<byte[]> ExportLogsAsync(LogSearchRequest request, string format = "json", CancellationToken cancellationToken = default)
    {
        try
        {
            // Build query string
            var queryParams = new List<string> { $"format={format}" };

            if (request.FromDate.HasValue)
                queryParams.Add($"fromDate={request.FromDate.Value:yyyy-MM-dd}");

            if (request.ToDate.HasValue)
                queryParams.Add($"toDate={request.ToDate.Value:yyyy-MM-dd}");

            if (!string.IsNullOrEmpty(request.Level))
                queryParams.Add($"level={Uri.EscapeDataString(request.Level)}");

            if (!string.IsNullOrEmpty(request.Source))
                queryParams.Add($"source={Uri.EscapeDataString(request.Source)}");

            if (!string.IsNullOrEmpty(request.SearchText))
                queryParams.Add($"searchText={Uri.EscapeDataString(request.SearchText)}");

            var url = $"/api/logs/export?{string.Join("&", queryParams)}";

            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsByteArrayAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error exporting logs: {ex.Message}");
            return Array.Empty<byte>();
        }
    }

    public async Task<List<string>> GetAvailableLogDatesAsync()
    {
        try
        {
            var dates = await _httpClient.GetFromJsonAsync<List<string>>("/api/logs/dates");
            return dates ?? new List<string>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting log dates: {ex.Message}");
            return new List<string>();
        }
    }

    public async Task<List<string>> GetLogSourcesAsync()
    {
        try
        {
            var sources = await _httpClient.GetFromJsonAsync<List<string>>("/api/logs/sources");
            return sources ?? new List<string>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting log sources: {ex.Message}");
            return new List<string>();
        }
    }

    public async Task<LogStatisticsDto> GetLogStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var fromParam = fromDate.HasValue ? $"fromDate={fromDate.Value:yyyy-MM-dd}" : "";
            var toParam = toDate.HasValue ? $"toDate={toDate.Value:yyyy-MM-dd}" : "";
            var separator = !string.IsNullOrEmpty(fromParam) && !string.IsNullOrEmpty(toParam) ? "&" : "";

            var stats = await _httpClient.GetFromJsonAsync<LogStatisticsDto>($"/api/logs/statistics?{fromParam}{separator}{toParam}", cancellationToken);
            return stats ?? new LogStatisticsDto();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting log statistics: {ex.Message}");
            return new LogStatisticsDto();
        }
    }
}
