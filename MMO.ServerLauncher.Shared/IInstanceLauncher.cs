namespace MMO.ServerLauncher.Shared;

public interface IInstanceLauncher
{
    Task LaunchInstance(Guid serverTypeId, string mapName);
    Task ShutdownInstance(Guid serverTypeId, ushort port);
}

public interface IInstanceManager
{
    Task Heartbeat();
    Task InstanceStarted(Guid serverTypeId, ushort port);
    Task InstanceStopped(Guid serverTypeId, ushort port);
}
