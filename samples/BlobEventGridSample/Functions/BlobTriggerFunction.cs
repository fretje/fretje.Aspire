using Azure.Storage.Blobs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace BlobEventGridSample.Functions;

public class BlobTriggerFunction(ILogger<BlobTriggerFunction> logger)
{
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
