using System.Diagnostics;
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
    private CancellationTokenSource _stoppingCts;

    private readonly List<InstanceProcess> _runningInstances = new();

    public async Task LaunchInstance(Guid serverId, string mapName, string token, string serverType)
    {
        if (_instanceManager is not null)
        {
            logger.LogInformation("OnLaunchInstance");

            var port = GetAvailablePort(_options.MinPort, _options.MaxPort);
            var project = _options.IsEditor ? $"\"{_options.PathToProject}\" " : "";
            var serverArguments = $"{project}{mapName}?listen -server -log -port={port} -ini:Engine:[MMOServer]:ApiToken={token} -servertype={serverType}";

            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = _options.PathToExecutable,
                    Arguments = Encoding.Default.GetString(Encoding.UTF8.GetBytes(serverArguments)),
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = false,
                },
            };
            
            var successfullyStarted = proc.Start();
            if (successfullyStarted)
            {
                var instanceInfo = new InstanceProcess{Port = port, ServerId = serverId, Process = proc};

                _runningInstances.Add(instanceInfo);
                await _instanceManager.InstanceStarted(instanceInfo.ServerId, instanceInfo.Port);//.ConfigureAwait(false);
            }
        }
    }

    public async Task ShutdownInstance(Guid serverId)
    {
        var instances = _runningInstances
            .Where(process => process.ServerId == serverId);
        foreach (var instance in instances)
        {
            await KillInstance(instance);
        }
    }

    private async Task ShutdownAllInstances()
    {
        var instances = _runningInstances.ToArray();
        foreach (var info in instances)
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
                await _instanceManager.InstanceStopped(info.ServerId).ConfigureAwait(false);
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

        //getting active udp listeners
        endPoints = properties.GetActiveUdpListeners();
        portArray.AddRange(endPoints
            .Where(n => n.Port >= startingPort)
            .Select(n => (ushort)n.Port));
        
        portArray.Sort();

        for (var i = startingPort; i < ushort.MaxValue; i++)
            if (!portArray.Contains(i))
                return i;

        return 0;
    }

    public async Task StartAsync(CancellationToken stoppingToken)
    {
        _stoppingCts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
        _connection = new HubConnectionBuilder()
            .WithUrl(_options.HubUrl, options =>
            {
                //options.Headers["Signature"] = 
                options.Headers["Authorization"] = $"GameServer {_options.ApiToken}";
            })
            .WithAutomaticReconnect()
            .Build();
        _instanceManager = _connection.CreateHubProxy<IInstanceManager>(cancellationToken: _stoppingCts.Token);

        _subscription = _connection.Register<IInstanceLauncher>(this);

        await _connection.StartAsync(_stoppingCts.Token).ConfigureAwait(false);
        _timer = new Timer(SendHeartbeat, null, TimeSpan.FromSeconds(5),
            TimeSpan.FromSeconds(60));
    }

    public async Task StopAsync(CancellationToken stoppingToken)
    {
        stoppingToken.Register(() => _stoppingCts.Cancel());
        _timer?.Change(Timeout.Infinite, 0);
        await ShutdownAllInstances().ConfigureAwait(false);
        await _stoppingCts.CancelAsync().ConfigureAwait(false);
    }

    public void Dispose()
    {
        _stoppingCts.Cancel();
        _timer?.Dispose();
        _subscription?.Dispose();
    }
}