#if DEBUG
using IkeaDocuScan.Shared.DTOs.Testing;
using System.Security.Claims;

namespace IkeaDocuScan_Web.Services;

/// <summary>
/// Service for managing test identities in development environment
/// ⚠️ WARNING: THIS CODE ONLY RUNS IN DEBUG MODE ⚠️
/// </summary>
public class TestIdentityService
{
    private readonly ILogger<TestIdentityService> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private const string SessionKey = "TestIdentity_Profile";

    public TestIdentityService(
        ILogger<TestIdentityService> logger,
        IHttpContextAccessor httpContextAccessor)
    {
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;

        _logger.LogWarning("⚠️ TestIdentityService is active - DEVELOPMENT MODE ONLY");
    }

    /// <summary>
    /// Get all available test identity profiles
    /// </summary>
    public List<TestIdentityProfile> GetAvailableProfiles()
    {
        return new List<TestIdentityProfile>
        {
            new TestIdentityProfile
            {
                ProfileId = "reset",
                DisplayName = "🔄 Reset to Real Identity",
                Username = "",
                Description = "Remove test identity and use your actual Windows identity",
                ADGroups = new(),
                IsSuperUser = false,
                HasAccess = false
            },
            new TestIdentityProfile
            {
                ProfileId = "superuser",
                DisplayName = "👑 Super User (DB Flag)",
                Username = "TEST\\SuperUserTest",
                Email = "superuser@test.local",
                Description = "Full system access via database SuperUser flag",
                ADGroups = new(),
                IsSuperUser = true,
                HasAccess = true,
                DatabaseUserId = 1001
            },
            new TestIdentityProfile
            {
                ProfileId = "superuser_ad",
                DisplayName = "👑 Super User (AD Group)",
                Username = "TEST\\SuperUserAD",
                Email = "superuserad@test.local",
                Description = "Full access via AD SuperUser group membership",
                ADGroups = new() { "Reader", "Publisher", "SuperUser" },
                IsSuperUser = true,
                HasAccess = true,
                DatabaseUserId = 1002
            },
            new TestIdentityProfile
            {
                ProfileId = "publisher",
                DisplayName = "📝 Publisher 1",
                Username = "TEST\\PublisherTest",
                Email = "publisher@test.local",
                Description = "Can create and edit documents (AD Publisher group + database permissions)",
                ADGroups = new() { "Reader", "Publisher" },
                IsSuperUser = false,
                HasAccess = true,
                DatabaseUserId = 1003
            },
            new TestIdentityProfile
            {
                ProfileId = "publisher2",
                DisplayName = "📝 Publisher 2",
                Username = "TEST\\PublisherTest2",
                Email = "publisher@test.local",
                Description = "Can create and edit documents (AD Publisher group + database permissions)",
                ADGroups = new() { "Reader", "Publisher" },
                IsSuperUser = false,
                HasAccess = true,
                DatabaseUserId = 1003
            },
            new TestIdentityProfile
            {
                ProfileId = "adadmin",
                DisplayName = "🔧 ADAdmin (Read-Only Admin)",
                Username = "TEST\\ADAdminTest",
                Email = "adadmin@test.local",
                Description = "Read-only admin access to user management, logs, and configuration (AD ADAdmin group)",
                ADGroups = new() { "Reader", "ADAdmin" },
                IsSuperUser = false,
                HasAccess = true,
                DatabaseUserId = 1007
            },
            new TestIdentityProfile
            {
                ProfileId = "reader",
                DisplayName = "👁️ Reader 1",
                Username = "TEST\\ReaderTest",
                Email = "reader@test.local",
                Description = "Can only view documents (AD Reader group + database permissions)",
                ADGroups = new() { "Reader" },
                IsSuperUser = false,
                HasAccess = true,
                DatabaseUserId = 1004
            },
            new TestIdentityProfile
            {
                ProfileId = "reader2",
                DisplayName = "👁️ Reader 2",
                Username = "TEST\\ReaderTest2",
                Email = "reader@test.local",
                Description = "Can only view documents (AD Reader group + database permissions)",
                ADGroups = new() { "Reader" },
                IsSuperUser = false,
                HasAccess = true,
                DatabaseUserId = 1004
            },
            /*
            new TestIdentityProfile
            {
                ProfileId = "db_only",
                DisplayName = "💾 Database Permissions Only",
                Username = "TEST\\DatabaseOnlyTest",
                Email = "dbonly@test.local",
                Description = "Has database permissions but no AD group memberships",
                ADGroups = new(),
                IsSuperUser = false,
                HasAccess = true,
                DatabaseUserId = 1005
            },
            new TestIdentityProfile
            {
                ProfileId = "ad_only",
                DisplayName = "🏢 AD Groups Only",
                Username = "TEST\\ADOnlyTest",
                Email = "adonly@test.local",
                Description = "Has AD group memberships but no database user record",
                ADGroups = new() { "Reader", "Publisher" },
                IsSuperUser = false,
                HasAccess = false,
                DatabaseUserId = null
            },
            */
            new TestIdentityProfile
            {
                ProfileId = "no_access",
                DisplayName = "🚫 No Access",
                Username = "TEST\\NoAccessTest",
                Email = "noaccess@test.local",
                Description = "User exists in database but has no permissions or AD groups",
                ADGroups = new(),
                IsSuperUser = false,
                HasAccess = false,
                DatabaseUserId = 1006
            },
            new TestIdentityProfile
            {
                ProfileId = "no_access2",
                DisplayName = "🚫 No Access2",
                Username = "TEST\\NoAccessTest2",
                Email = "noaccess@test.local",
                Description = "User exists in database but has no permissions or AD groups",
                ADGroups = new(),
                IsSuperUser = false,
                HasAccess = false,
                DatabaseUserId = 1006
            }

        };
    }

