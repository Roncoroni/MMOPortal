namespace MMOPortal.Shared;

public record GetChatResponse
{
    public IReadOnlyList<string> Messages { get; set; }
}

public record SendChatMessageParams
{
    public string Message { get; set; }
}

public interface IChatService
{
    public IObservable<GetChatResponse> MessagesObservable { get; }
    Task SendChatMessage(SendChatMessageParams message);
}

public interface IChatHubClient
{
    Task ChatUpdate();
}
