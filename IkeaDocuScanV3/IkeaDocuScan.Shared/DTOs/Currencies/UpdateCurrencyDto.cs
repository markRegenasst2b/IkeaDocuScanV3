using System.ComponentModel.DataAnnotations;

namespace IkeaDocuScan.Shared.DTOs.Currencies;

/// <summary>
/// DTO for updating an existing currency
/// Note: Currency code cannot be changed (it's the primary key)
/// </summary>
public class UpdateCurrencyDto
{
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
