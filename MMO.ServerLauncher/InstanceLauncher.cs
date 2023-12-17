using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Options;
using MMO.ServerLauncher.Shared;
using TypedSignalR.Client;

namespace MMO.ServerLauncher;

public class InstanceLauncher(ILogger<InstanceLauncher> logger, IOptions<InstanceLauncherOptions> config)
    : IHostedService, IDisposable, IInstanceLauncher
{
    private Timer? _timer = null;
    private readonly InstanceLauncherOptions _options = config.Value;
    private HubConnection? _connection;
    private IInstanceManager? _instanceManager;
    private IDisposable? _subscription;

    private readonly List<int> runningInstances = new List<int>();

    public Task<InstanceInfo?> LaunchInstance(string mapName)
    {
        logger.LogInformation("OnLaunchInstance");
        
        var port = GetAvailablePort(_options.MinPort, _options.MaxPort);
        var project = _options.IsEditor ? $"\"{_options.PathToProject}\" " : "";
        var serverArguments = $"{project}{mapName}?listen -server -log -port={port}";

        var proc = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = _options.PathToExecutable,
                Arguments = Encoding.Default.GetString(Encoding.UTF8.GetBytes(serverArguments)),
                UseShellExecute = false,
                RedirectStandardOutput = false,
                CreateNoWindow = false
            }
        };

        var successfullyStarted = proc.Start();

        runningInstances.Add(proc.Id);

        return Task.FromResult(
            successfullyStarted
                ? new InstanceInfo { Port = port }
                : null
        );
    }

    private void ShutdownAllInstances()
    {
        foreach (var instanceProcId in runningInstances)
        {
            try
            {
                var process = Process.GetProcessById(instanceProcId);
                process.Kill();
            }
            catch (Exception e)
            {
                logger.LogError(e, "Could not stop instance");
            }
        }
        runningInstances.Clear();
    }
    
    private void SendHeartbeat(object? state)
    {
        logger.LogInformation("Send Heartbeat");
        _instanceManager?.Heartbeat();
    }

    private static int GetAvailablePort(int startingPort, int lastPort)
    {
        IPEndPoint[] endPoints;
        List<int> portArray = new List<int>();

        IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();
        //getting active connections
        TcpConnectionInformation[] connections = properties.GetActiveTcpConnections();
        portArray.AddRange(connections
            .Where(n => n.LocalEndPoint.Port >= startingPort && n.LocalEndPoint.Port <= lastPort)
            .Select(n => n.LocalEndPoint.Port));

        //getting active tcp listeners
        endPoints = properties.GetActiveTcpListeners();
        portArray.AddRange(endPoints
            .Where(n => n.Port >= startingPort && n.Port <= lastPort)
            .Select(n => n.Port));
        
        portArray.Sort();

        for (var i = startingPort; i < UInt16.MaxValue; i++)
            if (!portArray.Contains(i))
                return i;

        return 0;
    }

    public async Task StartAsync(CancellationToken stoppingToken)
    {
        _connection = new HubConnectionBuilder()
            .WithUrl(_options.HubUrl)
            .WithAutomaticReconnect()
            .Build();

        _instanceManager = _connection.CreateHubProxy<IInstanceManager>(cancellationToken: stoppingToken);

        _subscription = _connection.Register<IInstanceLauncher>(this);

        await _connection.StartAsync(stoppingToken);
        _timer = new Timer(SendHeartbeat, null, TimeSpan.FromSeconds(5),
            TimeSpan.FromSeconds(60));
    }

    public Task StopAsync(CancellationToken stoppingToken)
    {
        _timer?.Change(Timeout.Infinite, 0);
        ShutdownAllInstances();

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
        _subscription?.Dispose();
    }
}