using System.Net.Http.Json;
using Microsoft.AspNetCore.SignalR.Client;
using MMOPortal.Client.Interfaces;

namespace MMOPortal.Client.Services;

public class ChatHttpService : IChatService
{
    private readonly HttpClient _httpClient;
    private readonly HubConnection _connection;

    private readonly List<string> _messages = new List<string>();
    private readonly IDisposable _updateConnection;
    public IReadOnlyList<string> Messages => _messages;

    public ChatHttpService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _connection = new HubConnectionBuilder()
            .WithUrl("/Chat")
            .WithAutomaticReconnect()
            .Build();

        _updateConnection = _connection.On("ChatUpdate", async () =>
        { 
            await UpdateChatMessages();
        });
    }

    public Task SendChatMessage(string message)
    {
        return _httpClient.PostAsJsonAsync("api/Chat", message);
    }

    public async Task UpdateChatMessages()
    {
        var messages = await _httpClient.GetFromJsonAsync<IList<string>>("api/Chat") ??
                       throw new InvalidOperationException();
        _messages.Clear();
        _messages.AddRange(messages);
    }
}