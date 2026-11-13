using Azure.Storage.Blobs;

namespace BlobEventGridSample.IntegrationTests.Infrastructure;

/// <summary>Fixture that uploads blobs to the samples container and polls the processed container for results.</summary>
public sealed class BlobStorageFixture
{
    private readonly BlobContainerClient _samplesContainer;
    private readonly BlobContainerClient _processedContainer;

    /// <summary>Initializes a new instance of the <see cref="BlobStorageFixture"/> class.</summary>
    /// <param name="connectionString">The storage connection string.</param>
    public BlobStorageFixture(string connectionString)
    {
        BlobServiceClient serviceClient = new(connectionString);
        _samplesContainer = serviceClient.GetBlobContainerClient("samples");
        _processedContainer = serviceClient.GetBlobContainerClient("processed");
    }

    /// <summary>Uploads a file to the samples container.</summary>
    /// <param name="fileName">The name of the blob to upload.</param>
    /// <param name="content">The blob content.</param>
    /// <param name="ct">A token to cancel the asynchronous operation.</param>
    /// <returns>A task that completes when the upload has finished.</returns>
    public async Task UploadFileAsync(string fileName, byte[] content, CancellationToken ct = default)
    {
        await _samplesContainer.CreateIfNotExistsAsync(cancellationToken: ct);
        await _samplesContainer.GetBlobClient(fileName).UploadAsync(new BinaryData(content), overwrite: true, ct);
    }

    /// <summary>Polls the processed container for a blob with the given name until it appears or the timeout expires.</summary>
    /// <param name="fileName">The name of the blob to wait for.</param>
    /// <param name="timeout">The maximum time to wait, or null for a default of 5 seconds.</param>
    /// <param name="ct">A token to cancel the asynchronous operation.</param>
    /// <returns>The name of the processed blob, or null if it did not appear in time.</returns>
    public async Task<string?> WaitForProcessedBlobAsync(string fileName, TimeSpan? timeout = null, CancellationToken ct = default)
    {
        var start = DateTime.UtcNow;
        var blobClient = _processedContainer.GetBlobClient(fileName);
        var effectiveTimeout = timeout ?? TimeSpan.FromSeconds(5);

        while (DateTime.UtcNow - start < effectiveTimeout)
        {
            if (await blobClient.ExistsAsync(ct))
            {
                return (await blobClient.DownloadContentAsync(ct))
                    .Value.Content.ToString();
            }

            await Task.Delay(400, ct);
        }

        return null;
    }
}
