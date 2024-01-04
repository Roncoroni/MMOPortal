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
    public async Task InstanceHeartbeat(Guid serverId, ushort serverPort)
    {
        /*await dbContext.GameServers.Where(server => 
                server.InstanceHostId == serverId
                && server.Port == serverPort
            )
            .ExecuteUpdateAsync(calls => 
                calls.SetProperty(source => source.));*/
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
/*
        foreach (var characterUpdate in characterBatchUpdate.Batch)
        {
            var updated = dbSet.Persist(mapper).InsertOrUpdate(characterUpdate);
            if (updated != null)
            {
                logger.LogInformation("Update succeeded: {}", updated.ToString());
            }
            else
            {
                logger.LogWarning("Update failed");
            }
        }*/

/*
            await Task.WhenAll(
                characterBatchUpdate.Batch
                    .Select(update => dbSet.Persist(mapper).InsertOrUpdateAsync(update)));
*/
        //await dbContext.SaveChangesAsync();
    }
}