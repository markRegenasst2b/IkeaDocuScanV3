using IkeaDocuScan.Infrastructure.Data;
using IkeaDocuScan.Infrastructure.Entities;
using IkeaDocuScan.Shared.Interfaces;
using IkeaDocuScan.Shared.Models.Authorization;
using IkeaDocuScan.Shared.DTOs.Authorization;
using Microsoft.EntityFrameworkCore;

namespace IkeaDocuScan_Web.Services;

/// <summary>
/// Scoped service for current user information and permissions
/// Loads user data once per request and caches it
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    private readonly AppDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<CurrentUserService> _logger;
    private readonly IEmailService _emailService;
    private CurrentUser? _cachedUser;
    private bool _loaded = false;

    public CurrentUserService(
        AppDbContext context,
        IHttpContextAccessor httpContextAccessor,
        ILogger<CurrentUserService> logger,
        IEmailService emailService)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
        _emailService = emailService;
    }

    /// <inheritdoc />
    public async Task<CurrentUser> GetCurrentUserAsync()
    {
        if (_loaded && _cachedUser != null)
        {
            return _cachedUser;
        }

        var username = _httpContextAccessor.HttpContext?.User?.Identity?.Name;

        if (string.IsNullOrWhiteSpace(username))
        {
            _logger.LogWarning("No authenticated user found");
            _cachedUser = new CurrentUser { HasAccess = false };
            _loaded = true;
            return _cachedUser;
        }

        try
        {
            _logger.LogInformation("Loading user permissions for: {Username}", username);

            // Load user from database
            var docuScanUser = await _context.DocuScanUsers
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.AccountName == username);

            if (docuScanUser == null)
            {
                _logger.LogWarning("User not found in DocuScanUser table: {Username}", username);
                _cachedUser = new CurrentUser
                {
                    AccountName = username,
                    HasAccess = false
                };
                _loaded = true;
                return _cachedUser;
            }

            // Check if super user
            if (docuScanUser.IsSuperUser)
            {
                _logger.LogInformation("User {Username} is SuperUser", username);
                _cachedUser = new CurrentUser
                {
                    UserId = docuScanUser.UserId,
                    AccountName = docuScanUser.AccountName,
                    IsSuperUser = true,
                    HasAccess = true,
                    LastLogon = docuScanUser.LastLogon,
                    AllowedDocumentTypes = null // null = all
                };
                _loaded = true;
                return _cachedUser;
            }

            // Load user permissions
            var permissions = await _context.UserPermissions
                .AsNoTracking()
                .Where(p => p.UserId == docuScanUser.UserId)
                .ToListAsync();

            if (permissions.Count == 0)
            {
                _logger.LogWarning("User {Username} has no permissions", username);
                _cachedUser = new CurrentUser
                {
                    UserId = docuScanUser.UserId,
                    AccountName = docuScanUser.AccountName,
                    IsSuperUser = false,
                    HasAccess = false,
                    LastLogon = docuScanUser.LastLogon
                };
                _loaded = true;
                return _cachedUser;
            }

            // Extract unique allowed values from permissions
            var allowedDocTypes = permissions
                .Where(p => p.DocumentTypeId.HasValue)
                .Select(p => p.DocumentTypeId!.Value)
                .Distinct()
                .ToList();

            _cachedUser = new CurrentUser
            {
                UserId = docuScanUser.UserId,
                AccountName = docuScanUser.AccountName,
                IsSuperUser = false,
                HasAccess = true,
                LastLogon = docuScanUser.LastLogon,
                AllowedDocumentTypes = allowedDocTypes.Count > 0 ? allowedDocTypes : null
            };

            _logger.LogInformation("User {Username} loaded with {PermissionCount} permissions",
                username, permissions.Count);

            _loaded = true;
            return _cachedUser;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading user permissions for {Username}", username);
            throw;
        }
    }

    /// <inheritdoc />
    public bool IsSuperUser
    {
        get
        {
            if (!_loaded) return false;
            return _cachedUser?.IsSuperUser ?? false;
        }
    }

    /// <inheritdoc />
    public bool HasAccess
    {
        get
        {
            if (!_loaded) return false;
            return _cachedUser?.HasAccess ?? false;
        }
    }

    /// <inheritdoc />
    public int? UserId
    {
        get
        {
            if (!_loaded) return null;
            return _cachedUser?.UserId;
        }
    }

    /// <inheritdoc />
    public async Task<AccessRequestResult> RequestAccessAsync(string username, string? reason = null)
    {
        try
        {
            _logger.LogInformation("Access request for user: {Username}, Reason: {Reason}",
                username, reason ?? "Not provided");

            // Check if user already exists
            var existingUser = await _context.DocuScanUsers
                .FirstOrDefaultAsync(u => u.AccountName == username);

            if (existingUser != null)
            {
                _logger.LogInformation("User {Username} already exists in database", username);
                return new AccessRequestResult
                {
                    Success = true,
                    Message = "Your account already exists in the system. Please contact your administrator to request permissions.",
                    UserCreated = false,
                    UserAlreadyExists = true
                };
            }

            // Create new user record
            var newUser = new DocuScanUser
            {
                AccountName = username,
                IsSuperUser = false,
                CreatedOn = DateTime.Now,
                LastLogon = null
            };

            _context.DocuScanUsers.Add(newUser);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created new DocuScanUser record for {Username}", username);

            // Send notification email to administrator
            try
            {
                await _emailService.SendAccessRequestNotificationAsync(username, reason);
                _logger.LogInformation("Access request notification email sent for {Username}", username);
            }
            catch (Exception emailEx)
            {
                _logger.LogError(emailEx, "Failed to send access request notification email for {Username}", username);
                // Don't fail the request if email fails
            }

            return new AccessRequestResult
            {
                Success = true,
                Message = "Your access request has been submitted successfully. An administrator will review your request and grant permissions.",
                UserCreated = true,
                UserAlreadyExists = false
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error requesting access for user {Username}", username);
            return new AccessRequestResult
            {
                Success = false,
                Message = $"An error occurred while processing your request: {ex.Message}",
                UserCreated = false,
                UserAlreadyExists = false
            };
        }
    }

    /// <inheritdoc />
    public async Task UpdateLastLogonAsync()
    {
        try
        {
            var username = _httpContextAccessor.HttpContext?.User?.Identity?.Name;
            if (string.IsNullOrWhiteSpace(username))
                return;

            var user = await _context.DocuScanUsers
                .FirstOrDefaultAsync(u => u.AccountName == username);

            if (user != null)
            {
                user.LastLogon = DateTime.Now;
                user.ModifiedOn = DateTime.Now;
                await _context.SaveChangesAsync();

                _logger.LogDebug("Updated last logon for user {Username}", username);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating last logon");
            // Don't throw - this is not critical
        }
    }

    /// <inheritdoc />
    public void InvalidateCache()
    {
        _cachedUser = null;
        _loaded = false;
        _logger.LogDebug("User cache invalidated");
    }
}
