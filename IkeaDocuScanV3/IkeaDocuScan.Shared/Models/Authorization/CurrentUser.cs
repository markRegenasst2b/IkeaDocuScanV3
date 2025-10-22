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
    /// List of counter party IDs the user can access (null = all)
    /// </summary>
    public List<int>? AllowedCounterParties { get; set; }

    /// <summary>
    /// List of country codes the user can access (null = all)
    /// </summary>
    public List<string>? AllowedCountries { get; set; }

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
    /// Check if user can access a specific counter party
    /// </summary>
    public bool CanAccessCounterParty(int? counterPartyId)
    {
        if (IsSuperUser)
            return true;

        if (!HasAccess)
            return false;

        if (!counterPartyId.HasValue)
            return true;

        if (AllowedCounterParties == null || AllowedCounterParties.Count == 0)
            return true;

        return AllowedCounterParties.Contains(counterPartyId.Value);
    }

    /// <summary>
    /// Check if user can access a specific country
    /// </summary>
    public bool CanAccessCountry(string? countryCode)
    {
        if (IsSuperUser)
            return true;

        if (!HasAccess)
            return false;

        if (string.IsNullOrWhiteSpace(countryCode))
            return true;

        if (AllowedCountries == null || AllowedCountries.Count == 0)
            return true;

        return AllowedCountries.Contains(countryCode, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Check if user can access a document based on all its attributes
    /// </summary>
    public bool CanAccessDocument(int? documentTypeId, int? counterPartyId, string? countryCode)
    {
        if (IsSuperUser)
            return true;

        if (!HasAccess)
            return false;

        return CanAccessDocumentType(documentTypeId) &&
               CanAccessCounterParty(counterPartyId) &&
               CanAccessCountry(countryCode);
    }
}
