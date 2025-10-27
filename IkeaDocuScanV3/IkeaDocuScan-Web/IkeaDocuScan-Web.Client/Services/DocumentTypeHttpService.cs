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

    public async Task<List<DocumentTypeDto>> GetAllIncludingDisabledAsync()
    {
        try
        {
            _logger.LogInformation("Fetching all document types including disabled from API");
            var result = await _http.GetFromJsonAsync<List<DocumentTypeDto>>("/api/documenttypes/all");
            return result ?? new List<DocumentTypeDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching all document types");
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

    public async Task<DocumentTypeDto> CreateAsync(CreateDocumentTypeDto dto)
    {
        try
        {
            _logger.LogInformation("Creating document type: {Name}", dto.DtName);
            var response = await _http.PostAsJsonAsync("/api/documenttypes", dto);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                var errorMessage = TryExtractErrorMessage(errorContent);
                throw new HttpRequestException(errorMessage);
            }

            var documentType = await response.Content.ReadFromJsonAsync<DocumentTypeDto>();
            return documentType ?? throw new InvalidOperationException("Failed to deserialize created document type");
        }
        catch (HttpRequestException)
        {
            throw; // Re-throw HttpRequestException with our custom message
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating document type");
            throw;
        }
    }

    public async Task<DocumentTypeDto> UpdateAsync(int id, UpdateDocumentTypeDto dto)
    {
        try
        {
            _logger.LogInformation("Updating document type ID: {Id}", id);
            var response = await _http.PutAsJsonAsync($"/api/documenttypes/{id}", dto);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                var errorMessage = TryExtractErrorMessage(errorContent);
                throw new HttpRequestException(errorMessage);
            }

            var documentType = await response.Content.ReadFromJsonAsync<DocumentTypeDto>();
            return documentType ?? throw new InvalidOperationException("Failed to deserialize updated document type");
        }
        catch (HttpRequestException)
        {
            throw; // Re-throw HttpRequestException with our custom message
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating document type ID: {Id}", id);
            throw;
        }
    }

    public async Task DeleteAsync(int id)
    {
        try
        {
            _logger.LogInformation("Deleting document type ID: {Id}", id);
            var response = await _http.DeleteAsync($"/api/documenttypes/{id}");

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                var errorMessage = TryExtractErrorMessage(errorContent);
                throw new HttpRequestException(errorMessage);
            }
        }
        catch (HttpRequestException)
        {
            throw; // Re-throw HttpRequestException with our custom message
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting document type ID: {Id}", id);
            throw;
        }
    }

    public async Task<bool> IsInUseAsync(int id)
    {
        try
        {
            _logger.LogInformation("Checking if document type ID {Id} is in use", id);
            var response = await _http.GetFromJsonAsync<UsageResponse>($"/api/documenttypes/{id}/usage");
            return response?.IsInUse ?? false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking usage for document type ID {Id}", id);
            throw;
        }
    }

    public async Task<(int documentCount, int documentNameCount, int userPermissionCount)> GetUsageCountAsync(int id)
    {
        try
        {
            _logger.LogInformation("Getting usage count for document type ID {Id}", id);
            var response = await _http.GetFromJsonAsync<UsageResponse>($"/api/documenttypes/{id}/usage");
            return (response?.DocumentCount ?? 0, response?.DocumentNameCount ?? 0, response?.UserPermissionCount ?? 0);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting usage count for document type ID {Id}", id);
            throw;
        }
    }

    private string TryExtractErrorMessage(string errorContent)
    {
        try
        {
            var options = new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            var errorResponse = System.Text.Json.JsonSerializer.Deserialize<ErrorResponse>(errorContent, options);
            if (errorResponse?.Error != null)
            {
                return errorResponse.Error;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to deserialize error response: {Content}", errorContent);
        }

        return !string.IsNullOrEmpty(errorContent) ? errorContent : "An error occurred";
    }

    private class UsageResponse
    {
        public int DtId { get; set; }
        public bool IsInUse { get; set; }
        public int DocumentCount { get; set; }
        public int DocumentNameCount { get; set; }
        public int UserPermissionCount { get; set; }
        public int TotalUsage { get; set; }
    }

    private class ErrorResponse
    {
        public string? Error { get; set; }
    }
}
