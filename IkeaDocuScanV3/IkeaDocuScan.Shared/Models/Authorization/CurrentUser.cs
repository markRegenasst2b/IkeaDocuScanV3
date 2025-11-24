namespace IkeaDocuScan.Shared.Models.Authorization;

/// <summary>
/// Represents the current authenticated user with their permissions
/// </summary>
public class CurrentUser
{
    public int UserId { get; set; }
    public string AccountName { get; set; } = string.Empty;
    public bool IsSuperUser { get; set; }
    public bool HasAccess { get; set; }
    public DateTime? LastLogon { get; set; }

    /// <summary>
    /// List of document type IDs the user can access (null = all)
    /// </summary>
    public List<int>? AllowedDocumentTypes { get; set; }

    /// <summary>
    /// Check if user can access a specific document type
    /// </summary>
    public bool CanAccessDocumentType(int? documentTypeId)
    {
        if (IsSuperUser)
            return true;

        if (!HasAccess)
            return false;

        if (!documentTypeId.HasValue)
            return true; // No document type restriction

        if (AllowedDocumentTypes == null || AllowedDocumentTypes.Count == 0)
            return true; // No restrictions = access all

        return AllowedDocumentTypes.Contains(documentTypeId.Value);
    }

    /// <summary>
    /// Check if user can access a document based on its document type
    /// </summary>
    public bool CanAccessDocument(int? documentTypeId)
    {
        if (IsSuperUser)
            return true;

        if (!HasAccess)
            return false;

        return CanAccessDocumentType(documentTypeId);
    }
}
