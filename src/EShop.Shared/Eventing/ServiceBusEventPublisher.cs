using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.Json;

namespace EShop.Shared.Eventing;

public interface IEventPublisher
{
    Task PublishAsync<T>(string destination, EventEnvelope<T> envelope, CancellationToken ct);
}

public sealed class ServiceBusPublisherOptions
{
    public TimeSpan TimeToLive { get; set; } = TimeSpan.FromDays(1);
    public int MaxCachedSenders { get; init; } = 64;
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
        
        var messageId = !string.IsNullOrWhiteSpace(envelope.Id)
         ? envelope.Id
         : Guid.NewGuid().ToString("N");

        var correlationId =
            envelope.CorrelationId
            ?? Activity.Current?.TraceId.ToString();

        using var scope = _logger.BeginScope(new Dictionary<string, object?>
        {
            ["sb.destination"] = destination,
            ["event.type"] = envelope.Type,
            ["sb.message_id"] = messageId,
            ["sb.correlation_id"] = correlationId
        });

         // Guard against unbounded destination churn.
        if (_options.MaxCachedSenders > 0 && _senders.Count > _options.MaxCachedSenders)
        {
            _logger.LogWarning(
                "Sender cache size {Count} exceeds MaxCachedSenders {Max}. " +
                "This may indicate dynamic/unbounded destination names.",
                _senders.Count, _options.MaxCachedSenders);
        }

        var sender = _senders.GetOrAdd(destination, _client.CreateSender);
        var body = BinaryData.FromString(JsonSerializer.Serialize(envelope, JsonOptions));

        var message = new ServiceBusMessage(body)
        {
            MessageId = messageId,
            Subject = envelope.Type,
            CorrelationId = correlationId,
            ContentType = "application/json",
            TimeToLive = _options.TimeToLive
        };

        if (!string.IsNullOrWhiteSpace(envelope.Source))
            message.ApplicationProperties["source"] = envelope.Source;

        if (envelope.Metadata is not null)
            foreach (var (key, value) in envelope.Metadata)
                message.ApplicationProperties[key] = CoerceApplicationProperty(value);

        try
        {
            await sender.SendMessageAsync(message, ct);
            _logger.LogInformation("Published event {Type} to {Destination}", envelope.Type, destination);
        }
        catch (ServiceBusException ex) when (ex.Reason is
            ServiceBusFailureReason.ServiceTimeout or
            ServiceBusFailureReason.ServiceCommunicationProblem or
            ServiceBusFailureReason.ServiceBusy)
        {
            // Transient-ish issues: SDK retries already happen, but log with context.
            _logger.LogWarning(ex,
                "Transient Service Bus failure publishing event. Reason={Reason}",
                ex.Reason);

            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish event to Service Bus.");
            throw;
        }
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var sender in _senders.Values)
            await sender.DisposeAsync();
    }

    /// <summary>
    /// Complex values are stringified to avoid runtime exceptions.
    /// </summary>
    private static object? CoerceApplicationProperty(object? value)
    {
        if (value is null) return null;

        // Allowed types include: string, bool, byte, sbyte, short, ushort, int, uint,
        // long, ulong, float, double, decimal, char, Guid, DateTime, DateTimeOffset, TimeSpan, Uri, etc.
        return value switch
        {
            string or bool
                or byte or sbyte
                or short or ushort
                or int or uint
                or long or ulong
                or float or double or decimal
                or Guid
                or DateTime or DateTimeOffset
                or TimeSpan
                => value,

            _ => value.ToString() ?? string.Empty
        };
    }
}
