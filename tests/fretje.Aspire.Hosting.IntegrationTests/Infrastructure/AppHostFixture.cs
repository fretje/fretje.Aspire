#pragma warning disable ASPIREPERSISTENCE001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Testing;

namespace fretje.Aspire.Hosting.IntegrationTests.Infrastructure;

/// <summary>Base fixture that builds and starts an Aspire AppHost for integration tests.</summary>
/// <typeparam name="TAppHost">The generated AppHost project type.</typeparam>
public abstract class AppHostFixture<TAppHost> : IAsyncLifetime
    where TAppHost : class
{
    private static readonly TimeSpan _buildTimeout = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan _startTimeout = TimeSpan.FromSeconds(90);

    private DistributedApplication? _app;

    /// <summary>Gets the running distributed application.</summary>
    public DistributedApplication App => _app ?? throw new InvalidOperationException("The fixture has not been initialized.");

    /// <summary>Gets arguments passed to the AppHost during testing.</summary>
    protected virtual IReadOnlyList<string> AppHostArguments => [];

    /// <summary>Initializes the application asynchronously.</summary>
    public async ValueTask InitializeAsync()
    {
        var builder = await DistributedApplicationTestingBuilder
            .CreateAsync<TAppHost>([.. AppHostArguments], XunitCt);

        builder.WithPersistenceMode(PersistenceMode.Session);

        _app = await builder.BuildAsync(XunitCt).WaitAsync(_buildTimeout, XunitCt);

        await _app.StartAsync(XunitCt).WaitAsync(_startTimeout, XunitCt);

        await OnStartedAsync(XunitCt);
    }

    /// <summary>Disposes the application asynchronously.</summary>
    public async ValueTask DisposeAsync()
    {
        try
        {
            await OnStoppingAsync(CancellationToken.None);
        }
        finally
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

    /// <summary>Runs sample-specific setup after the AppHost has started.</summary>
    /// <param name="ct">A token to cancel setup.</param>
    /// <returns>A task that completes when setup has finished.</returns>
    protected virtual ValueTask OnStartedAsync(CancellationToken ct) => ValueTask.CompletedTask;

    /// <summary>Runs sample-specific cleanup before the AppHost is stopped.</summary>
    /// <param name="ct">A token to cancel cleanup.</param>
    /// <returns>A task that completes when cleanup has finished.</returns>
    protected virtual ValueTask OnStoppingAsync(CancellationToken ct) => ValueTask.CompletedTask;

}

/// <summary>Extension methods for <see cref="IDistributedApplicationTestingBuilder"/> that set the persistence mode.</summary>
file static class DistributedApplicationTestingBuilderExtensions
{
    /// <summary>Sets the persistence mode for all resources that already have a <see cref="PersistenceAnnotation"/> to the given mode.</summary>
    /// <typeparam name="TBuilder">The type of the distributed application testing builder.</typeparam>
    /// <param name="builder">The distributed application testing builder.</param>
    /// <param name="mode">The persistence mode to apply.</param>
    /// <returns>The builder, for chaining.</returns>
    public static TBuilder WithPersistenceMode<TBuilder>(this TBuilder builder, PersistenceMode mode)
        where TBuilder : IDistributedApplicationTestingBuilder
    {
        foreach (var annotation in builder.Resources
            .SelectMany(resource => resource.Annotations
                .OfType<PersistenceAnnotation>()
                .Where(annotation => annotation.Mode != mode)))
        {
            annotation.Mode = mode;
        }

        return builder;
    }
}
