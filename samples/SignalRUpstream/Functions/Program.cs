using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Azure.Functions.Worker.OpenTelemetry;
using SignalRUpstream.Functions;

var builder = FunctionsApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddOpenTelemetry()
    .UseFunctionsWorkerDefaults();

builder.ConfigureFunctionsWebApplication();

builder.Services.AddServerlessHub<SampleHub>();

await builder.Build().RunAsync();
