namespace IkeaDocuScan.Shared.DTOs.Configuration;

/// <summary>
/// DTO for bulk SMTP configuration update
/// All settings are updated atomically and tested together
/// </summary>
public class SmtpConfigurationDto
{
    public required string SmtpHost { get; set; }
    public int SmtpPort { get; set; } = 587;
    public bool UseSsl { get; set; } = true;
    public string? SmtpUsername { get; set; }
    public string? SmtpPassword { get; set; }
    public required string FromAddress { get; set; }
    public string? FromName { get; set; }
}
