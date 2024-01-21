using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MMO.Game.Data;

namespace MMO.Game;

public class HeartbeatWatcher<T>(ILogger<HeartbeatWatcher<T>> logger, IDbContextFactory<T> dbContextFactory)
    : BackgroundService
    where T : DbContext, IGameDbContext
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Heartbeat Watcher {T} running", nameof(T));


        using PeriodicTimer timer = new(TimeSpan.FromMinutes(1));

        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                var dbContext = await dbContextFactory.CreateDbContextAsync(stoppingToken);
                await dbContext.InstanceHosts
                    .Where(host =>
                        host.Online && host.LastHeartbeat < DateTime.UtcNow.AddMinutes(-1)
                    ).ExecuteUpdateAsync(calls =>
                            calls.SetProperty(host => host.Online, false),
                        stoppingToken);

                await dbContext.GameServers
                    .Where(server =>
                        server.Online && server.LastHeartbeat < DateTime.UtcNow.AddMinutes(-1)
                    ).ExecuteUpdateAsync(calls =>
                            calls.SetProperty(server => server.Online, false),
                        stoppingToken);

                logger.LogInformation("Heartbeat Watcher {T} is working", nameof(T));
            }
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("Heartbeat Watcher {T} is stopping", nameof(T));
        }
    }
}