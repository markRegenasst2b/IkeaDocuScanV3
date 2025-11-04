using ExcelReporting.Attributes;

namespace ExcelReporting.Models;

/// <summary>
/// Abstract base class for all exportable DTOs
/// </summary>
public abstract class ExportableBase
{
    /// <summary>
    /// Timestamp when the export was generated (optional)
    /// </summary>
    [ExcelExport("Export Timestamp", ExcelDataType.Date, "yyyy-MM-dd HH:mm:ss", Order = int.MaxValue)]
    public DateTime? ExportedAt { get; set; }

    /// <summary>
    /// Virtual method for custom validation before export
    /// </summary>
    /// <param name="errorMessage">Error message if validation fails</param>
    /// <returns>True if valid for export, false otherwise</returns>
    public virtual bool ValidateForExport(out string? errorMessage)
    {
        errorMessage = null;
        return true;
    }

    /// <summary>
    /// Virtual method for pre-export transformations or calculations
    /// </summary>
    public virtual void PrepareForExport()
    {
        ExportedAt = DateTime.Now;
    }
}
