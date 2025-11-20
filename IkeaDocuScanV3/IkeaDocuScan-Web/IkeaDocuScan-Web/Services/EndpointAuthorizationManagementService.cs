using IkeaDocuScan.Infrastructure.Data;
using IkeaDocuScan.Infrastructure.Entities;
using IkeaDocuScan.Shared.DTOs;
using IkeaDocuScan.Shared.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IkeaDocuScan_Web.Services;

/// <summary>
/// Service for managing endpoint authorization permissions and cache invalidation
/// Implements CRUD operations for endpoint registry and role permissions
/// </summary>
public class EndpointAuthorizationManagementService : IEndpointAuthorizationManagementService
{
    private readonly AppDbContext _context;
    private readonly IEndpointAuthorizationService _authService;
    private readonly ILogger<EndpointAuthorizationManagementService> _logger;

    public EndpointAuthorizationManagementService(
        AppDbContext context,
        IEndpointAuthorizationService authService,
        ILogger<EndpointAuthorizationManagementService> logger)
    {
        _context = context;
        _authService = authService;
        _logger = logger;
    }

    public async Task<List<EndpointRegistryDto>> GetAllEndpointsAsync()
    {
        var endpoints = await _context.EndpointRegistries
            .Include(e => e.RolePermissions)
            .Where(e => e.IsActive)
            .OrderBy(e => e.Category)
            .ThenBy(e => e.Route)
            .ToListAsync();

        return endpoints.Select(MapToDto).ToList();
    }

    public async Task<EndpointRegistryDto?> GetEndpointByIdAsync(int endpointId)
    {
        var endpoint = await _context.EndpointRegistries
            .Include(e => e.RolePermissions)
            .FirstOrDefaultAsync(e => e.EndpointId == endpointId);

        return endpoint == null ? null : MapToDto(endpoint);
    }

    public async Task<EndpointRegistryDto?> GetEndpointByRouteAsync(string httpMethod, string route)
    {
        var endpoint = await _context.EndpointRegistries
            .Include(e => e.RolePermissions)
            .FirstOrDefaultAsync(e => e.HttpMethod == httpMethod && e.Route == route);

        return endpoint == null ? null : MapToDto(endpoint);
    }

    public async Task<List<string>> GetEndpointRolesAsync(int endpointId)
    {
        var roles = await _context.EndpointRolePermissions
            .Where(rp => rp.EndpointId == endpointId)
            .Select(rp => rp.RoleName)
            .ToListAsync();

        return roles;
    }

    public async Task UpdateEndpointRolesAsync(int endpointId, List<string> roleNames, string changedBy, string? changeReason = null)
    {
        // Validate permission change
        var validationErrors = await ValidatePermissionChangeAsync(endpointId, roleNames);
        if (validationErrors.Any())
        {
            throw new InvalidOperationException($"Validation failed: {string.Join(", ", validationErrors)}");
        }

        // Get existing endpoint
        var endpoint = await _context.EndpointRegistries
            .Include(e => e.RolePermissions)
            .FirstOrDefaultAsync(e => e.EndpointId == endpointId);

        if (endpoint == null)
        {
            throw new KeyNotFoundException($"Endpoint with ID {endpointId} not found");
        }

        // Get old roles for audit logging
        var oldRoles = endpoint.RolePermissions.Select(rp => rp.RoleName).OrderBy(r => r).ToList();
        var newRoles = roleNames.OrderBy(r => r).ToList();

        // Remove existing permissions
        _context.EndpointRolePermissions.RemoveRange(endpoint.RolePermissions);

        // Add new permissions
        foreach (var roleName in roleNames)
        {
            endpoint.RolePermissions.Add(new EndpointRolePermission
            {
                EndpointId = endpointId,
                RoleName = roleName
            });
        }

        // Update modified timestamp
        endpoint.ModifiedOn = DateTime.UtcNow;

        // Log the permission change
        var auditLog = new PermissionChangeAuditLog
        {
            EndpointId = endpointId,
            ChangedBy = changedBy,
            ChangeType = "RolePermissionUpdate",
            OldValue = string.Join(", ", oldRoles),
            NewValue = string.Join(", ", newRoles),
            ChangeReason = changeReason,
            ChangedOn = DateTime.UtcNow
        };

        _context.PermissionChangeAuditLogs.Add(auditLog);

        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Endpoint {EndpointId} ({Route}) permissions updated by {ChangedBy}. Old: [{OldRoles}], New: [{NewRoles}]",
            endpointId, endpoint.Route, changedBy, string.Join(", ", oldRoles), string.Join(", ", newRoles));

