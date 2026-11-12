using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Azure;

namespace fretje.Aspire.Hosting.LocalEventing;

public static class BlobStorageEventProxyBuilderExtensions
{
    private const string _defaultImage = "vincentfenet/blob-storage-notify-proxy";
    private const string _defaultTag = "1.2";

    /// <summary>Adds a blob proxy in front of the (emulated) Azure Storage account that <paramref name="blobs"/> belongs to.</summary>
    /// <param name="builder">The distributed application builder.</param>
    /// <param name="name">The name of the resource.</param>
    /// <param name="blobs">The blobs resource, e.g. <c>storage.AddBlobs("blobs")</c>; its connection string is what gets proxied.</param>
    /// <param name="port">Fixed host port, or null for a dynamic one.</param>
    public static IResourceBuilder<BlobStorageEventProxyResource> AddBlobStorageEventProxy(
        this IDistributedApplicationBuilder builder,
        string name,
        IResourceBuilder<AzureBlobStorageResource> blobs,
        int? port = 9999)
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

    /// <summary>Sends a notification to the given Event Grid topic for every blob operation passing through the proxy.</summary>
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
