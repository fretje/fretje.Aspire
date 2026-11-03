using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__blobsproxy")
    ?? throw new InvalidOperationException("Connection string not found");

BlobServiceClient blobServiceClient = new(connectionString);
var containerClient = blobServiceClient.GetBlobContainerClient("samples");

await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob).ConfigureAwait(false);

Console.WriteLine("Uploading test file to blob storage...");

var blobClient = containerClient.GetBlobClient("test.txt");
await blobClient.UploadAsync(BinaryData.FromString("Hello from the client!"), overwrite: true).ConfigureAwait(false);

Console.WriteLine("File uploaded successfully!");
Console.WriteLine("The blob EventGrid trigger should fire shortly...");

await Task.Delay(TimeSpan.FromSeconds(5)).ConfigureAwait(false);

Console.WriteLine("Done.");
