namespace IkeaDocuScan.Shared.Configuration;

/// <summary>
/// Configuration options for IkeaDocuScan application
/// Bound from appsettings.json "IkeaDocuScan" section
/// </summary>
public class IkeaDocuScanOptions
{
    /// <summary>
    /// Configuration section name in appsettings.json
    /// </summary>
    public const string SectionName = "IkeaDocuScan";

    /// <summary>
    /// Path to the folder containing scanned documents
    /// Can be local path (C:\ScannedDocuments) or UNC path (\\FileServer\Share\ScannedDocuments)
    /// </summary>
    public string ScannedFilesPath { get; set; } = string.Empty;

    /// <summary>
    /// List of allowed file extensions for scanned documents
    /// </summary>
    public string[] AllowedFileExtensions { get; set; } = new[]
    {
        ".pdf",
        ".jpg",
        ".jpeg",
        ".png",
        ".tif",
        ".tiff",
        ".bmp"
    };

    /// <summary>
    /// Maximum file size in bytes (default: 50MB)
    /// </summary>
    public long MaxFileSizeBytes { get; set; } = 52428800;

    /// <summary>
    /// Enable file caching for better performance
    /// </summary>
    public bool EnableFileListCaching { get; set; } = true;

    /// <summary>
    /// File list cache duration in seconds (default: 60 seconds)
    /// </summary>
    public int CacheDurationSeconds { get; set; } = 60;

    /// <summary>
    /// Validate configuration options
    /// </summary>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(ScannedFilesPath))
        {
            throw new InvalidOperationException(
                "ScannedFilesPath is required. Configure it in appsettings.json or appsettings.Local.json");
        }

        if (AllowedFileExtensions == null || AllowedFileExtensions.Length == 0)
        {
            throw new InvalidOperationException("At least one allowed file extension must be configured");
        }

        if (MaxFileSizeBytes <= 0)
        {
            throw new InvalidOperationException("MaxFileSizeBytes must be greater than 0");
        }
    }
}
