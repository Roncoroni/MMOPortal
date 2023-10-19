using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace MMOPortal.GameApi;

public class GameServerTokenEvents
{
    /// <summary>
    /// Invoked when a protocol message is first received.
    /// </summary>
    public Func<GameServerReceivedContext, Task> OnMessageReceived { get; set; } = context => Task.CompletedTask;

    /// <summary>
    /// Invoked when a protocol message is first received.
    /// </summary>
    /// <param name="context">The <see cref="GameServerReceivedContext"/>.</param>
    public virtual Task MessageReceivedAsync(GameServerReceivedContext context) => OnMessageReceived(context);
}

public class GameServerReceivedContext : ResultContext<GameServerTokenOptions>
{
    public GameServerReceivedContext(HttpContext context, AuthenticationScheme scheme, GameServerTokenOptions options) :
        base(context, scheme, options)
    {
    }

    /// <summary>
    /// Game Server Token. This will give the application an opportunity to retrieve a token from an alternative location.
    /// </summary>
    public string? Token { get; set; }
}