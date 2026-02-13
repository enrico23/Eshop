using Azure.Storage.Blobs;
using EShop.Shared.Services;
using Microsoft.Extensions.DependencyInjection;

namespace EShop.Shared.Extensions;

public static class AzureBlobStorageExtensions
{
    public static IServiceCollection AddAzureBlobStorage(
     this IServiceCollection services,
     string blobConnectionString,
     Action<ImageStorageServiceOptions>? configureSettings = null)
    {
        if (string.IsNullOrWhiteSpace(blobConnectionString))
        {
            throw new InvalidOperationException("Blob storage connection string is missing.");
        }

        services.AddSingleton(_ => new BlobServiceClient(blobConnectionString));
        services.AddSingleton(_ =>
        {
            var options = new ImageStorageServiceOptions();
            configureSettings?.Invoke(options);

            return options;
        });
        services.AddSingleton(sp =>
        {
            var serviceClient = sp.GetRequiredService<BlobServiceClient>();
            return serviceClient.GetBlobContainerClient("product-images");
        });
        services.AddSingleton<IBlobContainerResolver, BlobContainerResolver>();
        services.AddSingleton<IImageStorageService, ImageStorageService>();

        return services;
    }
}
