#pragma warning disable ASPIREPERSISTENCE001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Testing;

namespace BlobEventGridSample.IntegrationTests.Infrastructure;

/// <summary>Fixture that builds and starts the BlobEventGridSample AppHost for integration tests.</summary>
public sealed class AppHostFixture : IAsyncLifetime
{
    private static readonly TimeSpan _buildTimeout = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan _startTimeout = TimeSpan.FromSeconds(90);

    private DistributedApplication? _app;

    /// <summary>Gets the running distributed application.</summary>
    /// <returns>The running distributed application.</returns>
    public DistributedApplication App => _app ?? throw new InvalidOperationException("The fixture has not been initialized.");

    /// <summary>Gets the blob storage fixture used to upload and poll for processed blobs.</summary>
    /// <returns>The blob storage fixture.</returns>
    public BlobStorageFixture BlobStorage { get; private set; } = null!;

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    /// <summary>Initializes the application asynchronously.</summary>
    /// <returns>A task that completes when the application has been started.</returns>
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

    /// <summary>Disposes the application asynchronously.</summary>
    /// <returns>A task that completes when the application has been stopped and disposed.</returns>
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

/// <summary>Extension methods for <see cref="IDistributedApplicationTestingBuilder"/> that set the persistence mode.</summary>
file static class DistributedApplicationTestingBuilderExtensions
{
    /// <summary>Sets the persistence mode for all resources that already have a <see cref="PersistenceAnnotation"/> to the given mode.</summary>
    /// <typeparam name="TBuilder">The type of the distributed application testing builder.</typeparam>
    /// <param name="builder">The distributed application testing builder.</param>
    /// <param name="mode">The persistence mode to apply.</param>
    /// <returns>The builder, for chaining.</returns>
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
