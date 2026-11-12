using Aspire.Hosting.ApplicationModel;

namespace fretje.Aspire.Hosting.LocalEventing;

/// <summary>
/// A local Event Grid emulator (workleap/eventgridemulator) running as a container.
/// Topics and their webhook subscriptions are modelled as child <see cref="EventGridTopicResource"/>s
/// and are turned into the emulator's <c>Topics__{topic}__{index}</c> environment variables at startup.
/// </summary>
public sealed class EventGridEmulatorResource(string name) : ContainerResource(name)
{
    internal const string _httpEndpointName = "http";
    internal const int _containerPort = 6500;

    private readonly List<EventGridTopicResource> _topics = [];

    public EndpointReference HttpEndpoint => field ??= new(this, _httpEndpointName);

    public IReadOnlyList<EventGridTopicResource> Topics => _topics;

    internal void AddTopic(EventGridTopicResource topic) => _topics.Add(topic);
}
