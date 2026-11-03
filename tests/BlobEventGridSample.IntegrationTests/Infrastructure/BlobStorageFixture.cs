using Azure.Storage.Blobs;

namespace BlobEventGridSample.IntegrationTests.Infrastructure;

public sealed class BlobStorageFixture
{
    private readonly BlobContainerClient _samplesContainer;
    private readonly BlobContainerClient _processedContainer;

    public BlobStorageFixture(string connectionString)
    {
        BlobServiceClient serviceClient = new(connectionString);
        _samplesContainer = serviceClient.GetBlobContainerClient("samples");
        _processedContainer = serviceClient.GetBlobContainerClient("processed");
    }

    public async Task UploadFileAsync(string fileName, byte[] content, CancellationToken ct = default)
    {
        await _samplesContainer.CreateIfNotExistsAsync(cancellationToken: ct);
        await _samplesContainer.GetBlobClient(fileName).UploadAsync(new BinaryData(content), overwrite: true, ct);
    }

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
