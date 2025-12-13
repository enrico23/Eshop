using EShop.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace EShop.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public sealed class ImagesController(IFileStorage fileStorage) : ControllerBase
{
    // GET /api/images/{imageId}
    [HttpGet("{imageId}")]
    public async Task<IActionResult> GetImageByIdAsync([FromRoute] string imageId, CancellationToken ct = default)
    {
        var result = await fileStorage.GetImageByIdAsync(imageId, ct);
        return File(result.Stream, result.ContentType);
    }
}
