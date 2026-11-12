#pragma warning disable ASPIREPERSISTENCE001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
using fretje.Aspire.Hosting.LocalEventing;
using Microsoft.Extensions.Configuration;

var builder = DistributedApplication.CreateBuilder(args);

// Azure Storage emulator (Azurite)
var storage = builder.AddAzureStorage("storage")
    .RunAsEmulator(x => x.WithPersistentLifetime());
var blobs = storage.AddBlobs("blobs");

// Azure Functions with blob EventGrid trigger
var functions = builder.AddAzureFunctionsProject<Projects.BlobEventGridSample_Functions>("functions")
    .WithHostStorage(storage);

// Event Grid emulator
var eventGrid = builder.AddEventGridEmulator("eventgrid")
    .WithPersistentLifetime();

// Topic with blob trigger subscription
var blobsTopic = eventGrid.AddTopic("blobstopic")
    .WithBlobTriggerSubscription(functions, "BlobTriggerFunction");

// Blob storage event proxy - sits between your app and Azurite,
// forwarding blob requests and sending Event Grid notifications
var blobsProxy = builder.AddBlobStorageEventProxy("blobsproxy", blobs)
    .WithEventGridTopic(blobsTopic)
    .WithPersistentLifetime();

// External client that uploads files to blob storage
if (builder.Configuration.GetValue("IncludeClientProjects", defaultValue: true))
{
    builder.AddProject<Projects.BlobEventGridSample_Client>("client")
        .WithReference(blobsProxy)
        .WaitFor(blobsProxy);
}

await builder.Build().RunAsync();
