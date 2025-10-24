namespace IkeaDocuScan_Web.Client.Models;

/// <summary>
/// Represents a third party (counter party) that can be associated with a document.
/// Used in the dual-listbox selector for Available/Selected third parties.
/// </summary>
public class ThirdPartyItem
{
    /// <summary>
    /// Counter Party ID (can be numeric or alphanumeric)
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Counter Party display name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Counter Party alpha-numeric code
    /// </summary>
    public string? CounterPartyNoAlpha { get; set; }

    /// <summary>
    /// City location
    /// </summary>
    public string? City { get; set; }

    /// <summary>
    /// Country code
    /// </summary>
    public string? Country { get; set; }

    /// <summary>
    /// Indicates if this item is currently selected
    /// </summary>
    public bool IsSelected { get; set; }

    /// <summary>
    /// Display text for the listbox (Name - City, Country)
    /// </summary>
    public string DisplayText =>
        string.IsNullOrEmpty(City) && string.IsNullOrEmpty(Country)
            ? Name
            : $"{Name} - {City}, {Country}";

    public override bool Equals(object? obj)
    {
        return obj is ThirdPartyItem other && Id == other.Id;
    }

    public override int GetHashCode()
    {
        return Id?.GetHashCode() ?? 0;
    }
}
