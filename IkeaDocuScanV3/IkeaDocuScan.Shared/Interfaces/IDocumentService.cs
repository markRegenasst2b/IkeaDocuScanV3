using IkeaDocuScan.Shared.DTOs.Documents;

namespace IkeaDocuScan.Shared.Interfaces;

public interface IDocumentService
{
    Task<List<DocumentDto>> GetAllAsync();
    Task<DocumentDto?> GetByIdAsync(int id);
    Task<DocumentDto?> GetByBarCodeAsync(string barCode);
    Task<DocumentDto> CreateAsync(CreateDocumentDto dto);
    Task<DocumentDto> UpdateAsync(UpdateDocumentDto dto);
    Task DeleteAsync(int id);
    Task<DocumentSearchResultDto> SearchAsync(DocumentSearchRequestDto request);
    Task<DocumentFileDto?> GetDocumentFileAsync(int id);
}
