namespace IkeaDocuScan.Shared.DTOs.Excel;

/// <summary>
/// DTO for Excel column metadata (API serialization-safe)
/// </summary>
public class ExcelColumnMetadataDto
{
    /// <summary>
    /// Property name
    /// </summary>
    public string PropertyName { get; set; } = string.Empty;

    /// <summary>
    /// Display name for the column header
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Data type for formatting
    /// </summary>
    public string DataType { get; set; } = string.Empty;

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
}
