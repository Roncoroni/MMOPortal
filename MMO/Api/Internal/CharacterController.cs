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
    public class CharacterController(CharacterManagement characterManagement) : ControllerBase
    {
        [HttpGet]
        public IEnumerable<Character> GetCharacterList([FromRoute]Guid accountId)
        {
            return characterManagement.GetCharacterList(accountId);
        }
        
        [HttpGet("last")]
        public Task<Character> GetRecentUsedCharacter([FromRoute]Guid accountId)
        {
            return characterManagement.GetCharacter(accountId);
        }

        [HttpPost("create")]
        public async Task<Results<Created, BadRequest>> CreateCharacter([FromRoute]Guid accountId, CharacterCreate CreateParams)
        {
            var success = await characterManagement.CreateCharacter(accountId, CreateParams.CharacterName);
            return success ? TypedResults.Created() : TypedResults.BadRequest();
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
        
        [HttpGet("{characterId:guid}")]
        public Task<Character> GetCharacter([FromRoute]Guid accountId, [FromRoute]Guid characterId)
        {
            return characterManagement.GetCharacter(accountId, characterId);
        }
        
        [HttpPost("{characterId:guid}")]
        public async Task<Results<Ok, NotFound>> UpdateCharacter([FromRoute]Guid accountId, [FromRoute]Guid characterId, CharacterUpdate characterUpdate)
        {
            var success = await characterManagement.UpdateCharacter(accountId, characterId, characterUpdate);
            return success ? TypedResults.Ok() : TypedResults.NotFound();
        }
        
        
        [HttpDelete("{characterId:guid}")]
        public async Task<Results<Ok, NotFound>> DeleteCharacter([FromRoute]Guid accountId, [FromRoute]Guid characterId)
        {
            var success = await characterManagement.DeleteCharacter(accountId, characterId);
            return success ? TypedResults.Ok() : TypedResults.NotFound();
        }
    }
}
