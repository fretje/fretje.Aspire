using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Azure;

namespace fretje.Aspire.Hosting.LocalEventing;

/// <summary>Extension methods for wiring the blob storage notify proxy into an Aspire distributed application.</summary>
public static class BlobStorageEventProxyBuilderExtensions
{
    private const string _defaultImage = "vincentfenet/blob-storage-notify-proxy";
    private const string _defaultTag = "1.2";

    /// <summary>Adds a blob storage event proxy for the specified blobs resource and Event Grid topic.</summary>
    /// <param name="builder">The distributed application builder.</param>
    /// <param name="name">The name of the resource.</param>
    /// <param name="blobs">The blobs resource, e.g. <c>storage.AddBlobs("blobs")</c>; its connection string is what gets proxied.</param>
    /// <param name="topic">The Event Grid topic that receives notifications.</param>
    /// <param name="port">Fixed host port, or null to let Aspire assign one dynamically. Defaults to null.</param>
    /// <returns>A builder for the added blob storage event proxy resource.</returns>
    public static IResourceBuilder<BlobStorageEventProxyResource> AddBlobStorageEventProxy(
        this IDistributedApplicationBuilder builder,
        string name,
        IResourceBuilder<AzureBlobStorageResource> blobs,
        IResourceBuilder<EventGridTopicResource> topic,
        int? port = null) =>
        builder.AddBlobStorageEventProxy(name, blobs, port)
            .WithEventGridTopic(topic);

    /// <summary>Adds a blob storage event proxy for the specified blobs resource.</summary>
    /// <param name="builder">The distributed application builder.</param>
    /// <param name="name">The name of the resource.</param>
    /// <param name="blobs">The blobs resource, e.g. <c>storage.AddBlobs("blobs")</c>; its connection string is what gets proxied.</param>
    /// <param name="port">Fixed host port, or null to let Aspire assign one dynamically. Defaults to null.</param>
    /// <returns>A builder for the added blob storage event proxy resource.</returns>
    public static IResourceBuilder<BlobStorageEventProxyResource> AddBlobStorageEventProxy(
        this IDistributedApplicationBuilder builder,
        string name,
        IResourceBuilder<AzureBlobStorageResource> blobs,
        int? port = null)
    {
        var storage = builder.CreateResourceBuilder(blobs.Resource.Parent);
        BlobStorageEventProxyResource resource = new(name, blobs.Resource);

        return builder.AddResource(resource)
            .WithImage(_defaultImage, _defaultTag)
            .WithArgs("azurite-to-azure-event-grid")
            .WithHttpEndpoint(port, BlobStorageEventProxyResource._containerPort, BlobStorageEventProxyResource._httpEndpointName)
            .WithEnvironment("PROXY_PORT", BlobStorageEventProxyResource._containerPort.ToString(CultureInfo.InvariantCulture))
            .WithEnvironment("TARGET_SERVER", storage.GetEndpoint("blob"))
            .WaitFor(storage)
            .ExcludeFromManifest(); // local-dev only
    }

    /// <summary>Configures a blob storage event proxy to send notifications to the specified Event Grid topic.</summary>
    /// <param name="proxy">The blob storage event proxy resource to configure.</param>
    /// <param name="topic">The Event Grid topic that receives the notifications.</param>
    /// <returns>A builder for the blob storage event proxy resource.</returns>
    public static IResourceBuilder<BlobStorageEventProxyResource> WithEventGridTopic(
        this IResourceBuilder<BlobStorageEventProxyResource> proxy,
        IResourceBuilder<EventGridTopicResource> topic)
    {
        var emulator = proxy.ApplicationBuilder.CreateResourceBuilder(topic.Resource.Parent);

        return proxy
            .WithEnvironment("NOTIFICATION_ENDPOINT", topic.Resource.TopicEndpointExpression)
            .WaitFor(emulator);
    }
}
