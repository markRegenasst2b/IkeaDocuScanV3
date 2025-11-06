using IkeaDocuScan.Shared.DTOs.DocumentNames;

namespace IkeaDocuScan.Shared.Interfaces;

/// <summary>
/// Service for managing document names.
/// Document names are predefined names that can be associated with documents,
/// typically filtered by document type.
/// </summary>
public interface IDocumentNameService
{
    /// <summary>
    /// Get all document names
    /// </summary>
    Task<List<DocumentNameDto>> GetAllAsync();

    /// <summary>
    /// Get document names filtered by document type ID
    /// </summary>
    Task<List<DocumentNameDto>> GetByDocumentTypeIdAsync(int documentTypeId);

    /// <summary>
    /// Get a specific document name by ID
    /// </summary>
    Task<DocumentNameDto?> GetByIdAsync(int id);

    /// <summary>
    /// Create a new document name (SuperUser only)
    /// </summary>
    Task<DocumentNameDto> CreateAsync(CreateDocumentNameDto createDto);

    /// <summary>
    /// Update an existing document name (SuperUser only)
    /// </summary>
    Task<DocumentNameDto> UpdateAsync(UpdateDocumentNameDto updateDto);

    /// <summary>
    /// Delete a document name (SuperUser only)
    /// </summary>
    Task DeleteAsync(int id);
}
