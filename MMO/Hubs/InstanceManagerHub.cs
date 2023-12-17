using Microsoft.AspNetCore.SignalR;
using MMO.Game.Services;
using MMO.ServerLauncher.Shared;

namespace MMO.Hubs;

public class InstanceManagerHub(InstanceManagement management) : Hub<IInstanceLauncher>, IInstanceManager
{
    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
        await management.RegisterInstanceLauncher(Context.ConnectionId);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await management.UnregisterInstanceLauncher(Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }

    public async Task Heartbeat()
    {
        await management.Heartbeat(Context.ConnectionId);

        var missingServerTypes = management.MissingInstances(Context.ConnectionId);
        foreach (var serverType in missingServerTypes)
        {
            var instanceInfo = await Clients.Caller.LaunchInstance(serverType.MapName);
            if (instanceInfo is not null)
            {
                management.RegisterInstance(Context.ConnectionId, instanceInfo, serverType);
            }
        }
        
        /*
        if (management.InstanceCount() <= 0)
        {
            var instanceInfo = await Clients.Caller.LaunchInstance("CharacterSelection");
            if (instanceInfo is not null)
            {
                management.RegisterInstance(Context.ConnectionId, instanceInfo);
            }
        }*/
    }
}