using ExcelReporting.Attributes;
using ExcelReporting.Models;

namespace IkeaDocuScan.Shared.DTOs.ActionReminders;

/// <summary>
/// DTO for displaying action reminder data with Excel export support
/// </summary>
public class ActionReminderDto : ExportableBase
{
    [ExcelExport("Barcode", ExcelDataType.String, Order = 1)]
    public string BarCode { get; set; } = string.Empty;

    [ExcelExport("Document Type", ExcelDataType.String, Order = 2)]
    public string DocumentType { get; set; } = string.Empty;

    [ExcelExport("Document Name", ExcelDataType.String, Order = 3)]
    public string DocumentName { get; set; } = string.Empty;

    [ExcelExport("Document No", ExcelDataType.String, Order = 4)]
    public string? DocumentNo { get; set; }

    [ExcelExport("Counterparty", ExcelDataType.String, Order = 5)]
    public string CounterParty { get; set; } = string.Empty;

    [ExcelExport("Counterparty No", ExcelDataType.Number, "#,##0", Order = 6)]
    public int? CounterPartyNo { get; set; }

    [ExcelExport("Action Date", ExcelDataType.Date, "dd/MM/yyyy", Order = 7)]
    public DateTime? ActionDate { get; set; }

    [ExcelExport("Receiving Date", ExcelDataType.Date, "dd/MM/yyyy", Order = 8)]
    public DateTime? ReceivingDate { get; set; }

    [ExcelExport("Action Description", ExcelDataType.String, Order = 9)]
    public string? ActionDescription { get; set; }

    [ExcelExport("Comment", ExcelDataType.String, Order = 10)]
    public string? Comment { get; set; }
}
