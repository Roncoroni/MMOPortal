namespace MMO.Game.Data;

public enum GameServerStartType : byte
{
    OnDemand,
    OnePerNetwork,
    OnePerHost
}