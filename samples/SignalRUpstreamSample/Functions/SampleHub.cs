using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.SignalRService;

namespace SignalRUpstreamSample.Functions;

/// <summary>Azure Functions serverless hub that negotiates SignalR client connections and logs connection events.</summary>
/// <param name="serviceProvider">The service provider used by the serverless hub.</param>
/// <param name="logger">The logger instance.</param>
public class SampleHub(IServiceProvider serviceProvider, ILogger<SampleHub> logger) : ServerlessHub(serviceProvider)
{
    /// <summary>Creates a SignalR client negotiation response with a user ID from the request.</summary>
    /// <param name="request">The HTTP request that initiated the negotiation. An optional <c>userId</c> query parameter identifies the client.</param>
    /// <returns>The SignalR negotiation response.</returns>
    [Function("negotiate")]
    public async Task<HttpResponseData> NegotiateClientAsync([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData request, CancellationToken ct)
    {
        var userId = request.Query["userId"];
        if (string.IsNullOrWhiteSpace(userId))
        {
            userId = request.FunctionContext.InvocationId;
        }

        var response = request.CreateResponse(HttpStatusCode.OK);
        await response.WriteBytesAsync(
            (await NegotiateAsync(new() { UserId = userId })).ToArray(), ct);
        return response;
    }

    /// <summary>Logs when a client connects to the hub.</summary>
    /// <param name="context">The SignalR connection context.</param>
    [Function(nameof(OnConnected))]
    public void OnConnected([SignalRTrigger(nameof(SampleHub), "connections", "connected")] SignalRInvocationContext context) =>
        logger.LogInformation("SignalR client connected to {Hub} with connection {ConnectionId}", context.Hub, context.ConnectionId);

    /// <summary>Logs when a client disconnects from the hub.</summary>
    /// <param name="context">The SignalR connection context.</param>
    [Function(nameof(OnDisconnected))]
    public void OnDisconnected([SignalRTrigger(nameof(SampleHub), "connections", "disconnected")] SignalRInvocationContext context) =>
        logger.LogInformation("SignalR client disconnected from {Hub} with connection {ConnectionId}", context.Hub, context.ConnectionId);
}
