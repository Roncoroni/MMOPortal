using System.Reactive.Linq;
using System.Reactive.Subjects;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using MMOPortal.Shared;

namespace MMOPortal.Chat;

public class ChatService : IChatService, IDisposable
{
    private readonly ILogger<ChatService> _logger;

    public ChatService(
        IHubContext<ChatHub, IChatHubClient> hubContext,
        ILogger<ChatService> logger)
    {
        _logger = logger;
        MessagesObservable = _messagesSubject
            .Scan(
                new List<string>(),
                (list, s) =>
                {
                    list.Add(s);
                    return list;
                })
            .StartWith(new List<string>())
            .Replay(1)
            .AutoConnect();
        MessagesObservable
            .TakeUntil(_disposed)
            .Subscribe(_ =>
            {
                hubContext.Clients.All.ChatUpdate();
            });
    }

    private readonly Subject<string> _messagesSubject = new();

    public IObservable<IReadOnlyList<string>> MessagesObservable { get; }

    public Task SendChatMessage(string message)
    {
        _messagesSubject.OnNext(message);
        return Task.CompletedTask;
    }

    private readonly Subject<bool> _disposed = new();

    public void Dispose()
    {
        _disposed.OnNext(true);
        _messagesSubject.Dispose();
    }
}