using Azure.Storage.Blobs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace BlobEventGridSample.Functions;

/// <summary>Azure Function that triggers on blob creation events via Event Grid and copies the blob to a processed container.</summary>
/// <param name="logger">The logger instance.</param>
public class BlobTriggerFunction(ILogger<BlobTriggerFunction> logger)
{
    /// <summary>Runs the blob trigger function.</summary>
    /// <param name="_">The blob trigger payload (ignored).</param>
    /// <param name="name">The name of the blob that triggered the function.</param>
    /// <param name="metadata">The blob metadata from the Event Grid event.</param>
    /// <param name="sourceBlob">The source blob client for reading the blob content.</param>
    /// <param name="destContainer">The destination container client for writing the processed blob.</param>
    /// <param name="ct">A token to cancel the asynchronous operation.</param>
    /// <returns>A task that completes when the blob has been processed and copied.</returns>
    [Function("BlobTriggerFunction")]
    public async Task RunAsync(
        [BlobTrigger("samples/{name}", Source = BlobTriggerSource.EventGrid)] string _,
        string name,
        IDictionary<string, string> metadata,
        [BlobInput("samples/{name}")] BlobClient sourceBlob,
        [BlobInput("processed")] BlobContainerClient destContainer,
        CancellationToken ct)
    {
        logger.LogInformation("Blob trigger fired for blob '{Name}'", name);

        // Process the blob
        var content = await sourceBlob.OpenReadAsync(cancellationToken: ct);
        using StreamReader reader = new(content);
        var blobContent = await reader.ReadToEndAsync(ct);

        logger.LogInformation("Blob content: {Content}", blobContent);
        logger.LogInformation("Metadata: {@Metadata}", metadata);

        await destContainer.CreateIfNotExistsAsync(cancellationToken: ct);
        await destContainer.GetBlobClient(name).UploadAsync(new BinaryData(blobContent), overwrite: true, cancellationToken: ct);
    }
}
