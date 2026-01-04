namespace EShop.Shared.Eventing;

public interface IEventPublisher
{
    Task PublishAsync<T>(string destination, EventEnvelope<T> envelope, CancellationToken ct);
}
