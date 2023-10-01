using Microsoft.AspNetCore.SignalR;
using MMOPortal.Client.Interfaces;

namespace MMOPortal.SignalR;

public class ChatHub: Hub<IChatHubClient>
{
    public Task ChatUpdate()
        => Clients.All.ChatUpdate();
}