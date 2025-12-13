using EShop.API.Models;

namespace EShop.API.Services;

public interface IFileStorage
{
    bool Delete(string url);
    Task<ImageStreamResult> GetImageByIdAsync(string url, CancellationToken ct);
    Task<string> SaveImageAsync(IFormFile file, CancellationToken ct = default);
}

public class LocalFileStorage : IFileStorage
{
    private const long MaxFileSizeBytes = 5 * 1024 * 1024;

    private readonly IWebHostEnvironment _environment;

    private readonly string _root = string.Empty;

    private readonly string _uploadsFolder;

    public LocalFileStorage(IWebHostEnvironment environment)
    {
        _environment = environment;
        _root = _environment.WebRootPath ?? _environment.ContentRootPath;
        _uploadsFolder = Path.Combine(_root, "uploads");

        Directory.CreateDirectory(_uploadsFolder);
    }

    public bool Delete(string url)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(url);

        var filePath = Path.Combine(_uploadsFolder, url);
        System.IO.File.Delete(filePath);

        return true;
    }

    public async Task<string> SaveImageAsync(IFormFile file, CancellationToken ct = default)
    {
        if (file is null || file.Length == 0)
            throw new BadHttpRequestException("File is required.", StatusCodes.Status400BadRequest);

        if (file.Length > MaxFileSizeBytes)
        {
            throw new BadHttpRequestException(
                $"File is too large. Max {MaxFileSizeBytes / (1024 * 1024)} MB.",
                StatusCodes.Status413PayloadTooLarge);
        }

        if (!file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
        {
            throw new BadHttpRequestException("Only image files are allowed.", StatusCodes.Status400BadRequest);
        }

        
        


        var extension = Path.GetExtension(file.FileName);
        var fileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(_uploadsFolder, fileName);

        await using var stream = System.IO.File.Create(filePath);
        await file.CopyToAsync(stream, ct);

        return fileName;
    }

    public Task<ImageStreamResult> GetImageByIdAsync(string imageId, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(imageId);
        ct.ThrowIfCancellationRequested();

        var filePath = Path.GetFullPath(Path.Combine(_uploadsFolder, imageId));

        if (!File.Exists(filePath))
            throw new FileNotFoundException("Image not found.", filePath);

        var ext = Path.GetExtension(filePath);
        var contentType = ContentTypes.TryGetValue(ext, out var ctValue)
            ? ctValue
            : "application/octet-stream";

        var stream = new FileStream(
            filePath,
            new FileStreamOptions
            {
                Mode = FileMode.Open,
                Access = FileAccess.Read,
                Share = FileShare.Read,
                Options = FileOptions.Asynchronous | FileOptions.SequentialScan
            });

        return Task.FromResult(new ImageStreamResult(stream, contentType));
    }

    private static readonly IDictionary<string, string> ContentTypes =
         new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
         {
             [".jpg"] = "image/jpeg",
             [".jpeg"] = "image/jpeg",
             [".png"] = "image/png",
             [".gif"] = "image/gif",
             [".webp"] = "image/webp",
             [".bmp"] = "image/bmp",
             [".svg"] = "image/svg+xml",
             [".ico"] = "image/x-icon"
         };
}
