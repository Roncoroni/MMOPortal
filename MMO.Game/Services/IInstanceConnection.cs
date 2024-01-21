using MMO.Game.Data;

namespace MMO.Game.Services;

public interface IInstanceConnection
{
    Task StartInstance(GameServer server, GameServerDefinition definition);
}