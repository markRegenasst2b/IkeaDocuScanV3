namespace IkeaDocuScan.Shared.DTOs;

/// <summary>
/// DTO for EndpointRegistry entity
/// </summary>
public class EndpointRegistryDto
{
    public int EndpointId { get; set; }
    public string HttpMethod { get; set; } = string.Empty;
    public string Route { get; set; } = string.Empty;
    public string EndpointName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Category { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime? ModifiedOn { get; set; }

    /// <summary>
    /// List of role names that have access to this endpoint
    /// </summary>
    public List<string> AllowedRoles { get; set; } = new();
}

/// <summary>
/// DTO for creating a new endpoint registry entry
/// </summary>
public class CreateEndpointRegistryDto
{
    public string HttpMethod { get; set; } = string.Empty;
    public string Route { get; set; } = string.Empty;
    public string EndpointName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Category { get; set; }
    public bool IsActive { get; set; } = true;
    public List<string> AllowedRoles { get; set; } = new();
}

/// <summary>
/// DTO for updating an existing endpoint registry entry
/// </summary>
public class UpdateEndpointRegistryDto
{
    public string EndpointName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Category { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>
/// DTO for updating endpoint role permissions
/// </summary>
public class UpdateEndpointRolesDto
{
    public int EndpointId { get; set; }
    public List<string> RoleNames { get; set; } = new();
    public string ChangedBy { get; set; } = string.Empty;
    public string? ChangeReason { get; set; }
}

/// <summary>
/// DTO for checking endpoint access
/// </summary>
public class EndpointAccessCheckDto
{
    public string HttpMethod { get; set; } = string.Empty;
    public string Route { get; set; } = string.Empty;
}

/// <summary>
/// DTO for endpoint access check result
/// </summary>
public class EndpointAccessCheckResult
{
    public bool HasAccess { get; set; }
    public List<string> RequiredRoles { get; set; } = new();
    public List<string> UserRoles { get; set; } = new();
}