    /// <summary>
    /// Set the active test identity profile
    /// </summary>
    public void SetTestIdentity(string profileId)
    {
        var session = _httpContextAccessor.HttpContext?.Session;
        if (session == null)
        {
            _logger.LogError("Session is not available");
            throw new InvalidOperationException("Session is not available");
        }

        if (profileId == "reset")
        {
            session.Remove(SessionKey);
            _logger.LogWarning("⚠️ Test identity removed - using real Windows identity");
            return;
        }

        var profile = GetAvailableProfiles().FirstOrDefault(p => p.ProfileId == profileId);
        if (profile == null)
        {
            throw new ArgumentException($"Test profile '{profileId}' not found", nameof(profileId));
        }

        session.SetString(SessionKey, System.Text.Json.JsonSerializer.Serialize(profile));
        _logger.LogWarning("⚠️ Test identity set to: {Username} ({ProfileId})", profile.Username, profileId);
    }

    /// <summary>
    /// Get the current active test identity profile from session
    /// </summary>
    public TestIdentityProfile? GetCurrentTestIdentity()
    {
        var session = _httpContextAccessor.HttpContext?.Session;
        if (session == null) return null;

        var profileJson = session.GetString(SessionKey);
        if (string.IsNullOrEmpty(profileJson)) return null;

        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<TestIdentityProfile>(profileJson);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deserializing test identity profile");
            return null;
        }
    }

    /// <summary>
    /// Get current test identity status
    /// </summary>
    public TestIdentityStatus GetStatus()
    {
        var profile = GetCurrentTestIdentity();
        var claims = new List<string>();

        if (profile != null)
        {
            var claimsPrincipal = CreateClaimsPrincipal(profile);
            claims = claimsPrincipal.Claims
                .Select(c => $"{c.Type}: {c.Value}")
                .OrderBy(c => c)
                .ToList();
        }

        return new TestIdentityStatus
        {
            IsActive = profile != null,
            CurrentProfile = profile,
            ActiveClaims = claims
        };
    }

    /// <summary>
    /// Create a ClaimsPrincipal from a test identity profile
    /// </summary>
    public ClaimsPrincipal CreateClaimsPrincipal(TestIdentityProfile profile)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, profile.Username),
            new Claim(ClaimTypes.NameIdentifier, profile.Username),
            new Claim("HasAccess", profile.HasAccess.ToString()),
            new Claim("IsSuperUser", profile.IsSuperUser.ToString())
        };

        if (!string.IsNullOrEmpty(profile.Email))
        {
            claims.Add(new Claim(ClaimTypes.Email, profile.Email));
        }

        if (profile.DatabaseUserId.HasValue)
        {
            claims.Add(new Claim("UserId", profile.DatabaseUserId.Value.ToString()));
        }

        // Add SuperUser role claim if IsSuperUser flag is set (consistent with WindowsIdentityMiddleware)
        if (profile.IsSuperUser)
        {
            claims.Add(new Claim(ClaimTypes.Role, "SuperUser"));
        }

        // Add AD group role claims
        foreach (var group in profile.ADGroups)
        {
            // Don't duplicate SuperUser role if already added from IsSuperUser flag
            if (group == "SuperUser" && profile.IsSuperUser)
                continue;

            claims.Add(new Claim(ClaimTypes.Role, group));
        }

        var identity = new ClaimsIdentity(claims, "TestIdentity");
        return new ClaimsPrincipal(identity);
    }
}
#endif
