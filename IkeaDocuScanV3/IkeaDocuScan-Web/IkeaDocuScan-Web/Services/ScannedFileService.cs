using IkeaDocuScan.Shared.Configuration;
using IkeaDocuScan.Shared.DTOs.ScannedFiles;
using IkeaDocuScan.Shared.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace IkeaDocuScan_Web.Services;

/// <summary>
/// Service for managing scanned files from the server folder
/// Implements security checks to prevent path traversal attacks
/// </summary>
public class ScannedFileService : IScannedFileService
{
    private readonly IkeaDocuScanOptions _options;
    private readonly IMemoryCache _cache;
    private readonly ILogger<ScannedFileService> _logger;
    private const string CacheKey = "ScannedFilesList";

    public ScannedFileService(
        IOptions<IkeaDocuScanOptions> options,
        IMemoryCache cache,
        ILogger<ScannedFileService> logger)
    {
        _options = options.Value;
        _cache = cache;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<List<ScannedFileDto>> GetScannedFilesAsync()
    {
        try
        {
            // Check cache first
            if (_options.EnableFileListCaching && _cache.TryGetValue(CacheKey, out List<ScannedFileDto>? cachedFiles))
            {
                _logger.LogDebug("Returning {Count} files from cache", cachedFiles?.Count ?? 0);
                return cachedFiles ?? new List<ScannedFileDto>();
            }

            // Validate folder exists
            if (!Directory.Exists(_options.ScannedFilesPath))
            {
                _logger.LogWarning("Scanned files folder does not exist: {Path}", _options.ScannedFilesPath);
                return new List<ScannedFileDto>();
            }

            _logger.LogInformation("Scanning folder: {Path}", _options.ScannedFilesPath);

            // Get all files
            var directoryInfo = new DirectoryInfo(_options.ScannedFilesPath);
            var files = directoryInfo.GetFiles()
                .Where(f => IsFileAllowed(f.Extension))
                .Select(f => new ScannedFileDto
                {
                    FileName = f.Name,
                    FullPath = f.FullName,
                    SizeBytes = f.Length,
                    CreatedDate = f.CreationTime,
                    ModifiedDate = f.LastWriteTime,
                    Extension = f.Extension
                })
                .OrderByDescending(f => f.ModifiedDate)
                .ToList();

            _logger.LogInformation("Found {Count} scanned files", files.Count);

            // Cache the results
            if (_options.EnableFileListCaching)
            {
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromSeconds(_options.CacheDurationSeconds));

                _cache.Set(CacheKey, files, cacheOptions);
                _logger.LogDebug("Cached {Count} files for {Duration} seconds", files.Count, _options.CacheDurationSeconds);
            }

            return await Task.FromResult(files);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogError(ex, "Access denied to scanned files folder: {Path}", _options.ScannedFilesPath);
            throw new InvalidOperationException("Access denied to scanned files folder. Check folder permissions.", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving scanned files from: {Path}", _options.ScannedFilesPath);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<ScannedFileDto?> GetFileByNameAsync(string fileName)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentNullException(nameof(fileName));
            }

            // Security check
            if (!IsPathSafe(fileName))
            {
                _logger.LogWarning("Unsafe file path detected: {FileName}", fileName);
                return null;
            }

            var filePath = Path.Combine(_options.ScannedFilesPath, fileName);

            if (!File.Exists(filePath))
            {
                _logger.LogWarning("File not found: {FileName}", fileName);
                return null;
            }

            var fileInfo = new FileInfo(filePath);

            return await Task.FromResult(new ScannedFileDto
            {
                FileName = fileInfo.Name,
                FullPath = fileInfo.FullName,
                SizeBytes = fileInfo.Length,
                CreatedDate = fileInfo.CreationTime,
                ModifiedDate = fileInfo.LastWriteTime,
                Extension = fileInfo.Extension
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving file: {FileName}", fileName);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<byte[]?> GetFileContentAsync(string fileName)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentNullException(nameof(fileName));
            }

            // Security check
            if (!IsPathSafe(fileName))
            {
                _logger.LogWarning("Unsafe file path detected: {FileName}", fileName);
                throw new UnauthorizedAccessException($"Access denied to file: {fileName}");
            }

            var filePath = Path.Combine(_options.ScannedFilesPath, fileName);

            if (!File.Exists(filePath))
            {
                _logger.LogWarning("File not found: {FileName}", fileName);
                return null;
            }

            var fileInfo = new FileInfo(filePath);

            // Check file size
            if (fileInfo.Length > _options.MaxFileSizeBytes)
            {
                _logger.LogWarning("File too large: {FileName} ({Size} bytes)", fileName, fileInfo.Length);
                throw new InvalidOperationException($"File is too large. Maximum size: {_options.MaxFileSizeBytes} bytes");
            }

            _logger.LogInformation("Reading file content: {FileName} ({Size} bytes)", fileName, fileInfo.Length);

            return await File.ReadAllBytesAsync(filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading file content: {FileName}", fileName);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<bool> FileExistsAsync(string fileName)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return false;
            }

            // Security check
            if (!IsPathSafe(fileName))
            {
                _logger.LogWarning("Unsafe file path detected: {FileName}", fileName);
                return false;
            }

            var filePath = Path.Combine(_options.ScannedFilesPath, fileName);
            return await Task.FromResult(File.Exists(filePath));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking file existence: {FileName}", fileName);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<Stream?> GetFileStreamAsync(string fileName)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentNullException(nameof(fileName));
            }

            // Security check
            if (!IsPathSafe(fileName))
            {
                _logger.LogWarning("Unsafe file path detected: {FileName}", fileName);
                throw new UnauthorizedAccessException($"Access denied to file: {fileName}");
            }

            var filePath = Path.Combine(_options.ScannedFilesPath, fileName);

            if (!File.Exists(filePath))
            {
                _logger.LogWarning("File not found: {FileName}", fileName);
                return null;
            }

            _logger.LogInformation("Opening file stream: {FileName}", fileName);

            return await Task.FromResult<Stream>(new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error opening file stream: {FileName}", fileName);
            throw;
        }
    }

    /// <summary>
    /// Validate file path to prevent path traversal attacks
    /// </summary>
    private bool IsPathSafe(string fileName)
    {
        try
        {
            // Check for null or empty
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return false;
            }

            // Check for path traversal attempts
            if (fileName.Contains("..") || fileName.Contains("/") || fileName.Contains("\\"))
            {
                _logger.LogWarning("Path traversal attempt detected: {FileName}", fileName);
                return false;
            }

            // Validate against allowed extensions
            var extension = Path.GetExtension(fileName);
            if (!IsFileAllowed(extension))
            {
                _logger.LogWarning("File extension not allowed: {Extension}", extension);
                return false;
            }

            // Construct full path and normalize
            var fullPath = Path.Combine(_options.ScannedFilesPath, fileName);
            var normalizedPath = Path.GetFullPath(fullPath);
            var basePath = Path.GetFullPath(_options.ScannedFilesPath);

            // Ensure the file is within the allowed directory
            if (!normalizedPath.StartsWith(basePath, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("File path outside allowed directory: {Path}", normalizedPath);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating file path: {FileName}", fileName);
            return false;
        }
    }

    /// <summary>
    /// Check if file extension is allowed
    /// </summary>
    private bool IsFileAllowed(string extension)
    {
        if (string.IsNullOrWhiteSpace(extension))
        {
            return false;
        }

        return _options.AllowedFileExtensions.Contains(
            extension.ToLowerInvariant(),
            StringComparer.OrdinalIgnoreCase);
    }
}
