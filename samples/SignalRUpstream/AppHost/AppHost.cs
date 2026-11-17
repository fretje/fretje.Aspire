#pragma warning disable ASPIREPERSISTENCE001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
using Aspire.Hosting.Azure;
using fretje.Aspire.Hosting.Azure.SignalR;
using Microsoft.Extensions.Configuration;

var builder = DistributedApplication.CreateBuilder(args);

var storage = builder
    .AddAzureStorage("storage")
    .RunAsEmulator(emulator => emulator
        .WithPersistentLifetime());

var signalr = builder
    .AddAzureSignalR("signalr", AzureSignalRServiceMode.Serverless)
    .RunAsEmulator(emulator => emulator
        .WithPersistentLifetime());

var functionApp = builder
    .AddAzureFunctionsProject<Projects.SignalRUpstream_Functions>("functionapp")
    .WithHostStorage(storage)
    .WithEnvironment("AzureSignalRConnectionString", signalr)
    .WaitFor(signalr);

signalr.WithUpstreamFunctionsProject(functionApp);

if (builder.Configuration.GetValue("IncludeClientProjects", defaultValue: true))
{
    builder.AddProject<Projects.SignalRUpstream_Client>("client")
        .WithReference(functionApp)
        .WaitFor(functionApp);
}

await builder.Build().RunAsync();