        // Invalidate cache to pick up new permissions immediately
        await InvalidateCacheAsync();
    }

    public async Task<EndpointRegistryDto> CreateEndpointAsync(CreateEndpointRegistryDto dto, string createdBy)
    {
        // Check if endpoint already exists
        var existing = await _context.EndpointRegistries
            .FirstOrDefaultAsync(e => e.HttpMethod == dto.HttpMethod && e.Route == dto.Route);

        if (existing != null)
        {
            throw new InvalidOperationException($"Endpoint {dto.HttpMethod} {dto.Route} already exists");
        }

        var endpoint = new EndpointRegistry
        {
            HttpMethod = dto.HttpMethod,
            Route = dto.Route,
            EndpointName = dto.EndpointName,
            Description = dto.Description,
            Category = dto.Category,
            IsActive = dto.IsActive,
            CreatedOn = DateTime.UtcNow
        };

        _context.EndpointRegistries.Add(endpoint);
        await _context.SaveChangesAsync();

        // Add role permissions
        foreach (var roleName in dto.AllowedRoles)
        {
            _context.EndpointRolePermissions.Add(new EndpointRolePermission
            {
                EndpointId = endpoint.EndpointId,
                RoleName = roleName
            });
        }

        // Log creation
        var auditLog = new PermissionChangeAuditLog
        {
            EndpointId = endpoint.EndpointId,
            ChangedBy = createdBy,
            ChangeType = "EndpointCreated",
            NewValue = string.Join(", ", dto.AllowedRoles),
            ChangeReason = "New endpoint registered",
            ChangedOn = DateTime.UtcNow
        };

        _context.PermissionChangeAuditLogs.Add(auditLog);
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Endpoint created: {Method} {Route} with roles [{Roles}] by {CreatedBy}",
            endpoint.HttpMethod, endpoint.Route, string.Join(", ", dto.AllowedRoles), createdBy);

        await InvalidateCacheAsync();

        return MapToDto(await _context.EndpointRegistries
            .Include(e => e.RolePermissions)
            .FirstAsync(e => e.EndpointId == endpoint.EndpointId));
    }

    public async Task<EndpointRegistryDto> UpdateEndpointAsync(int endpointId, UpdateEndpointRegistryDto dto, string modifiedBy)
    {
        var endpoint = await _context.EndpointRegistries
            .Include(e => e.RolePermissions)
            .FirstOrDefaultAsync(e => e.EndpointId == endpointId);

        if (endpoint == null)
        {
            throw new KeyNotFoundException($"Endpoint with ID {endpointId} not found");
        }

        var oldName = endpoint.EndpointName;
        var oldDescription = endpoint.Description;
        var oldCategory = endpoint.Category;

        endpoint.EndpointName = dto.EndpointName;
        endpoint.Description = dto.Description;
        endpoint.Category = dto.Category;
        endpoint.IsActive = dto.IsActive;
        endpoint.ModifiedOn = DateTime.UtcNow;

        // Log metadata change
        var auditLog = new PermissionChangeAuditLog
        {
            EndpointId = endpointId,
            ChangedBy = modifiedBy,
            ChangeType = "EndpointMetadataUpdate",
            OldValue = $"Name: {oldName}, Desc: {oldDescription}, Cat: {oldCategory}",
            NewValue = $"Name: {dto.EndpointName}, Desc: {dto.Description}, Cat: {dto.Category}",
            ChangeReason = "Metadata update",
            ChangedOn = DateTime.UtcNow
        };

        _context.PermissionChangeAuditLogs.Add(auditLog);
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Endpoint {EndpointId} ({Route}) metadata updated by {ModifiedBy}",
            endpointId, endpoint.Route, modifiedBy);

        await InvalidateCacheAsync();

        return MapToDto(endpoint);
    }

    public async Task DeactivateEndpointAsync(int endpointId, string deactivatedBy, string? reason = null)
    {
        var endpoint = await _context.EndpointRegistries
            .Include(e => e.RolePermissions)
            .FirstOrDefaultAsync(e => e.EndpointId == endpointId);

        if (endpoint == null)
        {
            throw new KeyNotFoundException($"Endpoint with ID {endpointId} not found");
        }

        endpoint.IsActive = false;
        endpoint.ModifiedOn = DateTime.UtcNow;

        var auditLog = new PermissionChangeAuditLog
        {
            EndpointId = endpointId,
            ChangedBy = deactivatedBy,
            ChangeType = "EndpointDeactivated",
            OldValue = "Active: true",
            NewValue = "Active: false",
            ChangeReason = reason,
            ChangedOn = DateTime.UtcNow
        };

        _context.PermissionChangeAuditLogs.Add(auditLog);
        await _context.SaveChangesAsync();

        _logger.LogWarning(
            "Endpoint {EndpointId} ({Route}) deactivated by {DeactivatedBy}. Reason: {Reason}",
            endpointId, endpoint.Route, deactivatedBy, reason);

        await InvalidateCacheAsync();
    }

    public async Task ReactivateEndpointAsync(int endpointId, string reactivatedBy, string? reason = null)
    {
        var endpoint = await _context.EndpointRegistries
            .Include(e => e.RolePermissions)
            .FirstOrDefaultAsync(e => e.EndpointId == endpointId);

        if (endpoint == null)
        {
            throw new KeyNotFoundException($"Endpoint with ID {endpointId} not found");
        }

        endpoint.IsActive = true;
        endpoint.ModifiedOn = DateTime.UtcNow;

        var auditLog = new PermissionChangeAuditLog
        {
            EndpointId = endpointId,
            ChangedBy = reactivatedBy,
            ChangeType = "EndpointReactivated",
            OldValue = "Active: false",
            NewValue = "Active: true",
            ChangeReason = reason,
            ChangedOn = DateTime.UtcNow
        };

        _context.PermissionChangeAuditLogs.Add(auditLog);
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Endpoint {EndpointId} ({Route}) reactivated by {ReactivatedBy}. Reason: {Reason}",
            endpointId, endpoint.Route, reactivatedBy, reason);

        await InvalidateCacheAsync();
    }

    public async Task<List<string>> GetAvailableRolesAsync()
    {
        // Get all unique role names from EndpointRolePermission table
        var roles = await _context.EndpointRolePermissions
            .Select(rp => rp.RoleName)
            .Distinct()
            .OrderBy(r => r)
            .ToListAsync();

        return roles;
    }

    public async Task<List<PermissionChangeAuditLogDto>> GetAuditLogAsync(
        int? endpointId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null)
    {
        var query = _context.PermissionChangeAuditLogs
            .Include(al => al.Endpoint)
            .AsQueryable();

        if (endpointId.HasValue)
        {
            query = query.Where(al => al.EndpointId == endpointId.Value);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(al => al.ChangedOn >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(al => al.ChangedOn <= toDate.Value);
        }

        var logs = await query
            .OrderByDescending(al => al.ChangedOn)
            .ToListAsync();

        return logs.Select(al => new PermissionChangeAuditLogDto
        {
            AuditId = al.AuditId,
            EndpointId = al.EndpointId,
            ChangedBy = al.ChangedBy,
            ChangeType = al.ChangeType,
            OldValue = al.OldValue,
            NewValue = al.NewValue,
            ChangeReason = al.ChangeReason,
            ChangedOn = al.ChangedOn,
            EndpointRoute = al.Endpoint?.Route,
            HttpMethod = al.Endpoint?.HttpMethod,
            EndpointName = al.Endpoint?.EndpointName
        }).ToList();
    }

    public async Task InvalidateCacheAsync()
    {
        // Call the authorization service's cache invalidation method
        await _authService.InvalidateCacheAsync();

        _logger.LogInformation("Authorization cache invalidated manually");
    }

    public async Task SyncEndpointsFromCodeAsync()
    {
        // This is a placeholder for future auto-discovery functionality
        // Would use reflection to find all endpoint definitions and sync to database
        _logger.LogWarning("SyncEndpointsFromCodeAsync not yet implemented - manual seeding required");

        await Task.CompletedTask;
    }

    public async Task<List<string>> ValidatePermissionChangeAsync(int endpointId, List<string> newRoles)
    {
        var errors = new List<string>();

        // Rule 1: At least one role must be assigned
        if (!newRoles.Any())
        {
            errors.Add("At least one role must be assigned to the endpoint");
        }

        // Rule 2: Verify all role names are valid (non-empty, reasonable length)
        foreach (var role in newRoles)
        {
            if (string.IsNullOrWhiteSpace(role))
            {
                errors.Add("Role names cannot be empty or whitespace");
            }
            else if (role.Length > 50)
            {
                errors.Add($"Role name '{role}' exceeds maximum length of 50 characters");
            }
        }

        // Rule 3: Check for duplicate role names
        var duplicates = newRoles.GroupBy(r => r).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
        if (duplicates.Any())
        {
            errors.Add($"Duplicate roles detected: {string.Join(", ", duplicates)}");
        }

        // Rule 4: Verify endpoint exists
        var endpoint = await _context.EndpointRegistries
            .FirstOrDefaultAsync(e => e.EndpointId == endpointId);

        if (endpoint == null)
        {
            errors.Add($"Endpoint with ID {endpointId} does not exist");
        }

        return errors;
    }

    #region Helper Methods

    private static EndpointRegistryDto MapToDto(EndpointRegistry entity)
    {
        return new EndpointRegistryDto
        {
            EndpointId = entity.EndpointId,
            HttpMethod = entity.HttpMethod,
            Route = entity.Route,
            EndpointName = entity.EndpointName,
            Description = entity.Description,
            Category = entity.Category,
            IsActive = entity.IsActive,
            CreatedOn = entity.CreatedOn,
            ModifiedOn = entity.ModifiedOn,
            AllowedRoles = entity.RolePermissions.Select(rp => rp.RoleName).OrderBy(r => r).ToList()
        };
    }

    #endregion
}
