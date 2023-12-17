using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MMO.Game.Data;
using MMO.Game.DTO;

namespace MMO.Game.Services;

public class ServerManagement(
    ILogger<ServerManagement> logger,
    IGameDbContext dbContext)
{
    public async Task SaveCharacters(CharacterBatchUpdate characterBatchUpdate)
    {
        var dbSet = dbContext.Characters;

        //characterBatchUpdate.Batch;
        await dbSet
            .Join(
                characterBatchUpdate.Batch,
                character => character.CharacterId,
                update => update.CharacterId,
                (character, update) => new { character, update })
            .ExecuteUpdateAsync(calls => calls
                .SetProperty(tuple => tuple.character.PositionX, tuple => tuple.update.PositionX)
                .SetProperty(tuple => tuple.character.PositionY, tuple => tuple.update.PositionY)
                .SetProperty(tuple => tuple.character.PositionZ, tuple => tuple.update.PositionZ)
                .SetProperty(tuple => tuple.character.Orientation, tuple => tuple.update.Rotation)
            );
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