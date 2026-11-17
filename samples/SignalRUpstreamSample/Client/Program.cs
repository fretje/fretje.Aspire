using Microsoft.AspNetCore.SignalR.Client;

var functionAppUrl = Environment.GetEnvironmentVariable("services__functionapp__http__0")
    ?? throw new InvalidOperationException("The Aspire functionapp HTTP endpoint reference is not configured.");
var userId = Guid.NewGuid().ToString("N");
var hubUrl = $"{functionAppUrl.TrimEnd('/')}/api?userId={Uri.EscapeDataString(userId)}";

await using var connection = new HubConnectionBuilder()
    .WithUrl(hubUrl)
    .WithAutomaticReconnect()
    .Build();

connection.Reconnecting += error =>
{
    Console.WriteLine($"SignalR connection interrupted: {error?.Message ?? "reconnecting"}");
    return Task.CompletedTask;
};

connection.Reconnected += connectionId =>
{
    Console.WriteLine($"SignalR reconnected. Connection ID: {connectionId}");
    return Task.CompletedTask;
};

await connection.StartAsync();
Console.WriteLine($"Connected to the SampleHub as {userId}. Connection ID: {connection.ConnectionId}");
Console.WriteLine("Press Ctrl+C to disconnect and stop the client.");

await Task.Delay(Timeout.InfiniteTimeSpan);
