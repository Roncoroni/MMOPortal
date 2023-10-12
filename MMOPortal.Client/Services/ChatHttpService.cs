using System.Net.Http.Json;
using System.Reactive;
using System.Reactive.Linq;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using MMOPortal.Shared;
using ReactiveSignalR;

namespace MMOPortal.Client.Services;

public class ChatHttpService : IChatService, IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly HubConnection _connection;

    private readonly ILogger<ChatHttpService> Logger;

    public ChatHttpService(
        HttpClient httpClient,
        NavigationManager navigation,
        ILogger<ChatHttpService> logger)
    {
        _httpClient = httpClient;
        Logger = logger;
        _connection = new HubConnectionBuilder()
            .WithUrl(navigation.ToAbsoluteUri("/api/chat/hub"))
            .ConfigureLogging(logging => { logging.SetMinimumLevel(LogLevel.Information); })
            .WithAutomaticReconnect()
            .Build();

        _connection.StartAsync();
        MessagesObservable = _connection.On("ChatUpdate")
            .StartWith(Unit.Default)
            .Select(_ => _httpClient.GetFromJsonAsync<GetChatResponse>("api/chat"))
            .Switch()
            .Select(response => response ?? new GetChatResponse());

        _connection.Closed += error =>
        {
            Logger.LogInformation(error, "Connection closed: {ConnectionState}", _connection.State);

            return Task.CompletedTask;
        };

        _connection.Reconnecting += error =>
        {
            Logger.LogInformation(error, "Reconnection: {ConnectionState}", _connection.State);
            // Notify users the connection was lost and the client is reconnecting.
            // Start queuing or dropping messages.

            return Task.CompletedTask;
        };

        _connection.Reconnected += connectionId =>
        {
            Logger.LogInformation("Reconnected {ConnectionId}: {ConnectionState}", connectionId, _connection.State);

            // Notify users the connection was reestablished.
            // Start dequeuing messages queued while reconnecting if any.

            return Task.CompletedTask;
        };
    }

    public IObservable<GetChatResponse> MessagesObservable { get; }

    public Task SendChatMessage(SendChatMessageParams message)
    {
        return _httpClient.PostAsJsonAsync("api/chat", message);
    }

    public void Dispose()
    {
        _httpClient.Dispose();
        _connection.DisposeAsync().AsTask();
    }
}