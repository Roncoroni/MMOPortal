using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

namespace MMOPortal.GameApi.Authentication;

public class GameServerTokenHandler : SignInAuthenticationHandler<GameServerTokenOptions>
{
    private static readonly AuthenticateResult FailedUnprotectingToken =
        AuthenticateResult.Fail("Unprotected token failed");

    private static readonly AuthenticateResult ServerIdentity = AuthenticateResult.Fail("ServerIdentity invalid");

    private static readonly string TokenPrefix = $"{GameServerTokenDefaults.AuthenticationScheme} ";
    
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

        if (ticket?.Principal.FindFirstValue(GameServerTokenDefaults.ServerIdClaim) is not { } serverIdentity)
        {
            return FailedUnprotectingToken;
        }

        if (string.IsNullOrEmpty(serverIdentity))
        {
            return ServerIdentity;
        }

        return AuthenticateResult.Success(ticket);
    }

    protected override Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        Response.Headers.Append(HeaderNames.WWWAuthenticate, GameServerTokenDefaults.AuthenticationScheme);
        return base.HandleChallengeAsync(properties);
    }

    private string? GetGameServerTokenOrNull()
    {
        var authorization = Request.Headers.Authorization.ToString();

        
        return authorization.StartsWith(TokenPrefix, StringComparison.Ordinal)
            ? authorization[TokenPrefix.Length..]
            : null;
    }

    // No-op to avoid interfering with any mass sign-out logic.
    protected override Task HandleSignOutAsync(AuthenticationProperties? properties) => Task.CompletedTask;

    protected override async Task HandleSignInAsync(ClaimsPrincipal user, AuthenticationProperties? properties)
    {
        var schemeName = Scheme.Name;
        if (string.IsNullOrEmpty(user.FindFirstValue(GameServerTokenDefaults.ServerIdClaim)))
        {
            Context.Response.StatusCode = 401;
            return;
        }

        //var utcNow = TimeProvider.GetUtcNow();

        properties ??= new();
        //properties.ExpiresUtc = utcNow + TimeSpan.FromHours(1);//Options.ServerTokenExpiration;

        //Logger.AuthenticationSchemeSignedIn(Scheme!.Name);

        var response = new GameServerTokenResponse
        {
            AccessToken = Options.ServerTokenProtector.Protect(new(user, properties, schemeName))
        };
        
        await Context.Response.WriteAsJsonAsync(response);
    }
}