using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using MMOPortal.Client.Interfaces;
using MMOPortal.SignalR;

namespace MMOPortal.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly ILogger<ChatController> _logger;
        private readonly IChatService _chatService;
        
        public ChatController(ILogger<ChatController> logger, IChatService chatService, IHubContext<ChatHub> hubContext)
        {
            _logger = logger;
            _chatService = chatService;
        }

        [HttpPost()]
        public Task SendChatMessage([FromBody] string message)
        {
            _chatService.SendChatMessage(message);
            return Task.CompletedTask;
        }

        [HttpGet()]
        public Task<IReadOnlyList<string>> UpdateChatMessages()
        {
            return Task.FromResult(_chatService.Messages);
        }
    }
}