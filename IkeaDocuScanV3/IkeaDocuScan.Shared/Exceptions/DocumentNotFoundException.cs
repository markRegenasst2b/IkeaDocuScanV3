namespace IkeaDocuScan.Shared.Exceptions;

public class DocumentNotFoundException : BusinessException
{
    public int DocumentId { get; }

    public DocumentNotFoundException(int id)
        : base($"Document with ID {id} was not found")
    {
        DocumentId = id;
    }

    public DocumentNotFoundException(string message)
        : base(message)
    {
        DocumentId = 0;
    }
}
