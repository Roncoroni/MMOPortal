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
public class InstanceManagerHub(
    InstanceHostManagement hostManagement, 
    ILogger<InstanceManagerHub> logger
    )
    : Hub<IInstanceLauncher>, IInstanceManager
{
    public override async Task OnConnectedAsync()
    {
        Debug.Assert(Context.UserIdentifier is not null);
        await base.OnConnectedAsync();
        var feature = Context.Features.Get<IHttpConnectionFeature>();
        // here you could get your client remote address
        var remoteAddress = feature?.RemoteIpAddress?.MapToIPv4();
        await hostManagement.RegisterInstanceLauncher(Guid.Parse(Context.UserIdentifier), remoteAddress);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        Debug.Assert(Context.UserIdentifier is not null);
        await hostManagement.UnregisterInstanceLauncher(Guid.Parse(Context.UserIdentifier));
        await base.OnDisconnectedAsync(exception);
    }

    public async Task Heartbeat()
    {
        Debug.Assert(Context.UserIdentifier is not null);
        await hostManagement.InstanceHostHeartbeat(Guid.Parse(Context.UserIdentifier));

        /*var missingServerTypes = management.MissingInstances(Guid.Parse(Context.UserIdentifier));
        foreach (var serverType in missingServerTypes)
        {
            try
            {
                await StartInstance(serverType);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to launch new instance for ServerType {ServerTypeName}", serverType.Name);
            }
        }*/
    }

    public async Task InstanceStarted(Guid ServerId, ushort Port)
    {
        Debug.Assert(Context.UserIdentifier is not null);
        await hostManagement.RegisterInstance(ServerId, Port);
        logger.LogInformation("InstanceStarted Port:{Port}", Port);
    }

    public async Task InstanceStopped(Guid ServerId)
    {
        Debug.Assert(Context.UserIdentifier is not null);
        await hostManagement.UnregisterInstance(ServerId);
        logger.LogError("InstanceStopped");
    }
}