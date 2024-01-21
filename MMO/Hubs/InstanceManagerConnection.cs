using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.SignalR;
using MMO.Authentication;
using MMO.Game.Data;
using MMO.Game.Services;
using MMO.ServerLauncher.Shared;

namespace MMO.Hubs;

public class InstanceManagerConnection(
    IHubContext<InstanceManagerHub, IInstanceLauncher> hubContext,
    GameServerTokenHandler handler,
    IDataProtectionProvider dp,
    ILogger<InstanceManagerConnection> logger
) : IInstanceConnection
{
    public async Task StartInstance(GameServer server, GameServerDefinition definition)
    {
        var serverId = server.GameServerId.ToString();
        var hostId = server.InstanceHostId.ToString();
        logger.LogInformation("GUID:{ServerId}", serverId);
        var identity = new ClaimsPrincipal(
            new ClaimsIdentity(
                new List<Claim>
                {
                    new(GameServerTokenDefaults.ServerIdClaim, serverId),
                            
                }, GameServerTokenDefaults.AuthenticationScheme
            )
        );
                
        var serverTokenProtector = new TicketDataFormat(dp.CreateProtector(GameServerTokenConfigurationOptions._primaryPurpose, GameServerTokenDefaults.AuthenticationScheme, "Token"));
        var token = handler.GenerateToken(serverTokenProtector, identity, GameServerTokenDefaults.AuthenticationScheme, null);
        await hubContext.Clients.User(hostId).LaunchInstance(server.GameServerId, definition.MapName, token, definition.GameServerType.ToString());
    }
}