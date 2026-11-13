using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Microsoft.Extensions.DependencyInjection;

namespace BlobEventGridSample.IntegrationTests.Infrastructure;

/// <summary>Extension methods for asserting on log output emitted by resources of a running <see cref="DistributedApplication"/>.</summary>
internal static class DistributedApplicationLogAssertionExtensions
{
    private static readonly TimeSpan _defaultTimeout = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Opens a single WatchAsync stream for the named resource and returns the
    /// matching (console) log lines, in order. Returns null if the timeout expires
    /// or the log stream is closed before all expected lines are found.
    /// </summary>
    /// <param name="app">The distributed application to read logs from.</param>
    /// <param name="resourceName">The name of the resource to watch.</param>
    /// <param name="expectedLines">The log substrings to match, in order.</param>
    /// <param name="timeout">The maximum time to wait, or null for a default of 5 seconds.</param>
    /// <param name="ct">A token to cancel the asynchronous operation.</param>
    /// <returns>The matching log lines, or null if they were not found in time.</returns>
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
    /// <param name="app">The distributed application to read logs from.</param>
    /// <param name="resourceName">The name of the resource to watch.</param>
    /// <param name="expectedLine">The log substring to match.</param>
    /// <param name="timeout">The maximum time to wait, or null for a default of 5 seconds.</param>
    /// <param name="ct">A token to cancel the asynchronous operation.</param>
    /// <returns>A task that completes when the assertion has been made.</returns>
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
    /// <param name="app">The distributed application to read logs from.</param>
    /// <param name="resourceName">The name of the resource to watch.</param>
    /// <param name="expectedLines">The log substrings to match, in order.</param>
    /// <param name="timeout">The maximum time to wait, or null for a default of 5 seconds.</param>
    /// <param name="ct">A token to cancel the asynchronous operation.</param>
    /// <returns>A task that completes when the assertion has been made.</returns>
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
