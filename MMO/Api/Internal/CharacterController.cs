using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using MMO.Game.Data;
using MMO.Game.DTO;
using MMO.Game.Services;

namespace MMO.Api.Internal
{
    [Route("api/[controller]/{accountId:guid}")]
    [Authorize]
    [ApiController]
    public class CharacterController : ControllerBase
    {
        [HttpGet("characters")]
        public IEnumerable<Character> GetCharacters([FromRoute]Guid accountId, [FromServices]CharacterManagement characterManagement)
        {
            return characterManagement.GetCharacters(accountId);
        }
        
        [HttpGet("character")]
        public Task<Character> GetRecentUsedCharacter([FromRoute]Guid accountId, [FromServices]CharacterManagement characterManagement)
        {
            return characterManagement.GetCharacter(accountId);
        }
        
        [HttpPost("character")]
        public async Task<Results<Ok, NotFound>> GetRecentUsedCharacter([FromRoute]Guid accountId, CharacterUpdate characterUpdate, [FromServices]CharacterManagement characterManagement)
        {
            var success = await characterManagement.UpdateCharacter(accountId, characterUpdate);
            return success ? TypedResults.Ok() : TypedResults.NotFound();
        }
    }
}
