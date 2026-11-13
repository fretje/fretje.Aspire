using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Azure;

namespace fretje.Aspire.Hosting.LocalEventing;

/// <summary>
/// Sits between an app and Azurite, forwarding blob requests to storage and sending
/// Event Grid notifications for them (Azurite has no native Event Grid support).
/// Its connection string is the storage connection string with <c>BlobEndpoint</c> pointing at the proxy.
/// </summary>
/// <param name="name">The name of the resource.</param>
/// <param name="blobs">The blobs resource whose connection string is being proxied.</param>
public sealed class BlobStorageEventProxyResource(string name, AzureBlobStorageResource blobs)
    : ContainerResource(name), IResourceWithConnectionString
{
    /// <summary>The name of the HTTP endpoint exposed by the proxy container.</summary>
    internal const string _httpEndpointName = "http";

    /// <summary>The port the proxy container listens on.</summary>
    internal const int _containerPort = 9999;

    /// <summary>Gets the blobs resource whose connection string is being proxied.</summary>
    public AzureBlobStorageResource Blobs => blobs;

    /// <summary>Gets the HTTP endpoint exposed by the proxy container.</summary>
    public EndpointReference HttpEndpoint => field ??= new(this, _httpEndpointName);

    /// <summary>
    /// Gets the blobs connection string with its <c>BlobEndpoint</c> host/port replaced by the proxy
    /// (127.0.0.1, because the Azure SDK needs an IP to work properly with the emulator).
    /// Resolved lazily, so it is evaluated when the consuming resource starts.
    /// Intended for consumers running on the host (projects, IIS Express), not for containers.
    /// </summary>
    public ReferenceExpression ConnectionStringExpression =>
        field ??= ReferenceExpression.Create($"{new ProxiedBlobConnectionString(blobs, HttpEndpoint)}");
}
