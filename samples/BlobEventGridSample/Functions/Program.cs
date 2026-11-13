using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Azure.Functions.Worker.OpenTelemetry;

var builder = FunctionsApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddOpenTelemetry()
    .UseFunctionsWorkerDefaults();

builder.ConfigureFunctionsWebApplication();

await builder.Build().RunAsync();
