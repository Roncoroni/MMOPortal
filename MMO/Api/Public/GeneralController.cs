using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MMO.Api.DTO;
using MMO.Game.Services;

namespace MMO.Api.Public;

[Route("api/[controller]")]
[Authorize]
[ApiController]
public class GeneralController : ControllerBase
{
    [HttpGet("entry")]
    public async Task<ServerUrlResult> GetEntryServer(
        InstanceHostManagement instanceHostManagement,
        ILogger<GeneralController> logger)
    {
        var url = await instanceHostManagement.GetEntryServer();
        logger.LogInformation("GetEntryServer: {Url}", url);
        return new(url);
    }
}