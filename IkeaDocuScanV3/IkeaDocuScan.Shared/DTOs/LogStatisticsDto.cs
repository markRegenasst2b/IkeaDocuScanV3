namespace IkeaDocuScan.Shared.DTOs;

/// <summary>
/// Log statistics for dashboard display
/// </summary>
public class LogStatisticsDto
{
    public int ErrorCount { get; set; }
    public int WarningCount { get; set; }
    public int InformationCount { get; set; }
    public int TotalCount { get; set; }
    public List<SourceStatistic> TopSources { get; set; } = new();
    public List<DailyErrorCount> ErrorTrend { get; set; } = new();
}

/// <summary>
/// Log count by source
/// </summary>
public class SourceStatistic
{
    public string Source { get; set; } = string.Empty;
    public int Count { get; set; }
}

/// <summary>
/// Daily error count for trend chart
/// </summary>
public class DailyErrorCount
{
    public DateTime Date { get; set; }
    public int ErrorCount { get; set; }
    public int WarningCount { get; set; }
}
