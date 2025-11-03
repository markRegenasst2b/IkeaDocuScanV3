namespace IkeaDocuScan.PdfTools.Models;

/// <summary>
/// Contains metadata information about a PDF document.
/// </summary>
public class PdfMetadata
{
    /// <summary>
    /// Gets or sets the number of pages in the PDF document.
    /// </summary>
    public int NumberOfPages { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the PDF is encrypted.
    /// </summary>
    public bool IsEncrypted { get; set; }

    /// <summary>
    /// Gets or sets the title of the PDF document.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the author of the PDF document.
    /// </summary>
    public string? Author { get; set; }

    /// <summary>
    /// Gets or sets the subject of the PDF document.
    /// </summary>
    public string? Subject { get; set; }

    /// <summary>
    /// Gets or sets the creator application of the PDF document.
    /// </summary>
    public string? Creator { get; set; }

    /// <summary>
    /// Gets or sets the producer application of the PDF document.
    /// </summary>
    public string? Producer { get; set; }
}
