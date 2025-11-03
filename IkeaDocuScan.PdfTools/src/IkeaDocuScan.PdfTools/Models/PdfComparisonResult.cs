namespace IkeaDocuScan.PdfTools.Models;

/// <summary>
/// Contains the results of comparing two PDF documents.
/// </summary>
public class PdfComparisonResult
{
    /// <summary>
    /// Gets or sets the total character count of the first PDF's text content.
    /// </summary>
    public int Text1Length { get; set; }

    /// <summary>
    /// Gets or sets the total character count of the second PDF's text content.
    /// </summary>
    public int Text2Length { get; set; }

    /// <summary>
    /// Gets or sets the number of lines in the first PDF's text content.
    /// </summary>
    public int Text1Lines { get; set; }

    /// <summary>
    /// Gets or sets the number of lines in the second PDF's text content.
    /// </summary>
    public int Text2Lines { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the text content of both PDFs is identical.
    /// </summary>
    public bool IsIdentical { get; set; }

    /// <summary>
    /// Gets or sets the absolute difference in character count between the two PDFs.
    /// </summary>
    public int LengthDifference { get; set; }

    /// <summary>
    /// Gets or sets the similarity ratio between the two PDFs (0.0 to 1.0).
    /// A value of 1.0 indicates identical content, 0.0 indicates completely different content.
    /// </summary>
    public double SimilarityRatio { get; set; }

    /// <summary>
    /// Gets the similarity as a percentage (0 to 100).
    /// </summary>
    public double SimilarityPercentage => SimilarityRatio * 100.0;
}
