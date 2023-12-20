using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;

namespace MMO.Authentication;

public class GameServerTokenOptions : AuthenticationSchemeOptions
{
    private ISecureDataFormat<AuthenticationTicket>? _serverTokenProtector;

    /// <summary>
    /// Constructs the options used to authenticate using opaque bearer tokens.
    /// </summary>
    public GameServerTokenOptions()
    {
        Events = new();
    }

    /// <summary>
    /// Controls how much time the bearer token will remain valid from the point it is created.
    /// The expiration information is stored in the protected token. Because of that, an expired token will be rejected
    /// even if it is passed to the server after the client should have purged it.
    /// </summary>
    /// <remarks>
    /// Defaults to 1 hour.
    /// </remarks>
    public TimeSpan ServerTokenExpiration { get; set; } = TimeSpan.FromHours(1);

    /// <summary>
    /// If set, the <see cref="ServerTokenProtector"/> is used to protect and unprotect the identity and other properties which are stored in the
    /// token. If not provided, one will be created using <see cref="TicketDataFormat"/> and the <see cref="IDataProtectionProvider"/>
    /// from the application <see cref="IServiceProvider"/>.
    /// </summary>
    public ISecureDataFormat<AuthenticationTicket> ServerTokenProtector
    {
        get => _serverTokenProtector ??
               throw new InvalidOperationException($"{nameof(ServerTokenProtector)} was not set.");
        set => _serverTokenProtector = value;
    }
    
    /// <summary>
    /// The object provided by the application to process events raised by the bearer token authentication handler.
    /// The application may implement the interface fully, or it may create an instance of <see cref="BearerTokenEvents"/>
    /// and assign delegates only to the events it wants to process.
    /// </summary>
    public new GameServerTokenEvents Events
    {
        get { return (GameServerTokenEvents)base.Events!; }
        set { base.Events = value; }
    }
}