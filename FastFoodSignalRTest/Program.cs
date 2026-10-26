
using Microsoft.AspNetCore.SignalR.Client;

Console.Write("Introduz o JWT de teste: ");
string? token = Console.ReadLine();

if (string.IsNullOrWhiteSpace(token))
{
    Console.WriteLine("Token não fornecido.");
    return;
}

// Criar ligação ao OrdersHub
await using var connection = new HubConnectionBuilder()
    .WithUrl(
        "https://localhost:7098/hubs/orders",
        options =>
        {
            options.AccessTokenProvider =
                () => Task.FromResult<string?>(token);
        })
    .WithAutomaticReconnect()
    .Build();

// Escutar o evento OrdersChanged
connection.On("OrdersChanged", () =>
{
    Console.WriteLine(
        $"[{DateTime.Now:HH:mm:ss}] OrdersChanged recebido!");
});

// Estabelecer a ligação
await connection.StartAsync();

Console.WriteLine("Ligado ao SignalR!");
Console.WriteLine("Pressiona ENTER para terminar.");

Console.ReadLine();
