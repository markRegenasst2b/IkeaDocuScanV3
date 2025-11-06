using ExcelReporting.Attributes;
using ExcelReporting.Models;

namespace IkeaDocuScan.Shared.DTOs.Reports;

/// <summary>
/// DTO for Suppliers report - counterparty/supplier statistics
/// </summary>
public class SuppliersReportDto : ExportableBase
{
    [ExcelExport("Counter Party ID", ExcelDataType.Number, "#,##0", Order = 1)]
    public int CounterPartyId { get; set; }

    [ExcelExport("Counter Party Name", ExcelDataType.String, Order = 2)]
    public string CounterPartyName { get; set; } = string.Empty;

    [ExcelExport("Country", ExcelDataType.String, Order = 3)]
    public string? Country { get; set; }

    [ExcelExport("City", ExcelDataType.String, Order = 4)]
    public string? City { get; set; }

    [ExcelExport("Total Documents", ExcelDataType.Number, "#,##0", Order = 5)]
    public int TotalDocuments { get; set; }

    [ExcelExport("Active Contracts", ExcelDataType.Number, "#,##0", Order = 6)]
    public int? ActiveContracts { get; set; }

    [ExcelExport("Expired Contracts", ExcelDataType.Number, "#,##0", Order = 7)]
    public int? ExpiredContracts { get; set; }

    [ExcelExport("Latest Document Date", ExcelDataType.Date, "yyyy-MM-dd", Order = 8)]
    public DateTime? LatestDocumentDate { get; set; }

    [ExcelExport("Total Amount", ExcelDataType.Currency, "$#,##0.00", Order = 9)]
    public decimal? TotalAmount { get; set; }

    [ExcelExport("Created On", ExcelDataType.Date, "yyyy-MM-dd", Order = 10)]
    public DateTime? CreatedOn { get; set; }
}
