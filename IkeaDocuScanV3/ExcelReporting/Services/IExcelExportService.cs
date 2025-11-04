using ExcelReporting.Models;

namespace ExcelReporting.Services;

/// <summary>
/// Service interface for Excel generation
/// </summary>
public interface IExcelExportService
{
    /// <summary>
    /// Generates an Excel file from a collection of DTOs
    /// </summary>
    /// <typeparam name="T">DTO type inheriting from ExportableBase</typeparam>
    /// <param name="data">Collection of data to export</param>
    /// <param name="options">Optional configuration for export</param>
    /// <returns>Memory stream containing the Excel file</returns>
    Task<MemoryStream> GenerateExcelAsync<T>(
        IEnumerable<T> data,
        ExcelExportOptions? options = null)
        where T : ExportableBase;

    /// <summary>
    /// Gets metadata for a DTO type without generating Excel
    /// </summary>
    /// <typeparam name="T">DTO type inheriting from ExportableBase</typeparam>
    /// <returns>List of property metadata</returns>
    List<ExcelExportMetadata> GetMetadata<T>() where T : ExportableBase;

    /// <summary>
    /// Gets metadata for a DTO type by Type object
    /// </summary>
    /// <param name="type">DTO type to get metadata for</param>
    /// <returns>List of property metadata</returns>
    List<ExcelExportMetadata> GetMetadata(Type type);

    /// <summary>
    /// Validates that the data collection is within acceptable size limits
    /// </summary>
    /// <typeparam name="T">DTO type inheriting from ExportableBase</typeparam>
    /// <param name="data">Collection to validate</param>
    /// <param name="options">Export options containing row limits</param>
    /// <returns>Validation result with messages</returns>
    ExcelExportValidationResult ValidateDataSize<T>(
        IEnumerable<T> data,
        ExcelExportOptions options) where T : ExportableBase;
}

/// <summary>
/// Result of data size validation
/// </summary>
public class ExcelExportValidationResult
{
    /// <summary>
    /// Whether the export can proceed
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// Whether a warning should be shown to the user
    /// </summary>
    public bool HasWarning { get; set; }

    /// <summary>
    /// Validation or warning message
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Number of rows to be exported
    /// </summary>
    public int RowCount { get; set; }
}
