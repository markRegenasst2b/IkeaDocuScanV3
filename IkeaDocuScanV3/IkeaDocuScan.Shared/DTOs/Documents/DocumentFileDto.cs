namespace IkeaDocuScan.Shared.DTOs.Documents;

public class DocumentFileDto
{
    public byte[] FileBytes { get; set; } = Array.Empty<byte>();
    public string? FileName { get; set; }
    public string? ContentType { get; set; }
}
