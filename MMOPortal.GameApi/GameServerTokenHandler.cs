using System.Net;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

namespace MMOPortal.GameApi;

public class GameServerTokenHandler : SignInAuthenticationHandler<GameServerTokenOptions>
{
    private static readonly AuthenticateResult FailedUnprotectingToken =
        AuthenticateResult.Fail("Unprotected token failed");

    private static readonly AuthenticateResult TokenExpired = AuthenticateResult.Fail("Token expired");

    public GameServerTokenHandler(IOptionsMonitor<GameServerTokenOptions> options,
        ILoggerFactory logger, UrlEncoder encoder) : base(options, logger,
        encoder)
    {
    }

    private new GameServerTokenEvents Events => (GameServerTokenEvents)base.Events!;

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var messageReceivedContext = new GameServerReceivedContext(Context, Scheme, Options);

        await Events.MessageReceivedAsync(messageReceivedContext);

        if (messageReceivedContext.Result is not null)
        {
            return messageReceivedContext.Result;
        }

        var token = messageReceivedContext.Token ?? GetGameServerTokenOrNull();

        if (token is null)
        {
            return AuthenticateResult.NoResult();
        }

        var ticket = Options.ServerTokenProtector.Unprotect(token);

        if (ticket?.Properties?.ExpiresUtc is not { } expiresUtc)
        {
            return FailedUnprotectingToken;
        }

        if (TimeProvider.GetUtcNow() >= expiresUtc)
        {
            return TokenExpired;
        }

        return AuthenticateResult.Success(ticket);
    }

    protected override Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        Response.Headers.Append(HeaderNames.WWWAuthenticate, "Server");
        return base.HandleChallengeAsync(properties);
    }

    private string? GetGameServerTokenOrNull()
    {
        var authorization = Request.Headers.Authorization.ToString();

        return authorization.StartsWith("Server ", StringComparison.Ordinal)
            ? authorization["Server ".Length..]
            : null;
    }

    // No-op to avoid interfering with any mass sign-out logic.
    protected override Task HandleSignOutAsync(AuthenticationProperties? properties) => Task.CompletedTask;

    protected override async Task HandleSignInAsync(ClaimsPrincipal user, AuthenticationProperties? properties)
    {
        var utcNow = TimeProvider.GetUtcNow();
        var schemeName = Scheme?.Name ?? "GameServer";
        
        properties ??= new ();
        properties.ExpiresUtc = utcNow + TimeSpan.FromHours(1);//Options.ServerTokenExpiration;
        
        //Logger.AuthenticationSchemeSignedIn(Scheme!.Name);

        var token = Options.ServerTokenProtector.Protect(new(user, properties, schemeName));

        await Context.Response.WriteAsJsonAsync(token);
    }
}