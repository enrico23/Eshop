using Azure.Messaging.ServiceBus;
using EShop.Shared.Services;
using EShop.Shared.Services.Eventing;
using System.Text.Json;

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
            await using var processor = serviceBusClient.CreateProcessor("images-queue", 
                new ServiceBusProcessorOptions
            {
                MaxConcurrentCalls = 1,
                AutoCompleteMessages = false
            });

            processor.ProcessMessageAsync += MessageHandler;
            processor.ProcessErrorAsync += ErrorHandler;

            async Task MessageHandler(ProcessMessageEventArgs args)
            {
                var envelope = JsonSerializer.Deserialize<EventEnvelope<DeleteImageRequest>>(
                    args.Message.Body.ToString(),
                    new JsonSerializerOptions(JsonSerializerDefaults.Web));

                logger.LogInformation("Received message: {body}", envelope!.Data.Url);

                if (envelope?.Type == "image.deleted")
                {
                    await imageStorageService.DeleteImageAsync(envelope.Data.Url, stoppingToken);
                    await args.CompleteMessageAsync(args.Message, stoppingToken);
                }
            }

            Task ErrorHandler(ProcessErrorEventArgs args)
            {
                //// the error source tells me at what point in the processing an error occurred
                //Console.WriteLine(args.ErrorSource);
                //// the fully qualified namespace is available
                //Console.WriteLine(args.FullyQualifiedNamespace);
                //// as well as the entity path
                //Console.WriteLine(args.EntityPath);
                //Console.WriteLine(args.Exception.ToString());

                logger.LogError(args.Exception, "Message handler encountered an exception");
                return Task.CompletedTask;
            }

            await processor.StartProcessingAsync(stoppingToken);
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
    }
}
