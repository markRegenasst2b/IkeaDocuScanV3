using IkeaDocuScan_Web.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace IkeaDocuScan_Web.Client;

// This is a client authentication state provider that rehydrates the authentication state from the server
// See: https://learn.microsoft.com/en-us/aspnet/core/blazor/security/server/additional-scenarios
internal class PersistentAuthenticationStateProvider : AuthenticationStateProvider
{
    private static readonly Task<AuthenticationState> _unauthenticatedTask =
        Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));

    private readonly Task<AuthenticationState> _authenticationStateTask = _unauthenticatedTask;

    public PersistentAuthenticationStateProvider(PersistentComponentState state)
    {
        if (!state.TryTakeFromJson<UserInfo>(nameof(UserInfo), out var userInfo) || userInfo is null)
        {
            Console.WriteLine("PersistentAuthenticationStateProvider: No UserInfo found in persisted state");
            return;
        }

        Console.WriteLine($"PersistentAuthenticationStateProvider: Found user - UserId: {userInfo.UserId}, Name: {userInfo.Name}, HasAccess: {userInfo.HasAccess}, IsSuperUser: {userInfo.IsSuperUser}");

        Claim[] claims = [
            new Claim(ClaimTypes.NameIdentifier, userInfo.UserId),
            new Claim(ClaimTypes.Name, userInfo.Name ?? string.Empty),
            new Claim(ClaimTypes.Email, userInfo.Email ?? string.Empty),
            new Claim("HasAccess", userInfo.HasAccess.ToString()),
            new Claim("IsSuperUser", userInfo.IsSuperUser.ToString())];

        _authenticationStateTask = Task.FromResult(
            new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(claims,
                authenticationType: nameof(PersistentAuthenticationStateProvider)))));

        Console.WriteLine("PersistentAuthenticationStateProvider: Authentication state set successfully");
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync() => _authenticationStateTask;
}
