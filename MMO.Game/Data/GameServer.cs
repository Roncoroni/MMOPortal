using Microsoft.EntityFrameworkCore;

namespace MMO.Game.Data;

[PrimaryKey(nameof(InstanceHostId), nameof(Port))]
public class GameServer
{
    private InstanceHost? _instanceHost;
    public InstanceHost InstanceHost
    {
        set => _instanceHost = value;
        get => _instanceHost
               ?? throw new InvalidOperationException("Uninitialized property: " + nameof(InstanceHost));
    }
    public required Guid InstanceHostId { get; set; }
    public required ushort Port { get; set; }
    private GameServerType? _gameServerType;

    public GameServerType GameServerType
    {
        set => _gameServerType = value;
        get => _gameServerType
               ?? throw new InvalidOperationException("Uninitialized property: " + nameof(GameServerType));
    }
    public required Guid GameServerTypeId { get; set; }
}