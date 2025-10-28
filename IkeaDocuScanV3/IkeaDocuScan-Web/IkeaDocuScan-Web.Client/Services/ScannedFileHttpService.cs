using IkeaDocuScan.Shared.DTOs.ScannedFiles;
using IkeaDocuScan.Shared.Interfaces;
using System.Net.Http.Json;

namespace IkeaDocuScan_Web.Client.Services;

/// <summary>
/// HTTP client service for Scanned File operations
/// Implements IScannedFileService interface to call server APIs
/// </summary>
public class ScannedFileHttpService : IScannedFileService
{
    private readonly HttpClient _http;
    private readonly ILogger<ScannedFileHttpService> _logger;

    public ScannedFileHttpService(HttpClient http, ILogger<ScannedFileHttpService> logger)
    {
        _http = http;
        _logger = logger;
    }

    public async Task<List<ScannedFileDto>> GetScannedFilesAsync()
    {
        try
        {
            _logger.LogInformation("Fetching scanned files from API");
            var result = await _http.GetFromJsonAsync<List<ScannedFileDto>>("/api/scannedfiles");
            return result ?? new List<ScannedFileDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching scanned files");
            throw;
        }
    }

    public async Task<ScannedFileDto?> GetFileByNameAsync(string fileName)
    {
        try
        {
            _logger.LogInformation("Fetching scanned file {FileName} from API", fileName);
            return await _http.GetFromJsonAsync<ScannedFileDto>($"/api/scannedfiles/{Uri.EscapeDataString(fileName)}");
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Scanned file {FileName} not found", fileName);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching scanned file {FileName}", fileName);
            throw;
        }
    }

    public async Task<byte[]?> GetFileContentAsync(string fileName)
    {
        try
        {
            _logger.LogInformation("Fetching content for scanned file {FileName} from API", fileName);
            return await _http.GetByteArrayAsync($"/api/scannedfiles/{Uri.EscapeDataString(fileName)}/content");
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Scanned file content {FileName} not found", fileName);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching scanned file content {FileName}", fileName);
            throw;
        }
    }

    public async Task<bool> FileExistsAsync(string fileName)
    {
        try
        {
            _logger.LogInformation("Checking if scanned file {FileName} exists", fileName);
            var response = await _http.GetAsync($"/api/scannedfiles/{Uri.EscapeDataString(fileName)}/exists");
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<bool>();
                return result;
            }
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if scanned file {FileName} exists", fileName);
            return false;
        }
    }

    public async Task<Stream?> GetFileStreamAsync(string fileName)
    {
        try
        {
            _logger.LogInformation("Fetching stream for scanned file {FileName} from API", fileName);
            var response = await _http.GetAsync($"/api/scannedfiles/{Uri.EscapeDataString(fileName)}/stream", HttpCompletionOption.ResponseHeadersRead);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStreamAsync();
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching scanned file stream {FileName}", fileName);
            throw;
        }
    }

    public async Task<bool> DeleteFileAsync(string fileName)
    {
        try
        {
            _logger.LogInformation("Deleting scanned file {FileName}", fileName);
            var response = await _http.DeleteAsync($"/api/scannedfiles/{Uri.EscapeDataString(fileName)}");

            if (response.IsSuccessStatusCode)
            {
                return true;
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to delete scanned file {FileName}: {Error}", fileName, errorContent);
            throw new HttpRequestException($"Failed to delete file: {errorContent}");
        }
        catch (HttpRequestException)
        {
            throw; // Re-throw HTTP exceptions
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting scanned file {FileName}", fileName);
            throw;
        }
    }
}
