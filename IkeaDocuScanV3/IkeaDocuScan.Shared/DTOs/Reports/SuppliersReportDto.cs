using ExcelReporting.Attributes;
using ExcelReporting.Models;

namespace IkeaDocuScan.Shared.DTOs.Reports;

/// <summary>
/// DTO for Suppliers report - counterparty/supplier list for check-in display
/// </summary>
public class SuppliersReportDto : ExportableBase
{

    [ExcelExport("Counter Party No", ExcelDataType.String, Order = 2)]
    public string? CounterPartyNoAlpha { get; set; }

    [ExcelExport("Name", ExcelDataType.String, Order = 3)]
    public string? Name { get; set; }

    [ExcelExport("Country", ExcelDataType.String, Order = 4)]
    public string? Country { get; set; }

    [ExcelExport("Affiliated To", ExcelDataType.String, Order = 5)]
    public string? AffiliatedTo { get; set; }
}
