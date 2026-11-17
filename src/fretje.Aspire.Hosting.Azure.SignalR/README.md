# fretje.Aspire.Hosting.Azure.SignalR

[![NuGet](https://img.shields.io/nuget/v/fretje.Aspire.Hosting.Azure.SignalR.svg)](https://www.nuget.org/packages/fretje.Aspire.Hosting.Azure.SignalR)
[![NuGet downloads](https://img.shields.io/nuget/dt/fretje.Aspire.Hosting.Azure.SignalR.svg)](https://www.nuget.org/packages/fretje.Aspire.Hosting.Azure.SignalR)
[![CI](https://github.com/fretje/fretje.Aspire/actions/workflows/ci.yml/badge.svg)](https://github.com/fretje/fretje.Aspire/actions/workflows/ci.yml)
[![License: MIT](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/fretje/fretje.Aspire/blob/main/LICENSE)

Upstream endpoint support for the Azure SignalR Service emulator in [.NET Aspire](https://learn.microsoft.com/dotnet/aspire).
Point the emulator at one or more upstream URLs, such as an Azure Functions SignalR trigger, so serverless
SignalR messages and connection events reach your local app.

## Installation

```sh
dotnet add package fretje.Aspire.Hosting.Azure.SignalR
```

It builds on the `Aspire.Hosting.Azure.SignalR` hosting integration, which is brought in as a dependency.

## Usage

In your AppHost, run Azure SignalR Service as the emulator in serverless mode and add the upstreams:

```csharp
using Aspire.Hosting.Azure;
using fretje.Aspire.Hosting.Azure.SignalR;

var builder = DistributedApplication.CreateBuilder(args);

var signalR = builder.AddAzureSignalR("signalr", AzureSignalRServiceMode.Serverless)
    .RunAsEmulator();

var functions = builder.AddAzureFunctionsProject<Projects.MyFunctions>("functions")
    .WithReference(signalR)
    .WaitFor(signalR);

// Forward SignalR events to the SignalR trigger of the Functions app
signalR.WithUpstreamFunctionsProject(functions);

builder.Build().Run();
```

`WithUpstream` is added to the SignalR resource itself, so you can call it after the resources that reference
the SignalR resource have been declared. This avoids a circular reference between the SignalR and the Functions resource.

Make sure the `ConnectionStringSetting` of your SignalR bindings points at the setting holding the emulator
connection string. With `WithReference(signalR)` Aspire provides it as `ConnectionStrings__signalr`.

### Multiple upstreams and any URL

Call `WithUpstream` as often as you need, with an endpoint of any resource, or with any URL.
The URL can contain the `{hub}`, `{category}` and `{event}` placeholders:

```csharp
signalR
    // Only the "chat" hub goes to the API
    .WithUpstream(api, "/{hub}/api/{category}/{event}", hubPattern: "chat")
    // Everything else goes to the Functions app
    .WithUpstreamFunctionsProject(functions)
    // Any other URL works as well
    .WithUpstream("https://example.test/signalr/{hub}");
```

Azure SignalR Service sends an event to the **first** template whose rules match, so add the most specific
upstream first. The hub, category and event rules default to `*` (match everything); see the
[Azure SignalR upstream documentation](https://learn.microsoft.com/azure/azure-signalr/concept-upstream)
for the matching syntax.

## How it works

Every `WithUpstream` call adds a `SignalRUpstreamAnnotation` to the SignalR resource. When the emulator container
starts, the annotations are turned into the emulator's `UpstreamSettings__Templates__{index}__*` environment variables.
The URLs stay lazy `ReferenceExpression`s, so Aspire resolves host/container networking for you.

> [!NOTE]
> Upstream endpoints only apply to the local emulator. They are ignored when a real Azure SignalR Service
> is used and when the app is published, so nothing is added to the generated Bicep.

## API

| Method | Description |
| ------ | ----------- |
| `WithUpstream` (URL) | Adds an upstream with a fixed URL template |
| `WithUpstream` (`ReferenceExpression`) | Adds an upstream with a lazily resolved URL template |
| `WithUpstream` (resource) | Adds an upstream on the endpoint of another resource, with a path |
| `WithUpstreamFunctionsProject` | Adds the Azure Functions SignalR trigger webhook of a Functions resource as upstream |

## Building from source

```sh
git clone https://github.com/fretje/fretje.Aspire.git
cd fretje.Aspire
dotnet build
dotnet test
dotnet pack src/fretje.Aspire.Hosting.Azure.SignalR -c Release -o artifacts
```

## Contributing

Bug reports, feature ideas and pull requests are welcome, see [CONTRIBUTING.md](https://github.com/fretje/fretje.Aspire/blob/main/CONTRIBUTING.md).
Notable changes are listed in the [changelog](https://github.com/fretje/fretje.Aspire/blob/main/CHANGELOG.md).

## License

[MIT](https://github.com/fretje/fretje.Aspire/blob/main/LICENSE)
