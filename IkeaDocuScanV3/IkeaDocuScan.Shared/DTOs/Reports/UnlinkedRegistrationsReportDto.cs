using ExcelReporting.Attributes;
using ExcelReporting.Models;

namespace IkeaDocuScan.Shared.DTOs.Reports;

/// <summary>
/// DTO for Unlinked Registrations report - documents registered but not linked to files
/// </summary>
public class UnlinkedRegistrationsReportDto : ExportableBase
{
    [ExcelExport("Document ID", ExcelDataType.Number, "#,##0", Order = 1)]
    public int Id { get; set; }

    [ExcelExport("Bar Code", ExcelDataType.Hyperlink, "/documents/edit/{BarCode}", Order = 2)]
    public int BarCode { get; set; }

    [ExcelExport("Document Name", ExcelDataType.String, Order = 3)]
    public string Name { get; set; } = string.Empty;

    [ExcelExport("Document Type", ExcelDataType.String, Order = 4)]
    public string? DocumentTypeName { get; set; }

    [ExcelExport("Counter Party", ExcelDataType.String, Order = 5)]
    public string? CounterPartyName { get; set; }

    [ExcelExport("Document No", ExcelDataType.String, Order = 6)]
    public string? DocumentNo { get; set; }

    [ExcelExport("Has File", ExcelDataType.Boolean, Order = 7)]
    public bool HasFile { get; set; }

    [ExcelExport("Created On", ExcelDataType.Date, "yyyy-MM-dd HH:mm:ss", Order = 8)]
    public DateTime CreatedOn { get; set; }

    [ExcelExport("Created By", ExcelDataType.String, Order = 9)]
    public string CreatedBy { get; set; } = string.Empty;

    [ExcelExport("Days Since Creation", ExcelDataType.Number, "#,##0", Order = 10)]
    public int DaysSinceCreation { get; set; }
}
