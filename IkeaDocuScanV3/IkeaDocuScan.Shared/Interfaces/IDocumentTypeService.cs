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
    /// Get all document types including disabled ones (for administration)
    /// </summary>
    Task<List<DocumentTypeDto>> GetAllIncludingDisabledAsync();

    /// <summary>
    /// Get a document type by ID
    /// </summary>
    Task<DocumentTypeDto?> GetByIdAsync(int id);

    /// <summary>
    /// Create a new document type
    /// </summary>
    Task<DocumentTypeDto> CreateAsync(CreateDocumentTypeDto dto);

    /// <summary>
    /// Update an existing document type
    /// </summary>
    Task<DocumentTypeDto> UpdateAsync(int id, UpdateDocumentTypeDto dto);

    /// <summary>
    /// Delete a document type by ID
    /// </summary>
    Task DeleteAsync(int id);

    /// <summary>
    /// Check if a document type is in use
    /// </summary>
    Task<bool> IsInUseAsync(int id);

    /// <summary>
    /// Get usage count for a document type (documents, document names, user permissions)
    /// </summary>
    Task<(int documentCount, int documentNameCount, int userPermissionCount)> GetUsageCountAsync(int id);
}
