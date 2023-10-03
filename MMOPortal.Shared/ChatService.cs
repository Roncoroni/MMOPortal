namespace MMOPortal.Shared;

public interface IChatService
{
    public IObservable<IReadOnlyList<string>> MessagesObservable { get; }
    Task SendChatMessage(string message);
}

public interface IChatHubClient
{
    Task ChatUpdate();
}
