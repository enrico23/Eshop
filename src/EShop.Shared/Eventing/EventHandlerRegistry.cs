namespace EShop.Shared.Eventing;

public sealed class EventHandlerRegistry
{
    private readonly Dictionary<string, IEventHandler> _handlers = new(StringComparer.Ordinal);

    public EventHandlerRegistry(IEnumerable<IEventHandler> handlers)
    {
        foreach (var handler in handlers)
            _handlers[handler.EventType] = handler;
    }

    public bool TryGet(string eventType, out IEventHandler handler) =>
        _handlers.TryGetValue(eventType, out handler!);
}
