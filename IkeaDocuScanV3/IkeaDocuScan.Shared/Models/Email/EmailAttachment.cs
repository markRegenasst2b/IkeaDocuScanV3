namespace IkeaDocuScan.Shared.Models.Email;

/// <summary>
/// Represents an email attachment
/// </summary>
public class EmailAttachment
{
    /// <summary>
    /// File name with extension
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// File content as byte array
    /// </summary>
    public byte[] Content { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// MIME content type (e.g., "application/pdf", "image/jpeg")
    /// </summary>
    public string ContentType { get; set; } = "application/octet-stream";
}
