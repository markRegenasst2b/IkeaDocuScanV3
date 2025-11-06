using System.ComponentModel.DataAnnotations;

namespace IkeaDocuScan.Shared.DTOs.DocumentNames;

/// <summary>
/// DTO for updating an existing document name
/// </summary>
public class UpdateDocumentNameDto
{
    /// <summary>
    /// Document name ID (required for update)
    /// </summary>
    [Required]
    public int Id { get; set; }

    /// <summary>
    /// Document name text (required)
    /// </summary>
    [Required(ErrorMessage = "Name is required")]
    [StringLength(255, ErrorMessage = "Name cannot exceed 255 characters")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Optional document type ID that this name belongs to
    /// </summary>
    public int? DocumentTypeId { get; set; }
}
