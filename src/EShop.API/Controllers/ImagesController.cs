using EShop.Shared.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EShop.API.Controllers;

[AllowAnonymous]
[Route("api/[controller]")]
[ApiController]
public sealed class ImagesController(IImageStorageService imageStorageService) : ControllerBase
{
    // GET /api/images/{imageId}
    [HttpGet("{imageId}")]
    public async Task<IActionResult> GetImageByIdAsync([FromRoute] string imageId, CancellationToken ct = default)
    {
        var result = await imageStorageService.GetImageAsync(imageId, ct);
        return File(result.Stream, result.ContentType);
    }
}
