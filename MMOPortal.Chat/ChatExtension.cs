using System.Diagnostics.CodeAnalysis;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using MMOPortal.Shared;

namespace MMOPortal.Chat;

public static class ChatExtension
{
    public static void AddChat(this IServiceCollection services)
    {
        services.AddSignalR();
        services.AddSingleton<IChatService, ChatService>();
        services.AddSwaggerGen(options =>
            options.AddSignalRSwaggerGen(genOptions => genOptions.ScanAssembly(typeof(ChatExtension).Assembly)));
    }

    public static void UseChat(this IEndpointRouteBuilder endpoints, [StringSyntax("Route")] string path,WebApplication app )
    {
        var group = endpoints.MapGroup(path);
        group.MapHub<ChatHub>("/hub");
        group.MapGet("", ([FromServices] IChatService chatService) => chatService.MessagesObservable.Take(1).ToTask());
        group.MapPost("", void ([FromBody] string message, IChatService chatService) => chatService.SendChatMessage(message));
    }
}