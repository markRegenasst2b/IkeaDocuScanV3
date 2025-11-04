namespace ExcelReporting.Models;

/// <summary>
/// Defines supported data types for Excel export with corresponding formatting rules
/// </summary>
public enum ExcelDataType
{
    /// <summary>
    /// General text data
    /// </summary>
    String,

    /// <summary>
    /// Numeric values (int, decimal, double)
    /// </summary>
    Number,

    /// <summary>
    /// DateTime values
    /// </summary>
    Date,

    /// <summary>
    /// Monetary values with currency formatting
    /// </summary>
    Currency,

    /// <summary>
    /// Percentage values (0.0 - 1.0 range)
    /// </summary>
    Percentage,

    /// <summary>
    /// Boolean values (True/False, Yes/No)
    /// </summary>
    Boolean,

    /// <summary>
    /// URLs or clickable hyperlinks
    /// </summary>
    Hyperlink
}
