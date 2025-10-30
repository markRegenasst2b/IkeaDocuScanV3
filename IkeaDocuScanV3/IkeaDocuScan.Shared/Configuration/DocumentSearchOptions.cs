namespace IkeaDocuScan.Shared.Configuration;

/// <summary>
/// Configuration options for document search functionality
/// Bound from appsettings.json "DocumentSearch" section
/// </summary>
public class DocumentSearchOptions
{
    /// <summary>
    /// Configuration section name in appsettings.json
    /// </summary>
    public const string SectionName = "DocumentSearch";

    /// <summary>
    /// Maximum number of documents to retrieve in a search (default: 1000)
    /// Applied before pagination to prevent performance issues
    /// </summary>
    public int MaxResults { get; set; } = 1000;

    /// <summary>
    /// Default page size for search results (default: 25)
    /// </summary>
    public int DefaultPageSize { get; set; } = 25;

    /// <summary>
    /// Available page size options for users to select (default: 10, 25, 100)
    /// </summary>
    public int[] PageSizeOptions { get; set; } = new[] { 10, 25, 100 };

    /// <summary>
    /// Validate configuration options
    /// </summary>
    public void Validate()
    {
        if (MaxResults <= 0)
        {
            throw new InvalidOperationException("MaxResults must be greater than 0");
        }

        if (DefaultPageSize <= 0)
        {
            throw new InvalidOperationException("DefaultPageSize must be greater than 0");
        }

        if (PageSizeOptions == null || PageSizeOptions.Length == 0)
        {
            throw new InvalidOperationException("At least one page size option must be configured");
        }

        if (!PageSizeOptions.Contains(DefaultPageSize))
        {
            throw new InvalidOperationException("DefaultPageSize must be one of the PageSizeOptions");
        }
    }
}
