namespace IkeaDocuScan_Web.Services;

public class UserIdentityService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<UserIdentityService> _logger;

    public UserIdentityService(IHttpContextAccessor httpContextAccessor, ILogger<UserIdentityService> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public string? GetCurrentUsername()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        var username = httpContext?.User?.Identity?.Name;

        _logger.LogInformation("GetCurrentUsername called. HttpContext exists: {HasContext}, User exists: {HasUser}, Username: {Username}",
            httpContext != null,
            httpContext?.User != null,
            username);

        return username;
    }

    public bool IsAuthenticated()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        return httpContext?.User?.Identity?.IsAuthenticated ?? false;
    }
}
