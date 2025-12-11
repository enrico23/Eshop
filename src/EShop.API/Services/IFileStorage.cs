namespace EShop.API.Services;

public interface IFileStorage
{
    Task<string> SaveImageAsync(IFormFile file, CancellationToken ct = default);

    Task<(Stream Stream, string ContentType)> GetImageAsync(string imageId, CancellationToken ct = default);
}
