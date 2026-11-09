using Microsoft.AspNetCore.SignalR.Client;

namespace FastFoodDashboard.Services;

public sealed class SignalRService
{
    private readonly IConfiguration _configuration;

    private HubConnection? _connection;
    private IDisposable? _eventHandler;

    public SignalRService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task StartAsync(
        string eventName,
        Func<Task> onEvent,
        Func<Task<string?>>? tokenProvider = null,
        Func<Task>? onReconnected = null,
        Func<string, Task>? onStatusChanged = null)
    {
        // Terminar uma eventual ligação anterior
        await StopAsync();

        var apiBaseUrl = _configuration["Api:BaseUrl"]
            ?? throw new InvalidOperationException(
                "Endereço da API não configurado.");

        var hubUrl = new Uri(
            new Uri(apiBaseUrl),
            "hubs/orders");

        var connection = new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                if (tokenProvider is not null)
                    options.AccessTokenProvider = tokenProvider;
            })
            .WithAutomaticReconnect()
            .Build();

        _connection = connection;

        // Registar o evento que a página pretende receber
        _eventHandler = connection.On(eventName, onEvent);

        connection.Reconnecting += _ =>
            _connection == connection
                ? onStatusChanged?.Invoke("A reconectar...")
                    ?? Task.CompletedTask
                : Task.CompletedTask;

        connection.Reconnected += async _ =>
        {
            if (_connection != connection)
                return;

            if (onStatusChanged is not null)
                await onStatusChanged("Ligado");

            if (onReconnected is not null)
                await onReconnected();
        };

        connection.Closed += _ =>
            _connection == connection
                ? onStatusChanged?.Invoke("Desligado")
                    ?? Task.CompletedTask
                : Task.CompletedTask;

        try
        {
            await connection.StartAsync();

            if (_connection == connection &&
                onStatusChanged is not null)
            {
                await onStatusChanged("Ligado");
            }
        }
        catch
        {
            if (_connection == connection)
                await StopAsync();

            throw;
        }
    }

    public async Task StopAsync()
    {
        // Remover o handler registado
        _eventHandler?.Dispose();
        _eventHandler = null;

        // Terminar a ligação SignalR
        var connection = _connection;
        _connection = null;

        if (connection is not null)
            await connection.DisposeAsync();
    }
}