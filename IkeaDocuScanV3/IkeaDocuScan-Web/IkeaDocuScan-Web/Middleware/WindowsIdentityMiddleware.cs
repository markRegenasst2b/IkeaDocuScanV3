using System.Security.Claims;
using System.Security.Principal;

namespace IkeaDocuScan_Web.Middleware;

public class WindowsIdentityMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<WindowsIdentityMiddleware> _logger;

    public WindowsIdentityMiddleware(RequestDelegate next, ILogger<WindowsIdentityMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
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

            // Create claims from Windows identity
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, windowsIdentity.Name),
                new Claim(ClaimTypes.NameIdentifier, windowsIdentity.User?.Value ?? windowsIdentity.Name)
            };

            var identity = new ClaimsIdentity(claims, "Windows");
            context.User = new ClaimsPrincipal(identity);
        }

        await _next(context);
    }
}
