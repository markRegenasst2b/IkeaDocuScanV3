using ExcelReporting.Attributes;
using ExcelReporting.Models;

namespace IkeaDocuScan.Shared.DTOs.Reports;

/// <summary>
/// DTO for All Documents report - exports all documents in the system with full details
/// </summary>
public class AllDocumentsReportDto : ExportableBase
{
    [ExcelExport("CP No", ExcelDataType.String, Order = 1)]
    public string? CPNo { get; set; }

    [ExcelExport("CP No Alpha", ExcelDataType.String, Order = 2)]
    public string? CPNoAlpha { get; set; }

    [ExcelExport("CP Name", ExcelDataType.String, Order = 3)]
    public string? CPName { get; set; }

    [ExcelExport("Country Code", ExcelDataType.String, Order = 4)]
    public string? CountryCode { get; set; }

    [ExcelExport("Country", ExcelDataType.String, Order = 5)]
    public string? Country { get; set; }

    [ExcelExport("City", ExcelDataType.String, Order = 6)]
    public string? City { get; set; }

    [ExcelExport("Affiliated To", ExcelDataType.String, Order = 7)]
    public string? AffiliatedTo { get; set; }

    [ExcelExport("Doc Barcode", ExcelDataType.Number, "#,##0", Order = 8)]
    public int? DocBarcode { get; set; }

    [ExcelExport("Doc Type", ExcelDataType.String, Order = 9)]
    public string? DocType { get; set; }

    [ExcelExport("File Name", ExcelDataType.String, Order = 10)]
    public string? FileName { get; set; }

    [ExcelExport("Date of Contract", ExcelDataType.Date, "yyyy-MM-dd", Order = 11)]
    public DateTime? DateOfContract { get; set; }

    [ExcelExport("Comment", ExcelDataType.String, Order = 12)]
    public string? Comment { get; set; }

    [ExcelExport("Receiving Date", ExcelDataType.Date, "yyyy-MM-dd", Order = 13)]
    public DateTime? ReceivingDate { get; set; }

    [ExcelExport("Dispatch Date", ExcelDataType.Date, "yyyy-MM-dd", Order = 14)]
    public DateTime? DispatchDate { get; set; }

    [ExcelExport("Fax", ExcelDataType.Boolean, Order = 15)]
    public bool? Fax { get; set; }

    [ExcelExport("Original Received", ExcelDataType.Boolean, Order = 16)]
    public bool? OriginalReceived { get; set; }

    [ExcelExport("Action Date", ExcelDataType.Date, "yyyy-MM-dd", Order = 17)]
    public DateTime? ActionDate { get; set; }

    [ExcelExport("Action Description", ExcelDataType.String, Order = 18)]
    public string? ActionDescription { get; set; }

    [ExcelExport("Document No", ExcelDataType.String, Order = 19)]
    public string? DocumentNo { get; set; }

    [ExcelExport("Associated to PUA", ExcelDataType.String, Order = 20)]
    public string? AssociatedToPUA { get; set; }

    [ExcelExport("Version No", ExcelDataType.String, Order = 21)]
    public string? VersionNo { get; set; }

    [ExcelExport("Associated to Appendix", ExcelDataType.String, Order = 22)]
    public string? AssociatedToAppendix { get; set; }

    [ExcelExport("Valid Until", ExcelDataType.Date, "yyyy-MM-dd", Order = 23)]
    public DateTime? ValidUntil { get; set; }

    [ExcelExport("Currency Code", ExcelDataType.String, Order = 24)]
    public string? CurrencyCode { get; set; }

    [ExcelExport("Amount", ExcelDataType.Currency, "$#,##0.00", Order = 25)]
    public decimal? Amount { get; set; }

    [ExcelExport("Confidential", ExcelDataType.Boolean, Order = 26)]
    public bool? Confidential { get; set; }

    [ExcelExport("Third Party", ExcelDataType.Boolean, Order = 27)]
    public bool? ThirdParty { get; set; }

    [ExcelExport("Authorisation", ExcelDataType.Boolean, Order = 28)]
    public bool? Authorisation { get; set; }

    [ExcelExport("Bank Confirmation", ExcelDataType.Boolean, Order = 29)]
    public bool? BankConfirmation { get; set; }

    [ExcelExport("Translated Version Received", ExcelDataType.Boolean, Order = 30)]
    public bool? TranslatedVersionReceived { get; set; }
}
