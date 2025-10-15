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
    public TestAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Create a test user for development
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, Environment.UserName ?? "TestUser"),
            new Claim(ClaimTypes.NameIdentifier, Environment.UserName ?? "TestUser"),
            new Claim(ClaimTypes.Email, $"{Environment.UserName ?? "TestUser"}@test.local")
        };

        var identity = new ClaimsIdentity(claims, "TestScheme");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "TestScheme");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
