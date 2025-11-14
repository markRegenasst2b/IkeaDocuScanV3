using IkeaDocuScan.Shared.DTOs;
using IkeaDocuScan.Shared.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using System.Text;
using System.Text.Json;

namespace IkeaDocuScan_Web.Services;

/// <summary>
/// Service for reading and parsing Serilog JSON log files
/// </summary>
public class LogViewerService : ILogViewerService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<LogViewerService> _logger;
    private readonly IMemoryCache _cache;
    private readonly string _logDirectory;

    public LogViewerService(
        IConfiguration configuration,
        ILogger<LogViewerService> logger,
        IMemoryCache cache)
    {
        _configuration = configuration;
        _logger = logger;
        _cache = cache;

        // Get log directory from Serilog configuration
        var logPath = _configuration["Serilog:WriteTo:1:Args:path"] ?? "C:\\Logs\\IkeaDocuScan\\log-.json";
        _logDirectory = Path.GetDirectoryName(logPath) ?? "C:\\Logs\\IkeaDocuScan";

        // Ensure directory exists
        if (!Directory.Exists(_logDirectory))
        {
            try
            {
                Directory.CreateDirectory(_logDirectory);
                _logger.LogInformation("Created log directory: {LogDirectory}", _logDirectory);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create log directory: {LogDirectory}", _logDirectory);
            }
        }
    }

    public async Task<LogSearchResult> SearchLogsAsync(LogSearchRequest request, CancellationToken cancellationToken = default)
    {
        var allLogs = new List<LogEntryDto>();

        try
        {
            // Determine which log files to read based on date range
            var logFiles = GetLogFilesForDateRange(request.FromDate, request.ToDate);

            foreach (var logFile in logFiles)
            {
                if (cancellationToken.IsCancellationRequested) break;

                var logs = await ReadAndParseLogFileAsync(logFile, cancellationToken);
                allLogs.AddRange(logs);
            }

            // Apply filters
            var filteredLogs = allLogs.AsEnumerable();

            if (!string.IsNullOrEmpty(request.Level))
                filteredLogs = filteredLogs.Where(l => l.Level.Equals(request.Level, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrEmpty(request.Source))
                filteredLogs = filteredLogs.Where(l => l.Source?.Contains(request.Source, StringComparison.OrdinalIgnoreCase) == true);

            if (!string.IsNullOrEmpty(request.SearchText))
                filteredLogs = filteredLogs.Where(l =>
                    l.Message.Contains(request.SearchText, StringComparison.OrdinalIgnoreCase) ||
                    l.Exception?.Contains(request.SearchText, StringComparison.OrdinalIgnoreCase) == true);

            // Apply date filters
            if (request.FromDate.HasValue)
                filteredLogs = filteredLogs.Where(l => l.Timestamp >= request.FromDate.Value);

            if (request.ToDate.HasValue)
                filteredLogs = filteredLogs.Where(l => l.Timestamp <= request.ToDate.Value);

            // Sort
            filteredLogs = request.SortOrder.ToLower() == "asc"
                ? filteredLogs.OrderBy(l => l.Timestamp)
                : filteredLogs.OrderByDescending(l => l.Timestamp);

            var filteredList = filteredLogs.ToList();
            var totalCount = filteredList.Count;

            // Paginate
            var pagedLogs = filteredList
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            return new LogSearchResult
            {
                Logs = pagedLogs,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching logs");
            return new LogSearchResult
            {
                Logs = new List<LogEntryDto>(),
                TotalCount = 0,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }

    public async Task<byte[]> ExportLogsAsync(LogSearchRequest request, string format = "json", CancellationToken cancellationToken = default)
    {
        var result = await SearchLogsAsync(new LogSearchRequest
        {
            FromDate = request.FromDate,
            ToDate = request.ToDate,
            Level = request.Level,
            Source = request.Source,
            SearchText = request.SearchText,
            PageNumber = 1,
            PageSize = 10000 // Export max 10k records
        }, cancellationToken);

        if (format.ToLower() == "csv")
            return ExportToCsv(result.Logs);
        else
            return ExportToJson(result.Logs);
    }

    public async Task<List<string>> GetAvailableLogDatesAsync()
    {
        var dates = new List<string>();

        try
        {
            if (!Directory.Exists(_logDirectory))
                return dates;

            var files = Directory.GetFiles(_logDirectory, "log-*.json");

            foreach (var file in files)
            {
                var fileName = Path.GetFileNameWithoutExtension(file);
                var datePart = fileName.Replace("log-", "");

                if (DateTime.TryParseExact(datePart, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out var date))
                {
                    dates.Add(date.ToString("yyyy-MM-dd"));
                }
            }

            return await Task.FromResult(dates.OrderByDescending(d => d).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting available log dates");
            return dates;
        }
    }

    public async Task<List<string>> GetLogSourcesAsync()
    {
        // Cache log sources for 5 minutes
        return await _cache.GetOrCreateAsync("LogSources", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);

            try
            {
                // Get unique sources from last 7 days of logs
                var request = new LogSearchRequest
                {
                    FromDate = DateTime.Today.AddDays(-7),
                    ToDate = DateTime.Today.AddDays(1),
                    PageNumber = 1,
                    PageSize = int.MaxValue
                };

                var result = await SearchLogsAsync(request);

                return result.Logs
                    .Select(l => l.Source)
                    .Where(s => !string.IsNullOrEmpty(s))
                    .Distinct()
                    .OrderBy(s => s)
                    .ToList()!;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting log sources");
                return new List<string>();
            }
        }) ?? new List<string>();
    }

    public async Task<LogStatisticsDto> GetLogStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default)
    {
        // Cache statistics for 2 minutes
        var cacheKey = $"LogStats_{fromDate:yyyyMMdd}_{toDate:yyyyMMdd}";

        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2);

            try
            {
                var request = new LogSearchRequest
                {
                    FromDate = fromDate ?? DateTime.Today.AddDays(-1),
                    ToDate = toDate ?? DateTime.Today.AddDays(1),
                    PageNumber = 1,
                    PageSize = int.MaxValue
                };

                var result = await SearchLogsAsync(request, cancellationToken);

                var stats = new LogStatisticsDto
                {
                    ErrorCount = result.Logs.Count(l => l.Level.Equals("Error", StringComparison.OrdinalIgnoreCase) || l.Level.Equals("Fatal", StringComparison.OrdinalIgnoreCase)),
                    WarningCount = result.Logs.Count(l => l.Level.Equals("Warning", StringComparison.OrdinalIgnoreCase)),
                    InformationCount = result.Logs.Count(l => l.Level.Equals("Information", StringComparison.OrdinalIgnoreCase)),
                    TotalCount = result.Logs.Count,
                    TopSources = result.Logs
                        .Where(l => !string.IsNullOrEmpty(l.Source))
                        .GroupBy(l => l.Source)
                        .Select(g => new SourceStatistic { Source = g.Key!, Count = g.Count() })
                        .OrderByDescending(s => s.Count)
                        .Take(10)
                        .ToList(),
                    ErrorTrend = result.Logs
                        .Where(l => l.Level.Equals("Error", StringComparison.OrdinalIgnoreCase) ||
                                   l.Level.Equals("Fatal", StringComparison.OrdinalIgnoreCase) ||
                                   l.Level.Equals("Warning", StringComparison.OrdinalIgnoreCase))
                        .GroupBy(l => l.Timestamp.Date)
                        .Select(g => new DailyErrorCount
                        {
                            Date = g.Key,
                            ErrorCount = g.Count(l => l.Level.Equals("Error", StringComparison.OrdinalIgnoreCase) || l.Level.Equals("Fatal", StringComparison.OrdinalIgnoreCase)),
                            WarningCount = g.Count(l => l.Level.Equals("Warning", StringComparison.OrdinalIgnoreCase))
                        })
                        .OrderBy(d => d.Date)
                        .ToList()
                };

                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting log statistics");
                return new LogStatisticsDto();
            }
        }) ?? new LogStatisticsDto();
    }

    #region Private Helper Methods

    private List<string> GetLogFilesForDateRange(DateTime? fromDate, DateTime? toDate)
    {
        var from = fromDate ?? DateTime.Today.AddDays(-7);
        var to = toDate ?? DateTime.Today;

        var logFiles = new List<string>();

        for (var date = from.Date; date <= to.Date; date = date.AddDays(1))
        {
            var fileName = $"log-{date:yyyyMMdd}.json";
            var filePath = Path.Combine(_logDirectory, fileName);

            if (File.Exists(filePath))
                logFiles.Add(filePath);
        }

        return logFiles;
    }

    private async Task<List<LogEntryDto>> ReadAndParseLogFileAsync(string filePath, CancellationToken cancellationToken)
    {
        var logs = new List<LogEntryDto>();

        try
        {
            using var fileStream = new FileStream(
                filePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite); // Allow reading while Serilog is writing

            using var reader = new StreamReader(fileStream);

            while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
            {
                var line = await reader.ReadLineAsync(cancellationToken);
                if (string.IsNullOrWhiteSpace(line)) continue;

                try
                {
                    var logEntry = ParseCompactJsonLogEntry(line);
                    if (logEntry != null)
                        logs.Add(logEntry);
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning(ex, "Failed to parse log line: {Line}", line);
                }
            }
        }
        catch (IOException ex)
        {
            _logger.LogError(ex, "Error reading log file: {FilePath}", filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error reading log file: {FilePath}", filePath);
        }

        return logs;
    }

    private LogEntryDto? ParseCompactJsonLogEntry(string jsonLine)
    {
        try
        {
            using var doc = JsonDocument.Parse(jsonLine);
            var root = doc.RootElement;

            return new LogEntryDto
            {
                Timestamp = root.TryGetProperty("@t", out var t) ? t.GetDateTime() : DateTime.MinValue,
                Level = root.TryGetProperty("@l", out var level) ? level.GetString() ?? "Information" : "Information",
                Message = root.TryGetProperty("@mt", out var msg) ? msg.GetString() ?? "" :
                         root.TryGetProperty("@m", out var m) ? m.GetString() ?? "" : "",
                Exception = root.TryGetProperty("@x", out var ex) ? ex.GetString() : null,
                Source = root.TryGetProperty("SourceContext", out var src) ? src.GetString() : null,
                User = root.TryGetProperty("User", out var user) ? user.GetString() : null,
                RequestId = root.TryGetProperty("RequestId", out var reqId) ? reqId.GetString() : null,
                Properties = ExtractProperties(root)
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse log entry");
            return null;
        }
    }

    private Dictionary<string, object>? ExtractProperties(JsonElement root)
    {
        var props = new Dictionary<string, object>();

        foreach (var property in root.EnumerateObject())
        {
            // Skip standard Serilog compact JSON properties
            if (property.Name.StartsWith("@") ||
                property.Name == "SourceContext" ||
                property.Name == "User" ||
                property.Name == "RequestId" ||
                property.Name == "MachineName" ||
                property.Name == "ThreadId")
                continue;

            props[property.Name] = property.Value.ToString();
        }

        return props.Count > 0 ? props : null;
    }

    private byte[] ExportToJson(List<LogEntryDto> logs)
    {
        var json = JsonSerializer.Serialize(logs, new JsonSerializerOptions { WriteIndented = true });
        return Encoding.UTF8.GetBytes(json);
    }

    private byte[] ExportToCsv(List<LogEntryDto> logs)
    {
        var csv = new StringBuilder();
        csv.AppendLine("Timestamp,Level,Source,User,Message,Exception");

        foreach (var log in logs)
        {
            csv.AppendLine($"\"{log.Timestamp:yyyy-MM-dd HH:mm:ss}\",\"{log.Level}\",\"{log.Source}\",\"{log.User}\",\"{EscapeCsv(log.Message)}\",\"{EscapeCsv(log.Exception)}\"");
        }

        return Encoding.UTF8.GetBytes(csv.ToString());
    }

    private string EscapeCsv(string? value)
    {
        if (string.IsNullOrEmpty(value)) return "";
        return value.Replace("\"", "\"\"");
    }

    #endregion
}
