using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MMO.Game.Data;
using MMO.ServerLauncher.Shared;

namespace MMO.Game.Services;

public class InstanceManagement(ILogger<InstanceManagement> logger, IGameDbContext context)
{
    public Task RegisterInstanceLauncher(string id)
    {
        logger.LogInformation("Register instance launcher");
        context.InstanceHosts.ExecuteUpdate(calls =>
            calls.SetProperty(host => host.ConnectionId, id));
        return Task.CompletedTask;
    }

    public Task UnregisterInstanceLauncher(string id)
    {
        logger.LogInformation("Unregister instance launcher");
        return Task.CompletedTask;
    }

    public Task Heartbeat(string id)
    {
        logger.LogInformation("Heartbeat instance launcher");
        return Task.CompletedTask;
    }

    public void RegisterInstance(string launcherId, InstanceInfo newInstance, GameServerType serverType)
    {
        context.GameServers.Add(new GameServer
        {
            InstanceHost = context.InstanceHosts.Single(host => host.ConnectionId == launcherId),
            GameServerType = serverType
        });
        logger.LogInformation("New instance registered");
    }

    public IEnumerable<GameServerType> MissingInstances(string launcherId)
    {
        return context.GameServerTypes
            .Include(type => type.Instances)
            .ThenInclude(server => server.InstanceHost)
            .Where(type => type.StartType != GameServerStartType.OnDemand)
            .Where(type => type.StartType == GameServerStartType.OnePerNetwork && type.Instances.Count == 0)
            .Where(type => type.StartType == GameServerStartType.OnePerHost &&
                           type.Instances.Any(server => server.InstanceHost.ConnectionId == launcherId));
    }
}