using ExcelReporting.Attributes;
using ExcelReporting.Models;

namespace IkeaDocuScan.Shared.DTOs.Excel;

/// <summary>
/// DTO for exporting document data to Excel
/// </summary>
public class DocumentExportDto : ExportableBase
{
    [ExcelExport("Document ID", ExcelDataType.Number, "#,##0", Order = 1, IsExportable = false)]
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

    [ExcelExport("Sending Out Date", ExcelDataType.Date, "yyyy-MM-dd", Order = 19)]
    public DateTime? SendingOutDate { get; set; }

    [ExcelExport("Fw. To Signatories Date", ExcelDataType.Date, "yyyy-MM-dd", Order = 20)]
    public DateTime? ForwardedToSignatoriesDate { get; set; }

    [ExcelExport("Dispatch Date", ExcelDataType.Date, "yyyy-MM-dd", Order = 21)]
    public DateTime? DispatchDate { get; set; }

    [ExcelExport("Fax", ExcelDataType.Boolean, Order = 22)]
    public bool? Fax { get; set; }

    [ExcelExport("Translation Received", ExcelDataType.Boolean, Order = 23)]
    public bool? TranslationReceived { get; set; }

    [ExcelExport("Assoc to PUA/Agr No", ExcelDataType.String, Order = 24)]
    public string? AssociatedToPua { get; set; }

    [ExcelExport("Assoc to Appendix No", ExcelDataType.String, Order = 25)]
    public string? AssociatedToAppendix { get; set; }

    [ExcelExport("Authorization To", ExcelDataType.String, Order = 26)]
    public string? Authorisation { get; set; }

    [ExcelExport("Bank Confirmation", ExcelDataType.Boolean, Order = 27)]
    public bool? BankConfirmation { get; set; }

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
            SendingOutDate = dto.SendingOutDate,
            ForwardedToSignatoriesDate = dto.ForwardedToSignatoriesDate,
            DispatchDate = dto.DispatchDate,
            Fax = dto.Fax,
            TranslationReceived = null, // Not available in DocumentDto
            AssociatedToPua = dto.AssociatedToPua,
            AssociatedToAppendix = dto.AssociatedToAppendix,
            Authorisation = dto.Authorisation,
            BankConfirmation = dto.BankConfirmation,
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
            SendingOutDate = item.SendingOutDate,
            ForwardedToSignatoriesDate = item.ForwardedToSignatoriesDate,
            DispatchDate = item.DispatchDate,
            Fax = item.Fax,
            TranslationReceived = item.TranslationReceived,
            AssociatedToPua = item.AssociatedToPua,
            AssociatedToAppendix = item.AssociatedToAppendix,
            Authorisation = item.Authorisation,
            BankConfirmation = item.BankConfirmation,
            FileId = item.FileId
        };
    }
}
