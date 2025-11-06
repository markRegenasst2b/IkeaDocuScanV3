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

    /// <summary>
    /// Create a new document name (SuperUser only)
    /// </summary>
    public async Task<DocumentNameDto> CreateAsync(CreateDocumentNameDto createDto)
    {
        try
        {
            _logger.LogInformation("Creating document name: {Name}", createDto.Name);
            var response = await _http.PostAsJsonAsync("/api/documentnames", createDto);
            response.EnsureSuccessStatusCode();
            var created = await response.Content.ReadFromJsonAsync<DocumentNameDto>();
            return created ?? throw new InvalidOperationException("Failed to deserialize created document name");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating document name");
            throw;
        }
    }

    /// <summary>
    /// Update an existing document name (SuperUser only)
    /// </summary>
    public async Task<DocumentNameDto> UpdateAsync(UpdateDocumentNameDto updateDto)
    {
        try
        {
            _logger.LogInformation("Updating document name ID {Id}", updateDto.Id);
            var response = await _http.PutAsJsonAsync($"/api/documentnames/{updateDto.Id}", updateDto);
            response.EnsureSuccessStatusCode();
            var updated = await response.Content.ReadFromJsonAsync<DocumentNameDto>();
            return updated ?? throw new InvalidOperationException("Failed to deserialize updated document name");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating document name ID {Id}", updateDto.Id);
            throw;
        }
    }

    /// <summary>
    /// Delete a document name (SuperUser only)
    /// </summary>
    public async Task DeleteAsync(int id)
    {
        try
        {
            _logger.LogInformation("Deleting document name ID {Id}", id);
            var response = await _http.DeleteAsync($"/api/documentnames/{id}");
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting document name ID {Id}", id);
            throw;
        }
    }
}
