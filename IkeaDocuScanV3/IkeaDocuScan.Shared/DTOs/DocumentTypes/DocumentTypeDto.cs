namespace IkeaDocuScan.Shared.DTOs.DocumentTypes;

/// <summary>
/// Data transfer object for DocumentType
/// </summary>
public class DocumentTypeDto
{
    public int DtId { get; set; }
    public string DtName { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
}
