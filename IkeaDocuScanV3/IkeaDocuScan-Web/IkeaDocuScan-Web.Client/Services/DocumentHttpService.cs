using IkeaDocuScan.Shared.DTOs.Documents;
using IkeaDocuScan.Shared.Interfaces;
using System.Net.Http.Json;

namespace IkeaDocuScan_Web.Client.Services;

/// <summary>
/// HTTP client service for Document CRUD operations
/// Implements IDocumentService interface to call server APIs
/// </summary>
public class DocumentHttpService : IDocumentService
{
    private readonly HttpClient _http;
    private readonly ILogger<DocumentHttpService> _logger;

    public DocumentHttpService(HttpClient http, ILogger<DocumentHttpService> logger)
    {
        _http = http;
        _logger = logger;
    }

    public async Task<List<DocumentDto>> GetAllAsync()
    {
        try
        {
            _logger.LogInformation("Fetching all documents from API");
            var result = await _http.GetFromJsonAsync<List<DocumentDto>>("/api/documents");
            return result ?? new List<DocumentDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching documents");
            throw;
        }
    }

    public async Task<DocumentDto?> GetByIdAsync(int id)
    {
        try
        {
            _logger.LogInformation("Fetching document {DocumentId} from API", id);
            return await _http.GetFromJsonAsync<DocumentDto>($"/api/documents/{id}");
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Document {DocumentId} not found", id);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching document {DocumentId}", id);
            throw;
        }
    }

    public async Task<DocumentDto?> GetByBarCodeAsync(string barCode)
    {
        try
        {
            _logger.LogInformation("Fetching document by BarCode {BarCode} from API", barCode);
            return await _http.GetFromJsonAsync<DocumentDto>($"/api/documents/barcode/{barCode}");
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Document with BarCode {BarCode} not found", barCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching document by BarCode {BarCode}", barCode);
            throw;
        }
    }

    public async Task<DocumentDto> CreateAsync(CreateDocumentDto dto)
    {
        try
        {
            _logger.LogInformation("Creating new document via API");
            var response = await _http.PostAsJsonAsync("/api/documents", dto);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<DocumentDto>();
            return result!;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating document");
            throw;
        }
    }

    public async Task<DocumentDto> UpdateAsync(UpdateDocumentDto dto)
    {
        try
        {
            _logger.LogInformation("Updating document {DocumentId} via API", dto.Id);
            var response = await _http.PutAsJsonAsync($"/api/documents/{dto.Id}", dto);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<DocumentDto>();
            return result!;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating document {DocumentId}", dto.Id);
            throw;
        }
    }

    public async Task DeleteAsync(int id)
    {
        try
        {
            _logger.LogInformation("Deleting document {DocumentId} via API", id);
            var response = await _http.DeleteAsync($"/api/documents/{id}");
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting document {DocumentId}", id);
            throw;
        }
    }

    public async Task<DocumentSearchResultDto> SearchAsync(DocumentSearchRequestDto request)
    {
        try
        {
            _logger.LogInformation("Searching documents via API");
            var response = await _http.PostAsJsonAsync("/api/documents/search", request);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<DocumentSearchResultDto>();
            return result!;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching documents");
            throw;
        }
    }

    public async Task<DocumentFileDto?> GetDocumentFileAsync(int id)
    {
        try
        {
            _logger.LogInformation("Fetching document file for document {DocumentId} from API", id);
            return await _http.GetFromJsonAsync<DocumentFileDto>($"/api/documents/{id}/download");
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Document file for document {DocumentId} not found", id);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching document file");
            throw;
        }
    }
}
