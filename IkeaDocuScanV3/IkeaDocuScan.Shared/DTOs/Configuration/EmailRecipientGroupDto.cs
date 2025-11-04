namespace IkeaDocuScan.Shared.DTOs.Configuration;

/// <summary>
/// DTO for email recipient groups
/// </summary>
public class EmailRecipientGroupDto
{
    public int GroupId { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public string GroupKey { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public List<EmailRecipientDto> Recipients { get; set; } = new();
}

/// <summary>
/// DTO for individual email recipients
/// </summary>
public class EmailRecipientDto
{
    public int RecipientId { get; set; }
    public string EmailAddress { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
}

/// <summary>
/// DTO for updating email recipients
/// </summary>
public class UpdateEmailRecipientsDto
{
    public string GroupKey { get; set; } = string.Empty;
    public List<string> EmailAddresses { get; set; } = new();
}
