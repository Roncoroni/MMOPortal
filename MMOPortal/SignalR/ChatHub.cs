using Microsoft.AspNetCore.SignalR;
using MMOPortal.Client.Interfaces;
using SignalRSwaggerGen.Attributes;

namespace MMOPortal.SignalR;

[SignalRHub]
public class ChatHub: Hub<IChatHubClient>
{
    public Task ChatUpdate()
        => Clients.All.ChatUpdate();
}