# fretje.Aspire

A collection of .NET Aspire extensions and tooling.

## Extensions

### [fretje.Aspire.Hosting.LocalEventing](src/fretje.Aspire.Hosting.LocalEventing/README.md)

Local Event Grid emulator and blob storage event proxy for .NET Aspire. Enables local development of Azure Functions blob EventGrid triggers by proxying Azurite blob traffic and forwarding events to a local Event Grid emulator.

See the [project README](src/fretje.Aspire.Hosting.LocalEventing/README.md) for installation and usage.

## Repository layout

- `src/` – NuGet packages and libraries
- `samples/` – Example Aspire applications demonstrating the extensions
- `tests/` – Unit and integration tests
