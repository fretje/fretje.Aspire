using System.Text.RegularExpressions;
using Aspire.Hosting.ApplicationModel;

namespace fretje.Aspire.Hosting.LocalEventing;

/// <summary>
/// Takes the storage blobs connection string and swaps its <c>BlobEndpoint</c> authority for the proxy's,
/// keeping the path (e.g. <c>/devstoreaccount1</c>) and all credentials as they are.
/// </summary>
internal sealed partial class ProxiedBlobConnectionString(
    IResourceWithConnectionString blobs,
    EndpointReference proxyEndpoint)
    : IValueProvider, IManifestExpressionProvider, IValueWithReferences
{
    public string ValueExpression => blobs.ValueExpression;

    public IEnumerable<object> References => [blobs, proxyEndpoint];

    public async ValueTask<string?> GetValueAsync(CancellationToken cancellationToken = default) =>
        Rewrite(
            await blobs.GetValueAsync(cancellationToken).ConfigureAwait(false),
            await proxyEndpoint.Property(EndpointProperty.Port).GetValueAsync(cancellationToken).ConfigureAwait(false));

    public async ValueTask<string?> GetValueAsync(ValueProviderContext context, CancellationToken cancellationToken = default) =>
        Rewrite(
            await blobs.GetValueAsync(context, cancellationToken).ConfigureAwait(false),
            await proxyEndpoint.Property(EndpointProperty.Port).GetValueAsync(context, cancellationToken).ConfigureAwait(false));

    internal static string Rewrite(string? blobsConnectionString, string? proxyPort)
    {
        if (blobsConnectionString is null)
        {
            throw new InvalidOperationException("Failed to get blobs connection string");
        }

        if (proxyPort is null)
        {
            throw new InvalidOperationException("Failed to get blobs proxy port");
        }

        if (!BlobEndpointRegex().IsMatch(blobsConnectionString))
        {
            throw new InvalidOperationException("BlobEndpoint not found in blobs connection string");
        }

        // 127.0.0.1 instead of localhost: the azure sdk needs this to work properly with the emulator
        return BlobEndpointRegex().Replace(blobsConnectionString, $"BlobEndpoint=http://127.0.0.1:{proxyPort}$1");
    }

    [GeneratedRegex(@"BlobEndpoint=https?://[^/]+(/[^;]*)")]
    private static partial Regex BlobEndpointRegex();
}
