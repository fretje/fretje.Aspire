using Aspire.Hosting.Testing;

namespace fretje.Aspire.Hosting.IntegrationTests.Infrastructure;

/// <summary>Fixture that starts the Blob Event Grid sample and exposes its blob storage test helper.</summary>
public sealed class BlobEventGridAppHostFixture : AppHostFixture<Projects.BlobEventGridSample_AppHost>
{
    /// <summary>Gets the blob storage fixture used to upload and poll for processed blobs.</summary>
    public BlobStorageFixture BlobStorage { get; private set; } = null!;

    /// <inheritdoc />
    protected override IReadOnlyList<string> AppHostArguments => ["IncludeClientProjects=false"];

    /// <inheritdoc />
    protected override async ValueTask OnStartedAsync(CancellationToken ct) =>
        BlobStorage = new(await App.GetConnectionStringAsync("blobsproxy", ct)
            ?? throw new InvalidOperationException("Failed to get blobsproxy connection string"));
}
