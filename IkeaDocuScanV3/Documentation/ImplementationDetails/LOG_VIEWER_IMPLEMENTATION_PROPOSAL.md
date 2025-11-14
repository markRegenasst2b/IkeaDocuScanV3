# Log Viewer Implementation Proposal

**Version:** 1.0
**Date:** 2025-11-14
**Target Framework:** .NET 10.0
**Proposed For:** IkeaDocuScan V3

---

## Table of Contents

1. [Current State Analysis](#current-state-analysis)
2. [Proposed Architecture](#proposed-architecture)
3. [Implementation Plan](#implementation-plan)
4. [Technical Specification](#technical-specification)
5. [GUI Design](#gui-design)
6. [Security Considerations](#security-considerations)
7. [Performance Optimization](#performance-optimization)
8. [Alternative Approaches](#alternative-approaches)

---

## Current State Analysis

### Existing Logging Configuration

**Current Setup:**
- Uses `Microsoft.Extensions.Logging` (standard ASP.NET Core)
- Configured in `appsettings.json`:
  ```json
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  }
  ```
- Application runs in **IIS** on Windows Server
- No structured logging framework (Serilog/NLog) currently installed

### Current Log Destinations

When running in IIS, logs likely go to:

1. **stdout** - Captured by ASP.NET Core Module
   - Location: `C:\inetpub\wwwroot\IkeaDocuScan\logs\` (if enabled in web.config)
   - Format: Plain text console output
   - Retention: Manual cleanup required

2. **Windows Event Log** (if configured)
   - Log Name: Application
   - Source: IkeaDocuScan or ASP.NET Core
   - Limited querying capabilities

3. **Debug Output** (Development only)
   - Not available in production

### Limitations of Current Approach

❌ **No Centralized Access** - Logs scattered across stdout files and Event Log
❌ **No Filtering/Search** - Difficult to find specific log entries
❌ **No GUI Access** - SuperUser must RDP to server to view logs
❌ **No Structured Data** - Plain text logs hard to parse
❌ **No Retention Policy** - Manual log file management
❌ **Limited Context** - Missing correlation IDs, user context in some logs

---

## Proposed Architecture

### Recommended Approach: **Serilog with Rolling File Logging + GUI Viewer**

#### Why Serilog?

✅ **Rich Structured Logging** - JSON format for easy parsing
✅ **Rolling File Sink** - Automatic file rotation and retention
✅ **Multiple Sinks** - Console + File + Event Log simultaneously
✅ **Enrichers** - Add context (user, request ID, environment)
✅ **Performance** - Asynchronous logging, minimal overhead
✅ **Mature & Reliable** - Industry standard for .NET logging
✅ **Easy Integration** - Drop-in replacement for Microsoft.Extensions.Logging

### Architecture Components

```
┌─────────────────────────────────────────────────────────────┐
│                    Application Layer                         │
│  (Services, Controllers, Endpoints use ILogger<T>)           │
└──────────────────────┬──────────────────────────────────────┘
                       │
                       ▼
┌─────────────────────────────────────────────────────────────┐
│                  Serilog Pipeline                            │
│  ┌─────────────┐  ┌─────────────┐  ┌──────────────┐        │
│  │  Enrichers  │→ │   Filters   │→ │    Sinks     │        │
│  │ • User      │  │ • Min Level │  │ • File       │        │
│  │ • RequestId │  │ • Namespace │  │ • Console    │        │
│  │ • Machine   │  │             │  │ • Event Log  │        │
│  └─────────────┘  └─────────────┘  └──────────────┘        │
└──────────────────────┬──────────────────────────────────────┘
                       │
                       ▼
┌─────────────────────────────────────────────────────────────┐
│              Log Storage (File System)                       │
│  C:\Logs\IkeaDocuScan\                                       │
│  ├── log-20251114.json         (Today's log)                │
│  ├── log-20251113.json         (Yesterday)                  │
│  ├── log-20251112.json         (2 days ago)                 │
│  └── ...                        (Retention: 30 days)        │
└──────────────────────┬──────────────────────────────────────┘
                       │
                       ▼
┌─────────────────────────────────────────────────────────────┐
│              LogViewerService                                │
│  • Reads log files from disk                                │
│  • Parses JSON structured logs                              │
│  • Filters by level, date, source, message                  │
│  • Paginates results                                         │
│  • Returns LogEntryDto objects                              │
└──────────────────────┬──────────────────────────────────────┘
                       │
                       ▼
┌─────────────────────────────────────────────────────────────┐
│              LogViewerEndpoints (API)                        │
│  GET /api/logs?level=Error&from=2025-11-14&pageSize=50      │
│  • Secured with [Authorize("SuperUser")]                    │
│  • Returns paginated log entries                            │
└──────────────────────┬──────────────────────────────────────┘
                       │
                       ▼
┌─────────────────────────────────────────────────────────────┐
│              LogViewer.razor (GUI)                           │
│  SuperUser-only page for viewing and filtering logs         │
│  • Filter by level, date range, source                      │
│  • Search by message text                                   │
│  • Pagination controls                                      │
│  • Download filtered results                                │
│  • Color-coded by severity                                  │
└─────────────────────────────────────────────────────────────┘
```

---

## Implementation Plan

### Phase 1: Add Serilog (Backend)

**Estimated Time:** 4 hours

**Tasks:**

1. **Install NuGet Packages**
   ```bash
   dotnet add package Serilog.AspNetCore
   dotnet add package Serilog.Sinks.File
   dotnet add package Serilog.Sinks.Console
   dotnet add package Serilog.Enrichers.Environment
   dotnet add package Serilog.Enrichers.Thread
   dotnet add package Serilog.Settings.Configuration
   ```

2. **Configure Serilog in Program.cs**
   ```csharp
   using Serilog;

   var builder = WebApplication.CreateBuilder(args);

   // Configure Serilog
   Log.Logger = new LoggerConfiguration()
       .ReadFrom.Configuration(builder.Configuration)
       .Enrich.FromLogContext()
       .Enrich.WithMachineName()
       .Enrich.WithThreadId()
       .CreateLogger();

   builder.Host.UseSerilog();
   ```

3. **Configure in appsettings.json**
   ```json
   {
     "Serilog": {
       "Using": ["Serilog.Sinks.File", "Serilog.Sinks.Console"],
       "MinimumLevel": {
         "Default": "Information",
         "Override": {
           "Microsoft.AspNetCore": "Warning",
           "Microsoft.EntityFrameworkCore": "Information"
         }
       },
       "WriteTo": [
         {
           "Name": "Console",
           "Args": {
             "outputTemplate": "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"
           }
         },
         {
           "Name": "File",
           "Args": {
             "path": "C:\\Logs\\IkeaDocuScan\\log-.json",
             "rollingInterval": "Day",
             "retainedFileCountLimit": 30,
             "formatter": "Serilog.Formatting.Compact.CompactJsonFormatter, Serilog.Formatting.Compact",
             "shared": true,
             "fileSizeLimitBytes": 104857600,
             "rollOnFileSizeLimit": true
           }
         }
       ],
       "Enrich": ["FromLogContext", "WithMachineName", "WithThreadId"]
     }
   }
   ```

4. **Add User Context Enricher**
   ```csharp
   // Custom enricher to add current user to log context
   public class UserEnricher : ILogEventEnricher
   {
       private readonly IHttpContextAccessor _httpContextAccessor;

       public UserEnricher(IHttpContextAccessor httpContextAccessor)
       {
           _httpContextAccessor = httpContextAccessor;
       }

       public void Enrich(LogEvent logEvent, ILogEventPropertyFactory factory)
       {
           var user = _httpContextAccessor.HttpContext?.User?.Identity?.Name;
           if (!string.IsNullOrEmpty(user))
           {
               logEvent.AddPropertyIfAbsent(factory.CreateProperty("User", user));
           }
       }
   }
   ```

### Phase 2: Create Log Viewer Service (Backend)

**Estimated Time:** 6 hours

**Files to Create:**

1. **IkeaDocuScan.Shared/DTOs/LogEntryDto.cs**
   ```csharp
   public class LogEntryDto
   {
       public DateTime Timestamp { get; set; }
       public string Level { get; set; } = string.Empty;
       public string Message { get; set; } = string.Empty;
       public string? Exception { get; set; }
       public string? Source { get; set; }
       public string? User { get; set; }
       public string? RequestId { get; set; }
       public Dictionary<string, object>? Properties { get; set; }
   }

   public class LogSearchRequest
   {
       public DateTime? FromDate { get; set; }
       public DateTime? ToDate { get; set; }
       public string? Level { get; set; }
       public string? Source { get; set; }
       public string? SearchText { get; set; }
       public int PageNumber { get; set; } = 1;
       public int PageSize { get; set; } = 50;
       public string SortOrder { get; set; } = "desc"; // asc or desc
   }

   public class LogSearchResult
   {
       public List<LogEntryDto> Logs { get; set; } = new();
       public int TotalCount { get; set; }
       public int PageNumber { get; set; }
       public int PageSize { get; set; }
       public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
   }
   ```

2. **IkeaDocuScan.Shared/Interfaces/ILogViewerService.cs**
   ```csharp
   public interface ILogViewerService
   {
       Task<LogSearchResult> SearchLogsAsync(LogSearchRequest request, CancellationToken cancellationToken = default);
       Task<byte[]> ExportLogsAsync(LogSearchRequest request, string format = "json", CancellationToken cancellationToken = default);
       Task<List<string>> GetAvailableLogDatesAsync();
       Task<List<string>> GetLogSourcesAsync();
   }
   ```

3. **IkeaDocuScan-Web/Services/LogViewerService.cs**
   ```csharp
   public class LogViewerService : ILogViewerService
   {
       private readonly IConfiguration _configuration;
       private readonly ILogger<LogViewerService> _logger;
       private readonly string _logDirectory;

       public LogViewerService(IConfiguration configuration, ILogger<LogViewerService> logger)
       {
           _configuration = configuration;
           _logger = logger;
           _logDirectory = _configuration["Serilog:WriteTo:1:Args:path"]?.Replace("log-.json", "")
                           ?? @"C:\Logs\IkeaDocuScan\";
       }

       public async Task<LogSearchResult> SearchLogsAsync(LogSearchRequest request, CancellationToken cancellationToken = default)
       {
           var allLogs = new List<LogEntryDto>();

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
               filteredLogs = filteredLogs.Where(l => l.Message.Contains(request.SearchText, StringComparison.OrdinalIgnoreCase) ||
                                                       l.Exception?.Contains(request.SearchText, StringComparison.OrdinalIgnoreCase) == true);

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
               using var reader = new StreamReader(filePath, new FileStreamOptions
               {
                   Mode = FileMode.Open,
                   Access = FileAccess.Read,
                   Share = FileShare.ReadWrite // Allow reading while Serilog is writing
               });

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

           return logs;
       }

       private LogEntryDto? ParseCompactJsonLogEntry(string jsonLine)
       {
           using var doc = JsonDocument.Parse(jsonLine);
           var root = doc.RootElement;

           return new LogEntryDto
           {
               Timestamp = root.GetProperty("@t").GetDateTime(),
               Level = root.TryGetProperty("@l", out var level) ? level.GetString() ?? "Information" : "Information",
               Message = root.TryGetProperty("@mt", out var msg) ? msg.GetString() ?? "" : root.TryGetProperty("@m", out var m) ? m.GetString() ?? "" : "",
               Exception = root.TryGetProperty("@x", out var ex) ? ex.GetString() : null,
               Source = root.TryGetProperty("SourceContext", out var src) ? src.GetString() : null,
               User = root.TryGetProperty("User", out var user) ? user.GetString() : null,
               RequestId = root.TryGetProperty("RequestId", out var reqId) ? reqId.GetString() : null,
               Properties = ExtractProperties(root)
           };
       }

       private Dictionary<string, object>? ExtractProperties(JsonElement root)
       {
           var props = new Dictionary<string, object>();

           foreach (var property in root.EnumerateObject())
           {
               // Skip standard Serilog compact JSON properties
               if (property.Name.StartsWith("@") || property.Name == "SourceContext" || property.Name == "User" || property.Name == "RequestId")
                   continue;

               props[property.Name] = property.Value.ToString();
           }

           return props.Count > 0 ? props : null;
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

       public async Task<List<string>> GetAvailableLogDatesAsync()
       {
           var dates = new List<string>();

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

       public async Task<List<string>> GetLogSourcesAsync()
       {
           // Get unique sources from last 7 days of logs
           var request = new LogSearchRequest
           {
               FromDate = DateTime.Today.AddDays(-7),
               ToDate = DateTime.Today,
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
   }
   ```

### Phase 3: Create API Endpoints

**Estimated Time:** 2 hours

**File:** `IkeaDocuScan-Web/Endpoints/LogViewerEndpoints.cs`

```csharp
public static class LogViewerEndpoints
{
    public static void MapLogViewerEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/logs")
            .RequireAuthorization("SuperUser") // Only SuperUser can access
            .WithTags("LogViewer");

        // Search logs
        group.MapPost("/search", async (
            LogSearchRequest request,
            ILogViewerService logService,
            CancellationToken cancellationToken) =>
        {
            var result = await logService.SearchLogsAsync(request, cancellationToken);
            return Results.Ok(result);
        })
        .WithName("SearchLogs")
        .WithOpenApi();

        // Export logs
        group.MapPost("/export", async (
            LogSearchRequest request,
            [FromQuery] string format,
            ILogViewerService logService,
            CancellationToken cancellationToken) =>
        {
            var data = await logService.ExportLogsAsync(request, format, cancellationToken);
            var contentType = format.ToLower() == "csv" ? "text/csv" : "application/json";
            var fileName = $"logs-{DateTime.Now:yyyyMMddHHmmss}.{format}";

            return Results.File(data, contentType, fileName);
        })
        .WithName("ExportLogs")
        .WithOpenApi();

        // Get available log dates
        group.MapGet("/dates", async (ILogViewerService logService) =>
        {
            var dates = await logService.GetAvailableLogDatesAsync();
            return Results.Ok(dates);
        })
        .WithName("GetLogDates")
        .WithOpenApi();

        // Get log sources
        group.MapGet("/sources", async (ILogViewerService logService) =>
        {
            var sources = await logService.GetLogSourcesAsync();
            return Results.Ok(sources);
        })
        .WithName("GetLogSources")
        .WithOpenApi();
    }
}
```

Register in `Program.cs`:
```csharp
app.MapLogViewerEndpoints();
```

### Phase 4: Create GUI Log Viewer

**Estimated Time:** 8 hours

**File:** `IkeaDocuScan-Web.Client/Pages/LogViewer.razor`

```razor
@page "/admin/logs"
@using IkeaDocuScan.Shared.DTOs
@inject HttpClient Http
@inject NavigationManager Navigation
@attribute [Authorize(Policy = "SuperUser")]
@rendermode @(new InteractiveWebAssemblyRenderMode(prerender: false))

<PageTitle>System Logs - IkeaDocuScan</PageTitle>

<div class="log-viewer-container">
    <h2>
        <i class="fas fa-file-alt"></i> System Logs
    </h2>

    <div class="filters-panel card">
        <div class="card-body">
            <div class="row g-3">
                <!-- Date Range -->
                <div class="col-md-3">
                    <label class="form-label">From Date</label>
                    <input type="date" class="form-control" @bind="fromDate" @bind:event="oninput" />
                </div>
                <div class="col-md-3">
                    <label class="form-label">To Date</label>
                    <input type="date" class="form-control" @bind="toDate" @bind:event="oninput" />
                </div>

                <!-- Log Level -->
                <div class="col-md-2">
                    <label class="form-label">Level</label>
                    <select class="form-select" @bind="selectedLevel">
                        <option value="">All</option>
                        <option value="Trace">Trace</option>
                        <option value="Debug">Debug</option>
                        <option value="Information">Information</option>
                        <option value="Warning">Warning</option>
                        <option value="Error">Error</option>
                        <option value="Critical">Critical</option>
                    </select>
                </div>

                <!-- Source -->
                <div class="col-md-2">
                    <label class="form-label">Source</label>
                    <select class="form-select" @bind="selectedSource">
                        <option value="">All</option>
                        @foreach (var source in availableSources)
                        {
                            <option value="@source">@source</option>
                        }
                    </select>
                </div>

                <!-- Page Size -->
                <div class="col-md-2">
                    <label class="form-label">Page Size</label>
                    <select class="form-select" @bind="pageSize" @bind:after="SearchLogs">
                        <option value="25">25</option>
                        <option value="50">50</option>
                        <option value="100">100</option>
                        <option value="500">500</option>
                    </select>
                </div>
            </div>

            <div class="row g-3 mt-2">
                <!-- Search Text -->
                <div class="col-md-8">
                    <label class="form-label">Search Message</label>
                    <input type="text" class="form-control" placeholder="Search in message and exception..."
                           @bind="searchText" @bind:event="oninput" />
                </div>

                <!-- Actions -->
                <div class="col-md-4 d-flex align-items-end gap-2">
                    <button class="btn btn-primary flex-fill" @onclick="SearchLogs" disabled="@isLoading">
                        <i class="fas fa-search"></i> Search
                    </button>
                    <button class="btn btn-secondary" @onclick="ResetFilters" disabled="@isLoading">
                        <i class="fas fa-redo"></i> Reset
                    </button>
                    <div class="btn-group">
                        <button class="btn btn-success dropdown-toggle" data-bs-toggle="dropdown" disabled="@isLoading">
                            <i class="fas fa-download"></i> Export
                        </button>
                        <ul class="dropdown-menu">
                            <li><a class="dropdown-item" @onclick="() => ExportLogs(\"json\")">JSON</a></li>
                            <li><a class="dropdown-item" @onclick="() => ExportLogs(\"csv\")">CSV</a></li>
                        </ul>
                    </div>
                </div>
            </div>
        </div>
    </div>

    @if (isLoading)
    {
        <div class="text-center my-5">
            <div class="spinner-border text-primary" role="status">
                <span class="visually-hidden">Loading...</span>
            </div>
            <p class="mt-2">Loading logs...</p>
        </div>
    }
    else if (searchResult != null)
    {
        <div class="results-summary mt-3">
            <div class="d-flex justify-content-between align-items-center">
                <div>
                    <strong>@searchResult.TotalCount</strong> log entries found
                    (Page @searchResult.PageNumber of @searchResult.TotalPages)
                </div>
                <div>
                    <label class="me-2">Sort:</label>
                    <select class="form-select form-select-sm d-inline-block w-auto" @bind="sortOrder" @bind:after="SearchLogs">
                        <option value="desc">Newest First</option>
                        <option value="asc">Oldest First</option>
                    </select>
                </div>
            </div>
        </div>

        <div class="logs-list mt-3">
            @foreach (var log in searchResult.Logs)
            {
                <div class="log-entry card mb-2 log-level-@log.Level.ToLower()">
                    <div class="card-body">
                        <div class="log-header d-flex justify-content-between align-items-start">
                            <div class="flex-grow-1">
                                <span class="log-timestamp">@log.Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff")</span>
                                <span class="log-level badge bg-@GetLevelColor(log.Level)">@log.Level</span>
                                @if (!string.IsNullOrEmpty(log.Source))
                                {
                                    <span class="log-source text-muted">[@log.Source]</span>
                                }
                                @if (!string.IsNullOrEmpty(log.User))
                                {
                                    <span class="log-user text-info"><i class="fas fa-user"></i> @log.User</span>
                                }
                            </div>
                            @if (!string.IsNullOrEmpty(log.Exception))
                            {
                                <button class="btn btn-sm btn-outline-danger" @onclick="() => ToggleException(log)">
                                    <i class="fas @(expandedExceptions.Contains(log) ? "fa-chevron-up" : "fa-chevron-down")"></i>
                                    Exception
                                </button>
                            }
                        </div>
                        <div class="log-message mt-2">
                            @log.Message
                        </div>
                        @if (expandedExceptions.Contains(log) && !string.IsNullOrEmpty(log.Exception))
                        {
                            <div class="log-exception mt-2">
                                <pre class="bg-light p-2 border rounded">@log.Exception</pre>
                            </div>
                        }
                        @if (log.Properties != null && log.Properties.Any())
                        {
                            <div class="log-properties mt-2">
                                <small class="text-muted">
                                    @foreach (var prop in log.Properties.Take(5))
                                    {
                                        <span class="badge bg-secondary me-1">@prop.Key: @prop.Value</span>
                                    }
                                </small>
                            </div>
                        }
                    </div>
                </div>
            }
        </div>

        @if (searchResult.TotalPages > 1)
        {
            <nav class="mt-4">
                <ul class="pagination justify-content-center">
                    <li class="page-item @(currentPage == 1 ? "disabled" : "")">
                        <button class="page-link" @onclick="() => ChangePage(currentPage - 1)">Previous</button>
                    </li>

                    @for (int i = Math.Max(1, currentPage - 2); i <= Math.Min(searchResult.TotalPages, currentPage + 2); i++)
                    {
                        var page = i;
                        <li class="page-item @(currentPage == page ? "active" : "")">
                            <button class="page-link" @onclick="() => ChangePage(page)">@page</button>
                        </li>
                    }

                    <li class="page-item @(currentPage == searchResult.TotalPages ? "disabled" : "")">
                        <button class="page-link" @onclick="() => ChangePage(currentPage + 1)">Next</button>
                    </li>
                </ul>
            </nav>
        }
    }
    else if (!isLoading)
    {
        <div class="alert alert-info mt-3">
            <i class="fas fa-info-circle"></i> Use the filters above to search logs.
        </div>
    }
</div>

@code {
    private DateTime fromDate = DateTime.Today.AddDays(-7);
    private DateTime toDate = DateTime.Today;
    private string selectedLevel = "";
    private string selectedSource = "";
    private string searchText = "";
    private int pageSize = 50;
    private int currentPage = 1;
    private string sortOrder = "desc";

    private bool isLoading = false;
    private LogSearchResult? searchResult;
    private List<string> availableSources = new();
    private HashSet<LogEntryDto> expandedExceptions = new();

    protected override async Task OnInitializedAsync()
    {
        await LoadAvailableSources();
        await SearchLogs();
    }

    private async Task LoadAvailableSources()
    {
        try
        {
            availableSources = await Http.GetFromJsonAsync<List<string>>("/api/logs/sources") ?? new();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading sources: {ex.Message}");
        }
    }

    private async Task SearchLogs()
    {
        isLoading = true;
        expandedExceptions.Clear();

        try
        {
            var request = new LogSearchRequest
            {
                FromDate = fromDate,
                ToDate = toDate.AddDays(1).AddSeconds(-1), // Include entire day
                Level = string.IsNullOrEmpty(selectedLevel) ? null : selectedLevel,
                Source = string.IsNullOrEmpty(selectedSource) ? null : selectedSource,
                SearchText = string.IsNullOrEmpty(searchText) ? null : searchText,
                PageNumber = currentPage,
                PageSize = pageSize,
                SortOrder = sortOrder
            };

            var response = await Http.PostAsJsonAsync("/api/logs/search", request);
            response.EnsureSuccessStatusCode();
            searchResult = await response.Content.ReadFromJsonAsync<LogSearchResult>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error searching logs: {ex.Message}");
            // TODO: Show error to user
        }
        finally
        {
            isLoading = false;
        }
    }

    private async Task ChangePage(int page)
    {
        if (page < 1 || (searchResult != null && page > searchResult.TotalPages))
            return;

        currentPage = page;
        await SearchLogs();
    }

    private async Task ResetFilters()
    {
        fromDate = DateTime.Today.AddDays(-7);
        toDate = DateTime.Today;
        selectedLevel = "";
        selectedSource = "";
        searchText = "";
        currentPage = 1;
        await SearchLogs();
    }

    private async Task ExportLogs(string format)
    {
        try
        {
            var request = new LogSearchRequest
            {
                FromDate = fromDate,
                ToDate = toDate.AddDays(1).AddSeconds(-1),
                Level = string.IsNullOrEmpty(selectedLevel) ? null : selectedLevel,
                Source = string.IsNullOrEmpty(selectedSource) ? null : selectedSource,
                SearchText = string.IsNullOrEmpty(searchText) ? null : searchText
            };

            var response = await Http.PostAsJsonAsync($"/api/logs/export?format={format}", request);
            response.EnsureSuccessStatusCode();

            // Trigger download (browser will handle it)
            Navigation.NavigateTo($"/api/logs/export?format={format}", forceLoad: true);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error exporting logs: {ex.Message}");
        }
    }

    private void ToggleException(LogEntryDto log)
    {
        if (expandedExceptions.Contains(log))
            expandedExceptions.Remove(log);
        else
            expandedExceptions.Add(log);
    }

    private string GetLevelColor(string level)
    {
        return level.ToLower() switch
        {
            "trace" => "secondary",
            "debug" => "info",
            "information" => "success",
            "warning" => "warning",
            "error" => "danger",
            "critical" => "danger",
            _ => "secondary"
        };
    }
}
```

**CSS:** `IkeaDocuScan-Web.Client/wwwroot/css/logviewer.css`

```css
.log-viewer-container {
    padding: 20px;
    max-width: 1400px;
    margin: 0 auto;
}

.filters-panel {
    background: #f8f9fa;
    border: 1px solid #dee2e6;
}

.log-entry {
    border-left: 4px solid #6c757d;
    transition: box-shadow 0.2s;
}

.log-entry:hover {
    box-shadow: 0 2px 8px rgba(0,0,0,0.1);
}

.log-level-trace { border-left-color: #6c757d; }
.log-level-debug { border-left-color: #0dcaf0; }
.log-level-information { border-left-color: #198754; }
.log-level-warning { border-left-color: #ffc107; }
.log-level-error { border-left-color: #dc3545; }
.log-level-critical { border-left-color: #721c24; background-color: #f8d7da; }

.log-timestamp {
    font-family: 'Courier New', monospace;
    font-size: 0.9em;
    color: #6c757d;
}

.log-level {
    font-size: 0.8em;
    margin-left: 10px;
}

.log-source {
    font-size: 0.85em;
    margin-left: 10px;
}

.log-user {
    font-size: 0.85em;
    margin-left: 10px;
}

.log-message {
    font-size: 0.95em;
    line-height: 1.5;
}

.log-exception pre {
    font-size: 0.85em;
    max-height: 300px;
    overflow-y: auto;
}

.log-properties {
    border-top: 1px solid #dee2e6;
    padding-top: 10px;
}

.results-summary {
    padding: 10px;
    background: #e9ecef;
    border-radius: 4px;
}
```

### Phase 5: Testing & Documentation

**Estimated Time:** 4 hours

**Tasks:**
1. Test log viewer with various filters
2. Test pagination with large result sets
3. Test export functionality
4. Verify SuperUser-only access
5. Load test with large log files
6. Document configuration in deployment guide
7. Add to navigation menu

---

## Technical Specification

### Log File Format

**Serilog Compact JSON Format:**
```json
{"@t":"2025-11-14T10:30:45.1234567Z","@mt":"Document {DocumentId} created by user {User}","@l":"Information","DocumentId":12345,"User":"DOMAIN\\john.doe","SourceContext":"DocumentService","RequestId":"0HN1234567890"}
{"@t":"2025-11-14T10:30:46.7890123Z","@mt":"Failed to send email","@l":"Error","@x":"System.Net.Mail.SmtpException: Unable to connect...","SourceContext":"EmailService"}
```

**Properties:**
- `@t`: Timestamp (ISO 8601)
- `@mt`: Message template
- `@m`: Rendered message (if different from template)
- `@l`: Log level (Trace, Debug, Information, Warning, Error, Critical)
- `@x`: Exception stack trace
- `@i`: Event ID (optional)
- `SourceContext`: Logger category name
- Custom properties: Any additional structured data

### Performance Considerations

**Optimizations:**

1. **File I/O**
   - Use `FileShare.ReadWrite` to read while Serilog writes
   - Stream large files instead of loading entirely into memory
   - Limit maximum file size per query (e.g., max 7 days of logs at once)

2. **Parsing**
   - Parse JSON line-by-line (streaming)
   - Short-circuit filtering (stop early if possible)
   - Limit result set (max 10,000 records per export)

3. **Caching**
   - Cache available dates/sources for 5 minutes
   - No caching of log content (always fresh)

4. **Pagination**
   - Client-side pagination for filtered results
   - Limit page size options (25, 50, 100, 500)

**Estimated Performance:**
- 10MB log file (~100,000 entries): Parse in ~2-3 seconds
- Search with filters: ~1-2 seconds
- Export 10,000 records: ~3-5 seconds

### Security Considerations

**Access Control:**
- ✅ Only SuperUser role can access `/api/logs` endpoints
- ✅ Only SuperUser can view `/admin/logs` page
- ✅ Authorization enforced at API level (not just UI)

**Data Protection:**
- ⚠️ Logs may contain sensitive information:
  - User names
  - Document IDs
  - File paths
  - SQL queries (if EF Core logging enabled)
  - Exception messages with data
- ⚠️ Do NOT log:
  - Passwords
  - API keys
  - Credit card numbers
  - Personally identifiable information (PII)

**Recommendations:**
1. Sanitize logs before logging (use log filters)
2. Implement PII scrubbing in LogViewerService
3. Audit who accesses logs (add to audit trail)
4. Rate limit log queries (prevent abuse)

**Audit Trail Integration:**
```csharp
// In LogViewerEndpoints
await auditTrailService.LogAsync(
    AuditAction.Read,
    $"User viewed logs: {request.FromDate} to {request.ToDate}, Level={request.Level}",
    currentUser.UserId
);
```

---

## GUI Design

### Navigation

**Add to Admin Menu:**
```razor
<div class="nav-section">
    <div class="nav-section-header">Administration</div>
    <NavLink class="nav-link" href="/admin/users">
        <i class="fas fa-users"></i> User Permissions
    </NavLink>
    <NavLink class="nav-link" href="/admin/configuration">
        <i class="fas fa-cog"></i> Configuration
    </NavLink>
    <NavLink class="nav-link" href="/admin/audit">
        <i class="fas fa-history"></i> Audit Trail
    </NavLink>
    <NavLink class="nav-link" href="/admin/logs">
        <i class="fas fa-file-alt"></i> System Logs
    </NavLink>
</div>
```

### Features

**Filter Panel:**
- Date range picker (From/To dates)
- Log level dropdown (All, Trace, Debug, Info, Warning, Error, Critical)
- Source dropdown (populated from recent logs)
- Search text input (searches in message and exception)
- Page size selector (25, 50, 100, 500)

**Results Display:**
- Color-coded by severity
- Timestamp with millisecond precision
- Level badge
- Source category
- User (if available)
- Message
- Expandable exception details
- Custom properties (as badges)

**Actions:**
- Search button (apply filters)
- Reset button (clear all filters)
- Export dropdown (JSON or CSV)
- Pagination controls

**Enhancements (Future):**
- Real-time tail (via SignalR - watch logs live)
- Save filter presets
- Schedule log exports via email
- Log statistics dashboard (error count trends, top sources, etc.)

---

## Performance Optimization

### Recommendations

1. **Log Rotation**
   - Keep 30 days of logs (configurable)
   - Archive older logs to separate storage
   - Compress archived logs

2. **Indexing**
   - For very large deployments, consider:
     - ElasticSearch/OpenSearch for full-text search
     - Seq (commercial Serilog viewer)
     - Database logging with indexed columns

3. **Async Processing**
   - Use Serilog async sink for minimal performance impact
   - Background log parsing (if real-time not required)

4. **Resource Limits**
   - Max 10,000 records per export
   - Max 7 days per query
   - Rate limiting (max 10 queries per minute per user)

---

## Alternative Approaches

### Option B: Database Logging

**Pros:**
- Easy to query with SQL
- Leverages existing EF Core infrastructure
- Better for complex filtering
- Can use full-text search

**Cons:**
- Database overhead (writes on every log)
- Potential performance impact
- Database size growth
- Requires migration and table management

**When to Use:**
- If very advanced querying needed
- If log retention is short (<7 days)
- If database has capacity

### Option C: Third-Party Solutions

**Seq (https://datalust.co/seq):**
- Commercial Serilog log server
- Built-in web UI
- Advanced querying and dashboards
- Signal detection and alerts

**ElasticSearch + Kibana:**
- Enterprise-grade log aggregation
- Powerful search and visualization
- Horizontal scalability

**Application Insights (Azure):**
- Cloud-based logging
- Integration with Azure ecosystem
- Advanced analytics

**When to Use:**
- If budget allows
- If advanced features needed (dashboards, alerts)
- If multiple applications need centralized logging

---

## Recommendation

**For IkeaDocuScan:** Implement **Option A (Serilog + File Logging + GUI Viewer)**

**Rationale:**
✅ No additional infrastructure needed
✅ Minimal performance overhead
✅ Easy to implement and maintain
✅ Meets SuperUser requirements
✅ Can upgrade to Seq/ElasticSearch later if needed
✅ Works offline (doesn't require internet)
✅ Low cost (no licensing fees)

**Total Implementation Time:** ~24 hours (3 days)

**Maintenance:** Minimal (log rotation automatic, GUI is simple CRUD)

---

## Next Steps

1. **Approve Proposal** - Review and approve this design
2. **Install Serilog** - Add NuGet packages
3. **Configure Logging** - Update appsettings.json and Program.cs
4. **Implement Service** - Create LogViewerService
5. **Implement API** - Create LogViewerEndpoints
6. **Implement GUI** - Create LogViewer.razor page
7. **Test** - Comprehensive testing with real logs
8. **Document** - Update deployment and user guides
9. **Deploy** - Roll out to production

---

## Questions for Clarification

1. **Log Retention:** How many days of logs should be kept? (Recommended: 30 days)
2. **Performance:** What's the acceptable query response time? (Target: <3 seconds for typical queries)
3. **Features:** Are any additional features required? (e.g., real-time tail, email alerts)
4. **Access:** Should any other roles have read-only log access? (Currently SuperUser only)
5. **PII:** Are there specific data types that should be scrubbed from logs?

---

**Document Status:** Ready for Review
**Next Review Date:** TBD
