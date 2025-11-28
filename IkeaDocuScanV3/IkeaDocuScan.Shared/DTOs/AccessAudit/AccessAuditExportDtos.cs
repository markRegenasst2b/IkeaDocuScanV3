using ExcelReporting.Attributes;
using ExcelReporting.Models;

namespace IkeaDocuScan.Shared.DTOs.AccessAudit;

/// <summary>
/// Exportable DTO for document type access audit Excel export
/// </summary>
public class DocumentTypeAccessExportDto : ExportableBase
{
    [ExcelExport("Account Name", ExcelDataType.String, Order = 1)]
    public string AccountName { get; set; } = string.Empty;

    [ExcelExport("Last Logon", ExcelDataType.Date, "yyyy-MM-dd HH:mm", Order = 2)]
    public DateTime? LastLogon { get; set; }

    [ExcelExport("Super User", ExcelDataType.Boolean, Order = 3)]
    public bool IsSuperUser { get; set; }

    [ExcelExport("Access Type", ExcelDataType.String, Order = 4)]
    public string AccessType { get; set; } = string.Empty;

    [ExcelExport("Document Type", ExcelDataType.String, Order = 5)]
    public string DocumentTypeName { get; set; } = string.Empty;
}

/// <summary>
/// Exportable DTO for user access audit Excel export
/// </summary>
public class UserAccessExportDto : ExportableBase
{
    [ExcelExport("Document Type", ExcelDataType.String, Order = 1)]
    public string DocumentTypeName { get; set; } = string.Empty;

    [ExcelExport("Has Access", ExcelDataType.Boolean, Order = 2)]
    public bool HasAccess { get; set; }

    [ExcelExport("Access Type", ExcelDataType.String, Order = 3)]
    public string AccessType { get; set; } = string.Empty;

    [ExcelExport("User", ExcelDataType.String, Order = 4)]
    public string AccountName { get; set; } = string.Empty;
}
