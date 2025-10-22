using IkeaDocuScan.Shared.DTOs.ScannedFiles;

namespace IkeaDocuScan.Shared.Interfaces;

/// <summary>
/// Service interface for managing scanned files from the server folder
/// </summary>
public interface IScannedFileService
{
    /// <summary>
    /// Get all scanned files from the configured folder
    /// </summary>
    /// <returns>List of scanned files</returns>
    Task<List<ScannedFileDto>> GetScannedFilesAsync();

    /// <summary>
    /// Get a specific file by name
    /// </summary>
    /// <param name="fileName">Name of the file</param>
    /// <returns>File DTO or null if not found</returns>
    Task<ScannedFileDto?> GetFileByNameAsync(string fileName);

    /// <summary>
    /// Get file content as byte array
    /// </summary>
    /// <param name="fileName">Name of the file</param>
    /// <returns>File content or null if not found</returns>
    Task<byte[]?> GetFileContentAsync(string fileName);

    /// <summary>
    /// Check if a file exists in the scanned files folder
    /// </summary>
    /// <param name="fileName">Name of the file</param>
    /// <returns>True if file exists</returns>
    Task<bool> FileExistsAsync(string fileName);

    /// <summary>
    /// Get file stream for reading (useful for large files)
    /// </summary>
    /// <param name="fileName">Name of the file</param>
    /// <returns>File stream or null if not found</returns>
    Task<Stream?> GetFileStreamAsync(string fileName);
}
