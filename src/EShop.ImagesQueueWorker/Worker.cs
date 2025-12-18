using Azure.Messaging.ServiceBus;

namespace EShop.ImagesQueueWorker;

public class Worker(ILogger<Worker> logger, ServiceBusClient serviceBusClient) : BackgroundService
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
                var body = args.Message.Body.ToString();
                logger.LogInformation("Received message: {body}", body);
                // Simulate image processing
                await Task.Delay(2000, stoppingToken);
                logger.LogInformation("Processed message: {body}", body);
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
