using Aspire.Hosting.ApplicationModel;

namespace fretje.Aspire.Hosting.LocalEventing;

/// <summary>
/// A topic on the Event Grid emulator. Its connection string is the URL events can be published to,
/// so a publisher can use <c>.WithReference(topic)</c>.
/// </summary>
public sealed class EventGridTopicResource(string name, string topicName, EventGridEmulatorResource parent)
    : Resource(name), IResourceWithParent<EventGridEmulatorResource>
{
    private readonly List<ReferenceExpression> _subscriptions = [];

    public EventGridEmulatorResource Parent => parent;

    /// <summary>Gets the topic name as the emulator knows it (can differ from the Aspire resource name).</summary>
    public string TopicName => topicName;

    /// <summary>Gets the URL that events for this topic are POSTed to.</summary>
    public ReferenceExpression TopicEndpointExpression =>
        ReferenceExpression.Create($"{parent.HttpEndpoint}/{TopicName}/api/events");

    /// <summary>Gets the Webhook URLs the emulator forwards events of this topic to.</summary>
    public IReadOnlyList<ReferenceExpression> Subscriptions => _subscriptions;

    internal void AddSubscription(ReferenceExpression webhookUrl) => _subscriptions.Add(webhookUrl);
}
