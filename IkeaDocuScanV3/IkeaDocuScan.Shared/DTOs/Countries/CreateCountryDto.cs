using System.ComponentModel.DataAnnotations;

namespace IkeaDocuScan.Shared.DTOs.Countries;

/// <summary>
/// DTO for creating a new country
/// </summary>
public class CreateCountryDto
{
    /// <summary>
    /// Country code (ISO 3166-1 alpha-2, e.g., US, GB, DE) - max 2 characters
    /// </summary>
    [Required(ErrorMessage = "Country code is required")]
    [StringLength(2, MinimumLength = 2, ErrorMessage = "Country code must be exactly 2 characters")]
    [RegularExpression(@"^[A-Z]{2}$", ErrorMessage = "Country code must be 2 uppercase letters")]
    public string CountryCode { get; set; } = string.Empty;

    /// <summary>
    /// Country name
    /// </summary>
    [Required(ErrorMessage = "Country name is required")]
    [StringLength(128, ErrorMessage = "Country name cannot exceed 128 characters")]
    public string Name { get; set; } = string.Empty;
}
