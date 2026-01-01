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
    private readonly ILogger<EventConsumeWorker> _logger = logger;
    private readonly ServiceBusClient _serviceBusClient = serviceBusClient;
    private readonly IImageStorageService imageStorageService = imageStorageService;
    private readonly ServiceBusConsumerOptions _serviceBusConsumerOptions = serviceBusConsumerOptions.Value;

    private ServiceBusProcessor? _processor;

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
        var cancellationToken = args.CancellationToken;

        using var activity = ImagesQueueWorkerActivitySource.ActivitySource
            .StartActivity("ServiceBus Process Message", ActivityKind.Consumer);
        activity?.SetTag("messaging.system", "azure.servicebus");
        activity?.SetTag("messaging.destination", args.EntityPath);
        activity?.SetTag("messaging.message_id", args.Message.MessageId);
        activity?.SetTag("messaging.correlation_id", args.Message.CorrelationId);
        activity?.SetTag("messaging.delivery_count", args.Message.DeliveryCount);

        try
        {
            var envelope = JsonSerializer.Deserialize<EventEnvelope<DeleteImageRequest>>(
                args.Message.Body.ToString(),
                new JsonSerializerOptions(JsonSerializerDefaults.Web));

            if (envelope?.Data?.Url is null)
            {
                _logger.LogWarning(
                    "Invalid message payload. MessageId={MessageId}. DeadLettering.",
                    args.Message.MessageId);

                await args.DeadLetterMessageAsync(args.Message, "InvalidPayload", "Missing Data.Url");
                return;
            }

            _logger.LogInformation(
                "Received message. Type={Type} Url={Url} MessageId={MessageId} DeliveryCount={DeliveryCount}",
                envelope.Type, envelope.Data.Url, args.Message.MessageId, args.Message.DeliveryCount);


            if (envelope?.Type == "image.deleted")
            {
                await imageStorageService.DeleteImageAsync(envelope.Data.Url, cancellationToken);
            }

            if (!_serviceBusConsumerOptions.AutoComplete)
            {
                await args.CompleteMessageAsync(args.Message, args.CancellationToken);
            }
        }
        catch (ServiceBusException)
        {

            throw;
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
