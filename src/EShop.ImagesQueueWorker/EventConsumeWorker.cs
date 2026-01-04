using Azure.Messaging.ServiceBus;
using EShop.Shared.Eventing;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Text.Json;

namespace EShop.ImagesQueueWorker;

public class EventConsumeWorker(
    ILogger<EventConsumeWorker> logger, 
    ServiceBusClient serviceBusClient,
    IOptions<ServiceBusConsumerOptions> serviceBusConsumerOptions,
     EventHandlerRegistry registry) 
    : BackgroundService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly ILogger<EventConsumeWorker> _logger = logger;
    private readonly ServiceBusClient _serviceBusClient = serviceBusClient;
    private readonly ServiceBusConsumerOptions _serviceBusConsumerOptions = serviceBusConsumerOptions.Value;
    private readonly EventHandlerRegistry _registry = registry;

    private ServiceBusProcessor? _processor;

    private static readonly Action<ILogger, string, Exception?> _invalidPayloadLog =
        LoggerMessage.Define<string>(
            LogLevel.Warning,
            new EventId(1001, nameof(InvalidPayload)),
            "Invalid payload. MessageId={MessageId}. DeadLettering.");

    private static readonly Action<ILogger, string?, string, Exception?> _jsonParseFailureLog =
        LoggerMessage.Define<string?, string>(
            LogLevel.Warning,
            new EventId(1002, nameof(JsonParseFailure)),
            "JSON parse failure. MessageId={MessageId}. Error={Error}. DeadLettering.");

    private static readonly Action<ILogger, string?, string, Exception?> _unknownTypeLog =
        LoggerMessage.Define<string?, string>(
            LogLevel.Warning,
            new EventId(1003, nameof(UnknownType)),
            "Unknown event type. MessageId={MessageId} Type={Type}. DeadLettering.");

    private static readonly Action<ILogger, string?, string, int, Exception?> _messageReceivedLog =
        LoggerMessage.Define<string?, string, int>(
            LogLevel.Information,
            new EventId(1000, nameof(MessageReceived)),
            "Received message. MessageId={MessageId} Type={Type} DeliveryCount={DeliveryCount}");

    private static void MessageReceived() { }
    private static void InvalidPayload() { }
    private static void JsonParseFailure() { }
    private static void UnknownType() { }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        _processor = _serviceBusClient.CreateProcessor(_serviceBusConsumerOptions.QueueName, new ServiceBusProcessorOptions
        {
            MaxConcurrentCalls = _serviceBusConsumerOptions.MaxConcurrentCalls,
            AutoCompleteMessages = _serviceBusConsumerOptions.AutoComplete,
            PrefetchCount = _serviceBusConsumerOptions.PrefetchCount,
            MaxAutoLockRenewalDuration = TimeSpan.FromMinutes(5)
        });

        _processor.ProcessMessageAsync += MessageHandler;
        _processor.ProcessErrorAsync += ErrorHandler;

        await base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_processor is null)
            throw new InvalidOperationException("Processor not initialized.");

        await _processor.StartProcessingAsync(stoppingToken);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_processor is not null)
        {
            try
            {
                await _processor.StopProcessingAsync(cancellationToken);
            }
            finally
            {
                await _processor.DisposeAsync();
            }
        }

        await base.StopAsync(cancellationToken);
    }

    private async Task MessageHandler(ProcessMessageEventArgs args)
    {
        using var activity = ImagesQueueWorkerActivitySource.ActivitySource
            .StartActivity("servicebus.process", ActivityKind.Consumer);

        activity?.SetTag("messaging.system", "azure.servicebus");
        activity?.SetTag("messaging.destination", args.EntityPath);

        var cancellationToken = args.CancellationToken;
        var messageId = args.Message.MessageId ?? "(null)";

        try
        {
            var envelope = args.Message.Body.ToObjectFromJson<EventEnvelope>(JsonOptions);

            if (string.IsNullOrWhiteSpace(envelope?.Type))
            {
                activity?.SetStatus(ActivityStatusCode.Error);
                activity?.SetTag("app.outcome", "error");

                _invalidPayloadLog(_logger, messageId, null);
                await args.DeadLetterMessageAsync(args.Message, "InvalidPayload", "Missing Type", args.CancellationToken);
                return;
            }

            activity?.SetTag("event.type", envelope.Type);

            if (!_registry.TryGet(envelope.Type, out var handler))
            {
                activity?.SetStatus(ActivityStatusCode.Error);
                activity?.SetTag("app.outcome", "error");

                _unknownTypeLog(_logger, messageId, envelope.Type, null);
                await args.DeadLetterMessageAsync(args.Message, "UnknownType", envelope.Type, args.CancellationToken);
                return;
            }

            _messageReceivedLog(_logger, messageId, envelope.Type!, args.Message.DeliveryCount, null);

            await handler.ExecuteAsync(envelope.Data, cancellationToken);

            if (!_serviceBusConsumerOptions.AutoComplete)
                await args.CompleteMessageAsync(args.Message, cancellationToken);

            activity?.SetTag("app.outcome", "success");
        }
        catch (JsonException jex)
        {
            activity?.AddException(jex);
            activity?.SetStatus(ActivityStatusCode.Error);
            activity?.SetTag("app.outcome", "error");

            _jsonParseFailureLog(_logger, args.Message.MessageId, jex.Message, null);
            await args.DeadLetterMessageAsync(args.Message, "InvalidJson", jex.Message, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Normal shutdown path
        }
        catch (Exception ex)
        {
            activity?.AddException(ex);
            activity?.SetStatus(ActivityStatusCode.Error);
            activity?.SetTag("app.outcome", "error");

            _logger.LogError(ex, "Unhandled message processing error. MessageId={MessageId}", args.Message.MessageId);

            throw; // Let processor abandon/retry
        }
    }

    private static void ProcessError() { }

    private static readonly Action<ILogger, ServiceBusErrorSource, string, string, Exception?> _processErrorLog =
        LoggerMessage.Define<ServiceBusErrorSource, string, string>(
            LogLevel.Warning,
            new EventId(1005, nameof(ProcessError)),
           "ServiceBus processor error. ErrorSource={ErrorSource} EntityPath={EntityPath} Namespace={Namespace}");

    private Task ErrorHandler(ProcessErrorEventArgs args)
    {
        _processErrorLog(_logger, args.ErrorSource, args.EntityPath, args.FullyQualifiedNamespace, args.Exception);

        return Task.CompletedTask;
    }
}
