using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MMO.Game.Data;
using MMO.Game.DTO;

namespace MMO.Game.Services;

public class ServerManagement(
    ILogger<ServerManagement> logger,
    IGameDbContext dbContext)
{
    public async Task InstanceHeartbeat(Guid serverId)
    {
        await dbContext.GameServers.Where(server =>
                server.GameServerId == serverId
            )
            .ExecuteUpdateAsync(calls =>
                calls
                    .SetProperty(source => source.Online, true)
                    .SetProperty(source => source.LastHeartbeat, DateTime.UtcNow));
    }

    public async Task WasKilled(Guid serverId)
    {
        await dbContext.GameServers.Where(server =>
                server.GameServerId == serverId
            )
            .ExecuteUpdateAsync(calls =>
                calls
                    .SetProperty(source => source.Online, false)
                    .SetProperty(source => source.LastHeartbeat, DateTime.UtcNow));
    }

    public async Task SaveCharacters(CharacterBatchUpdate characterBatchUpdate)
    {
        var dbSet = dbContext.Characters;

        var debugInfo = JsonSerializer.Serialize(characterBatchUpdate.Batch);

        logger.LogInformation("SaveCharacters: {DebugInfo}", debugInfo);

        foreach (var update in characterBatchUpdate.Batch)
        {
            await dbSet
                .Where(character => character.CharacterId == update.CharacterId)
                .ExecuteUpdateAsync(calls => calls
                    .SetProperty(character => character.PositionX, update.PositionX)
                    .SetProperty(character => character.PositionY, update.PositionY)
                    .SetProperty(character => character.PositionZ, update.PositionZ)
                    .SetProperty(character => character.Orientation, update.Rotation)
                );
        }
    }
}