using Aspire.Hosting.ApplicationModel;

namespace fretje.Aspire.Hosting.LocalEventing;

/// <summary>
/// A local Event Grid emulator (workleap/eventgridemulator) running as a container.
/// Topics and their webhook subscriptions are modelled as child <see cref="EventGridTopicResource"/>s
/// and are turned into the emulator's <c>Topics__{topic}__{index}</c> environment variables at startup.
/// </summary>
public sealed class EventGridEmulatorResource(string name) : ContainerResource(name)
{
    internal const string HttpEndpointName = "http";
    internal const int ContainerPort = 6500;

    private readonly List<EventGridTopicResource> _topics = [];

    public EndpointReference HttpEndpoint => field ??= new(this, HttpEndpointName);

    public IReadOnlyList<EventGridTopicResource> Topics => _topics;

    internal void AddTopic(EventGridTopicResource topic) => _topics.Add(topic);
}

/// <summary>
/// A topic on the Event Grid emulator. Its connection string is the URL events can be published to,
/// so a publisher can use <c>.WithReference(topic)</c>.
/// </summary>
public sealed class EventGridTopicResource(string name, string topicName, EventGridEmulatorResource parent)
    : Resource(name), IResourceWithParent<EventGridEmulatorResource>
{
    private readonly List<ReferenceExpression> _subscriptions = [];

    public EventGridEmulatorResource Parent => parent;

    /// <summary>The topic name as the emulator knows it (can differ from the Aspire resource name).</summary>
    public string TopicName => topicName;

    /// <summary>URL that events for this topic are POSTed to.</summary>
    public ReferenceExpression TopicEndpointExpression =>
        ReferenceExpression.Create($"{parent.HttpEndpoint}/{TopicName}/api/events");

    /// <summary>Webhook URLs the emulator forwards events of this topic to.</summary>
    public IReadOnlyList<ReferenceExpression> Subscriptions => _subscriptions;

    internal void AddSubscription(ReferenceExpression webhookUrl) => _subscriptions.Add(webhookUrl);
}
