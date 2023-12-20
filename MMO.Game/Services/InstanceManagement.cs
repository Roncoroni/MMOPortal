using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MMO.Game.Data;
using MMO.ServerLauncher.Shared;

namespace MMO.Game.Services;

public class InstanceManagement(ILogger<InstanceManagement> logger, IGameDbContext context)
{
    public async Task RegisterInstanceLauncher(Guid id, IPAddress? remoteAddress)
    {
        var address = remoteAddress?.ToString();
        logger.LogInformation("Register instance launcher");
        await context.InstanceHosts
            .Where(host => host.InstanceHostId == id)
            .ExecuteUpdateAsync(calls =>
                calls.SetProperty(host => host.Online, true)
                    .SetProperty(host => host.LastHeartbeat, DateTime.Now)
                    .SetProperty(host => host.Address, host => host.Address ?? address));
    }

    public async Task UnregisterInstanceLauncher(Guid id)
    {
        logger.LogInformation("Unregister instance launcher");
        await context.InstanceHosts
            .Where(host => host.InstanceHostId == id)
            .ExecuteUpdateAsync(calls =>
                calls.SetProperty(host => host.Online, false)
                    .SetProperty(host => host.LastHeartbeat, DateTime.Now));
        await context.GameServers
            .Where(server => server.InstanceHost.InstanceHostId == id)
            .ExecuteDeleteAsync();
    }

    public async Task Heartbeat(Guid id)
    {
        logger.LogInformation("Heartbeat instance launcher");
        await context.InstanceHosts
            .Where(host => host.InstanceHostId == id)
            .ExecuteUpdateAsync(calls =>
                calls.SetProperty(host => host.LastHeartbeat, DateTime.Now));
    }

    public async Task RegisterInstance(Guid launcherId, Guid serverTypeId, ushort port)
    {
        context.GameServers.Add(new GameServer
        {
            InstanceHostId = launcherId,
            Port = port,
            GameServerTypeId = serverTypeId
        });
        await context.SaveChangesAsync();
        logger.LogInformation("New instance registered");
    }


    public async Task UnregisterInstance(Guid launcherId, Guid serverTypeId, ushort port)
    {
        var count = await context.GameServers.Where(server =>
                server.InstanceHostId == launcherId
                && server.Port == port
                && server.GameServerTypeId == serverTypeId)
            .ExecuteDeleteAsync();
        logger.LogInformation("{Count} instances unregistered", count);
    }

    public IEnumerable<GameServerType> MissingInstances(Guid launcherId)
    {
        return context.GameServerTypes
            .Include(type => type.Instances)
            .Where(type => (type.StartType == GameServerStartType.OnePerNetwork 
                               && type.Instances.Count == 0)
                           || (type.StartType == GameServerStartType.OnePerHost 
                               && type.Instances.All(server => server.InstanceHostId != launcherId)));
    }
}