using System.ComponentModel.DataAnnotations;

namespace IkeaDocuScan.Shared.DTOs.Countries;

/// <summary>
/// DTO for updating an existing country
/// Note: Country code cannot be changed (it's the primary key)
/// </summary>
public class UpdateCountryDto
{
    /// <summary>
    /// Country name
    /// </summary>
    [Required(ErrorMessage = "Country name is required")]
    [StringLength(128, ErrorMessage = "Country name cannot exceed 128 characters")]
    public string Name { get; set; } = string.Empty;
}
