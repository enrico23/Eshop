using EShop.Shared.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EShop.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public sealed class ImagesController(IImageStorageService imageStorageService) : ControllerBase
{
    // GET /api/images/{imageId}
    [AllowAnonymous]
    [HttpGet("{imageId}")]
    public async Task<IActionResult> GetImageByIdAsync(
        [FromRoute] string imageId, 
        [FromQuery] ImageStoragePurpose purpose = ImageStoragePurpose.Products, 
        CancellationToken ct = default)
    {
        var result = await imageStorageService.GetImageAsync(purpose, imageId, ct);
        return File(result.Stream, result.ContentType);
    }
}
