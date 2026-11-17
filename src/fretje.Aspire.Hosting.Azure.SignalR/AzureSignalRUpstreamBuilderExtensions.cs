using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;

namespace fretje.Aspire.Hosting.Azure.SignalR;

/// <summary>Extension methods for configuring upstream endpoints on an Azure SignalR resource that runs as the local emulator.</summary>
public static class AzureSignalRUpstreamBuilderExtensions
{
    private const string _anyPattern = "*";
    private const string _templateKeyPrefix = "UpstreamSettings__Templates__";
    private const string _functionsWebhookPath = "/runtime/webhooks/signalr";

    /// <summary>Adds an upstream endpoint that the local Azure SignalR emulator forwards serverless events to.</summary>
    /// <param name="signalR">The Azure SignalR resource builder.</param>
    /// <param name="urlTemplate">
    /// The upstream URL template. It can contain the <c>{hub}</c>, <c>{category}</c> and <c>{event}</c> placeholders.
    /// Keep it a <see cref="ReferenceExpression"/> (for example built from an endpoint) so that host/container networking is resolved by Aspire.
    /// </param>
    /// <param name="hubPattern">The rule matching hub names. <c>*</c> matches every hub.</param>
    /// <param name="categoryPattern">The rule matching categories. <c>*</c> matches every category.</param>
    /// <param name="eventPattern">The rule matching event names. <c>*</c> matches every event.</param>
    /// <returns>A builder for the Azure SignalR resource.</returns>
    /// <remarks>
    /// <para>
    /// Can be called multiple times, before or after <c>RunAsEmulator</c>. The templates are handed to the emulator in call order.
    /// Azure SignalR Service sends an event to the first template whose rules match, so add the most specific template first.
    /// </para>
    /// <para>
    /// Upstream endpoints are only applied while the resource runs as the local emulator. They are ignored when a real
    /// Azure SignalR Service is used and when the app is published, so nothing ends up in the generated Bicep.
    /// </para>
    /// </remarks>
    public static IResourceBuilder<AzureSignalRResource> WithUpstream(
        this IResourceBuilder<AzureSignalRResource> signalR,
        ReferenceExpression urlTemplate,
        string hubPattern = _anyPattern,
        string categoryPattern = _anyPattern,
        string eventPattern = _anyPattern)
    {
        var isFirstUpstream = !signalR.Resource.HasAnnotationOfType<SignalRUpstreamAnnotation>();
        signalR.WithAnnotation(new SignalRUpstreamAnnotation(urlTemplate, hubPattern, categoryPattern, eventPattern));

        if (isFirstUpstream)
        {
            // Registered once and evaluated when the emulator container starts, so upstreams added anywhere in the AppHost are included.
            var resource = signalR.Resource;
            signalR.WithAnnotation(new EnvironmentCallbackAnnotation(context => AddUpstreamEnvironment(resource, context)));
        }

        return signalR;
    }

    /// <summary>Adds an upstream endpoint with a fixed URL template that the local Azure SignalR emulator forwards serverless events to.</summary>
    /// <param name="signalR">The Azure SignalR resource builder.</param>
    /// <param name="urlTemplate">The upstream URL template. It can contain the <c>{hub}</c>, <c>{category}</c> and <c>{event}</c> placeholders.</param>
    /// <param name="hubPattern">The rule matching hub names. <c>*</c> matches every hub.</param>
    /// <param name="categoryPattern">The rule matching categories. <c>*</c> matches every category.</param>
    /// <param name="eventPattern">The rule matching event names. <c>*</c> matches every event.</param>
    /// <returns>A builder for the Azure SignalR resource.</returns>
    /// <remarks>See the remarks of the <see cref="ReferenceExpression"/> overload for the semantics of upstream endpoints.</remarks>
    public static IResourceBuilder<AzureSignalRResource> WithUpstream(
        this IResourceBuilder<AzureSignalRResource> signalR,
        string urlTemplate,
        string hubPattern = _anyPattern,
        string categoryPattern = _anyPattern,
        string eventPattern = _anyPattern) =>
        signalR.WithUpstream(ReferenceExpression.Create($"{urlTemplate}"), hubPattern, categoryPattern, eventPattern);

