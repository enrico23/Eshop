namespace EShop.API.Services;

public interface IFileStorage
{
    Task<string> SaveImageAsync(IFormFile file, CancellationToken ct = default);
}

public class LocalFileStorage(IWebHostEnvironment environment) : IFileStorage
{
    private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5MB


    public async Task<string> SaveImageAsync(IFormFile file, CancellationToken ct = default)
    {
        if (file is null || file.Length == 0)
        {
            throw new BadHttpRequestException("File is required.", StatusCodes.Status400BadRequest);
        }

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

        var root = environment.WebRootPath ?? environment.ContentRootPath;
        var uploadsFolder = Path.Combine(root, "uploads");
        Directory.CreateDirectory(uploadsFolder);


        var extension = Path.GetExtension(file.FileName);
        var fileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(uploadsFolder, fileName);

        await using var stream = System.IO.File.Create(filePath);
        await file.CopyToAsync(stream, ct);

        return $"/uploads/{fileName}";
    }
}
