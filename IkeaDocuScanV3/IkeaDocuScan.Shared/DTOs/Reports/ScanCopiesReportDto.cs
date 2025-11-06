using ExcelReporting.Attributes;
using ExcelReporting.Models;

namespace IkeaDocuScan.Shared.DTOs.Reports;

/// <summary>
/// DTO for Scan Copies report - documents that are fax copies but original not yet received
/// </summary>
public class ScanCopiesReportDto : ExportableBase
{
    [ExcelExport("Bar Code", ExcelDataType.Number, "#,##0", Order = 1)]
    public int BarCode { get; set; }

    [ExcelExport("Document Type", ExcelDataType.String, Order = 2)]
    public string? DocumentType { get; set; }

    [ExcelExport("Document Name", ExcelDataType.String, Order = 3)]
    public string? DocumentName { get; set; }

    [ExcelExport("Document No", ExcelDataType.String, Order = 4)]
    public string? DocumentNo { get; set; }

    [ExcelExport("Counter Party", ExcelDataType.String, Order = 5)]
    public string? Counterparty { get; set; }

    [ExcelExport("Counter Party No", ExcelDataType.String, Order = 6)]
    public string? CounterpartyNo { get; set; }
}
