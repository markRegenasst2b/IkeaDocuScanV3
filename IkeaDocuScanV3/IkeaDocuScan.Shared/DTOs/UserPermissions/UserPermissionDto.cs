namespace IkeaDocuScan.Shared.DTOs.UserPermissions;

/// <summary>
/// Data transfer object for UserPermission
/// </summary>
public class UserPermissionDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string AccountName { get; set; } = string.Empty;
    public int? DocumentTypeId { get; set; }
    public string? DocumentTypeName { get; set; }
    public int? CounterPartyId { get; set; }
    public string? CounterPartyName { get; set; }
    public string? CounterPartyNoAlpha { get; set; }
    public string? CountryCode { get; set; }
    public string? CountryName { get; set; }
}

/// <summary>
/// DTO for creating a new UserPermission
/// </summary>
public class CreateUserPermissionDto
{
    public int UserId { get; set; }
    public int? DocumentTypeId { get; set; }
    public int? CounterPartyId { get; set; }
    public string? CountryCode { get; set; }
}

/// <summary>
/// DTO for updating an existing UserPermission
/// </summary>
public class UpdateUserPermissionDto
{
    public int Id { get; set; }
    public int? DocumentTypeId { get; set; }
    public int? CounterPartyId { get; set; }
    public string? CountryCode { get; set; }
}
