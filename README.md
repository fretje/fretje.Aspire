# fretje.Aspire

A collection of .NET Aspire extensions and tooling.

## Extensions

### [fretje.Aspire.Hosting.LocalEventing](src/fretje.Aspire.Hosting.LocalEventing/README.md)

Local Event Grid emulator and blob storage event proxy for .NET Aspire. Enables local development
of Azure Functions blob EventGrid triggers by proxying Azurite blob traffic and forwarding events
to a local Event Grid emulator.

See the [project README](src/fretje.Aspire.Hosting.LocalEventing/README.md) for installation and
usage.

### [fretje.Aspire.Hosting.Azure.SignalR](src/fretje.Aspire.Hosting.Azure.SignalR/README.md)

Upstream endpoint support for the Azure SignalR Service emulator in .NET Aspire. Point the emulator
at one or more upstream URLs, such as an Azure Functions SignalR trigger, so serverless SignalR
messages and connection events reach your local app.

See the [project README](src/fretje.Aspire.Hosting.Azure.SignalR/README.md) for installation and
usage.

## Repository layout

- `src/` – NuGet packages and libraries
- `samples/` – Example Aspire applications demonstrating the extensions
- `tests/` – Unit and integration tests
