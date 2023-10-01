namespace MMOPortal.Client.Interfaces;

public interface IChatService
{
    public IReadOnlyList<string> Messages { get; }
    Task SendChatMessage(string message);
    Task UpdateChatMessages();
}

public interface IChatHubClient
{
    Task ChatUpdate();
}