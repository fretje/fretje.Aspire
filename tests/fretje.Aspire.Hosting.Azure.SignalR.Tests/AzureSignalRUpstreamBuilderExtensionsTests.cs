using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Azure;
using Microsoft.Extensions.Logging.Abstractions;

namespace fretje.Aspire.Hosting.Azure.SignalR.Tests;

public class AzureSignalRUpstreamBuilderExtensionsTests
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public void WithUpstream_Url_AddsAnnotationWithDefaultPatterns()
    {
        var builder = DistributedApplication.CreateBuilder([]);
        var signalR = builder.AddAzureSignalR("signalr", AzureSignalRServiceMode.Serverless)
            .RunAsEmulator();

        signalR.WithUpstream("https://example.test/signalr");

        var upstream = signalR.Resource.Annotations.OfType<SignalRUpstreamAnnotation>().ShouldHaveSingleItem();
        upstream.HubPattern.ShouldBe("*");
        upstream.CategoryPattern.ShouldBe("*");
        upstream.EventPattern.ShouldBe("*");
    }

    [Fact]
    public void WithUpstream_Url_StoresPatterns()
    {
        var builder = DistributedApplication.CreateBuilder([]);
        var signalR = builder.AddAzureSignalR("signalr", AzureSignalRServiceMode.Serverless)
            .RunAsEmulator();

        signalR.WithUpstream("https://example.test/signalr", "chat", "messages", "send");

        var upstream = signalR.Resource.Annotations.OfType<SignalRUpstreamAnnotation>().ShouldHaveSingleItem();
        upstream.HubPattern.ShouldBe("chat");
        upstream.CategoryPattern.ShouldBe("messages");
        upstream.EventPattern.ShouldBe("send");
    }

    [Fact]
    public void WithUpstream_CalledMultipleTimes_KeepsCallOrder()
    {
        var builder = DistributedApplication.CreateBuilder([]);
        var signalR = builder.AddAzureSignalR("signalr", AzureSignalRServiceMode.Serverless)
            .RunAsEmulator();

        signalR
            .WithUpstream("https://first.test", "first")
            .WithUpstream("https://second.test", "second");

        var upstreams = signalR.Resource.Annotations.OfType<SignalRUpstreamAnnotation>().ToList();
        upstreams.Count.ShouldBe(2);
        upstreams[0].HubPattern.ShouldBe("first");
        upstreams[1].HubPattern.ShouldBe("second");
    }

    [Fact]
    public void WithUpstream_Resource_AddsEndpointTemplate()
    {
        var builder = DistributedApplication.CreateBuilder([]);
        var signalR = builder.AddAzureSignalR("signalr", AzureSignalRServiceMode.Serverless)
            .RunAsEmulator();
        var target = builder.AddResource(new TestResource("api"));

        signalR.WithUpstream(target, "/{hub}/api/{category}/{event}", hubPattern: "chat");

        var upstream = signalR.Resource.Annotations.OfType<SignalRUpstreamAnnotation>().ShouldHaveSingleItem();
        upstream.UrlTemplate.Format.ShouldBe("{0}/{{hub}}/api/{{category}}/{{event}}");
        upstream.HubPattern.ShouldBe("chat");
    }

    [Fact]
    public void WithUpstreamFunctionsProject_AddsFunctionsWebhookTemplate()
    {
        var builder = DistributedApplication.CreateBuilder([]);
        var signalR = builder.AddAzureSignalR("signalr", AzureSignalRServiceMode.Serverless)
            .RunAsEmulator();
        var functionApp = builder.AddResource(new TestResource("functions"));

        signalR.WithUpstreamFunctionsProject(functionApp, hubPattern: "chat");

        var upstream = signalR.Resource.Annotations.OfType<SignalRUpstreamAnnotation>().ShouldHaveSingleItem();
        upstream.UrlTemplate.Format.ShouldBe("{0}/runtime/webhooks/signalr");
        upstream.HubPattern.ShouldBe("chat");
        upstream.CategoryPattern.ShouldBe("*");
        upstream.EventPattern.ShouldBe("*");
    }

    [Fact]
    public async Task WithUpstream_SetsEmulatorEnvironmentVariables()
    {
        var builder = DistributedApplication.CreateBuilder([]);
        var signalR = builder.AddAzureSignalR("signalr", AzureSignalRServiceMode.Serverless)
            .RunAsEmulator()
            .WithUpstream("https://example.test/{hub}/api/{category}/{event}", "chat", "messages", "send");

        var environment = await GetEnvironmentAsync(signalR);

        environment["UpstreamSettings__Templates__0__UrlTemplate"].ShouldBe("https://example.test/{hub}/api/{category}/{event}");
        environment["UpstreamSettings__Templates__0__HubPattern"].ShouldBe("chat");
        environment["UpstreamSettings__Templates__0__CategoryPattern"].ShouldBe("messages");
        environment["UpstreamSettings__Templates__0__EventPattern"].ShouldBe("send");
    }

    [Fact]
    public async Task WithUpstream_CalledMultipleTimes_UsesConsecutiveTemplateIndexes()
    {
        var builder = DistributedApplication.CreateBuilder([]);
        var signalR = builder.AddAzureSignalR("signalr", AzureSignalRServiceMode.Serverless)
            .RunAsEmulator()
            .WithUpstream("https://first.test", "first")
            .WithUpstream("https://second.test", "second");

        var environment = await GetEnvironmentAsync(signalR);

        environment["UpstreamSettings__Templates__0__UrlTemplate"].ShouldBe("https://first.test");
        environment["UpstreamSettings__Templates__0__HubPattern"].ShouldBe("first");
        environment["UpstreamSettings__Templates__1__UrlTemplate"].ShouldBe("https://second.test");
        environment["UpstreamSettings__Templates__1__HubPattern"].ShouldBe("second");
        environment.ShouldNotContainKey("UpstreamSettings__Templates__2__UrlTemplate");
    }

    [Fact]
    public async Task WithUpstream_BeforeRunAsEmulator_SetsEmulatorEnvironmentVariables()
    {
        var builder = DistributedApplication.CreateBuilder([]);
        var signalR = builder.AddAzureSignalR("signalr", AzureSignalRServiceMode.Serverless)
            .WithUpstream("https://example.test")
            .RunAsEmulator();

        var environment = await GetEnvironmentAsync(signalR);

        environment["UpstreamSettings__Templates__0__UrlTemplate"].ShouldBe("https://example.test");
    }

    private static async Task<Dictionary<string, string>> GetEnvironmentAsync(IResourceBuilder<AzureSignalRResource> signalR)
    {
        var configuration = await ExecutionConfigurationBuilder.Create(signalR.Resource)
            .WithEnvironmentVariablesConfig()
            .BuildAsync(signalR.ApplicationBuilder.ExecutionContext, NullLogger.Instance, Ct);

        return configuration.EnvironmentVariables.ToDictionary();
    }

    private sealed class TestResource(string name) : IResource, IResourceWithEndpoints
    {
        public string Name { get; } = name;
        public ResourceAnnotationCollection Annotations { get; } = [];

        public IResourceBuilder<IResourceWithEndpoints> GetEndpoint(string? name = null) =>
            throw new NotSupportedException();
    }
}
