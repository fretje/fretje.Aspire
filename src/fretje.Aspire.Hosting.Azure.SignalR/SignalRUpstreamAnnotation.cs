using Aspire.Hosting.ApplicationModel;

namespace fretje.Aspire.Hosting.Azure.SignalR;

/// <summary>
/// An upstream endpoint that Azure SignalR Service (or its local emulator) forwards serverless messages and
/// connection events to. Every call to <c>WithUpstream</c> adds one annotation; their order on the resource is
/// the order of the upstream templates.
/// </summary>
/// <param name="urlTemplate">The upstream URL template. It can contain the <c>{hub}</c>, <c>{category}</c> and <c>{event}</c> placeholders.</param>
/// <param name="hubPattern">The rule matching hub names. <c>*</c> matches every hub.</param>
/// <param name="categoryPattern">The rule matching categories. <c>*</c> matches every category.</param>
/// <param name="eventPattern">The rule matching event names. <c>*</c> matches every event.</param>
public sealed class SignalRUpstreamAnnotation(
    ReferenceExpression urlTemplate, string hubPattern, string categoryPattern, string eventPattern) : IResourceAnnotation
{
    /// <summary>Gets the upstream URL template.</summary>
    public ReferenceExpression UrlTemplate => urlTemplate;

    /// <summary>Gets the rule matching hub names.</summary>
    public string HubPattern => hubPattern;

    /// <summary>Gets the rule matching categories.</summary>
    public string CategoryPattern => categoryPattern;

    /// <summary>Gets the rule matching event names.</summary>
    public string EventPattern => eventPattern;
}
