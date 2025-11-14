namespace IkeaDocuScan.Shared.DTOs;

/// <summary>
/// Represents a single log entry
/// </summary>
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

/// <summary>
/// Request parameters for searching logs
/// </summary>
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

/// <summary>
/// Paginated log search results
/// </summary>
public class LogSearchResult
{
    public List<LogEntryDto> Logs { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}
