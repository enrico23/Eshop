using Azure.Messaging.ServiceBus;
using EShop.Shared.Services;

namespace EShop.ImagesQueueWorker;

public class Worker(
    ILogger<Worker> logger, 
    ServiceBusClient serviceBusClient,
    IImageStorageService imageStorageService) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {           
            await using var processor = serviceBusClient.CreateProcessor("images-queue", new ServiceBusProcessorOptions
            {
                MaxConcurrentCalls = 1,
                AutoCompleteMessages = false
            });

            processor.ProcessMessageAsync += async args =>
            {
                var imageId = args.Message.Body.ToString();
                logger.LogInformation("Received message: {body}", imageId);
       
                await imageStorageService.DeleteImageAsync(imageId, stoppingToken);

                logger.LogInformation("Processed message: {body}", imageId);
                await args.CompleteMessageAsync(args.Message, stoppingToken);
            };

            processor.ProcessErrorAsync += args =>
            {
                logger.LogError(args.Exception, "Message handler encountered an exception");
                return Task.CompletedTask;
            };

            await processor.StartProcessingAsync(stoppingToken);
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
    }
}
