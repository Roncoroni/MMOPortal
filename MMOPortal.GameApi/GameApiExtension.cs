using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace MMOPortal.GameApi;

public static class GameApiExtension
{
    private const string AuthenticationScheme = "GameServer";

    public static void AddGameApi(this IServiceCollection services)
    {
        services.TryAddEnumerable(ServiceDescriptor
            .Singleton<IConfigureOptions<GameServerTokenOptions>, GameServerTokenConfigurationOptions>());
        services.AddAuthentication()
            .AddScheme<GameServerTokenOptions, GameServerTokenHandler>(AuthenticationScheme, _ => { });
    }

    public static void UseGameApi(this IEndpointRouteBuilder endpoints, [StringSyntax("Route")] string path)
    {
        var group = endpoints.MapGroup(path).RequireAuthorization(builder =>
        {
            builder.AuthenticationSchemes = new List<string> { AuthenticationScheme };
            builder.RequireAuthenticatedUser();
        });
        group.MapGet("Status", () => true);
        group.MapGet("token", () =>
            {
                var identity = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>(), AuthenticationScheme));

                return Results.SignIn(identity, null, AuthenticationScheme);
            })
            .AllowAnonymous();
    }
}