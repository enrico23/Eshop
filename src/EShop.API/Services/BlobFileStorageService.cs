using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using EShop.API.Models;

namespace EShop.API.Services;

public interface IBlobFileStorageService
{
    Task<string> UploadImageAsync(IFormFile file, CancellationToken ct = default);

    Task<bool> DeleteImageAsync(string imageId, CancellationToken ct = default);

    Task<(Stream Stream, string ContentType)> GetImageAsync(string imageId, CancellationToken ct = default);
}

public class BlobFileStorageService : IBlobFileStorageService
{
    private readonly BlobContainerClient container;
    private readonly BlobFileStorageServiceOptions blobFileStorageServiceOptions;
    private readonly long _maxUploadSizeBytes;

    public BlobFileStorageService(
        BlobContainerClient container, 
        BlobFileStorageServiceOptions blobFileStorageServiceOptions)
    {
        this.container = container;
        this.blobFileStorageServiceOptions = blobFileStorageServiceOptions;
        _maxUploadSizeBytes = blobFileStorageServiceOptions.MaxUploadSizeBytes ?? 52428800;
    }

    public async Task<string> UploadImageAsync(IFormFile file, CancellationToken ct = default)
    {
        if (file is null || file.Length == 0)
            throw new BadHttpRequestException("File is required.", StatusCodes.Status400BadRequest);
       
        if (file.Length > _maxUploadSizeBytes)
            throw new BadHttpRequestException($"File is too large. Max {_maxUploadSizeBytes / (1024 * 1024)} MB.", StatusCodes.Status413PayloadTooLarge);

        if (string.IsNullOrWhiteSpace(file.ContentType) || !file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            throw new BadHttpRequestException("Only image files are allowed.", StatusCodes.Status400BadRequest);

        var blobName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";

        var blob = container.GetBlobClient(blobName);

        await using Stream stream = file.OpenReadStream();

        await blob.UploadAsync(stream, new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders { ContentType = file.ContentType }
        }, ct);

        return blobName;
    }

    public async Task<bool> DeleteImageAsync(string imageId, CancellationToken ct = default)
    {
        if(string.IsNullOrWhiteSpace(imageId))
            throw new BadHttpRequestException("Image ID is required.", StatusCodes.Status400BadRequest);
        
        var blobClient = container.GetBlobClient(imageId);

        var response = await blobClient.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots, cancellationToken: ct);

        return response.Value;
    }

    public async Task<(Stream Stream, string ContentType)> GetImageAsync(string imageId, CancellationToken ct = default)
    {

        if (string.IsNullOrWhiteSpace(imageId))
            throw new BadHttpRequestException("Image ID is required.", StatusCodes.Status400BadRequest);

        var blobClient = container.GetBlobClient(imageId);

        if (!await blobClient.ExistsAsync(ct))
            throw new FileNotFoundException($"Image '{imageId}' not found.");

        var response = await blobClient.DownloadAsync(ct);

        var contentType = response.Value.Details.ContentType ?? "application/octet-stream";

        return (response.Value.Content, contentType);
    }
}
