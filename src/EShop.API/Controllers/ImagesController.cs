using EShop.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace EShop.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public sealed class ImagesController(IBlobFileStorageService blobFileStorageService) : ControllerBase
{
    // GET /api/images/{imageId}
    [HttpGet("{imageId}")]
    public async Task<IActionResult> GetImageByIdAsync([FromRoute] string imageId, CancellationToken ct = default)
    {
        var result = await blobFileStorageService.GetImageAsync(imageId, ct);
        return File(result.Stream, result.ContentType);
    }
}