    /// <summary>Adds an upstream endpoint on <paramref name="target"/> that the local Azure SignalR emulator forwards serverless events to.</summary>
    /// <param name="signalR">The Azure SignalR resource builder.</param>
    /// <param name="target">The resource whose endpoint hosts the upstream.</param>
    /// <param name="path">The path appended to the target endpoint URL. It can contain the <c>{hub}</c>, <c>{category}</c> and <c>{event}</c> placeholders.</param>
    /// <param name="endpointName">The name of the endpoint on <paramref name="target"/> to use.</param>
    /// <param name="hubPattern">The rule matching hub names. <c>*</c> matches every hub.</param>
    /// <param name="categoryPattern">The rule matching categories. <c>*</c> matches every category.</param>
    /// <param name="eventPattern">The rule matching event names. <c>*</c> matches every event.</param>
    /// <returns>A builder for the Azure SignalR resource.</returns>
    /// <remarks>See the remarks of the <see cref="ReferenceExpression"/> overload for the semantics of upstream endpoints.</remarks>
    public static IResourceBuilder<AzureSignalRResource> WithUpstream(
        this IResourceBuilder<AzureSignalRResource> signalR,
        IResourceBuilder<IResourceWithEndpoints> target,
        string path,
        string endpointName = "http",
        string hubPattern = _anyPattern,
        string categoryPattern = _anyPattern,
        string eventPattern = _anyPattern)
    {
        var endpoint = target.GetEndpoint(endpointName);

        return signalR.WithUpstream(ReferenceExpression.Create($"{endpoint}{path}"), hubPattern, categoryPattern, eventPattern);
    }

    /// <summary>Adds the Azure Functions SignalR trigger webhook (<c>/runtime/webhooks/signalr</c>) of <paramref name="functionApp"/> as an upstream endpoint.</summary>
    /// <param name="signalR">The Azure SignalR resource builder.</param>
    /// <param name="functionApp">The Azure Functions resource hosting the SignalR trigger.</param>
    /// <param name="hubPattern">The rule matching hub names. <c>*</c> matches every hub.</param>
    /// <param name="categoryPattern">The rule matching categories. <c>*</c> matches every category.</param>
    /// <param name="eventPattern">The rule matching event names. <c>*</c> matches every event.</param>
    /// <returns>A builder for the Azure SignalR resource.</returns>
    /// <remarks>See the remarks of the <see cref="ReferenceExpression"/> overload for the semantics of upstream endpoints.</remarks>
    public static IResourceBuilder<AzureSignalRResource> WithUpstreamFunctionsProject(
        this IResourceBuilder<AzureSignalRResource> signalR,
        IResourceBuilder<IResourceWithEndpoints> functionApp,
        string hubPattern = _anyPattern,
        string categoryPattern = _anyPattern,
        string eventPattern = _anyPattern) =>
        signalR.WithUpstream(functionApp, _functionsWebhookPath, hubPattern: hubPattern, categoryPattern: categoryPattern, eventPattern: eventPattern);

    private static void AddUpstreamEnvironment(AzureSignalRResource resource, EnvironmentCallbackContext context)
    {
        var index = 0;
        foreach (var upstream in resource.Annotations.OfType<SignalRUpstreamAnnotation>())
        {
            var prefix = string.Create(CultureInfo.InvariantCulture, $"{_templateKeyPrefix}{index++}__");

            // ReferenceExpression is kept lazy, so host/container networking is resolved by Aspire.
            context.EnvironmentVariables[$"{prefix}UrlTemplate"] = upstream.UrlTemplate;
            context.EnvironmentVariables[$"{prefix}HubPattern"] = upstream.HubPattern;
            context.EnvironmentVariables[$"{prefix}CategoryPattern"] = upstream.CategoryPattern;
            context.EnvironmentVariables[$"{prefix}EventPattern"] = upstream.EventPattern;
        }
    }
}
