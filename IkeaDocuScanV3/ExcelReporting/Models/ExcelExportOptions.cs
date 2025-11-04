namespace ExcelReporting.Models;

/// <summary>
/// Configuration options for Excel export
/// </summary>
public class ExcelExportOptions
{
    /// <summary>
    /// Name of the worksheet
    /// </summary>
    public string SheetName { get; set; } = "Export";

    /// <summary>
    /// Include header row with column names
    /// </summary>
    public bool IncludeHeader { get; set; } = true;

    /// <summary>
    /// Auto-fit column widths based on content
    /// </summary>
    public bool AutoFitColumns { get; set; } = true;

    /// <summary>
    /// Apply formatting to header row (bold, background color, etc.)
    /// </summary>
    public bool ApplyHeaderFormatting { get; set; } = true;

    /// <summary>
    /// Header row background color (hex format)
    /// </summary>
    public string HeaderBackgroundColor { get; set; } = "#4472C4";

    /// <summary>
    /// Header row font color (hex format)
    /// </summary>
    public string HeaderFontColor { get; set; } = "#FFFFFF";

    /// <summary>
    /// Freeze the header row for scrolling
    /// </summary>
    public bool FreezeHeaderRow { get; set; } = true;

    /// <summary>
    /// Enable auto-filters on the header row
    /// </summary>
    public bool EnableFilters { get; set; } = true;

    /// <summary>
    /// Maximum column width in characters (null for no limit)
    /// </summary>
    public int? MaxColumnWidth { get; set; } = 50;

    /// <summary>
    /// Default date format
    /// </summary>
    public string DateFormat { get; set; } = "yyyy-MM-dd";

    /// <summary>
    /// Default currency format
    /// </summary>
    public string CurrencyFormat { get; set; } = "$#,##0.00";

    /// <summary>
    /// Default number format
    /// </summary>
    public string NumberFormat { get; set; } = "#,##0.00";

    /// <summary>
    /// Default percentage format
    /// </summary>
    public string PercentageFormat { get; set; } = "0.00%";

    /// <summary>
    /// Warning row count threshold
    /// </summary>
    public int WarningRowCount { get; set; } = 10000;

    /// <summary>
    /// Maximum allowed row count
    /// </summary>
    public int MaximumRowCount { get; set; } = 50000;
}
