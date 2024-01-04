using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace MMO.Game.Data;

public interface IGameDbContext
{
    public DbSet<Character> Characters { get; set; }
    public DbSet<InstanceHost> InstanceHosts { get; set; }
    public DbSet<GameServerDefinition> GameServerDefinitions { get; set; }
    public DbSet<GameServer> GameServers { get; set; }
    public int SaveChanges();
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken));
    public EntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class;
}

public static class GameDbContextExtension
{
    public static void OnGameModelCreating(this IGameDbContext context, ModelBuilder builder)
    {
        builder.Entity<GameServerDefinition>()
            .HasDiscriminator(definition => definition.GameServerType)
            .HasValue<EntryServerDefinition>(GameServerType.Entry)
            .HasValue<WorldServerDefinition>(GameServerType.World)
            .IsComplete();
    }
}