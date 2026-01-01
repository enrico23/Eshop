using Azure.Messaging.ServiceBus;
using EShop.Shared.Eventing;
using EShop.Shared.Services;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Text.Json;

namespace EShop.ImagesQueueWorker;

public class EventConsumeWorker(
    ILogger<EventConsumeWorker> logger, 
    ServiceBusClient serviceBusClient,
    IOptions<ServiceBusConsumerOptions> serviceBusConsumerOptions,
    IImageStorageService imageStorageService) 
    : BackgroundService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly ILogger<EventConsumeWorker> _logger = logger;
    private readonly ServiceBusClient _serviceBusClient = serviceBusClient;
    private readonly IImageStorageService _imageStorageService = imageStorageService;
    private readonly ServiceBusConsumerOptions _serviceBusConsumerOptions = serviceBusConsumerOptions.Value;

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

    private static readonly Action<ILogger, string?, string, string, int, Exception?> _messageReceivedLog =
        LoggerMessage.Define<string?, string, string, int>(
            LogLevel.Information,
            new EventId(1000, nameof(MessageReceived)),
            "Received message. MessageId={MessageId} Type={Type} Url={Url} DeliveryCount={DeliveryCount}");

    private static void MessageReceived() { }
    private static void InvalidPayload() { }
    private static void JsonParseFailure() { }
    private static void UnknownType() { }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        _processor = _serviceBusClient.CreateProcessor("images-queue", new ServiceBusProcessorOptions
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
            .StartActivity("ServiceBus Process Message", ActivityKind.Consumer);

        activity?.SetTag("messaging.system", "azure.servicebus");
        activity?.SetTag("messaging.destination", args.EntityPath);
        activity?.SetTag("messaging.message_id", args.Message.MessageId);
        activity?.SetTag("messaging.correlation_id", args.Message.CorrelationId);
        activity?.SetTag("messaging.delivery_count", args.Message.DeliveryCount);

        var cancellationToken = args.CancellationToken;

        try
        {
            var envelope = args.Message.Body.ToObjectFromJson<EventEnvelope<DeleteImageRequest>>(JsonOptions);

            if (envelope?.Data?.Url is null)
            {
                _invalidPayloadLog(_logger, args.Message.MessageId, null);

                await args.DeadLetterMessageAsync(args.Message, "InvalidPayload", "Missing Data.Url", cancellationToken);

                return;
            }

            if (!string.Equals(envelope.Type, "image.deleted", StringComparison.Ordinal))
            {
                _unknownTypeLog(_logger, args.Message.MessageId, envelope.Type ?? "(null)", null);

                await args.DeadLetterMessageAsync(args.Message, "UnknownType", envelope.Type ?? "(null)", cancellationToken);

                return;
            }

            _messageReceivedLog(
               _logger,
               args.Message.MessageId,
               envelope.Type!,
               envelope.Data.Url!,
               args.Message.DeliveryCount,
               null);

            await _imageStorageService.DeleteImageAsync(envelope.Data.Url, cancellationToken);

            if (!_serviceBusConsumerOptions.AutoComplete)
                await args.CompleteMessageAsync(args.Message, cancellationToken);
        }
        catch (JsonException jex)
        {
            activity?.AddException(ex);
            activity?.SetStatus(ActivityStatusCode.Error);

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

            _logger.LogError(ex, "Unhandled message processing error. MessageId={MessageId}", args.Message.MessageId);
            throw; // Let processor abandon/retry
        }
    }

    private Task ErrorHandler(ProcessErrorEventArgs args)
    {
        _logger.LogError(
            args.Exception,
            "ServiceBus processor error. ErrorSource={ErrorSource} EntityPath={EntityPath} Namespace={Namespace}",
            args.ErrorSource,
            args.EntityPath,
            args.FullyQualifiedNamespace);

        return Task.CompletedTask;
    }
}
