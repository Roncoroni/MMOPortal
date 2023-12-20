using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.SignalR;
using MMO.Authentication;
using MMO.Game.Services;
using MMO.ServerLauncher.Shared;

namespace MMO.Hubs;

public class ServerIdProvider : IUserIdProvider
{
    public string GetUserId(HubConnectionContext connection)
    {
        return connection.User?.FindFirst(GameServerTokenDefaults.ServerIdClaim)?.Value!;
    }
}

[Authorize("InstanceLauncher")]
public class InstanceManagerHub(InstanceManagement management, ILogger<InstanceManagerHub> logger)
    : Hub<IInstanceLauncher>, IInstanceManager
{
    public override async Task OnConnectedAsync()
    {
        Debug.Assert(Context.UserIdentifier is not null);
        await base.OnConnectedAsync();
        var feature = Context.Features.Get<IHttpConnectionFeature>();
        // here you could get your client remote address
        var remoteAddress = feature?.RemoteIpAddress;
        await management.RegisterInstanceLauncher(Guid.Parse(Context.UserIdentifier), remoteAddress);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        Debug.Assert(Context.UserIdentifier is not null);
        await management.UnregisterInstanceLauncher(Guid.Parse(Context.UserIdentifier));
        await base.OnDisconnectedAsync(exception);
    }

    public async Task Heartbeat()
    {
        Debug.Assert(Context.UserIdentifier is not null);
        await management.Heartbeat(Guid.Parse(Context.UserIdentifier));

        var missingServerTypes = management.MissingInstances(Guid.Parse(Context.UserIdentifier));
        foreach (var serverType in missingServerTypes)
        {
            try
            {
                await Clients.Caller.LaunchInstance(serverType.GameServerTypeId, serverType.MapName);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to launch new instance for ServerType {ServerTypeName}", serverType.Name);
            }
        }
    }

    public async Task InstanceStarted(Guid ServerTypeId, ushort Port)
    {
        Debug.Assert(Context.UserIdentifier is not null);
        await management.RegisterInstance(Guid.Parse(Context.UserIdentifier), ServerTypeId, Port);
        logger.LogInformation("InstanceStarted Port:{Port}", Port);
    }

    public async Task InstanceStopped(Guid ServerTypeId, ushort Port)
    {
        Debug.Assert(Context.UserIdentifier is not null);
        await management.UnregisterInstance(Guid.Parse(Context.UserIdentifier), ServerTypeId, Port);
        logger.LogError("InstanceStopped Port:{Port}", Port);
    }
}