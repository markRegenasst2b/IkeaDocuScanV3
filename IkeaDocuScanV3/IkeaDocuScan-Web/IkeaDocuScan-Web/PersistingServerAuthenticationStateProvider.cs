using IkeaDocuScan_Web.Client;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using System.Security.Claims;

namespace IkeaDocuScan_Web;

// This is a server-side authentication state provider that uses PersistentComponentState to flow the
// authentication state to the client which is then fixed for the lifetime of the WebAssembly application.
internal sealed class PersistingServerAuthenticationStateProvider : AuthenticationStateProvider, IDisposable
{
    private readonly PersistentComponentState _state;
    private readonly PersistingComponentStateSubscription _subscription;
    private readonly ILogger<PersistingServerAuthenticationStateProvider> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    private Task<AuthenticationState>? _authenticationStateTask;

    public PersistingServerAuthenticationStateProvider(
        PersistentComponentState persistentComponentState,
        ILogger<PersistingServerAuthenticationStateProvider> logger,
        IHttpContextAccessor httpContextAccessor)
    {
        _state = persistentComponentState;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;

        AuthenticationStateChanged += OnAuthenticationStateChanged;
        _subscription = _state.RegisterOnPersisting(OnPersistingAsync, RenderMode.InteractiveWebAssembly);
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        if (_authenticationStateTask != null)
        {
            return _authenticationStateTask;
        }

        // Get the user from HttpContext
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.User != null)
        {
            _logger.LogInformation("Getting auth state from HttpContext. IsAuthenticated: {IsAuthenticated}, Name: {Name}",
                httpContext.User.Identity?.IsAuthenticated,
                httpContext.User.Identity?.Name);

            _authenticationStateTask = Task.FromResult(new AuthenticationState(httpContext.User));
            return _authenticationStateTask;
        }

        _logger.LogWarning("HttpContext or User is null");
        return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));
    }

    private void OnAuthenticationStateChanged(Task<AuthenticationState> task)
    {
        _authenticationStateTask = task;
    }

    private async Task OnPersistingAsync()
    {
        if (_authenticationStateTask is null)
        {
            // Get the current authentication state if not set
            _authenticationStateTask = GetAuthenticationStateAsync();
        }

        var authenticationState = await _authenticationStateTask;
        var principal = authenticationState.User;

        _logger.LogInformation("Persisting authentication state. IsAuthenticated: {IsAuthenticated}, Name: {Name}",
            principal.Identity?.IsAuthenticated,
            principal.Identity?.Name);

        if (principal.Identity?.IsAuthenticated == true)
        {
            var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var name = principal.FindFirst(ClaimTypes.Name)?.Value;
            var email = principal.FindFirst(ClaimTypes.Email)?.Value;

            // Windows Authentication uses Name claim as the primary identifier
            // Use NameIdentifier if available, otherwise fall back to Name
            var effectiveUserId = userId ?? name ?? principal.Identity.Name ?? "Unknown";

            _logger.LogInformation("Persisting user: {UserId}, {Name}, {Email}", effectiveUserId, name, email);

            _state.PersistAsJson(nameof(UserInfo), new UserInfo
            {
                UserId = effectiveUserId,
                Name = name ?? principal.Identity.Name,
                Email = email
            });
        }
        else
        {
            _logger.LogWarning("User is not authenticated during persist operation");
        }
    }

    public void Dispose()
    {
        _subscription.Dispose();
        AuthenticationStateChanged -= OnAuthenticationStateChanged;
    }
}
