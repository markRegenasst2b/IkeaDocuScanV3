namespace IkeaDocuScan.Shared.DTOs.ScannedFiles;

/// <summary>
/// DTO representing a scanned file from the server folder
/// </summary>
public class ScannedFileDto
{
    public string FileName { get; set; } = string.Empty;
    public string FullPath { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime ModifiedDate { get; set; }
    public string Extension { get; set; } = string.Empty;

    /// <summary>
    /// Formatted file size (e.g., "2.5 MB")
    /// </summary>
    public string SizeFormatted => FormatFileSize(SizeBytes);

    /// <summary>
    /// File type description based on extension
    /// </summary>
    public string FileType => GetFileType();

    private static string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = bytes;
        int order = 0;

        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }

        return $"{len:0.##} {sizes[order]}";
    }

    private string GetFileType()
    {
        return Extension.ToLowerInvariant() switch
        {
            ".pdf" => "PDF Document",
            ".jpg" or ".jpeg" => "JPEG Image",
            ".png" => "PNG Image",
            ".tif" or ".tiff" => "TIFF Image",
            ".bmp" => "Bitmap Image",
            _ => "Unknown"
        };
    }

    /// <summary>
    /// Get CSS icon class based on file type
    /// </summary>
    public string GetIconClass()
    {
        return Extension.ToLowerInvariant() switch
        {
            ".pdf" => "fa fa-file-pdf text-danger",
            ".jpg" or ".jpeg" or ".png" or ".bmp" => "fa fa-file-image text-primary",
            ".tif" or ".tiff" => "fa fa-file-image text-info",
            _ => "fa fa-file text-secondary"
        };
    }
}
