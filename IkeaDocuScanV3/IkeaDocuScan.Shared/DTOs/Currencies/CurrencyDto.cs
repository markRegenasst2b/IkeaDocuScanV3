namespace IkeaDocuScan.Shared.DTOs.Currencies;

/// <summary>
/// DTO for Currency entity
/// </summary>
public class CurrencyDto
{
    /// <summary>
    /// Currency code (ISO 4217, e.g., USD, EUR, GBP)
    /// </summary>
    public string CurrencyCode { get; set; } = string.Empty;

    /// <summary>
    /// Currency name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Number of decimal places for this currency
    /// </summary>
    public int DecimalPlaces { get; set; } = 2;

    /// <summary>
    /// Display text combining code and name
    /// </summary>
    public string DisplayText => $"{CurrencyCode} - {Name}";
}
