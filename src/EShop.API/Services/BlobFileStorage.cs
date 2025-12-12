using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace EShop.API.Services;

public class BlobFileStorage(BlobContainerClient container) : IFileStorage
{
    private const long MaxFileSizeBytes = 5 * 1024 * 1024;

    public async Task<string> SaveImageAsync(IFormFile file, CancellationToken ct = default)
    {
        if (file is null || file.Length == 0)
            throw new BadHttpRequestException("File is required.", StatusCodes.Status400BadRequest);

        if (file.Length > MaxFileSizeBytes)
            throw new BadHttpRequestException($"File is too large. Max {MaxFileSizeBytes / (1024 * 1024)} MB.", StatusCodes.Status413PayloadTooLarge);

        if (string.IsNullOrWhiteSpace(file.ContentType) || !file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            throw new BadHttpRequestException("Only image files are allowed.", StatusCodes.Status400BadRequest);

        await container.CreateIfNotExistsAsync(PublicAccessType.Blob, cancellationToken: ct);

        var blobName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";

        var blob = container.GetBlobClient(blobName);

        await using Stream stream = file.OpenReadStream();

        await blob.UploadAsync(stream, new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders { ContentType = file.ContentType }
        }, ct);

        return blobName;
    }

    public async Task<(Stream Stream, string ContentType)> GetImageAsync(string imageId, CancellationToken ct = default)
    {
        var blobClient = container.GetBlobClient(imageId);

        if (!await blobClient.ExistsAsync(ct))
            throw new FileNotFoundException($"Image '{imageId}' not found.");

        var response = await blobClient.DownloadAsync(ct);

        var contentType = response.Value.Details.ContentType ?? "application/octet-stream";

        return (response.Value.Content, contentType);
    }

    public async Task<bool> DeleteImageAsync(string imageId, CancellationToken ct = default)
    {
        var blobClient = container.GetBlobClient(imageId);

        var response = await blobClient.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots, cancellationToken: ct);

        return response.Value;
    }
}
