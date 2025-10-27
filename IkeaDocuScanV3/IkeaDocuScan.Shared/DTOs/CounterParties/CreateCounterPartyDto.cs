using System.ComponentModel.DataAnnotations;

namespace IkeaDocuScan.Shared.DTOs.CounterParties;

/// <summary>
/// DTO for creating a new counter party
/// </summary>
public class CreateCounterPartyDto
{
    [Required(ErrorMessage = "Name is required")]
    [StringLength(128, ErrorMessage = "Name cannot exceed 128 characters")]
    public string Name { get; set; } = string.Empty;

    [StringLength(32, ErrorMessage = "Counter party number cannot exceed 32 characters")]
    public string? CounterPartyNoAlpha { get; set; }

    [StringLength(255, ErrorMessage = "Address cannot exceed 255 characters")]
    public string? Address { get; set; }

    [Required(ErrorMessage = "City is required")]
    [StringLength(128, ErrorMessage = "City cannot exceed 128 characters")]
    public string City { get; set; } = string.Empty;

    [Required(ErrorMessage = "Country is required")]
    [StringLength(2, MinimumLength = 2, ErrorMessage = "Country code must be exactly 2 characters")]
    public string Country { get; set; } = string.Empty;

    [StringLength(128, ErrorMessage = "Affiliated to cannot exceed 128 characters")]
    public string? AffiliatedTo { get; set; }

    public bool DisplayAtCheckIn { get; set; } = true;

    public DateTime Since { get; set; } = DateTime.Now;

    [StringLength(255, ErrorMessage = "Comments cannot exceed 255 characters")]
    public string? Comments { get; set; }
}
