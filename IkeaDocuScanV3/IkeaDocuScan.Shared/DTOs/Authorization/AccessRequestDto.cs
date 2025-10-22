namespace IkeaDocuScan.Shared.DTOs.Authorization;

/// <summary>
/// DTO for access request
/// </summary>
public class AccessRequestDto
{
    public string Username { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public DateTime RequestedAt { get; set; }
}

/// <summary>
/// Result of access request
/// </summary>
public class AccessRequestResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool UserCreated { get; set; }
    public bool UserAlreadyExists { get; set; }
}
