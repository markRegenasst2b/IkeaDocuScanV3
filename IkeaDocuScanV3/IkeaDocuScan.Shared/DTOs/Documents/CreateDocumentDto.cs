namespace IkeaDocuScan.Shared.DTOs.Documents;

public class CreateDocumentDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int CategoryId { get; set; }
}
