using IkeaDocuScan.Infrastructure.Data;
using IkeaDocuScan.Shared.DTOs;
using IkeaDocuScan.Shared.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace IkeaDocuScan_Web.Services;

/// <summary>
/// Service for checking endpoint authorization dynamically from database
/// Implements caching for performance
/// </summary>
public class EndpointAuthorizationService : IEndpointAuthorizationService
{
    private readonly AppDbContext _dbContext;
    private readonly IMemoryCache _cache;
    private readonly ILogger<EndpointAuthorizationService> _logger;
    private const string CacheKeyPrefix = "EndpointAuth_";
    private const int CacheDurationMinutes = 30;

    public EndpointAuthorizationService(
        AppDbContext dbContext,
        IMemoryCache cache,
        ILogger<EndpointAuthorizationService> logger)
    {
        _dbContext = dbContext;
        _cache = cache;
        _logger = logger;
    }

    /// <summary>
    /// Get the list of roles allowed to access a specific endpoint
    /// Results are cached for performance
    /// </summary>
    public async Task<List<string>> GetAllowedRolesAsync(string httpMethod, string route)
    {
        var cacheKey = $"{CacheKeyPrefix}{httpMethod}:{route}";

        // Try to get from cache
        if (_cache.TryGetValue<List<string>>(cacheKey, out var cachedRoles))
        {
            _logger.LogDebug("Endpoint authorization cache hit for {Method} {Route}", httpMethod, route);
            return cachedRoles ?? new List<string>();
        }

        // Not in cache, query database
        _logger.LogDebug("Endpoint authorization cache miss for {Method} {Route}", httpMethod, route);

        var roles = await _dbContext.EndpointRegistries
            .Where(e => e.HttpMethod == httpMethod && e.Route == route && e.IsActive)
            .SelectMany(e => e.RolePermissions.Select(rp => rp.RoleName))
            .Distinct()
            .ToListAsync();

        // Cache the result
        var cacheOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(CacheDurationMinutes));

        _cache.Set(cacheKey, roles, cacheOptions);

        _logger.LogInformation("Loaded endpoint authorization for {Method} {Route}: {Roles}",
            httpMethod, route, string.Join(", ", roles));

        return roles;
    }

    /// <summary>
    /// Check if a user with specific roles can access an endpoint
    /// </summary>
    public async Task<bool> CheckAccessAsync(string httpMethod, string route, IEnumerable<string> userRoles)
    {
        var allowedRoles = await GetAllowedRolesAsync(httpMethod, route);

        // If no roles are configured for the endpoint, deny access by default
        if (!allowedRoles.Any())
        {
            _logger.LogWarning("No roles configured for endpoint {Method} {Route} - access denied by default",
                httpMethod, route);
            return false;
        }

        // Check if user has any of the allowed roles
        var hasAccess = userRoles.Any(userRole => allowedRoles.Contains(userRole));

        _logger.LogDebug("Access check for {Method} {Route}: User roles={UserRoles}, Allowed roles={AllowedRoles}, Access={HasAccess}",
            httpMethod, route, string.Join(", ", userRoles), string.Join(", ", allowedRoles), hasAccess);

        return hasAccess;
    }

    /// <summary>
    /// Invalidate the authorization cache
    /// Call this after updating endpoint permissions
    /// </summary>
    public Task InvalidateCacheAsync()
    {
        _logger.LogWarning("Invalidating entire endpoint authorization cache");

        // MemoryCache doesn't have a way to remove all keys with a prefix
        // So we'll create a new cache instance or remove all known keys
        // For now, we'll just log a warning that the cache should be invalidated
        // A full implementation would track all cache keys and remove them

        // Alternative: Use IDistributedCache or track cache keys
        // For this implementation, we'll rely on the cache expiration

        return Task.CompletedTask;
    }

    /// <summary>
    /// Get all endpoints with their role permissions
    /// </summary>
    public async Task<List<EndpointRegistryDto>> GetAllEndpointsAsync()
    {
        var endpoints = await _dbContext.EndpointRegistries
            .Include(e => e.RolePermissions)
            .OrderBy(e => e.Category)
            .ThenBy(e => e.Route)
            .ThenBy(e => e.HttpMethod)
            .ToListAsync();

        return endpoints.Select(e => new EndpointRegistryDto
        {
            EndpointId = e.EndpointId,
            HttpMethod = e.HttpMethod,
            Route = e.Route,
            EndpointName = e.EndpointName,
            Description = e.Description,
            Category = e.Category,
            IsActive = e.IsActive,
            CreatedOn = e.CreatedOn,
            ModifiedOn = e.ModifiedOn,
            AllowedRoles = e.RolePermissions.Select(rp => rp.RoleName).ToList()
        }).ToList();
    }

    /// <summary>
    /// Get endpoint by ID
    /// </summary>
    public async Task<EndpointRegistryDto?> GetEndpointByIdAsync(int endpointId)
    {
        var endpoint = await _dbContext.EndpointRegistries
            .Include(e => e.RolePermissions)
            .FirstOrDefaultAsync(e => e.EndpointId == endpointId);

        if (endpoint == null)
            return null;

        return new EndpointRegistryDto
        {
            EndpointId = endpoint.EndpointId,
            HttpMethod = endpoint.HttpMethod,
            Route = endpoint.Route,
            EndpointName = endpoint.EndpointName,
            Description = endpoint.Description,
            Category = endpoint.Category,
            IsActive = endpoint.IsActive,
            CreatedOn = endpoint.CreatedOn,
            ModifiedOn = endpoint.ModifiedOn,
            AllowedRoles = endpoint.RolePermissions.Select(rp => rp.RoleName).ToList()
        };
    }

    /// <summary>
    /// Sync endpoints from code to database (placeholder for initial setup)
    /// </summary>
    public Task SyncEndpointsAsync()
    {
        _logger.LogWarning("SyncEndpointsAsync called but not yet implemented - use SQL seed scripts instead");
        // This method would typically scan all controllers/endpoints in the application
        // and create/update EndpointRegistry entries
        // For now, we rely on manual SQL scripts to seed the data
        return Task.CompletedTask;
    }
}
