using Aspire.Hosting.Testing;

namespace fretje.Aspire.Hosting.IntegrationTests.Infrastructure;

/// <summary>Fixture that starts the SignalR upstream sample without its standalone client resource.</summary>
public sealed class SignalRUpstreamAppHostFixture : AppHostFixture<Projects.SignalRUpstream_AppHost>
{
    private SignalRClientFixture? _client;

    /// <summary>Gets the SignalR client connected to the sample hub endpoint.</summary>
    public SignalRClientFixture Client => _client
        ?? throw new InvalidOperationException("The SignalR client has not been initialized.");

    /// <inheritdoc />
    protected override IReadOnlyList<string> AppHostArguments => ["IncludeClientProjects=false"];

    /// <inheritdoc />
    protected override ValueTask OnStartedAsync(CancellationToken ct)
    {
        _client = new($"{App.GetEndpoint("functions").ToString().TrimEnd('/')}/api");
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    protected override ValueTask OnStoppingAsync(CancellationToken ct) =>
        _client?.DisposeAsync() ?? ValueTask.CompletedTask;
}
