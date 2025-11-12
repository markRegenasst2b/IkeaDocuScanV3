namespace IkeaDocuScan.Shared.DTOs.Testing;

/// <summary>
/// DTO for test identity profile (DEVELOPMENT ONLY)
/// </summary>
public class TestIdentityProfile
{
    public string ProfileId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string? Email { get; set; }
    public List<string> ADGroups { get; set; } = new();
    public bool IsSuperUser { get; set; }
    public bool HasAccess { get; set; }
    public int? DatabaseUserId { get; set; }
    public string Description { get; set; } = string.Empty;
}
/// <summary>
/// DTO for current test identity status
/// </summary>
public class TestIdentityStatus
{
    public bool IsActive { get; set; }
    public TestIdentityProfile? CurrentProfile { get; set; }
    public List<string> ActiveClaims { get; set; } = new();
}
