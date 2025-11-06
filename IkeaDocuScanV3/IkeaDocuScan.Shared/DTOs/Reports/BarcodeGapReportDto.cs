using ExcelReporting.Attributes;
using ExcelReporting.Models;

namespace IkeaDocuScan.Shared.DTOs.Reports;

/// <summary>
/// DTO for Barcode Gaps report - identifies missing barcodes in the sequence
/// </summary>
public class BarcodeGapReportDto : ExportableBase
{
    [ExcelExport("Gap Start", ExcelDataType.Number, "#,##0", Order = 1)]
    public int GapStart { get; set; }

    [ExcelExport("Gap End", ExcelDataType.Number, "#,##0", Order = 2)]
    public int GapEnd { get; set; }

    [ExcelExport("Gap Size", ExcelDataType.Number, "#,##0", Order = 3)]
    public int GapSize { get; set; }

    [ExcelExport("Previous Barcode", ExcelDataType.Number, "#,##0", Order = 4)]
    public int? PreviousBarcode { get; set; }

    [ExcelExport("Next Barcode", ExcelDataType.Number, "#,##0", Order = 5)]
    public int? NextBarcode { get; set; }
}
