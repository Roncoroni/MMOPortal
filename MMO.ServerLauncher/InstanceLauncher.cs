using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Text;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Options;
using MMO.ServerLauncher.Shared;
using TypedSignalR.Client;

namespace MMO.ServerLauncher;

public record InstanceProcess
{
    public Process Process { get; set; }
    public ushort Port { get; set; }
    public Guid ServerTypeId { get; set; }
}

public class InstanceLauncher(ILogger<InstanceLauncher> logger, IOptions<InstanceLauncherOptions> config)
    : IHostedService, IDisposable, IInstanceLauncher
{
    private Timer? _timer = null;
    private readonly InstanceLauncherOptions _options = config.Value;
    private HubConnection? _connection;
    private IInstanceManager? _instanceManager;
    private IDisposable? _subscription;

    private readonly List<InstanceProcess> _runningInstances = new();

    public async Task LaunchInstance(Guid serverTypeId, string mapName)
    {
        if (_instanceManager is not null)
        {
            logger.LogInformation("OnLaunchInstance");

            var port = GetAvailablePort(_options.MinPort, _options.MaxPort);
            var project = _options.IsEditor ? $"\"{_options.PathToProject}\" " : "";
            var serverArguments = $"{project}{mapName}?listen -server -port={port}";

            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = _options.PathToExecutable,
                    Arguments = Encoding.Default.GetString(Encoding.UTF8.GetBytes(serverArguments)),
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = false,
                }
            };

            var successfullyStarted = proc.Start();
            if (successfullyStarted)
            {
                var instanceInfo = new InstanceProcess{Port = port, ServerTypeId = serverTypeId, Process = proc};

                _runningInstances.Add(instanceInfo);
                await _instanceManager.InstanceStarted(instanceInfo.ServerTypeId, instanceInfo.Port);
            }
        }
    }

    public async Task ShutdownInstance(Guid serverTypeId, ushort port)
    {
        var instances = _runningInstances
            .Where(process => process.ServerTypeId == serverTypeId && process.Port == port);
        foreach (var instance in instances)
        {
            await KillInstance(instance);
        }
    }

    private async Task ShutdownAllInstances()
    {
        foreach (var info in _runningInstances)
        {
            await KillInstance(info);
        }
    }

    private async Task KillInstance(InstanceProcess info)
    {
        try
        {
            info.Process.Kill();
            if (_instanceManager is not null)
            {
                await _instanceManager.InstanceStopped(info.ServerTypeId, info.Port);
            }

            _runningInstances.Remove(info);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Could not stop instance");
        }
    }

    private void SendHeartbeat(object? state)
    {
        logger.LogInformation("Send Heartbeat");

        _instanceManager?.Heartbeat();
    }

    private static ushort GetAvailablePort(ushort startingPort, ushort lastPort)
    {
        List<ushort> portArray = [];

        var properties = IPGlobalProperties.GetIPGlobalProperties();
        //getting active connections
        var connections = properties.GetActiveTcpConnections();
        portArray.AddRange(connections
            .Where(n => n.LocalEndPoint.Port >= startingPort && n.LocalEndPoint.Port <= lastPort)
            .Select(n => (ushort)n.LocalEndPoint.Port));

        //getting active tcp listeners
        var endPoints = properties.GetActiveTcpListeners();
        portArray.AddRange(endPoints
            .Where(n => n.Port >= startingPort && n.Port <= lastPort)
            .Select(n => (ushort)n.Port));

        portArray.Sort();

        for (var i = startingPort; i < ushort.MaxValue; i++)
            if (!portArray.Contains(i))
                return i;

        return 0;
    }

    public async Task StartAsync(CancellationToken stoppingToken)
    {
        _connection = new HubConnectionBuilder()
            .WithUrl(_options.HubUrl, options =>
            {
                //options.Headers["Signature"] = 
                options.Headers["Authorization"] = $"GameServer {_options.ApiToken}";
            })
            .WithAutomaticReconnect()
            .Build();
        _instanceManager = _connection.CreateHubProxy<IInstanceManager>(cancellationToken: stoppingToken);

        _subscription = _connection.Register<IInstanceLauncher>(this);

        await _connection.StartAsync(stoppingToken);
        _timer = new Timer(SendHeartbeat, null, TimeSpan.FromSeconds(5),
            TimeSpan.FromSeconds(60));
    }

    public async Task StopAsync(CancellationToken stoppingToken)
    {
        _timer?.Change(Timeout.Infinite, 0);
        await ShutdownAllInstances();
    }

    public void Dispose()
    {
        _timer?.Dispose();
        _subscription?.Dispose();
    }
}