using System.Reflection;

namespace ExcelReporting.Models;

/// <summary>
/// Metadata extracted from ExcelExport attributes for a single property
/// </summary>
public class ExcelExportMetadata
{
    /// <summary>
    /// PropertyInfo for reflection-based value retrieval
    /// </summary>
    public PropertyInfo Property { get; set; } = null!;

    /// <summary>
    /// Display name for the column header
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Data type for formatting
    /// </summary>
    public ExcelDataType DataType { get; set; }

    /// <summary>
    /// Format string for the data type
    /// </summary>
    public string Format { get; set; } = string.Empty;

    /// <summary>
    /// Column ordering
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// Whether this property is exportable
    /// </summary>
    public bool IsExportable { get; set; }

    /// <summary>
    /// Gets the formatted value from an instance
    /// </summary>
    /// <param name="instance">Object instance to get value from</param>
    /// <returns>Formatted string representation of the value</returns>
    public string GetFormattedValue(object instance)
    {
        var value = Property.GetValue(instance);
        if (value == null) return string.Empty;

        try
        {
            return DataType switch
            {
                ExcelDataType.Date => value is DateTime dt ? dt.ToString(Format) : value.ToString() ?? string.Empty,
                ExcelDataType.Currency => value is decimal dec ? dec.ToString(Format) :
                                         value is double dbl ? dbl.ToString(Format) :
                                         value is float flt ? flt.ToString(Format) :
                                         value.ToString() ?? string.Empty,
                ExcelDataType.Number => FormatNumber(value),
                ExcelDataType.Percentage => value is decimal pct ? pct.ToString(Format) :
                                           value is double pctDbl ? pctDbl.ToString(Format) :
                                           value.ToString() ?? string.Empty,
                ExcelDataType.Boolean => value is bool b ? (b ? "Yes" : "No") : value.ToString() ?? string.Empty,
                ExcelDataType.Hyperlink => value.ToString() ?? string.Empty,
                _ => value.ToString() ?? string.Empty
            };
        }
        catch
        {
            return value.ToString() ?? string.Empty;
        }
    }

    /// <summary>
    /// Formats numeric values
    /// </summary>
    private string FormatNumber(object value)
    {
        return value switch
        {
            int i => i.ToString(Format),
            long l => l.ToString(Format),
            decimal d => d.ToString(Format),
            double db => db.ToString(Format),
            float f => f.ToString(Format),
            _ => value.ToString() ?? string.Empty
        };
    }

    /// <summary>
    /// Gets the raw value from an instance without formatting
    /// </summary>
    /// <param name="instance">Object instance to get value from</param>
    /// <returns>Raw value</returns>
    public object? GetValue(object instance)
    {
        return Property.GetValue(instance);
    }
}
