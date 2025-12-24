using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using EShop.Shared.Exceptions;
using Microsoft.AspNetCore.Http;

namespace EShop.Shared.Services;

public interface IImageStorageService
{
    Task<bool> DeleteImageAsync(string imageId, CancellationToken ct = default);
}

public sealed class ImageStorageService : IImageStorageService
{
    private readonly BlobContainerClient _container;
    
    private readonly long _maxUploadSizeBytes;

    private readonly ServiceBusClient _serviceBusClient;

    public ImageStorageService(
        BlobContainerClient container, 
        ServiceBusClient serviceBusClient)
    {
        _container = container;
        _maxUploadSizeBytes = 52428800;
        _serviceBusClient = serviceBusClient;
    }

    public async Task<bool> DeleteImageAsync(string imageId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(imageId))
            throw new BadRequestException("Image ID is required.", StatusCodes.Status400BadRequest);

        var blobClient = _container.GetBlobClient(imageId);

        var response = await blobClient.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots, cancellationToken: ct);

        await SendDeleteImageMessageAsync(imageId, ct);

        return response.Value;
    }

    private async Task<bool> SendDeleteImageMessageAsync(
       string imageName,
       CancellationToken cancellationToken)
    {
        var sender = _serviceBusClient.CreateSender("images-queue");

        var message = new ServiceBusMessage(imageName)
        {
            MessageId = Guid.NewGuid().ToString(),
            Subject = "image.deleted"
        };

        await sender.SendMessageAsync(message, cancellationToken);

        return true;
    }
}
