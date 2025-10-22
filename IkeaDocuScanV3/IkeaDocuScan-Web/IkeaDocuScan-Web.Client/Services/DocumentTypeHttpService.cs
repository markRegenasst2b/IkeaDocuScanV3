using IkeaDocuScan.Shared.DTOs.DocumentTypes;
using IkeaDocuScan.Shared.Interfaces;
using System.Net.Http.Json;

namespace IkeaDocuScan_Web.Client.Services;

/// <summary>
/// HTTP client service for DocumentType operations
/// Implements IDocumentTypeService interface to call server APIs
/// </summary>
public class DocumentTypeHttpService : IDocumentTypeService
{
    private readonly HttpClient _http;
    private readonly ILogger<DocumentTypeHttpService> _logger;

    public DocumentTypeHttpService(HttpClient http, ILogger<DocumentTypeHttpService> logger)
    {
        _http = http;
        _logger = logger;
    }

    public async Task<List<DocumentTypeDto>> GetAllAsync()
    {
        try
        {
            _logger.LogInformation("Fetching all document types from API");
            var result = await _http.GetFromJsonAsync<List<DocumentTypeDto>>("/api/documenttypes");
            return result ?? new List<DocumentTypeDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching document types");
            throw;
        }
    }

    public async Task<DocumentTypeDto?> GetByIdAsync(int id)
    {
        try
        {
            _logger.LogInformation("Fetching document type {DocumentTypeId} from API", id);
            return await _http.GetFromJsonAsync<DocumentTypeDto>($"/api/documenttypes/{id}");
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Document type {DocumentTypeId} not found", id);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching document type {DocumentTypeId}", id);
            throw;
        }
    }
}
