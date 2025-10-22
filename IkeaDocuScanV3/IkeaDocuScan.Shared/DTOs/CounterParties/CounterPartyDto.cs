namespace IkeaDocuScan.Shared.DTOs.CounterParties;

/// <summary>
/// Data transfer object for CounterParty
/// </summary>
public class CounterPartyDto
{
    public int CounterPartyId { get; set; }
    public string? Name { get; set; }
    public string? CounterPartyNoAlpha { get; set; }
    public string? Address { get; set; }
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string? AffiliatedTo { get; set; }
    public bool DisplayAtCheckIn { get; set; }
    public DateTime Since { get; set; }
    public string? Comments { get; set; }
}
