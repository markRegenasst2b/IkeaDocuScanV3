namespace IkeaDocuScan.Shared.DTOs.Documents;

/// <summary>
/// Search results with pagination metadata
/// </summary>
public class DocumentSearchResultDto
{
    /// <summary>
    /// List of documents matching search criteria
    /// </summary>
    public List<DocumentSearchItemDto> Items { get; set; } = new();

    /// <summary>
    /// Total number of documents matching criteria (before pagination, limited by max results)
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Current page number (1-based)
    /// </summary>
    public int CurrentPage { get; set; }

    /// <summary>
    /// Number of items per page
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Total number of pages
    /// </summary>
    public int TotalPages { get; set; }

    /// <summary>
    /// Indicates if the max result limit was reached (e.g., 1000 documents)
    /// </summary>
    public bool MaxLimitReached { get; set; }

    /// <summary>
    /// The maximum result limit applied
    /// </summary>
    public int MaxLimit { get; set; }

    /// <summary>
    /// Has previous page
    /// </summary>
    public bool HasPreviousPage => CurrentPage > 1;

    /// <summary>
    /// Has next page
    /// </summary>
    public bool HasNextPage => CurrentPage < TotalPages;
}
