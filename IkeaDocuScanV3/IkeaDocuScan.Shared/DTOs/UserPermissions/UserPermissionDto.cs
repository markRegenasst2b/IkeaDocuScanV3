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
}

/// <summary>
/// DTO for creating a new UserPermission
/// </summary>
public class CreateUserPermissionDto
{
    public int UserId { get; set; }
    public int? DocumentTypeId { get; set; }
}

/// <summary>
/// DTO for updating an existing UserPermission
/// </summary>
public class UpdateUserPermissionDto
{
    public int Id { get; set; }
    public int? DocumentTypeId { get; set; }
}

/// <summary>
/// DTO for batch updating document type permissions for a user
/// </summary>
public class BatchUpdateDocumentTypePermissionsDto
{
    /// <summary>
    /// The user ID to update permissions for
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// List of document type IDs the user should have access to.
    /// Permissions not in this list will be removed.
    /// Permissions in this list that don't exist will be created.
    /// </summary>
    public List<int> DocumentTypeIds { get; set; } = new();
}

/// <summary>
/// Result of a batch update operation
/// </summary>
public class BatchUpdateResultDto
{
    public int PermissionsAdded { get; set; }
    public int PermissionsRemoved { get; set; }
    public int TotalPermissions { get; set; }
}
