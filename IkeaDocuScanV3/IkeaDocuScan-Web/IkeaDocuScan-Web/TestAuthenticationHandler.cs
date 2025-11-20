#if DEBUG
using IkeaDocuScan_Web.Services;
#endif
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace IkeaDocuScan_Web;

/// <summary>
/// A test authentication handler for development purposes on Linux/WSL
/// where Windows Authentication is not available.
/// </summary>
public class TestAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
#if DEBUG
    private readonly TestIdentityService? _testIdentityService;
#endif

    public TestAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder
#if DEBUG
        , TestIdentityService? testIdentityService = null
#endif
        )
        : base(options, logger, encoder)
    {
#if DEBUG
        _testIdentityService = testIdentityService;
#endif
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
#if DEBUG
        // Check if there's an active test identity profile in session
        if (_testIdentityService != null)
        {
            var testProfile = _testIdentityService.GetCurrentTestIdentity();
            if (testProfile != null)
            {
                // Use the test identity profile
                var principal1 = _testIdentityService.CreateClaimsPrincipal(testProfile);
                var ticket1 = new AuthenticationTicket(principal1, "TestScheme");

                Logger.LogInformation(
                    "Using test identity profile: {ProfileId} ({Username}) with roles: {Roles}",
                    testProfile.ProfileId,
                    testProfile.Username,
                    string.Join(", ", testProfile.ADGroups.Concat(testProfile.IsSuperUser ? new[] { "SuperUser" } : Array.Empty<string>())));

                return Task.FromResult(AuthenticateResult.Success(ticket1));
            }
        }
#endif

        // Fallback: Use environment username with role detection
        var username = Environment.UserName ?? "TestUser";

        // Create base claims
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.NameIdentifier, username),
            new Claim(ClaimTypes.Email, $"{username}@test.local")
        };

        // Add role claims based on username (test profiles)
        // Test profiles: reader, publisher, adadmin, superuser
        var usernameLower = username.ToLowerInvariant();

        if (usernameLower.Contains("reader"))
        {
            claims.Add(new Claim(ClaimTypes.Role, "Reader"));
            claims.Add(new Claim("HasAccess", "True"));
            Logger.LogInformation("Test user {Username} assigned Reader role", username);
        }

        if (usernameLower.Contains("publisher"))
        {
            claims.Add(new Claim(ClaimTypes.Role, "Publisher"));
            claims.Add(new Claim("HasAccess", "True"));
            Logger.LogInformation("Test user {Username} assigned Publisher role", username);
        }

        if (usernameLower.Contains("adadmin"))
        {
            claims.Add(new Claim(ClaimTypes.Role, "ADAdmin"));
            claims.Add(new Claim("HasAccess", "True"));
            Logger.LogInformation("Test user {Username} assigned ADAdmin role", username);
        }

        if (usernameLower.Contains("superuser"))
        {
            claims.Add(new Claim(ClaimTypes.Role, "SuperUser"));
            claims.Add(new Claim("IsSuperUser", "True"));
            claims.Add(new Claim("HasAccess", "True"));
            Logger.LogInformation("Test user {Username} assigned SuperUser role", username);
        }

        // If no role matched, default to HasAccess = False
        if (!claims.Any(c => c.Type == ClaimTypes.Role))
        {
            claims.Add(new Claim("HasAccess", "False"));
            Logger.LogWarning("Test user {Username} has no role assigned - no access", username);
        }

        var identity = new ClaimsIdentity(claims, "TestScheme");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "TestScheme");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
