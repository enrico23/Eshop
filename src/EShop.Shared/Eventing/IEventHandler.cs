using System.Text.Json;

namespace EShop.Shared.Eventing;

public interface IEventHandler
{
    string EventType { get; }
    Task ExecuteAsync(JsonElement data, CancellationToken ct);
}
