using IkeaDocuScan.Shared.DTOs;

namespace IkeaDocuScan.Shared.Interfaces;

/// <summary>
/// Service for viewing and searching application logs
/// </summary>
public interface ILogViewerService
{
    /// <summary>
    /// Search logs with filters and pagination
    /// </summary>
    Task<LogSearchResult> SearchLogsAsync(LogSearchRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Export logs matching the search criteria to JSON or CSV format
    /// </summary>
    Task<byte[]> ExportLogsAsync(LogSearchRequest request, string format = "json", CancellationToken cancellationToken = default);

    /// <summary>
    /// Get list of dates that have available log files
    /// </summary>
    Task<List<string>> GetAvailableLogDatesAsync();

    /// <summary>
    /// Get list of log sources (SourceContext) from recent logs
    /// </summary>
    Task<List<string>> GetLogSourcesAsync();

    /// <summary>
    /// Get log statistics for the dashboard
    /// </summary>
    Task<LogStatisticsDto> GetLogStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default);
}
