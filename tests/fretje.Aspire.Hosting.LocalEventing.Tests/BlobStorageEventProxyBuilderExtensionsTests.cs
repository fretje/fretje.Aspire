using Aspire.Hosting;

namespace fretje.Aspire.Hosting.LocalEventing.Tests;

public class BlobStorageEventProxyBuilderExtensionsTests
{
    [Fact]
    public void AddBlobStorageEventProxy_AddsResourceWithCorrectConfiguration()
    {
        var builder = DistributedApplication.CreateBuilder([]);
        var storage = builder.AddAzureStorage("storage");
        var blobs = storage.AddBlobs("blobs");

        var proxy = builder.AddBlobStorageEventProxy("blobsproxy", blobs);

        proxy.Resource.ShouldBeOfType<BlobStorageEventProxyResource>();
        proxy.Resource.Name.ShouldBe("blobsproxy");
    }

    [Fact]
    public void WithEventGridTopic_SetsNotificationEndpoint()
    {
        var builder = DistributedApplication.CreateBuilder([]);
        var storage = builder.AddAzureStorage("storage");
        var blobs = storage.AddBlobs("blobs");
        var eventGrid = builder.AddEventGridEmulator("eventgrid");
        var topic = eventGrid.AddTopic("blobstopic");

        var proxy = builder.AddBlobStorageEventProxy("blobsproxy", blobs)
            .WithEventGridTopic(topic);

        proxy.Resource.ShouldBeOfType<BlobStorageEventProxyResource>();
    }
}
