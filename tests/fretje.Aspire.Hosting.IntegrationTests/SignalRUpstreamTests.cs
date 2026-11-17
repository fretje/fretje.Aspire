using fretje.Aspire.Hosting.IntegrationTests.Infrastructure;

namespace fretje.Aspire.Hosting.IntegrationTests;

public class SignalRUpstreamTests(SignalRUpstreamAppHostFixture fixture) : IClassFixture<SignalRUpstreamAppHostFixture>
{
    private readonly SignalRUpstreamAppHostFixture _fixture = fixture;

    [Fact]
    public async Task ClientConnection_ShouldLogConnectedAndDisconnectedEvents()
    {
        await _fixture.Client.ConnectAsync(XunitCt);

        await _fixture.App.ShouldHaveLoggedAsync(
            "functionapp",
            "Executed 'Functions.OnConnected'",
            ct: XunitCt);

        await _fixture.Client.DisconnectAsync(XunitCt);

        await _fixture.App.ShouldHaveLoggedAsync(
            "functionapp",
            "Executed 'Functions.OnDisconnected'",
            ct: XunitCt);
    }
}
