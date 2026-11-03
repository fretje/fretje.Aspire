using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;

namespace fretje.Aspire.Hosting.LocalEventing.Tests;

public class EventGridEmulatorBuilderExtensionsTests
{
    [Fact]
    public void AddEventGridEmulator_AddsResource()
    {
        var builder = DistributedApplication.CreateBuilder([]);

        var emulator = builder.AddEventGridEmulator("eventgrid");

        emulator.Resource.ShouldBeOfType<EventGridEmulatorResource>();
        emulator.Resource.Name.ShouldBe("eventgrid");
    }

    [Fact]
    public void AddTopic_AddsTopicToEmulator()
    {
        var builder = DistributedApplication.CreateBuilder([]);
        var emulator = builder.AddEventGridEmulator("eventgrid");

        var topic = emulator.AddTopic("blobstopic");

        topic.Resource.ShouldBeOfType<EventGridTopicResource>();
        topic.Resource.Name.ShouldBe("blobstopic");
        topic.Resource.TopicName.ShouldBe("blobstopic");
    }

    [Fact]
    public void AddTopic_WithCustomTopicName_UsesCustomName()
    {
        var builder = DistributedApplication.CreateBuilder([]);
        var emulator = builder.AddEventGridEmulator("eventgrid");

        var topic = emulator.AddTopic("blobstopic", "custom-topic");

        topic.Resource.TopicName.ShouldBe("custom-topic");
    }

    [Fact]
    public void WithWebhookSubscription_AddsSubscription()
    {
        var builder = DistributedApplication.CreateBuilder([]);
        var topic = builder.AddEventGridEmulator("eventgrid")
            .AddTopic("testtopic");
        var target = builder.AddResource(new TestResource("target"));

        topic.WithWebhookSubscription(target, "/api/webhook");

        topic.Resource.Subscriptions.Count.ShouldBe(1);
    }

    [Fact]
    public void WithBlobTriggerSubscription_AddsSubscription()
    {
        var builder = DistributedApplication.CreateBuilder([]);
        var topic = builder.AddEventGridEmulator("eventgrid")
            .AddTopic("blobstopic");
        var functionApp = builder.AddResource(new TestResource("functions"));

        topic.WithBlobTriggerSubscription(functionApp, "BlobTriggerFunction");

        topic.Resource.Subscriptions.Count.ShouldBe(1);
    }

    [Fact]
    public void WithEventGridTriggerSubscription_AddsSubscription()
    {
        var builder = DistributedApplication.CreateBuilder([]);
        var topic = builder.AddEventGridEmulator("eventgrid")
            .AddTopic("eventgridtopic");
        var functionApp = builder.AddResource(new TestResource("functions"));

        topic.WithEventGridTriggerSubscription(functionApp, "EventGridFunction");

        topic.Resource.Subscriptions.Count.ShouldBe(1);
    }

    private sealed class TestResource(string name) : IResource, IResourceWithEndpoints
    {
        public string Name { get; } = name;
        public ResourceAnnotationCollection Annotations { get; } = [];

        public IResourceBuilder<IResourceWithEndpoints> GetEndpoint(string? name = null) => throw new NotImplementedException();
    }
}
