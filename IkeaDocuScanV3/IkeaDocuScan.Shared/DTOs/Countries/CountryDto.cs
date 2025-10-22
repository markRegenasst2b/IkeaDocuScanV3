namespace IkeaDocuScan.Shared.DTOs.Countries;

/// <summary>
/// Data transfer object for Country
/// </summary>
public class CountryDto
{
    public string CountryCode { get; set; } = string.Empty;
    public string? Name { get; set; }
}
