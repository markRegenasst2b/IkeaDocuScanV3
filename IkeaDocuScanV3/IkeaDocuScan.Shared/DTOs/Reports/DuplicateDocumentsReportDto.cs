using ExcelReporting.Attributes;
using ExcelReporting.Models;

namespace IkeaDocuScan.Shared.DTOs.Reports;

/// <summary>
/// DTO for Duplicate Documents report - identifies potential duplicate documents
/// Shows summary of documents that appear multiple times with the same key attributes
/// </summary>
public class DuplicateDocumentsReportDto : ExportableBase
{
    [ExcelExport("Document Type", ExcelDataType.String, Order = 1)]
    public string DocumentType { get; set; } = string.Empty;

    [ExcelExport("Document No", ExcelDataType.String, Order = 2)]
    public string? DocumentNo { get; set; }

    [ExcelExport("Version No", ExcelDataType.String, Order = 3)]
    public string? VersionNo { get; set; }

    [ExcelExport("Counter Party No", ExcelDataType.String, Order = 4)]
    public string? CounterPartyNoAlpha { get; set; }

    [ExcelExport("Counter Party Name", ExcelDataType.String, Order = 5)]
    public string? Counterparty { get; set; }

    [ExcelExport("Duplicate Count", ExcelDataType.Number, "#,##0", Order = 6)]
    public int Count { get; set; }
}
