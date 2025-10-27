using System.Net.Http.Json;
using IkeaDocuScan.Shared.DTOs.DocumentNames;
using IkeaDocuScan.Shared.Interfaces;

namespace IkeaDocuScan_Web.Client.Services;

/// <summary>
/// Client-side HTTP service for document names
/// </summary>
public class DocumentNameHttpService : IDocumentNameService
{
    private readonly HttpClient _http;
    private readonly ILogger<DocumentNameHttpService> _logger;

    public DocumentNameHttpService(HttpClient http, ILogger<DocumentNameHttpService> logger)
    {
        _http = http;
        _logger = logger;
    }

    /// <summary>
    /// Get all document names
    /// </summary>
    public async Task<List<DocumentNameDto>> GetAllAsync()
    {
        try
        {
            _logger.LogInformation("Fetching all document names from API");
            var documentNames = await _http.GetFromJsonAsync<List<DocumentNameDto>>("/api/documentnames");
            return documentNames ?? new List<DocumentNameDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching document names");
            throw;
        }
    }

    /// <summary>
    /// Get document names filtered by document type ID
    /// </summary>
    public async Task<List<DocumentNameDto>> GetByDocumentTypeIdAsync(int documentTypeId)
    {
        try
        {
            _logger.LogInformation("Fetching document names for DocumentTypeId {DocumentTypeId}", documentTypeId);
            var documentNames = await _http.GetFromJsonAsync<List<DocumentNameDto>>(
                $"/api/documentnames/bytype/{documentTypeId}");
            return documentNames ?? new List<DocumentNameDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching document names for DocumentTypeId {DocumentTypeId}", documentTypeId);
            throw;
        }
    }

    /// <summary>
    /// Get a specific document name by ID
    /// </summary>
    public async Task<DocumentNameDto?> GetByIdAsync(int id)
    {
        try
        {
            _logger.LogInformation("Fetching document name with ID {Id}", id);
            return await _http.GetFromJsonAsync<DocumentNameDto>($"/api/documentnames/{id}");
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Document name with ID {Id} not found", id);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching document name with ID {Id}", id);
            throw;
        }
    }
}
