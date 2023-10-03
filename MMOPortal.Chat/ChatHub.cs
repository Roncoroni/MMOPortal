using Microsoft.AspNetCore.SignalR;
using MMOPortal.Shared;
using SignalRSwaggerGen.Attributes;

namespace MMOPortal.Chat;

[SignalRHub]
public class ChatHub: Hub<IChatHubClient>
{
    public Task ChatUpdate()
        => Clients.All.ChatUpdate();
}