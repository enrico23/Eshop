using EShop.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace EShop.API.Controllers;

[ApiController]
[Route("api/images")]
public class ImagesController(IFileStorage fileStorage) : ControllerBase
{
    private readonly IFileStorage _fileStorage = fileStorage;

    // GET /api/images/{imageId}
    [HttpGet("{imageId}")]
    public async Task<IActionResult> Get(string imageId, CancellationToken ct)
    {
        var (stream, contentType) = await _fileStorage.GetImageAsync(imageId, ct);

        // ASP.NET Core will dispose the stream when the response ends
        return File(stream, contentType);
    }
}

