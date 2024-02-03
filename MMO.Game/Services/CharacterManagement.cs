using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MMO.Game.Data;
using MMO.Game.DTO;

namespace MMO.Game.Services;

public class CharacterManagement(
    ILogger<CharacterManagement> logger,
    IGameDbContext dbContext)
{
    public async Task<bool> CreateCharacter(Guid AccountId, string CharacterName)
    {
        await dbContext.Characters.AddAsync(new Character
        {
            Name = CharacterName,
            AccountId = AccountId,
        });
        await dbContext.SaveChangesAsync();
        return true;
    }
    
    public IQueryable<Character> GetCharacterList(Guid AccountId)
    {
        return dbContext.Characters.Where(character => character.AccountId == AccountId);
    }

    public Task<Character> GetCharacter(Guid AccountId)
    {
        return dbContext.Characters.FirstAsync(character => character.AccountId == AccountId);
    }
    
    public Task<Character> GetCharacter(Guid AccountId, Guid CharacterId)
    {
        return dbContext.Characters.SingleAsync(character => character.AccountId == AccountId && character.CharacterId == CharacterId);
    }

    public async Task<bool> UpdateCharacter(Guid AccountId, Guid CharacterId, CharacterUpdate characterUpdate)
    {
        var count = await dbContext.Characters.Where(character => character.AccountId == AccountId &&
                                                                  character.CharacterId == CharacterId)
            .ExecuteUpdateAsync(calls => calls
                .SetProperty(character => character.PositionX, characterUpdate.PositionX)
                .SetProperty(character => character.PositionY, characterUpdate.PositionY)
                .SetProperty(character => character.PositionZ, characterUpdate.PositionZ)
                .SetProperty(character => character.Orientation, characterUpdate.Rotation)
            );
        return count > 0;
    }
    
    public async Task<bool> DeleteCharacter(Guid AccountId, Guid CharacterId)
    {
        await dbContext.Characters
            .Where(character => character.CharacterId == CharacterId && character.AccountId == AccountId)
            .ExecuteDeleteAsync();
        return true;
    }
}