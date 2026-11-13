using System.Text.RegularExpressions;
using Aspire.Hosting.ApplicationModel;

namespace fretje.Aspire.Hosting.LocalEventing;

/// <summary>
/// Takes the storage blobs connection string and swaps its <c>BlobEndpoint</c> authority for the proxy's,
/// keeping the path (e.g. <c>/devstoreaccount1</c>) and all credentials as they are.
/// </summary>
/// <param name="blobs">The blobs resource whose connection string is being proxied.</param>
/// <param name="proxyEndpoint">The proxy's HTTP endpoint.</param>
internal sealed partial class ProxiedBlobConnectionString(
    IResourceWithConnectionString blobs,
    EndpointReference proxyEndpoint)
    : IValueProvider, IManifestExpressionProvider, IValueWithReferences
{
    /// <summary>Gets the manifest expression for this connection string.</summary>
    public string ValueExpression => blobs.ValueExpression;

    /// <summary>Gets the resources this connection string depends on.</summary>
    public IEnumerable<object> References => [blobs, proxyEndpoint];

    [GeneratedRegex(@"BlobEndpoint=https?://[^/]+(/[^;]*)", RegexOptions.ExplicitCapture, matchTimeoutMilliseconds: 1000)]
    private static partial Regex BlobEndpointRegex { get; }

    /// <summary>Gets the proxied connection string at resource startup time.</summary>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>The proxied blobs connection string.</returns>
    public async ValueTask<string?> GetValueAsync(CancellationToken cancellationToken = default) =>
        Rewrite(
            await blobs.GetValueAsync(cancellationToken).ConfigureAwait(false),
            await proxyEndpoint.Property(EndpointProperty.Port).GetValueAsync(cancellationToken).ConfigureAwait(false));

    /// <summary>Gets the proxied connection string with the given value-provider context.</summary>
    /// <param name="context">The value-provider context.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>The proxied blobs connection string.</returns>
    public async ValueTask<string?> GetValueAsync(ValueProviderContext context, CancellationToken cancellationToken = default) =>
        Rewrite(
            await blobs.GetValueAsync(context, cancellationToken).ConfigureAwait(false),
            await proxyEndpoint.Property(EndpointProperty.Port).GetValueAsync(context, cancellationToken).ConfigureAwait(false));

    /// <summary>Replaces the <c>BlobEndpoint</c> authority in a connection string with the proxy's.</summary>
    /// <param name="blobsConnectionString">The original blobs connection string.</param>
    /// <param name="proxyPort">The proxy's host port.</param>
    /// <returns>The connection string with the proxy's authority.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the connection string or proxy port cannot be resolved.</exception>
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

        if (!BlobEndpointRegex.IsMatch(blobsConnectionString))
        {
            throw new InvalidOperationException("BlobEndpoint not found in blobs connection string");
        }

        // 127.0.0.1 instead of localhost: the azure sdk needs this to work properly with the emulator
        return BlobEndpointRegex.Replace(blobsConnectionString, $"BlobEndpoint=http://127.0.0.1:{proxyPort}$1");
    }
}
