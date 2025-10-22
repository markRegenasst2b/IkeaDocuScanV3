using IkeaDocuScan.Shared.DTOs.DocumentTypes;

namespace IkeaDocuScan.Shared.Interfaces;

/// <summary>
/// Service interface for DocumentType operations
/// </summary>
public interface IDocumentTypeService
{
    /// <summary>
    /// Get all document types (enabled only)
    /// </summary>
    Task<List<DocumentTypeDto>> GetAllAsync();

    /// <summary>
    /// Get a document type by ID
    /// </summary>
    Task<DocumentTypeDto?> GetByIdAsync(int id);
}
