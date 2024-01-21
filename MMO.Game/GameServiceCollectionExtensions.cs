using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MMO.Game.Data;
using MMO.Game.Services;

namespace MMO.Game;

public static class GameServiceCollectionExtensions
{
    public static IServiceCollection AddGame<TDbContext>(
        this IServiceCollection services) where TDbContext : DbContext, IGameDbContext
    {
        services.AddTransient<CharacterManagement>();
        services.AddTransient<ServerManagement>();
        services.AddTransient<IGameDbContext>(provider => provider.GetRequiredService<TDbContext>());
        services.AddTransient<InstanceHostManagement>();

        services.AddHostedService<HeartbeatWatcher<TDbContext>>();
        return services;
    }
}