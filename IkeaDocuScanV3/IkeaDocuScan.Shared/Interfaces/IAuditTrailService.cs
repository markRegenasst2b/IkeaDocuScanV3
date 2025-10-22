using IkeaDocuScan.Shared.Enums;

namespace IkeaDocuScan.Shared.Interfaces;

/// <summary>
/// Service interface for managing audit trail entries
/// </summary>
public interface IAuditTrailService
{
    /// <summary>
    /// Log an audit trail entry with the specified action and barcode
    /// </summary>
    /// <param name="action">The action performed</param>
    /// <param name="barCode">The barcode of the document</param>
    /// <param name="details">Optional details about the action</param>
    /// <param name="username">Optional username (if not provided, current user will be used)</param>
    /// <returns>Task representing the async operation</returns>
    Task LogAsync(AuditAction action, string barCode, string? details = null, string? username = null);

    /// <summary>
    /// Log an audit trail entry for a document ID (barcode will be looked up)
    /// </summary>
    /// <param name="action">The action performed</param>
    /// <param name="documentId">The ID of the document</param>
    /// <param name="details">Optional details about the action</param>
    /// <param name="username">Optional username (if not provided, current user will be used)</param>
    /// <returns>Task representing the async operation</returns>
    Task LogByDocumentIdAsync(AuditAction action, int documentId, string? details = null, string? username = null);

    /// <summary>
    /// Log multiple audit entries in a single transaction
    /// </summary>
    /// <param name="action">The action performed</param>
    /// <param name="barCodes">Collection of barcodes</param>
    /// <param name="details">Optional details about the action</param>
    /// <param name="username">Optional username (if not provided, current user will be used)</param>
    /// <returns>Task representing the async operation</returns>
    Task LogBatchAsync(AuditAction action, IEnumerable<string> barCodes, string? details = null, string? username = null);

    /// <summary>
    /// Get audit trail entries for a specific document barcode
    /// </summary>
    /// <param name="barCode">The barcode to query</param>
    /// <param name="limit">Maximum number of entries to return (default: 100)</param>
    /// <returns>List of audit trail entries</returns>
    Task<List<AuditTrailDto>> GetByBarCodeAsync(string barCode, int limit = 100);

    /// <summary>
    /// Get audit trail entries for a specific user
    /// </summary>
    /// <param name="username">The username to query</param>
    /// <param name="limit">Maximum number of entries to return (default: 100)</param>
    /// <returns>List of audit trail entries</returns>
    Task<List<AuditTrailDto>> GetByUserAsync(string username, int limit = 100);

    /// <summary>
    /// Get recent audit trail entries
    /// </summary>
    /// <param name="limit">Maximum number of entries to return (default: 100)</param>
    /// <returns>List of audit trail entries</returns>
    Task<List<AuditTrailDto>> GetRecentAsync(int limit = 100);

    /// <summary>
    /// Get audit trail entries within a date range
    /// </summary>
    /// <param name="startDate">Start date (inclusive)</param>
    /// <param name="endDate">End date (inclusive)</param>
    /// <param name="action">Optional filter by action type</param>
    /// <returns>List of audit trail entries</returns>
    Task<List<AuditTrailDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, AuditAction? action = null);
}

/// <summary>
/// DTO for audit trail entries
/// </summary>
public class AuditTrailDto
{
    public int Id { get; set; }
    public DateTime Timestamp { get; set; }
    public string User { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string? Details { get; set; }
    public string BarCode { get; set; } = string.Empty;
}
