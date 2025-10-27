using System.ComponentModel.DataAnnotations;

namespace IkeaDocuScan.Shared.DTOs.Currencies;

/// <summary>
/// DTO for creating a new currency
/// </summary>
public class CreateCurrencyDto
{
    /// <summary>
    /// Currency code (ISO 4217, e.g., USD, EUR, GBP) - max 3 characters
    /// </summary>
    [Required(ErrorMessage = "Currency code is required")]
    [StringLength(3, MinimumLength = 3, ErrorMessage = "Currency code must be exactly 3 characters")]
    [RegularExpression(@"^[A-Z]{3}$", ErrorMessage = "Currency code must be 3 uppercase letters")]
    public string CurrencyCode { get; set; } = string.Empty;

    /// <summary>
    /// Currency name
    /// </summary>
    [Required(ErrorMessage = "Currency name is required")]
    [StringLength(128, ErrorMessage = "Currency name cannot exceed 128 characters")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Number of decimal places for this currency (typically 0-4)
    /// </summary>
    [Range(0, 4, ErrorMessage = "Decimal places must be between 0 and 4")]
    public int DecimalPlaces { get; set; } = 2;
}
