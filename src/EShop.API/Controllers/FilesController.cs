using EShop.API.Services;
using EShop.Shared.Representations.Product;
using Microsoft.AspNetCore.Mvc;

namespace EShop.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FilesController(IFileStorage fileStorage) : ControllerBase
{
    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ProductImageDto>> Upload(
        [FromForm] IFormFile file, CancellationToken ct)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded.");

        string relativeUrl;
        try
        {
            relativeUrl = await fileStorage.SaveImageAsync(file, ct);
        }
        catch (OperationCanceledException)
        {
            return StatusCode(StatusCodes.Status499ClientClosedRequest);
        }
        catch (BadHttpRequestException ex)
        {
            return StatusCode(ex.StatusCode, ex.Message);
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, "Error saving file.");
        }

        var model = new ProductImageDto
        {
            Url = relativeUrl,
            AltText = file.Name,
            IsPrimary = true,
            SortOrder = 0
        };

        // You can plug in your fileStorage here later
        return Ok(model);
    }
}
