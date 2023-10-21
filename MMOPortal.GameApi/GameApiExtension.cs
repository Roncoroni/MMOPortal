using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using MMOPortal.GameApi.Authentication;
using MMOPortal.GameApi.Data;

namespace MMOPortal.GameApi;

public static class GameApiExtension
{
    public static void AddGameApi<TDbContext>(this IServiceCollection services) where TDbContext : DbContext
    {
        services.TryAddEnumerable(ServiceDescriptor
            .Singleton<IConfigureOptions<GameServerTokenOptions>, GameServerTokenConfigurationOptions>());

        services.AddScoped<DbContext, TDbContext>();


        services.AddAuthentication()
            .AddScheme<GameServerTokenOptions, GameServerTokenHandler>(GameServerTokenDefaults.AuthenticationScheme,
                _ => { });
    }

    public static void UseGameApi<TDbContext>(this IEndpointRouteBuilder endpoints, [StringSyntax("Route")] string path)
        where TDbContext : DbContext
    {
        var group = endpoints.MapGroup(path).RequireAuthorization(builder =>
        {
            builder.AuthenticationSchemes = new List<string> { GameServerTokenDefaults.AuthenticationScheme };
            builder.RequireClaim(GameServerTokenDefaults.ServerIdClaim);
        });
        group.MapPost("register",
                async Task<Results<Ok<GameServerTokenResponse>, EmptyHttpResult, ProblemHttpResult>>([FromBody] RegisterServerParams serverIdentity, HttpContext context, TDbContext dbContext) =>
                {
                    var identity = new ClaimsPrincipal(
                        new ClaimsIdentity(
                            new List<Claim>
                            {
                                new(GameServerTokenDefaults.ServerIdClaim, serverIdentity.ServerId)
                            }, GameServerTokenDefaults.AuthenticationScheme
                        )
                    );

                    var result = Results.SignIn(identity, null, GameServerTokenDefaults.AuthenticationScheme);
                    await result.ExecuteAsync(context);
                    dbContext.Set<GameServer>().Update(new GameServer { ServerGuid = serverIdentity.ServerId });
                    await dbContext.SaveChangesAsync();
                    return TypedResults.Empty;
                })
            .AllowAnonymous();
        
        group.MapGet("status", (ClaimsPrincipal user) => user.Claims.Select(claim => claim.ToString()));
        group.MapGet("list", (TDbContext dbContext) =>
        {
            return dbContext.Set<GameServer>();
        });
    }
}