using ExcelReporting.Attributes;
using ExcelReporting.Models;

namespace IkeaDocuScan.Shared.DTOs.Reports;

/// <summary>
/// DTO for All Documents report - exports all documents in the system
/// </summary>
public class AllDocumentsReportDto : ExportableBase
{
    [ExcelExport("Document ID", ExcelDataType.Number, "#,##0", Order = 1, IsExportable = false)]
    public int Id { get; set; }

    [ExcelExport("Bar Code", ExcelDataType.Hyperlink, "/documents/preview/{Id}", Order = 2)]
    public int BarCode { get; set; }

    [ExcelExport("Document Name", ExcelDataType.String, Order = 3)]
    public string Name { get; set; } = string.Empty;

    [ExcelExport("Document Type", ExcelDataType.String, Order = 4)]
    public string? DocumentTypeName { get; set; }

    [ExcelExport("Counter Party", ExcelDataType.String, Order = 5)]
    public string? CounterPartyName { get; set; }

    [ExcelExport("Country", ExcelDataType.String, Order = 6)]
    public string? Country { get; set; }

    [ExcelExport("Document No", ExcelDataType.String, Order = 7)]
    public string? DocumentNo { get; set; }

    [ExcelExport("Version No", ExcelDataType.String, Order = 8)]
    public string? VersionNo { get; set; }

    [ExcelExport("Date of Contract", ExcelDataType.Date, "yyyy-MM-dd", Order = 9)]
    public DateTime? DateOfContract { get; set; }

    [ExcelExport("Receiving Date", ExcelDataType.Date, "yyyy-MM-dd", Order = 10)]
    public DateTime? ReceivingDate { get; set; }

    [ExcelExport("Action Date", ExcelDataType.Date, "yyyy-MM-dd", Order = 11)]
    public DateTime? ActionDate { get; set; }

    [ExcelExport("Valid Until", ExcelDataType.Date, "yyyy-MM-dd", Order = 12)]
    public DateTime? ValidUntil { get; set; }

    [ExcelExport("Currency", ExcelDataType.String, Order = 13)]
    public string? CurrencyCode { get; set; }

    [ExcelExport("Amount", ExcelDataType.Currency, "$#,##0.00", Order = 14)]
    public decimal? Amount { get; set; }

    [ExcelExport("Confidential", ExcelDataType.Boolean, Order = 15)]
    public bool? Confidential { get; set; }

    [ExcelExport("Original Received", ExcelDataType.Boolean, Order = 16)]
    public bool? OriginalReceived { get; set; }

    [ExcelExport("Created On", ExcelDataType.Date, "yyyy-MM-dd HH:mm:ss", Order = 17)]
    public DateTime CreatedOn { get; set; }

    [ExcelExport("Created By", ExcelDataType.String, Order = 18)]
    public string CreatedBy { get; set; } = string.Empty;

    [ExcelExport("Modified On", ExcelDataType.Date, "yyyy-MM-dd HH:mm:ss", Order = 19)]
    public DateTime? ModifiedOn { get; set; }

    [ExcelExport("Modified By", ExcelDataType.String, Order = 20)]
    public string? ModifiedBy { get; set; }
}
