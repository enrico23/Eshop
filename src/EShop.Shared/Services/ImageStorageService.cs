using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using EShop.Shared.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace EShop.Shared.Services;

public enum ImageStoragePurpose
{
    CustomerContent = 0,
    UiAssets = 1
}

public interface IBlobContainerResolver
{
    BlobContainerClient Get(ImageStoragePurpose purpose);
}

public sealed class BlobContainerResolver(
    BlobServiceClient serviceClient,
    IOptions<ImageStorageServiceOptions> options) : IBlobContainerResolver
{
    private readonly ImageStorageServiceOptions _opt = options.Value;

    public BlobContainerClient Get(ImageStoragePurpose purpose) =>
        purpose switch
        {
            ImageStoragePurpose.CustomerContent => serviceClient.GetBlobContainerClient(_opt.CustomerContainer),
            ImageStoragePurpose.UiAssets => serviceClient.GetBlobContainerClient(_opt.UiAssetsContainer),
            _ => throw new ArgumentOutOfRangeException(nameof(purpose), purpose, "Unknown storage purpose.")
        };
}

public interface IImageStorageService
{
    Task<string> UploadImageAsync(
        ImageStoragePurpose purpose, IFormFile file, CancellationToken ct = default);

    Task<bool> DeleteImageAsync(
        ImageStoragePurpose purpose, string imageId, CancellationToken ct = default);

    Task<(Stream Stream, string ContentType)> GetImageAsync(
        ImageStoragePurpose purpose, string imageId, CancellationToken ct = default);
}

public sealed class ImageStorageService(
        IBlobContainerResolver containers,
        IOptions<ImageStorageServiceOptions> options) 
    : IImageStorageService
{
    private readonly ImageStorageServiceOptions _opt = options.Value;

    public async Task<string> UploadImageAsync(
        ImageStoragePurpose purpose, IFormFile file, CancellationToken ct = default)
    {
        if (file is null || file.Length == 0)
            throw new BadRequestException("File is required.", StatusCodes.Status400BadRequest);

        if (file.Length > _opt.MaxUploadSizeBytes)
            throw new BadRequestException($"File is too large. Max {_opt.MaxUploadSizeBytes} MB.", StatusCodes.Status413PayloadTooLarge);

        if (string.IsNullOrWhiteSpace(file.ContentType) || !file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            throw new BadRequestException("Only image files are allowed.", StatusCodes.Status400BadRequest);

        var container = containers.Get(purpose);

        var blobName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";

        var blob = container.GetBlobClient(blobName);

        await using Stream stream = file.OpenReadStream();

        await blob.UploadAsync(stream, new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders { ContentType = file.ContentType }
        }, ct);

        return blobName;
    }

    public async Task<bool> DeleteImageAsync(
        ImageStoragePurpose purpose, string imageId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(imageId))
            throw new BadRequestException("Image ID is required.", StatusCodes.Status400BadRequest);

        var container = containers.Get(purpose);

        var blobClient = container.GetBlobClient(imageId);

        var response = await blobClient.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots, cancellationToken: ct);

        return response.Value;
    }

    public async Task<(Stream Stream, string ContentType)> GetImageAsync(
        ImageStoragePurpose purpose, string imageId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(imageId))
            throw new BadRequestException("Image ID is required.", StatusCodes.Status400BadRequest);

        var container = containers.Get(purpose);

        var blobClient = container.GetBlobClient(imageId);

        if (!await blobClient.ExistsAsync(ct))
            throw new FileNotFoundException($"Image '{imageId}' not found.");

        var response = await blobClient.DownloadAsync(ct);

        var contentType = response.Value.Details.ContentType ?? "application/octet-stream";

        return (response.Value.Content, contentType);
    }
}
