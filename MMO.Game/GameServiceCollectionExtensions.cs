using Microsoft.Extensions.DependencyInjection;
using MMO.Game.Data;
using MMO.Game.Services;

namespace MMO.Game;

public static class GameServiceCollectionExtensions
{
    public static IServiceCollection AddGame<TDbContext>(
        this IServiceCollection services) where TDbContext : IGameDbContext
    {
        services.AddScoped<CharacterManagement>();
        services.AddScoped<ServerManagement>();
        services.AddScoped<IGameDbContext>(provider => provider.GetRequiredService<TDbContext>());
        services.AddScoped<InstanceManagement>();
        return services;
    }
}