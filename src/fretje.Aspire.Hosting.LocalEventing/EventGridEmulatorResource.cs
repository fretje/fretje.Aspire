using Aspire.Hosting.ApplicationModel;

namespace fretje.Aspire.Hosting.LocalEventing;

/// <summary>
/// A local Event Grid emulator (workleap/eventgridemulator) running as a container.
/// Topics and their webhook subscriptions are modelled as child <see cref="EventGridTopicResource"/>s
/// and are turned into the emulator's <c>Topics__{topic}__{index}</c> environment variables at startup.
/// </summary>
/// <param name="name">The name of the resource.</param>
public sealed class EventGridEmulatorResource(string name) : ContainerResource(name)
{
    /// <summary>The name of the HTTP endpoint exposed by the emulator container.</summary>
    internal const string _httpEndpointName = "http";

    /// <summary>The port the emulator container listens on.</summary>
    internal const int _containerPort = 6500;

    private readonly List<EventGridTopicResource> _topics = [];

    /// <summary>Gets the HTTP endpoint exposed by the emulator container.</summary>
    public EndpointReference HttpEndpoint => field ??= new(this, _httpEndpointName);

    /// <summary>Gets the topics declared on this emulator.</summary>
    public IReadOnlyList<EventGridTopicResource> Topics => _topics;

    /// <summary>Adds a topic to the emulator.</summary>
    /// <param name="topic">The topic to add.</param>
    internal void AddTopic(EventGridTopicResource topic) => _topics.Add(topic);
}
