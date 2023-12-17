namespace MMO.ServerLauncher.Shared;

public record InstanceInfo
{
    public int Port { get; set; }
}

public interface IInstanceLauncher
{
    Task<InstanceInfo?> LaunchInstance(string mapName);
}

public interface IInstanceManager
{
    Task Heartbeat();
}
