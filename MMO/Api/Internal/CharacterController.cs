using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using MMO.Api.DTO;
using MMO.Game.Data;
using MMO.Game.DTO;
using MMO.Game.Services;

namespace MMO.Api.Internal
{
    [Route("api/[controller]/{accountId:guid}")]
    [Authorize("InstanceLauncher")]
    [ApiController]
    public class CharacterController : ControllerBase
    {
        [HttpGet("characters")]
        public IEnumerable<Character> GetCharacters([FromRoute]Guid accountId, CharacterManagement characterManagement)
        {
            return characterManagement.GetCharacters(accountId);
        }
        
        [HttpGet("{characterId:guid}/world")]
        public async Task<ServerUrlResult> GetWorld(
            [FromRoute]Guid accountId, 
            [FromRoute]Guid characterId, 
            InstanceHostManagement instanceHostManagement,
            ILogger<CharacterController> logger)
        {
            var url = await instanceHostManagement.GetWorldServer(accountId, characterId);
            logger.LogInformation("GetWorldServer: {Url}", url);
            return new(url);
        }
        
        [HttpGet("character")]
        public Task<Character> GetRecentUsedCharacter([FromRoute]Guid accountId, CharacterManagement characterManagement)
        {
            return characterManagement.GetCharacter(accountId);
        }
        
        [HttpGet("{characterId:guid}")]
        public Task<Character> GetCharacter([FromRoute]Guid accountId, [FromRoute]Guid characterId, CharacterManagement characterManagement)
        {
            return characterManagement.GetCharacter(accountId, characterId);
        }
        
        [HttpPost("character")]
        public async Task<Results<Ok, NotFound>> GetRecentUsedCharacter([FromRoute]Guid accountId, CharacterUpdate characterUpdate, CharacterManagement characterManagement)
        {
            var success = await characterManagement.UpdateCharacter(accountId, characterUpdate);
            return success ? TypedResults.Ok() : TypedResults.NotFound();
        }
    }
}
