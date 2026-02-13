using Azure.Storage.Blobs;

namespace EShop.BlobStorageInitializer;

public class BlobInitializerService(
    BlobServiceClient blobServiceClient,
    ILogger<BlobInitializerService> logger,
    IHostApplicationLifetime hostApplicationLifetime) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Azure Blob Storage Initializer started!");

        await blobServiceClient.GetBlobContainerClient("product-images")
          .CreateIfNotExistsAsync(Azure.Storage.Blobs.Models.PublicAccessType.None, null, stoppingToken);

        await blobServiceClient.GetBlobContainerClient("category-images")
         .CreateIfNotExistsAsync(Azure.Storage.Blobs.Models.PublicAccessType.None, null, stoppingToken);

        logger.LogInformation("Azure Blob Storage Initializer finsihed!");

        hostApplicationLifetime.StopApplication();
    }
}