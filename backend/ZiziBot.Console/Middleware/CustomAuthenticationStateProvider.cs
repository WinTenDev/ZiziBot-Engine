using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace ZiziBot.Console.Middleware;

public class CustomAuthenticationStateProvider(ProtectedLocalStorage protectedLocalStorage) : AuthenticationStateProvider
{
    private readonly ProtectedLocalStorage _protectedLocalStorage = protectedLocalStorage;

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var identity = new ClaimsIdentity();
        var user = new ClaimsPrincipal(identity);

        return await Task.FromResult(new AuthenticationState(user));
    }

    public ClaimsPrincipal? AuthenticateUser(string bearerToken)
    {
        var data = bearerToken.DecodeJwtToken();

        var identity = new ClaimsIdentity(data.Claims, "TelegramAuthentication");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(claimsPrincipal)));

        return claimsPrincipal;
    }
}