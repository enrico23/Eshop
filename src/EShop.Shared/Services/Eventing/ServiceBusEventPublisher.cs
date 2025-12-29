using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.Json;

namespace EShop.Shared.Services.Eventing;

public interface IEventPublisher
{
    Task PublishAsync<T>(string destination, EventEnvelope<T> envelope, CancellationToken ct);
}

public sealed class ServiceBusPublisherOptions
{
    public TimeSpan TimeToLive { get; set; } = TimeSpan.FromDays(1);
}

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
