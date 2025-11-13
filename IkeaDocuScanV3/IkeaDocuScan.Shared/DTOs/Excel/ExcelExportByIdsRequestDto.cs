namespace IkeaDocuScan.Shared.DTOs.Excel;

/// <summary>
/// Request DTO for exporting documents to Excel by document IDs
/// </summary>
public class ExcelExportByIdsRequestDto
{
    /// <summary>
    /// List of document IDs to export
    /// </summary>
    public List<int> DocumentIds { get; set; } = new();

    /// <summary>
    /// Optional title for the Excel sheet
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Optional context information to include in the export
    /// </summary>
    public Dictionary<string, string>? Context { get; set; }
}
