# fretje.Aspire.Hosting.LocalEventing

[![NuGet](https://img.shields.io/nuget/v/fretje.Aspire.Hosting.LocalEventing.svg)](https://www.nuget.org/packages/fretje.Aspire.Hosting.LocalEventing)
[![NuGet downloads](https://img.shields.io/nuget/dt/fretje.Aspire.Hosting.LocalEventing.svg)](https://www.nuget.org/packages/fretje.Aspire.Hosting.LocalEventing)
[![CI](https://github.com/fretje/fretje.Aspire/actions/workflows/ci.yml/badge.svg)](https://github.com/fretje/fretje.Aspire/actions/workflows/ci.yml)
[![License: MIT](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/fretje/fretje.Aspire/blob/main/LICENSE)

Local Event Grid emulator and blob storage event proxy for [.NET Aspire](https://learn.microsoft.com/dotnet/aspire). Enables local development of Azure Functions blob EventGrid triggers by proxying Azurite blob traffic and forwarding events to a local Event Grid emulator.

## Installation

```sh
dotnet add package fretje.Aspire.Hosting.LocalEventing
```

or add it to your project / `Directory.Build.props` manually:

```xml
<PackageReference Include="fretje.Aspire.Hosting.LocalEventing" Version="x.y.z" />
```

Requires .NET SDK 10.0+.

## Usage

In your Aspire AppHost (`AppHost.cs`):

```csharp
using fretje.Aspire.Hosting.LocalEventing;

var builder = DistributedApplication.CreateBuilder(args);

// Azure Storage emulator (Azurite)
var storage = builder.AddAzureStorage("storage")
    .RunAsEmulator();
var blobs = storage.AddBlobs("blobs");

// Azure Functions with blob EventGrid trigger
var functions = builder.AddAzureFunctionsProject<Projects.Functions>("functions")
    .WithHostStorage(storage);

// Event Grid emulator
var eventGrid = builder.AddEventGridEmulator("eventgrid");

// Topic with blob trigger subscription
var blobsTopic = eventGrid.AddTopic("blobstopic")
    .WithBlobTriggerSubscription(functions, "BlobTriggerFunction");

// Blob storage event proxy - sits between your app and Azurite,
// forwarding blob requests and sending Event Grid notifications
var blobsProxy = builder.AddBlobStorageEventProxy("blobsproxy", blobs)
    .WithEventGridTopic(blobsTopic);

// External client that uploads files to blob storage
builder.AddProject<Projects.Client>("client")
    .WithReference(blobsProxy)
    .WaitFor(blobsProxy);

builder.Build().Run();
```

### In your Azure Functions project

```csharp
[Function("BlobTriggerFunction")]
public async Task RunAsync(
    [BlobTrigger("samples/{name}", Source = BlobTriggerSource.EventGrid)] string _,
    string name,
    IDictionary<string, string> metadata,
    [BlobInput("samples/{name}")] BlobClient sourceBlob,
    [BlobInput("processed")] BlobContainerClient destContainer,
    CancellationToken ct)
{
    // Your blob processing logic here
}
```

## How it works

1. **BlobStorageEventProxy** - A container (`vincentfenet/blob-storage-notify-proxy`) that sits between your application and Azurite. It forwards all blob operations to Azurite and sends Event Grid notifications for each operation.

2. **EventGridEmulator** - A container (`workleap/eventgridemulator`) that receives Event Grid events and forwards them to registered webhook subscriptions (like Azure Functions blob triggers).

3. **Connection string rewriting** - The proxy's connection string is the Azurite connection string with `BlobEndpoint` rewritten to point at the proxy (using `127.0.0.1` for Azure SDK compatibility).

## Resources

| Resource | Description |
|----------|-------------|
| `AddBlobStorageEventProxy` | Adds a blob proxy in front of Azurite that forwards events to Event Grid |
| `AddEventGridEmulator` | Adds a local Event Grid emulator container |
| `AddTopic` | Declares a topic on the Event Grid emulator |
| `WithBlobTriggerSubscription` | Subscribes an Azure Functions blob trigger (EventGrid source) to a topic |
| `WithEventGridTriggerSubscription` | Subscribes an Azure Functions `[EventGridTrigger]` to a topic |

## Building from source

```sh
git clone https://github.com/fretje/fretje.Aspire.git
cd fretje.Aspire
dotnet build
dotnet test
dotnet pack src/fretje.Aspire.Hosting.LocalEventing -c Release -o artifacts
```

## Contributing

Bug reports, feature ideas and pull requests are welcome, see [CONTRIBUTING.md](https://github.com/fretje/fretje.Aspire/blob/main/CONTRIBUTING.md).
Notable changes are listed in the [changelog](https://github.com/fretje/fretje.Aspire/blob/main/CHANGELOG.md).

## License

[MIT](https://github.com/fretje/fretje.Aspire/blob/main/LICENSE)
