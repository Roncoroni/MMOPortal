using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MMO.Data;
using MMO.Game.DTO;
using MMO.Game.Services;

namespace MMO.Api.Internal
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class ServerController : ControllerBase
    {
        [HttpPost("characters")]
        public async Task<Results<Ok, NotFound>> SaveCharacters(
            CharacterBatchUpdate characterBatchUpdate,
            [FromServices] ServerManagement serverManagement
            )
        {
            await serverManagement.SaveCharacters(characterBatchUpdate);
            return TypedResults.Ok();
        }
        
        [HttpGet("account/{accountId:guid}")]
        public async Task<Results<Ok, NotFound>> GetAccountStatus(
            [FromRoute] Guid accountId,
            [FromServices] UserManager<ApplicationUser> userManager
        )
        {
            if (await userManager.FindByIdAsync(accountId.ToString()) is not { } user)
            {
                return TypedResults.NotFound();
            }

            return TypedResults.Ok();
        }
    }
}
