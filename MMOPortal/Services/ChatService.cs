using Microsoft.AspNetCore.SignalR;
using MMOPortal.Client.Interfaces;
using MMOPortal.SignalR;

namespace MMOPortal.Services;

public class ChatService: IChatService
{
    private readonly IHubContext<ChatHub, IChatHubClient> _hubContext;
    
    public ChatService(IHubContext<ChatHub, IChatHubClient> hubContext)
    {
        _hubContext = hubContext;
    }
    private readonly List<string> _messages = new List<string>();

    public IReadOnlyList<string> Messages => _messages;

    public Task SendChatMessage(string message)
    {
        _messages.Add(message);
        _hubContext.Clients.All.ChatUpdate();
        return Task.CompletedTask;
    }

    public Task UpdateChatMessages()
    {
        return Task.CompletedTask;
    }
}