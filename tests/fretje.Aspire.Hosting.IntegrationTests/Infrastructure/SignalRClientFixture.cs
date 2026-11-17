using Microsoft.AspNetCore.SignalR.Client;

namespace fretje.Aspire.Hosting.IntegrationTests.Infrastructure;

/// <summary>Manages a SignalR client connection to the upstream sample hub.</summary>
public sealed class SignalRClientFixture : IAsyncDisposable
{
    private readonly HubConnection _connection;

    /// <summary>Initializes a new instance of the <see cref="SignalRClientFixture"/> class.</summary>
    /// <param name="hubUrl">The URL of the sample hub endpoint.</param>
    public SignalRClientFixture(string hubUrl)
    {
        var userId = $"integration-test-{Guid.NewGuid():N}";
        var connectionUrl = $"{hubUrl}?userId={Uri.EscapeDataString(userId)}";
        _connection = new HubConnectionBuilder()
            .WithUrl(connectionUrl)
            .Build();
    }

    /// <summary>Connects the client to the hub.</summary>
    /// <param name="ct">A token to cancel the connection.</param>
    public Task ConnectAsync(CancellationToken ct = default) => _connection.StartAsync(ct);

    /// <summary>Disconnects the client from the hub.</summary>
    /// <param name="ct">A token to cancel the disconnection.</param>
    public Task DisconnectAsync(CancellationToken ct = default) => _connection.StopAsync(ct);

    /// <inheritdoc />
    public ValueTask DisposeAsync() => _connection.DisposeAsync();
}
