using Microsoft.EntityFrameworkCore;

namespace MMO.Game.Data;

public class GameServer
{
    public Guid GameServerId { get; set; }
    
    private InstanceHost? _instanceHost;
    public InstanceHost InstanceHost
    {
        set => _instanceHost = value;
        get => _instanceHost
               ?? throw new InvalidOperationException("Uninitialized property: " + nameof(InstanceHost));
    }
    public required Guid InstanceHostId { get; set; }
    public ushort Port { get; set; }
    private GameServerDefinition? _gameServerDefinition;

    public GameServerDefinition GameServerDefinition
    {
        set => _gameServerDefinition = value;
        get => _gameServerDefinition
               ?? throw new InvalidOperationException("Uninitialized property: " + nameof(GameServerDefinition));
    }
    public required Guid GameServerDefinitionId { get; set; }
    
    public bool Online { get; set; } = false;
    public DateTime LastHeartbeat { get; set; } = DateTime.UtcNow;
}