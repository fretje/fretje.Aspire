using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Azure;

namespace fretje.Aspire.Hosting.LocalEventing;

/// <summary>
/// Sits between an app and Azurite, forwarding blob requests to storage and sending
/// Event Grid notifications for them (Azurite has no native Event Grid support).
/// Its connection string is the storage connection string with <c>BlobEndpoint</c> pointing at the proxy.
/// </summary>
public sealed class BlobStorageEventProxyResource(string name, AzureBlobStorageResource blobs)
    : ContainerResource(name), IResourceWithConnectionString
{
    internal const string HttpEndpointName = "http";
    internal const int ContainerPort = 9999;

    public AzureBlobStorageResource Blobs => blobs;

    public EndpointReference HttpEndpoint => field ??= new(this, HttpEndpointName);

    /// <summary>
    /// The blobs connection string with its <c>BlobEndpoint</c> host/port replaced by the proxy
    /// (127.0.0.1, because the Azure SDK needs an IP to work properly with the emulator).
    /// Resolved lazily, so it is evaluated when the consuming resource starts.
    /// Intended for consumers running on the host (projects, IIS Express), not for containers.
    /// </summary>
    public ReferenceExpression ConnectionStringExpression =>
        field ??= ReferenceExpression.Create($"{new ProxiedBlobConnectionString(blobs, HttpEndpoint)}");
}
