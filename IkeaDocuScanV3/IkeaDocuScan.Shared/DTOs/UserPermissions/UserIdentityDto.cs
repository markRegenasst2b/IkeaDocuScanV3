namespace IkeaDocuScan.Shared.DTOs.UserPermissions;

/// <summary>
/// Data transfer object for user identity and claims information
/// </summary>
public class UserIdentityDto
{
    public string? UserName { get; set; }
    public string? AuthenticationType { get; set; }
    public bool IsAuthenticated { get; set; }
    public List<UserClaimDto> Claims { get; set; } = new();
}

/// <summary>
/// Data transfer object for user claims
/// </summary>
public class UserClaimDto
{
    public string Type { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}
