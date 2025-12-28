# Generic Event Publisher Snippets

`src/EShop.Shared/Services/EventEnvelope.cs`
```csharp
using System.Collections.Generic;

namespace EShop.Shared.Services;

public sealed record EventEnvelope<T>(
    string Type,
    T Data,
    string? Id = null,
    string? CorrelationId = null,
    string? Source = null,
    IDictionary<string, string>? Metadata = null);
```

`src/EShop.Shared/Services/IEventPublisher.cs`
```csharp
namespace EShop.Shared.Services;

public interface IEventPublisher
{
    Task PublishAsync<T>(string destination, EventEnvelope<T> envelope, CancellationToken ct);
}
```

`src/EShop.Shared/Services/ServiceBusEventPublisher.cs`
```csharp
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EShop.Shared.Services;

public sealed class ServiceBusEventPublisher(
    ServiceBusClient client,
    IOptions<ServiceBusPublisherOptions> options,
    ILogger<ServiceBusEventPublisher> logger)
    : IEventPublisher, IAsyncDisposable
{
    private readonly ServiceBusClient _client = client;
    private readonly ServiceBusPublisherOptions _options = options.Value;
    private readonly ILogger<ServiceBusEventPublisher> _logger = logger;
    private readonly ConcurrentDictionary<string, ServiceBusSender> _senders = new();
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task PublishAsync<T>(string destination, EventEnvelope<T> envelope, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(destination);
        ArgumentNullException.ThrowIfNull(envelope);

        var sender = _senders.GetOrAdd(destination, _client.CreateSender);
        var body = BinaryData.FromString(JsonSerializer.Serialize(envelope, JsonOptions));

        var message = new ServiceBusMessage(body)
        {
            MessageId = envelope.Id ?? Guid.NewGuid().ToString("N"),
            Subject = envelope.Type,
            CorrelationId = envelope.CorrelationId ?? Activity.Current?.TraceId.ToString(),
            ContentType = "application/json",
            TimeToLive = _options.TimeToLive
        };

        if (!string.IsNullOrWhiteSpace(envelope.Source))
            message.ApplicationProperties["source"] = envelope.Source;

        if (envelope.Metadata is not null)
        {
            foreach (var (key, value) in envelope.Metadata)
                message.ApplicationProperties[key] = value;
        }

        await sender.SendMessageAsync(message, ct);
        _logger.LogInformation("Published event {Type} to {Destination}", envelope.Type, destination);
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var sender in _senders.Values)
            await sender.DisposeAsync();
    }
}

public sealed class ServiceBusPublisherOptions
{
    public TimeSpan TimeToLive { get; init; } = TimeSpan.FromDays(1);
}
```

`src/EShop.API/Program.cs`
```csharp
builder.Services.Configure<ServiceBusPublisherOptions>(options =>
{
    options.TimeToLive = TimeSpan.FromDays(1);
});

builder.Services.AddSingleton<IEventPublisher, ServiceBusEventPublisher>();
```

`src/EShop.API/Services/ProductService.cs`
```csharp
public sealed record DeleteImageRequest(string Url);

await _eventPublisher.PublishAsync(
    "images-queue",
    new EventEnvelope<DeleteImageRequest>(
        Type: "image.deleted",
        Data: new DeleteImageRequest(image.Url),
        CorrelationId: Activity.Current?.TraceId.ToString(),
        Source: "eshop.api"),
    ct);
```

`src/EShop.ImagesQueueWorker/Worker.cs`
```csharp
using System.Text.Json;

var envelope = JsonSerializer.Deserialize<EventEnvelope<DeleteImageRequest>>(
    args.Message.Body.ToString(),
    new JsonSerializerOptions(JsonSerializerDefaults.Web));

if (envelope?.Type == "image.deleted")
{
    await imageStorageService.DeleteImageAsync(envelope.Data.Url, stoppingToken);
    await args.CompleteMessageAsync(args.Message, stoppingToken);
}
```
