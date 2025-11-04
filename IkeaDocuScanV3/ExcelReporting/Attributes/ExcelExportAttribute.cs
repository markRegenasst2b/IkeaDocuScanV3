using ExcelReporting.Models;

namespace ExcelReporting.Attributes;

/// <summary>
/// Attribute to decorate DTO properties with export metadata for grid display and Excel export
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class ExcelExportAttribute : Attribute
{
    /// <summary>
    /// Display name for the column header
    /// </summary>
    public string DisplayName { get; }

    /// <summary>
    /// Data type for formatting purposes
    /// </summary>
    public ExcelDataType DataType { get; }

    /// <summary>
    /// Format string for the data type (e.g., "yyyy-MM-dd", "$#,##0.00")
    /// </summary>
    public string Format { get; }

    /// <summary>
    /// Column ordering (lower values appear first)
    /// </summary>
    public int Order { get; set; } = 0;

    /// <summary>
    /// Whether this property should be included in exports
    /// </summary>
    public bool IsExportable { get; set; } = true;

    /// <summary>
    /// Initializes a new instance of the ExcelExportAttribute
    /// </summary>
    /// <param name="displayName">Display name for the column header</param>
    /// <param name="dataType">Data type for formatting</param>
    /// <param name="format">Optional format string (uses default if not specified)</param>
    public ExcelExportAttribute(
        string displayName,
        ExcelDataType dataType = ExcelDataType.String,
        string? format = null)
    {
        DisplayName = displayName;
        DataType = dataType;
        Format = format ?? GetDefaultFormat(dataType);
    }

    /// <summary>
    /// Gets the default format string for a data type
    /// </summary>
    private static string GetDefaultFormat(ExcelDataType dataType)
    {
        return dataType switch
        {
            ExcelDataType.Date => "yyyy-MM-dd",
            ExcelDataType.Currency => "$#,##0.00",
            ExcelDataType.Number => "#,##0.00",
            ExcelDataType.Percentage => "0.00%",
            ExcelDataType.Boolean => "Yes/No",
            ExcelDataType.Hyperlink => "",
            _ => ""
        };
    }
}
