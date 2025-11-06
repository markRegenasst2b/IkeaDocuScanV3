using ExcelReporting.Models;

namespace IkeaDocuScan_Web.Client.Services;

/// <summary>
/// Service for passing data to the ExcelPreview page using in-memory state
/// This avoids the need for complex query parameters and allows passing arbitrary DTO collections
/// </summary>
public class ExcelPreviewDataService
{
    private List<ExportableBase>? _data;
    private Dictionary<string, string>? _context;
    private string? _title;

    /// <summary>
    /// Sets the data to be previewed, along with optional context information
    /// </summary>
    /// <param name="data">Collection of DTOs derived from ExportableBase</param>
    /// <param name="title">Optional title for the preview page</param>
    /// <param name="context">Optional context information to display (filters, parameters, etc.)</param>
    public void SetData(IEnumerable<ExportableBase> data, string? title = null, Dictionary<string, string>? context = null)
    {
        _data = data.ToList();
        _title = title;
        _context = context;
    }

    /// <summary>
    /// Gets the data and clears the internal state to prevent stale data
    /// </summary>
    /// <returns>Tuple containing data, title, and context</returns>
    public (List<ExportableBase>? Data, string? Title, Dictionary<string, string>? Context) GetData()
    {
        var result = (_data, _title, _context);

        // Clear after retrieval to prevent stale data on page refresh or back navigation
        Clear();

        return result;
    }

    /// <summary>
    /// Checks if data is available without clearing it
    /// </summary>
    /// <returns>True if data is available, false otherwise</returns>
    public bool HasData() => _data != null && _data.Any();

    /// <summary>
    /// Clears all stored data
    /// </summary>
    public void Clear()
    {
        _data = null;
        _title = null;
        _context = null;
    }
}
