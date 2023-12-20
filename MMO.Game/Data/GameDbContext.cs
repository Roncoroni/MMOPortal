using Microsoft.EntityFrameworkCore;

namespace MMO.Game.Data;

public interface IGameDbContext
{
    public DbSet<Character> Characters { get; set; }
    public DbSet<InstanceHost> InstanceHosts { get; set; }
    public DbSet<GameServerType> GameServerTypes { get; set; }
    public DbSet<GameServer> GameServers { get; set; }
    public int SaveChanges();
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken));
}