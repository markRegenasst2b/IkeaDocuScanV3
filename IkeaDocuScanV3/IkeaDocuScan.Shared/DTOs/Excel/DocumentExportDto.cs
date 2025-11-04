using ExcelReporting.Attributes;
using ExcelReporting.Models;

namespace IkeaDocuScan.Shared.DTOs.Excel;

/// <summary>
/// DTO for exporting document data to Excel
/// </summary>
public class DocumentExportDto : ExportableBase
{
    [ExcelExport("Document ID", ExcelDataType.Number, "#,##0", Order = 1)]
    public int Id { get; set; }

    [ExcelExport("Document Name", ExcelDataType.String, Order = 2)]
    public string Name { get; set; } = string.Empty;

    [ExcelExport("Bar Code", ExcelDataType.Number, "#,##0", Order = 3)]
    public int BarCode { get; set; }

    [ExcelExport("Document Type", ExcelDataType.String, Order = 4)]
    public string? DocumentTypeName { get; set; }

    [ExcelExport("Counter Party", ExcelDataType.String, Order = 5)]
    public string? CounterPartyName { get; set; }

    [ExcelExport("Document No", ExcelDataType.String, Order = 6)]
    public string? DocumentNo { get; set; }

    [ExcelExport("Date of Contract", ExcelDataType.Date, "yyyy-MM-dd", Order = 7)]
    public DateTime? DateOfContract { get; set; }

    [ExcelExport("Receiving Date", ExcelDataType.Date, "yyyy-MM-dd", Order = 8)]
    public DateTime? ReceivingDate { get; set; }

    [ExcelExport("Action Date", ExcelDataType.Date, "yyyy-MM-dd", Order = 9)]
    public DateTime? ActionDate { get; set; }

    [ExcelExport("Valid Until", ExcelDataType.Date, "yyyy-MM-dd", Order = 10)]
    public DateTime? ValidUntil { get; set; }

    [ExcelExport("Currency", ExcelDataType.String, Order = 11)]
    public string? CurrencyCode { get; set; }

    [ExcelExport("Amount", ExcelDataType.Currency, "$#,##0.00", Order = 12)]
    public decimal? Amount { get; set; }

    [ExcelExport("Confidential", ExcelDataType.Boolean, Order = 13)]
    public bool? Confidential { get; set; }

    [ExcelExport("Original Received", ExcelDataType.Boolean, Order = 14)]
    public bool? OriginalReceived { get; set; }

    [ExcelExport("Comment", ExcelDataType.String, Order = 15)]
    public string? Comment { get; set; }

    [ExcelExport("Action Description", ExcelDataType.String, Order = 16)]
    public string? ActionDescription { get; set; }

    [ExcelExport("Version No", ExcelDataType.String, Order = 17)]
    public string? VersionNo { get; set; }

    [ExcelExport("Third Party", ExcelDataType.String, Order = 18)]
    public string? ThirdParty { get; set; }

    [ExcelExport("Created On", ExcelDataType.Date, "yyyy-MM-dd HH:mm", Order = 19)]
    public DateTime CreatedOn { get; set; }

    [ExcelExport("Created By", ExcelDataType.String, Order = 20)]
    public string CreatedBy { get; set; } = string.Empty;

    [ExcelExport("Modified On", ExcelDataType.Date, "yyyy-MM-dd HH:mm", Order = 21)]
    public DateTime? ModifiedOn { get; set; }

    [ExcelExport("Modified By", ExcelDataType.String, Order = 22)]
    public string? ModifiedBy { get; set; }

    // Non-exported properties (for internal use)
    [ExcelExport("Internal ID", IsExportable = false)]
    public int? FileId { get; set; }

    /// <summary>
    /// Creates a DocumentExportDto from a DocumentDto
    /// </summary>
    public static DocumentExportDto FromDocumentDto(DTOs.Documents.DocumentDto dto)
    {
        return new DocumentExportDto
        {
            Id = dto.Id,
            Name = dto.Name,
            BarCode = dto.BarCode,
            DocumentTypeName = dto.DocumentTypeName,
            CounterPartyName = dto.CounterPartyName,
            DocumentNo = dto.DocumentNo,
            DateOfContract = dto.DateOfContract,
            ReceivingDate = dto.ReceivingDate,
            ActionDate = dto.ActionDate,
            ValidUntil = dto.ValidUntil,
            CurrencyCode = dto.CurrencyCode,
            Amount = dto.Amount,
            Confidential = dto.Confidential,
            OriginalReceived = dto.OriginalReceived,
            Comment = dto.Comment,
            ActionDescription = dto.ActionDescription,
            VersionNo = dto.VersionNo,
            ThirdParty = dto.ThirdParty,
            CreatedOn = dto.CreatedOn,
            CreatedBy = dto.CreatedBy,
            ModifiedOn = dto.ModifiedOn,
            ModifiedBy = dto.ModifiedBy,
            FileId = dto.FileId
        };
    }

    /// <summary>
    /// Creates a DocumentExportDto from a DocumentSearchItemDto
    /// </summary>
    public static DocumentExportDto FromSearchItem(DTOs.Documents.DocumentSearchItemDto item)
    {
        return new DocumentExportDto
        {
            Id = item.Id,
            Name = item.Name ?? item.DocumentName ?? string.Empty,
            BarCode = item.BarCode,
            DocumentTypeName = item.DocumentType,
            CounterPartyName = item.Counterparty,
            DocumentNo = item.DocumentNo,
            DateOfContract = item.DateOfContract,
            ReceivingDate = item.ReceivingDate,
            ActionDate = item.ActionDate,
            ValidUntil = item.ValidUntil,
            CurrencyCode = item.CurrencyCode,
            Amount = item.Amount,
            Confidential = item.Confidential,
            OriginalReceived = item.OriginalReceived,
            Comment = item.Comment,
            ActionDescription = item.ActionDescription,
            VersionNo = item.VersionNo,
            ThirdParty = item.ThirdParty,
            CreatedOn = DateTime.Now, // Search results don't include audit fields
            CreatedBy = string.Empty,
            ModifiedOn = null,
            ModifiedBy = null,
            FileId = item.FileId
        };
    }
}
