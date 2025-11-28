using IkeaDocuScan.Infrastructure.Data;
using IkeaDocuScan.Infrastructure.Entities;
using IkeaDocuScan.Shared.DTOs.UserPermissions;
using IkeaDocuScan.Shared.Interfaces;
using IkeaDocuScan.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace IkeaDocuScan_Web.Services;

/// <summary>
/// Service for UserPermission operations
/// </summary>
public class UserPermissionService : IUserPermissionService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<UserPermissionService> _logger;

    public UserPermissionService(
        IDbContextFactory<AppDbContext> contextFactory,
        IHttpContextAccessor httpContextAccessor,
        ILogger<UserPermissionService> logger)
    {
        _contextFactory = contextFactory;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task<List<DocuScanUserDto>> GetAllUsersAsync(string? accountNameFilter = null)
    {
        _logger.LogInformation("Fetching all DocuScan users with filter: {Filter}", accountNameFilter ?? "none");

        await using var context = await _contextFactory.CreateDbContextAsync();

        var query = context.DocuScanUsers
            .Include(u => u.UserPermissions)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(accountNameFilter))
        {
            var filter = accountNameFilter.ToLower();
            query = query.Where(u => u.AccountName.ToLower().Contains(filter));
        }

        var users = await query
            .OrderBy(u => u.AccountName)
            .ToListAsync();

        return users.Select(u => new DocuScanUserDto
        {
            UserId = u.UserId,
            AccountName = u.AccountName,
            LastLogon = u.LastLogon,
            IsSuperUser = u.IsSuperUser,
            CreatedOn = u.CreatedOn,
            ModifiedOn = u.ModifiedOn,
            PermissionCount = u.UserPermissions.Count
        }).ToList();
    }

    public async Task<List<UserPermissionDto>> GetAllAsync(string? accountNameFilter = null)
    {
        _logger.LogInformation("Fetching all user permissions with filter: {Filter}", accountNameFilter ?? "none");

        await using var context = await _contextFactory.CreateDbContextAsync();

        var query = context.UserPermissions
            .Include(up => up.User)
            .Include(up => up.DocumentType)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(accountNameFilter))
        {
            var filter = accountNameFilter.ToLower();
            query = query.Where(up => up.User.AccountName.ToLower().Contains(filter));
        }

        var permissions = await query
            .OrderBy(up => up.User.AccountName)
            .ThenBy(up => up.Id)
            .ToListAsync();

        return permissions.Select(MapToDto).ToList();
    }

    public async Task<UserPermissionDto?> GetByIdAsync(int id)
    {
        _logger.LogInformation("Fetching user permission with ID: {Id}", id);

        await using var context = await _contextFactory.CreateDbContextAsync();
        var permission = await context.UserPermissions
            .Include(up => up.User)
            .Include(up => up.DocumentType)
            .AsNoTracking()
            .FirstOrDefaultAsync(up => up.Id == id);

        if (permission == null)
        {
            _logger.LogWarning("User permission with ID {Id} not found", id);
            return null;
        }

        return MapToDto(permission);
    }

    public async Task<List<UserPermissionDto>> GetByUserIdAsync(int userId)
    {
        _logger.LogInformation("Fetching permissions for user ID: {UserId}", userId);

        await using var context = await _contextFactory.CreateDbContextAsync();
        var permissions = await context.UserPermissions
            .Include(up => up.User)
            .Include(up => up.DocumentType)
            .AsNoTracking()
            .Where(up => up.UserId == userId)
            .OrderBy(up => up.Id)
            .ToListAsync();

        return permissions.Select(MapToDto).ToList();
    }

    public async Task<UserPermissionDto> CreateAsync(CreateUserPermissionDto dto)
    {
        _logger.LogInformation("Creating new user permission for user ID: {UserId}", dto.UserId);

        await using var context = await _contextFactory.CreateDbContextAsync();

        // Validate user exists
        var userExists = await context.DocuScanUsers.AnyAsync(u => u.UserId == dto.UserId);
        if (!userExists)
        {
            throw new ValidationException($"User with ID {dto.UserId} not found");
        }

        var entity = new UserPermission
        {
            UserId = dto.UserId,
            DocumentTypeId = dto.DocumentTypeId
        };

        context.UserPermissions.Add(entity);
        await context.SaveChangesAsync();

        _logger.LogInformation("Created user permission with ID: {Id}", entity.Id);

        // Reload with navigation properties
        return (await GetByIdAsync(entity.Id))!;
    }

    public async Task<UserPermissionDto> UpdateAsync(UpdateUserPermissionDto dto)
    {
        _logger.LogInformation("Updating user permission with ID: {Id}", dto.Id);

        await using var context = await _contextFactory.CreateDbContextAsync();
        var entity = await context.UserPermissions.FindAsync(dto.Id);

        if (entity == null)
        {
            throw new ValidationException($"UserPermission with ID {dto.Id} not found");
        }

        entity.DocumentTypeId = dto.DocumentTypeId;

        await context.SaveChangesAsync();

        _logger.LogInformation("Updated user permission with ID: {Id}", dto.Id);

        return (await GetByIdAsync(entity.Id))!;
    }

    public async Task DeleteAsync(int id)
    {
        _logger.LogInformation("Deleting user permission with ID: {Id}", id);

        await using var context = await _contextFactory.CreateDbContextAsync();
        var entity = await context.UserPermissions.FindAsync(id);

        if (entity == null)
        {
            throw new ValidationException($"UserPermission with ID {id} not found");
        }

        context.UserPermissions.Remove(entity);
        await context.SaveChangesAsync();

        _logger.LogInformation("Deleted user permission with ID: {Id}", id);
    }

    public async Task DeleteUserAsync(int userId)
    {
        _logger.LogInformation("Deleting user with ID: {UserId} and all their permissions", userId);

        await using var context = await _contextFactory.CreateDbContextAsync();

        // Check if user exists
        var user = await context.DocuScanUsers
            .Include(u => u.UserPermissions)
            .FirstOrDefaultAsync(u => u.UserId == userId);

        if (user == null)
        {
            throw new ValidationException($"User with ID {userId} not found");
        }

        // Delete all user permissions first (cascade delete should handle this, but being explicit)
        if (user.UserPermissions.Any())
        {
            context.UserPermissions.RemoveRange(user.UserPermissions);
            _logger.LogInformation("Deleting {Count} permissions for user {UserId}", user.UserPermissions.Count, userId);
        }

        // Delete the user
        context.DocuScanUsers.Remove(user);
        await context.SaveChangesAsync();

        _logger.LogInformation("Deleted user with ID: {UserId} and all their permissions", userId);
    }

    public async Task<DocuScanUserDto> CreateUserAsync(CreateDocuScanUserDto dto)
    {
        _logger.LogInformation("Creating new DocuScan user: {AccountName}", dto.AccountName);

        await using var context = await _contextFactory.CreateDbContextAsync();

        // Check if user with same account name already exists
        var existsByAccountName = await context.DocuScanUsers
            .AnyAsync(u => u.AccountName == dto.AccountName);

        if (existsByAccountName)
        {
            throw new ValidationException($"User with account name '{dto.AccountName}' already exists");
        }

        var entity = new DocuScanUser
        {
            AccountName = dto.AccountName,
            IsSuperUser = dto.IsSuperUser,
            CreatedOn = DateTime.UtcNow,
            LastLogon = null,
            ModifiedOn = null
        };

        context.DocuScanUsers.Add(entity);
        await context.SaveChangesAsync();

        _logger.LogInformation("Created DocuScan user with ID: {UserId}", entity.UserId);

        return new DocuScanUserDto
        {
            UserId = entity.UserId,
            AccountName = entity.AccountName,
            LastLogon = entity.LastLogon,
            IsSuperUser = entity.IsSuperUser,
            CreatedOn = entity.CreatedOn,
            ModifiedOn = entity.ModifiedOn,
            PermissionCount = 0
        };
    }

    public async Task<DocuScanUserDto> UpdateUserAsync(UpdateDocuScanUserDto dto)
    {
        _logger.LogInformation("Updating DocuScan user ID: {UserId}", dto.UserId);

        await using var context = await _contextFactory.CreateDbContextAsync();

        var entity = await context.DocuScanUsers
            .Include(u => u.UserPermissions)
            .FirstOrDefaultAsync(u => u.UserId == dto.UserId);

        if (entity == null)
        {
            throw new ValidationException($"User with ID {dto.UserId} not found");
        }

        // Check if another user has the same account name
        var duplicateAccountName = await context.DocuScanUsers
            .AnyAsync(u => u.AccountName == dto.AccountName && u.UserId != dto.UserId);

        if (duplicateAccountName)
        {
            throw new ValidationException($"Another user with account name '{dto.AccountName}' already exists");
        }

        entity.AccountName = dto.AccountName;
        entity.IsSuperUser = dto.IsSuperUser;
        entity.ModifiedOn = DateTime.UtcNow;

        await context.SaveChangesAsync();

        _logger.LogInformation("Updated DocuScan user ID: {UserId}", dto.UserId);

        return new DocuScanUserDto
        {
            UserId = entity.UserId,
            AccountName = entity.AccountName,
            LastLogon = entity.LastLogon,
            IsSuperUser = entity.IsSuperUser,
            CreatedOn = entity.CreatedOn,
            ModifiedOn = entity.ModifiedOn,
            PermissionCount = entity.UserPermissions.Count
        };
    }

    public async Task<BatchUpdateResultDto> BatchUpdateDocumentTypePermissionsAsync(BatchUpdateDocumentTypePermissionsDto dto)
    {
        _logger.LogInformation("Batch updating document type permissions for user ID: {UserId}, DocumentTypes: {Count}",
            dto.UserId, dto.DocumentTypeIds.Count);

        await using var context = await _contextFactory.CreateDbContextAsync();

        // Validate user exists
        var userExists = await context.DocuScanUsers.AnyAsync(u => u.UserId == dto.UserId);
        if (!userExists)
        {
            throw new ValidationException($"User with ID {dto.UserId} not found");
        }

        // Get current permissions for this user (only those with DocumentTypeId set)
        var currentPermissions = await context.UserPermissions
            .Where(p => p.UserId == dto.UserId && p.DocumentTypeId.HasValue)
            .ToListAsync();

        var currentDocTypeIds = currentPermissions
            .Where(p => p.DocumentTypeId.HasValue)
            .Select(p => p.DocumentTypeId!.Value)
            .ToHashSet();

        var requestedDocTypeIds = dto.DocumentTypeIds.ToHashSet();

        // Calculate what to add and remove
        var toAdd = requestedDocTypeIds.Except(currentDocTypeIds).ToList();
        var toRemove = currentDocTypeIds.Except(requestedDocTypeIds).ToList();

        int added = 0;
        int removed = 0;

        // Remove permissions no longer needed
        if (toRemove.Count > 0)
        {
            var permissionsToRemove = currentPermissions
                .Where(p => p.DocumentTypeId.HasValue && toRemove.Contains(p.DocumentTypeId.Value))
                .ToList();

            context.UserPermissions.RemoveRange(permissionsToRemove);
            removed = permissionsToRemove.Count;
            _logger.LogInformation("Removing {Count} permissions for user {UserId}", removed, dto.UserId);
        }

        // Add new permissions
        if (toAdd.Count > 0)
        {
            var newPermissions = toAdd.Select(docTypeId => new UserPermission
            {
                UserId = dto.UserId,
                DocumentTypeId = docTypeId
            }).ToList();

            context.UserPermissions.AddRange(newPermissions);
            added = newPermissions.Count;
            _logger.LogInformation("Adding {Count} permissions for user {UserId}", added, dto.UserId);
        }

        await context.SaveChangesAsync();

        // Get final count
        var totalPermissions = await context.UserPermissions
            .CountAsync(p => p.UserId == dto.UserId);

        _logger.LogInformation("Batch update complete for user {UserId}: Added={Added}, Removed={Removed}, Total={Total}",
            dto.UserId, added, removed, totalPermissions);

        return new BatchUpdateResultDto
        {
            PermissionsAdded = added,
            PermissionsRemoved = removed,
            TotalPermissions = totalPermissions
        };
    }

    public async Task<List<UserPermissionDto>> GetMyPermissionsAsync()
    {
        var username = _httpContextAccessor.HttpContext?.User?.Identity?.Name;

        if (string.IsNullOrEmpty(username))
        {
            _logger.LogWarning("GetMyPermissionsAsync called but no authenticated user found");
            return new List<UserPermissionDto>();
        }

        _logger.LogInformation("Fetching permissions for current user: {Username}", username);

        await using var context = await _contextFactory.CreateDbContextAsync();

        var permissions = await context.UserPermissions
            .Include(up => up.User)
            .Include(up => up.DocumentType)
            .AsNoTracking()
            .Where(up => up.User.AccountName == username)
            .OrderBy(up => up.Id)
            .ToListAsync();

        return permissions.Select(MapToDto).ToList();
    }

    private static UserPermissionDto MapToDto(UserPermission entity)
    {
        return new UserPermissionDto
        {
            Id = entity.Id,
            UserId = entity.UserId,
            AccountName = entity.User?.AccountName ?? string.Empty,
            DocumentTypeId = entity.DocumentTypeId,
            DocumentTypeName = entity.DocumentType?.DtName
        };
    }
}
