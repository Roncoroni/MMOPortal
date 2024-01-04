namespace MMO.ServerLauncher.Shared;

public interface IInstanceLauncher
{
    Task LaunchInstance(Guid serverId, string mapName, string token, string serverType);
    Task ShutdownInstance(Guid serverId);
}

public interface IInstanceManager
{
    Task Heartbeat();
    Task InstanceStarted(Guid serverId, ushort port);
    Task InstanceStopped(Guid serverId);
}
