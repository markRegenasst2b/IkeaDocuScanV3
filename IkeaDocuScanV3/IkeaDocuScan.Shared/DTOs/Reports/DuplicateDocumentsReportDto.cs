using ExcelReporting.Attributes;
using ExcelReporting.Models;

namespace IkeaDocuScan.Shared.DTOs.Reports;

/// <summary>
/// DTO for Duplicate Documents report - identifies potential duplicate documents
/// </summary>
public class DuplicateDocumentsReportDto : ExportableBase
{
    [ExcelExport("Document ID", ExcelDataType.Number, "#,##0", Order = 1)]
    public int Id { get; set; }

    [ExcelExport("Bar Code", ExcelDataType.Hyperlink, "/documents/preview/{Id}", Order = 2)]
    public int BarCode { get; set; }

    [ExcelExport("Document Name", ExcelDataType.String, Order = 3)]
    public string Name { get; set; } = string.Empty;

    [ExcelExport("Document Type", ExcelDataType.String, Order = 4)]
    public string? DocumentTypeName { get; set; }

    [ExcelExport("Document No", ExcelDataType.String, Order = 5)]
    public string? DocumentNo { get; set; }

    [ExcelExport("Version No", ExcelDataType.String, Order = 6)]
    public string? VersionNo { get; set; }

    [ExcelExport("Counter Party", ExcelDataType.String, Order = 7)]
    public string? CounterPartyName { get; set; }

    [ExcelExport("Date of Contract", ExcelDataType.Date, "yyyy-MM-dd", Order = 8)]
    public DateTime? DateOfContract { get; set; }

    [ExcelExport("Duplicate Group", ExcelDataType.Number, "#,##0", Order = 9)]
    public int? DuplicateGroup { get; set; }

    [ExcelExport("Created On", ExcelDataType.Date, "yyyy-MM-dd HH:mm:ss", Order = 10)]
    public DateTime CreatedOn { get; set; }
}
