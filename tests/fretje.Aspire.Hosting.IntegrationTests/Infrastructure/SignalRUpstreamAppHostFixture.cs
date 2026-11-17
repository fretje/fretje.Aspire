using Aspire.Hosting.Testing;

namespace fretje.Aspire.Hosting.IntegrationTests.Infrastructure;

/// <summary>Fixture that starts the SignalR upstream sample without its standalone client resource.</summary>
public sealed class SignalRUpstreamAppHostFixture : AppHostFixture<Projects.SignalRUpstreamSample_AppHost>
{
    /// <summary>Gets the SignalR client connected to the sample hub endpoint.</summary>
    public SignalRClientFixture Client { get; private set; } = null!;

    /// <inheritdoc />
    protected override IReadOnlyList<string> AppHostArguments => ["IncludeClientProjects=false"];

    /// <inheritdoc />
    protected override ValueTask OnStartedAsync(CancellationToken ct)
    {
        Client = new($"{App.GetEndpoint("functionapp").ToString().TrimEnd('/')}/api");
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    protected override ValueTask OnStoppingAsync(CancellationToken ct) => Client.DisposeAsync();
}
