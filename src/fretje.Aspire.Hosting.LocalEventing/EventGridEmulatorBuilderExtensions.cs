using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;

namespace fretje.Aspire.Hosting.LocalEventing;

public static class EventGridEmulatorBuilderExtensions
{
    private const string _defaultImage = "workleap/eventgridemulator";
    private const string _defaultTag = "0.6.37";

    /// <summary>Adds a local Event Grid emulator container.</summary>
    /// <param name="builder">The distributed application builder.</param>
    /// <param name="name">The name of the resource.</param>
    /// <param name="port">Fixed host port, or null for a dynamic one.</param>
    public static IResourceBuilder<EventGridEmulatorResource> AddEventGridEmulator(
        this IDistributedApplicationBuilder builder, string name, int? port = EventGridEmulatorResource.ContainerPort)
    {
        EventGridEmulatorResource resource = new(name);

        return builder.AddResource(resource)
            .WithImage(_defaultImage, _defaultTag)
            .WithHttpEndpoint(port, EventGridEmulatorResource.ContainerPort, EventGridEmulatorResource.HttpEndpointName)
            .ExcludeFromManifest() // local-dev only
            .WithEnvironment(context =>
            {
                // Evaluated when the container starts, so subscriptions added anywhere in the AppHost are included.
                foreach (var topic in resource.Topics)
                {
                    var index = 0;
                    foreach (var subscription in topic.Subscriptions)
                    {
                        // ReferenceExpression is kept lazy, so host/container networking is resolved by Aspire.
                        context.EnvironmentVariables[$"Topics__{topic.TopicName}__{index++}"] = subscription;
                    }
                }
            });
    }

    /// <summary>Declares a topic on the emulator.</summary>
    /// <param name="emulator">The Event Grid emulator resource.</param>
    /// <param name="name">Aspire resource name (must be unique in the app model).</param>
    /// <param name="topicName">Topic name used in the emulator URL; defaults to <paramref name="name"/>.</param>
    public static IResourceBuilder<EventGridTopicResource> AddTopic(
        this IResourceBuilder<EventGridEmulatorResource> emulator, string name, string? topicName = null)
    {
        EventGridTopicResource topic = new(name, topicName ?? name, emulator.Resource);
        emulator.Resource.AddTopic(topic);

        return emulator.ApplicationBuilder.AddResource(topic)
            .ExcludeFromManifest();
    }

    /// <summary>
    /// Forwards the topic's events to an arbitrary webhook on <paramref name="target"/>.
    /// </summary>
    public static IResourceBuilder<EventGridTopicResource> WithWebhookSubscription(
        this IResourceBuilder<EventGridTopicResource> topic,
        IResourceBuilder<IResourceWithEndpoints> target,
        string path,
        string endpointName = "http")
    {
        var endpoint = target.GetEndpoint(endpointName);
        topic.Resource.AddSubscription(ReferenceExpression.Create($"{endpoint}{path}"));

        return topic;
    }

    /// <summary>Subscribes an Azure Functions Event Grid–based blob trigger (<c>Source = BlobTriggerSource.EventGrid</c>).</summary>
    public static IResourceBuilder<EventGridTopicResource> WithBlobTriggerSubscription(
        this IResourceBuilder<EventGridTopicResource> topic, IResourceBuilder<IResourceWithEndpoints> functionApp, string functionName) =>
        topic.WithWebhookSubscription(
            functionApp, $"/runtime/webhooks/blobs?functionName=Host.Functions.{functionName}");

    /// <summary>Subscribes an Azure Functions <c>[EventGridTrigger]</c> function.</summary>
    public static IResourceBuilder<EventGridTopicResource> WithEventGridTriggerSubscription(
        this IResourceBuilder<EventGridTopicResource> topic, IResourceBuilder<IResourceWithEndpoints> functionApp, string functionName) =>
        topic.WithWebhookSubscription(
            functionApp, $"/runtime/webhooks/EventGrid?functionName={functionName}");
}
