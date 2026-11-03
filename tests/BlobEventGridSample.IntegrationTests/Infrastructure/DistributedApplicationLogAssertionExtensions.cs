using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Microsoft.Extensions.DependencyInjection;

namespace BlobEventGridSample.IntegrationTests.Infrastructure;

internal static class DistributedApplicationLogAssertionExtensions
{
    private static readonly TimeSpan _defaultTimeout = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Opens a single WatchAsync stream for the named resource and returns the
    /// matching (console) log lines, in order. Returns null if the timeout expires
    /// or the log stream is closed before all expected lines are found.
    /// </summary>
    public static async Task<string[]?> WaitForLogAsync(
        this DistributedApplication app,
        string resourceName,
        string[] expectedLines,
        TimeSpan? timeout = null,
        CancellationToken ct = default)
    {
        var effectiveTimeout = timeout ?? _defaultTimeout;
        var loggerService = app.Services.GetRequiredService<ResourceLoggerService>();
        var model = app.Services.GetRequiredService<DistributedApplicationModel>();
        var resource = model.Resources.First(r => r.Name == resourceName);

        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        linkedCts.CancelAfter(effectiveTimeout);

        List<string> matches = [];
        var index = 0;

        try
        {
            await foreach (var batch in loggerService.WatchAsync(resource).WithCancellation(linkedCts.Token))
            {
                foreach (var line in batch)
                {
                    if (index < expectedLines.Length
                        && line.Content.Contains(expectedLines[index], StringComparison.OrdinalIgnoreCase))
                    {
                        matches.Add(line.Content);
                        index++;
                        if (index == expectedLines.Length)
                        {
                            return [.. matches];
                        }
                    }
                }
            }

            return null;
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            return null;
        }
    }

    /// <summary>
    /// Asserts that the given resource emitted a log line containing the expected substring,
    /// within the timeout.
    /// </summary>
    public static Task ShouldHaveLoggedAsync(
        this DistributedApplication app,
        string resourceName,
        string expectedLine,
        TimeSpan? timeout = null,
        CancellationToken ct = default) =>
        ShouldHaveLoggedAsync(app, resourceName, [expectedLine], timeout, ct);

    /// <summary>
    /// Asserts that the given resource emitted log lines containing each of the expected
    /// substrings, in order, within the timeout.
    /// </summary>
    public static async Task ShouldHaveLoggedAsync(
        this DistributedApplication app,
        string resourceName,
        string[] expectedLines,
        TimeSpan? timeout = null,
        CancellationToken ct = default) =>
        _ = await app.WaitForLogAsync(resourceName, expectedLines, timeout, ct)
            ?? throw new ShouldAssertException(
                $"Expected a log line containing '{string.Join("', then '", expectedLines)}' from resource '{resourceName}' within {timeout ?? _defaultTimeout}, but none was found.");
}
