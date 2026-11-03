#pragma warning disable ASPIREPERSISTENCE001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Testing;

namespace BlobEventGridSample.IntegrationTests.Infrastructure;

public sealed class AppHostFixture : IAsyncLifetime
{
    private static readonly TimeSpan _buildTimeout = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan _startTimeout = TimeSpan.FromSeconds(90);

    private DistributedApplication? _app;

    public DistributedApplication App => _app ?? throw new InvalidOperationException("The fixture has not been initialized.");

    public BlobStorageFixture BlobStorage { get; private set; } = null!;

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        var builder = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.BlobEventGridSample_AppHost>(["IncludeClientProjects=false"], Ct);

        _app = await builder
            .WithPersistenceMode(PersistenceMode.Session)
            .BuildAsync(Ct).WaitAsync(_buildTimeout, Ct);

        await _app.StartAsync(Ct).WaitAsync(_startTimeout, Ct);

        // Uploads must go through the proxy so that Event Grid notifications are emitted
        BlobStorage = new(await _app.GetConnectionStringAsync("blobsproxy", Ct)
            ?? throw new InvalidOperationException("Failed to get blobsproxy connection string"));
    }

    public async ValueTask DisposeAsync()
    {
        if (_app is { })
        {
            try
            {
                await _app.StopAsync(CancellationToken.None);
            }
            finally
            {
                await _app.DisposeAsync();
            }
        }
    }
}

file static class DistributedApplicationTestingBuilderExtensions
{
    public static TBuilder WithPersistenceMode<TBuilder>(
        this TBuilder builder, PersistenceMode mode)
        where TBuilder : IDistributedApplicationTestingBuilder
    {
        foreach (var annotation in builder.Resources
            .SelectMany(r => r.Annotations
            .OfType<PersistenceAnnotation>()
            .Where(c => c.Mode != mode)))
        {
            annotation.Mode = mode;
        }

        return builder;
    }
}
