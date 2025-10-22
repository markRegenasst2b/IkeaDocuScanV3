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
    private readonly ILogger<UserPermissionService> _logger;

    public UserPermissionService(
        IDbContextFactory<AppDbContext> contextFactory,
        ILogger<UserPermissionService> logger)
    {
        _contextFactory = contextFactory;
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
            UserIdentifier = u.UserIdentifier,
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
            .Include(up => up.CounterParty)
            .Include(up => up.CountryCodeNavigation)
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
            .Include(up => up.CounterParty)
            .Include(up => up.CountryCodeNavigation)
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
            .Include(up => up.CounterParty)
            .Include(up => up.CountryCodeNavigation)
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
            DocumentTypeId = dto.DocumentTypeId,
            CounterPartyId = dto.CounterPartyId,
            CountryCode = dto.CountryCode
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
        entity.CounterPartyId = dto.CounterPartyId;
        entity.CountryCode = dto.CountryCode;

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

    private static UserPermissionDto MapToDto(UserPermission entity)
    {
        return new UserPermissionDto
        {
            Id = entity.Id,
            UserId = entity.UserId,
            AccountName = entity.User?.AccountName ?? string.Empty,
            DocumentTypeId = entity.DocumentTypeId,
            DocumentTypeName = entity.DocumentType?.DtName,
            CounterPartyId = entity.CounterPartyId,
            CounterPartyName = entity.CounterParty?.Name,
            CounterPartyNoAlpha = entity.CounterParty?.CounterPartyNoAlpha,
            CountryCode = entity.CountryCode,
            CountryName = entity.CountryCodeNavigation?.Name
        };
    }
}
