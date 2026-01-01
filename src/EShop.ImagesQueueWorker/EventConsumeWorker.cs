using Azure.Messaging.ServiceBus;
using EShop.Shared.Eventing;
using EShop.Shared.Services;
using System.Text.Json;

namespace EShop.ImagesQueueWorker;

public class EventConsumeWorker(
    ILogger<EventConsumeWorker> logger, 
    ServiceBusClient serviceBusClient,
    IImageStorageService imageStorageService) : BackgroundService
{
    private readonly ILogger<EventConsumeWorker> _logger = logger;
    private readonly ServiceBusClient _serviceBusClient = serviceBusClient;
    private readonly IImageStorageService imageStorageService = imageStorageService;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await using var processor = _serviceBusClient.CreateProcessor("images-queue", 
                new ServiceBusProcessorOptions
            {
                MaxConcurrentCalls = 1,
                AutoCompleteMessages = false
            });

            processor.ProcessMessageAsync += MessageHandler;
            processor.ProcessErrorAsync += ErrorHandler;

            await processor.StartProcessingAsync(stoppingToken);
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
    }

    private async Task MessageHandler(ProcessMessageEventArgs args)
    {
        var cancellationToken = args.CancellationToken;
        var envelope = JsonSerializer.Deserialize<EventEnvelope<DeleteImageRequest>>(
            args.Message.Body.ToString(),
            new JsonSerializerOptions(JsonSerializerDefaults.Web));

        _logger.LogInformation("Received message: {body}", envelope!.Data.Url);

        if (envelope?.Type == "image.deleted")
        {
            await imageStorageService.DeleteImageAsync(envelope.Data.Url, cancellationToken);
            await args.CompleteMessageAsync(args.Message);
        }
    }

    private Task ErrorHandler(ProcessErrorEventArgs args)
    {
        //// the error source tells me at what point in the processing an error occurred
        //Console.WriteLine(args.ErrorSource);
        //// the fully qualified namespace is available
        //Console.WriteLine(args.FullyQualifiedNamespace);
        //// as well as the entity path
        //Console.WriteLine(args.EntityPath);
        //Console.WriteLine(args.Exception.ToString());

        _logger.LogError(args.Exception, "Message handler encountered an exception");
        return Task.CompletedTask;
    }
}
