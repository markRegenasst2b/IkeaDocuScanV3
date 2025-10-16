namespace IkeaDocuScan.Shared.DTOs.Documents;

public class UpdateDocumentDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int CategoryId { get; set; }
}
