using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MMO.Game.Data;
using MMO.Game.DTO;

namespace MMO.Game.Services;

public class CharacterManagement(
    ILogger<CharacterManagement> logger,
    IGameDbContext dbContext)
{
    public IQueryable<Character> GetCharacters(Guid AccountId)
    {
        return dbContext.Characters.Where(character => character.AccountId == AccountId);
    }

    public Task<Character> GetCharacter(Guid AccountId)
    {
        return dbContext.Characters.SingleAsync(character => character.AccountId == AccountId);
    }
    
    public Task<Character> GetCharacter(Guid AccountId, Guid CharacterId)
    {
        return dbContext.Characters.SingleAsync(character => character.AccountId == AccountId && character.CharacterId == CharacterId);
    }

    public async Task<bool> UpdateCharacter(Guid AccountId, CharacterUpdate characterUpdate)
    {
        var characterSet = dbContext.Characters;

        var count = await characterSet.Where(character => character.AccountId == AccountId &&
                                                          character.CharacterId == characterUpdate.CharacterId)
            .ExecuteUpdateAsync(calls => calls
                .SetProperty(character => character.PositionX, characterUpdate.PositionX)
                .SetProperty(character => character.PositionY, characterUpdate.PositionY)
                .SetProperty(character => character.PositionZ, characterUpdate.PositionZ)
                .SetProperty(character => character.Orientation, characterUpdate.Rotation)
            );
        return count > 0;
    }
}