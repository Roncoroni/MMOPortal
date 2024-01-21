using System.Net;
using System.Net.Sockets;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;
using MMO.Game.Data;

namespace MMO.Game.Services;

public class InstanceHostManagement(
    ILogger<InstanceHostManagement> logger,
    IGameDbContext dbContext,
    Lazy<IInstanceConnection> instanceConnection)
{
    public async Task RegisterInstanceLauncher(Guid id, IPAddress? remoteAddress)
    {
        var address = remoteAddress?.AddressFamily == AddressFamily.InterNetworkV6 ? $"[{remoteAddress.ToString()}]" : remoteAddress?.ToString();
        logger.LogInformation("Register instance launcher");
        await dbContext.InstanceHosts
            .Where(host => host.InstanceHostId == id)
            .ExecuteUpdateAsync(calls =>
                calls.SetProperty(host => host.Online, true)
                    .SetProperty(host => host.LastHeartbeat, DateTime.UtcNow)
                    .SetProperty(host => host.Address, host => host.Address ?? address));
    }

    public async Task UnregisterInstanceLauncher(Guid id)
    {
        logger.LogInformation("Unregister instance launcher");
        await dbContext.InstanceHosts
            .Where(host => host.InstanceHostId == id)
            .ExecuteUpdateAsync(calls =>
                calls.SetProperty(host => host.Online, false)
                    .SetProperty(host => host.LastHeartbeat, DateTime.UtcNow));
        await dbContext.GameServers
            .Where(server => server.InstanceHost.InstanceHostId == id)
            .ExecuteDeleteAsync();
    }

    public async Task InstanceHostHeartbeat(Guid id)
    {
        logger.LogInformation("Heartbeat instance launcher");
        await dbContext.InstanceHosts
            .Where(host => host.InstanceHostId == id)
            .ExecuteUpdateAsync(calls =>
                calls.SetProperty(host => host.LastHeartbeat, DateTime.UtcNow));
    }

    public async Task RegisterInstance(Guid serverId, ushort port)
    {
        await dbContext.GameServers
            .Where(server => server.GameServerId == serverId)
            .ExecuteUpdateAsync(
                calls => calls
                    .SetProperty(server => server.Online, false)
                    .SetProperty(server => server.Port, port)
                    .SetProperty(server => server.LastHeartbeat, DateTime.UtcNow));

        var gameServer = await dbContext.GameServers
            .Include(server => server.InstanceHost)
            .FirstAsync(server => server.GameServerId == serverId);
        logger.LogInformation("New instance registered {Address}:{Port}", gameServer.InstanceHost.Address, gameServer.Port);
    }


    public async Task UnregisterInstance(Guid serverId)
    {
        await dbContext.GameServers
            .Where(server => server.GameServerId == serverId)
            .ExecuteUpdateAsync(
                calls => calls
                    .SetProperty(server => server.Online, false));
        logger.LogInformation("instances unregistered");
    }
    
    public async Task<string> GetEntryServer()
    {
        var entryServerQuery = dbContext.GameServers
            .Include(server => server.GameServerDefinition)
            .Include(server => server.InstanceHost)
            .Where(server => server.GameServerDefinition.GameServerType == GameServerType.Entry);
        var entryServer = await entryServerQuery.FirstOrDefaultAsync();

        if (entryServer is null)
        {
            entryServer = await StartInstance(GameServerType.Entry);
        }
        return ConstructUrl(entryServer.InstanceHost.Address ?? "", entryServer.Port);
    }

    public Task<string> GetWorldServer()
    {
        return GetWorldServer(Guid.Empty, Guid.Empty);
    }
    public async Task<string> GetWorldServer(Guid AccountId, Guid CharacterId)
    {
        var worldServerQuery = dbContext.GameServers
            .Include(server => server.GameServerDefinition)
            .Include(server => server.InstanceHost)
            .Where(server => server.GameServerDefinition.GameServerType == GameServerType.World && server.Online);
        
        var worldServer = await worldServerQuery.FirstOrDefaultAsync();

        if (worldServer is null)
        {
            worldServer = await StartInstance(GameServerType.World);
        }

        return ConstructUrl(worldServer.InstanceHost.Address ?? "", worldServer.Port);
    }

    private static string ConstructUrl(string address, ushort port)
    {
        return $"{address}:{port}";
    }

    private async Task<GameServer> StartInstance(GameServerType serverType)
    {
        var offset = DateTime.UtcNow.AddMinutes(-5);
        var serverDefinition = await dbContext.GameServerDefinitions.AsNoTracking().FirstAsync(definition =>
            definition.GameServerType == serverType);
        
        var gameServer = await dbContext.GameServers.AsNoTracking()
            .Include(server => server.InstanceHost)
            .Where(server => server.InstanceHost.Online && 
                             (!server.Online && server.LastHeartbeat < offset))
            .FirstOrDefaultAsync();
        EntityEntry<GameServer> entry;
        
        if (gameServer is null)
        {
            var host = await dbContext.InstanceHosts
                .Where(host => host.Online)
                .Include(host => host.GameServers)
                .OrderBy(host => host.GameServers.Count)
                .FirstAsync();

            var tempGameServer = new GameServer
            {
                InstanceHostId = host.InstanceHostId,
                GameServerDefinitionId = serverDefinition.GameServerDefinitionId
            };
            entry = dbContext.GameServers.Add(tempGameServer);
            await dbContext.SaveChangesAsync();
            gameServer = tempGameServer;
            logger.LogInformation("New GameServer GUID:{ServerId} ({Alt})", entry.Entity.GameServerId, gameServer.GameServerId);
        }
        else
        {
            entry = dbContext.Entry(gameServer);
            logger.LogInformation("Reuse GameServer GUID:{ServerId} ({Alt})", entry.Entity.GameServerId, gameServer.GameServerId);
        }
        
        await instanceConnection.Value.StartInstance(gameServer, serverDefinition);

        await entry.ReloadAsync();
        var tryNr = 0;
        while (entry.Entity.Port == 0 && tryNr++ < 10)
        {
            await Task.Delay(500);
            await entry.ReloadAsync();
        }
        
        return entry.Entity;
    }
}