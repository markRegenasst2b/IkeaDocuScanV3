using ExcelReporting.Attributes;
using ExcelReporting.Models;

namespace IkeaDocuScan.Shared.DTOs.Reports;

/// <summary>
/// DTO for Scan Copies report - scanned files and their status
/// </summary>
public class ScanCopiesReportDto : ExportableBase
{
    [ExcelExport("File Name", ExcelDataType.String, Order = 1)]
    public string FileName { get; set; } = string.Empty;

    [ExcelExport("File Path", ExcelDataType.String, Order = 2)]
    public string? FilePath { get; set; }

    [ExcelExport("File Size (KB)", ExcelDataType.Number, "#,##0.00", Order = 3)]
    public decimal? FileSizeKB { get; set; }

    [ExcelExport("Linked to Document", ExcelDataType.Boolean, Order = 4)]
    public bool IsLinked { get; set; }

    [ExcelExport("Bar Code", ExcelDataType.Number, "#,##0", Order = 5)]
    public int? BarCode { get; set; }

    [ExcelExport("Document ID", ExcelDataType.Number, "#,##0", Order = 6)]
    public int? DocumentId { get; set; }

    [ExcelExport("Scan Date", ExcelDataType.Date, "yyyy-MM-dd HH:mm:ss", Order = 7)]
    public DateTime? ScanDate { get; set; }

    [ExcelExport("File Extension", ExcelDataType.String, Order = 8)]
    public string? FileExtension { get; set; }

    [ExcelExport("Status", ExcelDataType.String, Order = 9)]
    public string Status { get; set; } = string.Empty;
}
