#if DEBUG
using IkeaDocuScan_Web.Services;

namespace IkeaDocuScan_Web.Middleware;

/// <summary>
/// Middleware to inject test identities in development mode
/// ⚠️ WARNING: THIS MIDDLEWARE ONLY RUNS IN DEBUG MODE ⚠️
/// </summary>
public class TestIdentityMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TestIdentityMiddleware> _logger;

    public TestIdentityMiddleware(
        RequestDelegate next,
        ILogger<TestIdentityMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, TestIdentityService testIdentityService)
    {
        // Check if a test identity is active
        var testProfile = testIdentityService.GetCurrentTestIdentity();

        if (testProfile != null)
        {
            // Create and set the test identity
            var testPrincipal = testIdentityService.CreateClaimsPrincipal(testProfile);
            context.User = testPrincipal;

            _logger.LogWarning("⚠️ TEST IDENTITY ACTIVE: {Username} ({ProfileId})",
                testProfile.Username, testProfile.ProfileId);
        }
        else
        {
            // Check for query string activation (for automation/URL-based testing)
            var testUserParam = context.Request.Query["testUser"].FirstOrDefault();
            if (!string.IsNullOrEmpty(testUserParam))
            {
                try
                {
                    testIdentityService.SetTestIdentity(testUserParam);

                    // Redirect to remove query string
                    var redirectUrl = context.Request.Path.ToString();
                    context.Response.Redirect(redirectUrl);
                    return;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to set test identity from query string: {ProfileId}", testUserParam);
                }
            }
        }

        await _next(context);
    }
}

/// <summary>
/// Extension method to register TestIdentityMiddleware
/// </summary>
public static class TestIdentityMiddlewareExtensions
{
    public static IApplicationBuilder UseTestIdentity(this IApplicationBuilder builder)
    {
#if DEBUG
        return builder.UseMiddleware<TestIdentityMiddleware>();
#else
        // In non-DEBUG builds, this is a no-op
        return builder;
#endif
    }
}
#endif
