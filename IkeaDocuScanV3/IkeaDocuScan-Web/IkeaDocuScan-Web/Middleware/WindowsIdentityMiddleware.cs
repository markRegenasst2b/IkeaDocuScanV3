using System.Runtime.Versioning;
using System.Security.Claims;
using System.Security.Principal;
using IkeaDocuScan.Infrastructure.Data;
using IkeaDocuScan.Shared.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace IkeaDocuScan_Web.Middleware;

[SupportedOSPlatform("windows")]
public class WindowsIdentityMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<WindowsIdentityMiddleware> _logger;
    private readonly IkeaDocuScanOptions _options;

    public WindowsIdentityMiddleware(
        RequestDelegate next,
        ILogger<WindowsIdentityMiddleware> logger,
        IOptions<IkeaDocuScanOptions> options)
    {
        _next = next;
        _logger = logger;
        _options = options.Value;
    }

    public async Task InvokeAsync(HttpContext context, AppDbContext dbContext)
    {
        // Get the current Windows identity
        var windowsIdentity = WindowsIdentity.GetCurrent();

        _logger.LogInformation("WindowsIdentityMiddleware: Current user is {Name}, IsAuthenticated: {IsAuthenticated}",
            windowsIdentity.Name,
            windowsIdentity.IsAuthenticated);

        // If HttpContext.User is not authenticated but we have a Windows identity, use it
        if (context.User?.Identity?.IsAuthenticated != true && windowsIdentity.IsAuthenticated)
        {
            _logger.LogInformation("Setting HttpContext.User to Windows identity: {Name}", windowsIdentity.Name);

            // Load user permissions from database
            var username = windowsIdentity.Name;
            var userPermissions = await LoadUserPermissionsAsync(dbContext, username);

            // Create claims from Windows identity
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, windowsIdentity.Name),
                new Claim(ClaimTypes.NameIdentifier, windowsIdentity.User?.Value ?? windowsIdentity.Name)
            };

            // Add permission claims if user exists
            if (userPermissions != null)
            {
                claims.Add(new Claim("UserId", userPermissions.UserId.ToString()));
                claims.Add(new Claim("IsSuperUser", userPermissions.IsSuperUser.ToString()));
                claims.Add(new Claim("HasAccess", userPermissions.HasAccess.ToString()));

                if (userPermissions.IsSuperUser)
                {
                    claims.Add(new Claim(ClaimTypes.Role, "SuperUser"));
                }

                _logger.LogInformation("Added permission claims for user {Username}: SuperUser={IsSuperUser}, HasAccess={HasAccess}",
                    username, userPermissions.IsSuperUser, userPermissions.HasAccess);
            }
            else
            {
                // User not found - no access
                claims.Add(new Claim("HasAccess", "false"));
                _logger.LogWarning("User {Username} not found in DocuScanUser table", username);
            }

            // Add AD group role claims
            AddADGroupClaims(windowsIdentity, claims);

            var identity = new ClaimsIdentity(claims, "Windows");
            context.User = new ClaimsPrincipal(identity);
        }

        await _next(context);
    }

    private async Task<UserPermissionInfo?> LoadUserPermissionsAsync(AppDbContext dbContext, string username)
    {
        try
        {
            var user = await dbContext.DocuScanUsers
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.AccountName == username);

            if (user == null)
                return null;

            // If super user, has full access
            if (user.IsSuperUser)
            {
                return new UserPermissionInfo
                {
                    UserId = user.UserId,
                    IsSuperUser = true,
                    HasAccess = true
                };
            }

            // Check if user has any permissions
            var hasPermissions = await dbContext.UserPermissions
                .AsNoTracking()
                .AnyAsync(p => p.UserId == user.UserId);

            return new UserPermissionInfo
            {
                UserId = user.UserId,
                IsSuperUser = false,
                HasAccess = hasPermissions
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading user permissions for {Username}", username);
            return null;
        }
    }

    /// <summary>
    /// Add AD group role claims based on Windows group membership
    /// </summary>
    private void AddADGroupClaims(WindowsIdentity windowsIdentity, List<Claim> claims)
    {
        try
        {
            var principal = new WindowsPrincipal(windowsIdentity);

            // Check Reader group
            if (!string.IsNullOrWhiteSpace(_options.ADGroupReader))
            {
                if (principal.IsInRole(_options.ADGroupReader))
                {
                    claims.Add(new Claim(ClaimTypes.Role, "Reader"));
                    _logger.LogInformation("User {Username} is in AD group {GroupName}",
                        windowsIdentity.Name, _options.ADGroupReader);
                }
            }

            // Check Publisher group
            if (!string.IsNullOrWhiteSpace(_options.ADGroupPublisher))
            {
                if (principal.IsInRole(_options.ADGroupPublisher))
                {
                    claims.Add(new Claim(ClaimTypes.Role, "Publisher"));
                    _logger.LogInformation("User {Username} is in AD group {GroupName}",
                        windowsIdentity.Name, _options.ADGroupPublisher);
                }
            }

            // Check ADAdmin AD group (replaces old SuperUser AD group check)
            if (!string.IsNullOrWhiteSpace(_options.ADGroupADAdmin))
            {
                if (principal.IsInRole(_options.ADGroupADAdmin))
                {
                    claims.Add(new Claim(ClaimTypes.Role, "ADAdmin"));
                    _logger.LogInformation("User {Username} is in AD group {GroupName} - assigned ADAdmin role",
                        windowsIdentity.Name, _options.ADGroupADAdmin);
                }
            }

            // NOTE: SuperUser role is now ONLY assigned via database flag (IsSuperUser = true)
            // The old ADGroupSuperUser check has been removed
            // SuperUser role claim is added below based on database flag only
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking AD group membership for {Username}",
                windowsIdentity.Name);
            // Don't throw - continue without AD group claims
        }
    }

    private class UserPermissionInfo
    {
        public int UserId { get; set; }
        public bool IsSuperUser { get; set; }
        public bool HasAccess { get; set; }
    }
}
