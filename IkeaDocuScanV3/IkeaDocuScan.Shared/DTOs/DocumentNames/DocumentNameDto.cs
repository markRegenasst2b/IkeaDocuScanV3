namespace IkeaDocuScan.Shared.DTOs.DocumentNames;

/// <summary>
/// DTO for DocumentName entity
/// </summary>
public class DocumentNameDto
{
    /// <summary>
    /// Document name ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Document name text
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Optional document type ID that this name belongs to
    /// </summary>
    public int? DocumentTypeId { get; set; }

    /// <summary>
    /// Document type name (populated from join)
    /// </summary>
    public string? DocumentTypeName { get; set; }
}
