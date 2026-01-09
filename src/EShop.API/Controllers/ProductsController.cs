using EShop.API.Models;
using EShop.API.Services;
using EShop.Shared.Representations;
using EShop.Shared.Representations.Product;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EShop.API.Controllers;

[AllowAnonymous]
[Route("api/[controller]")]
[ApiController]
public class ProductsController(IProductService productService) : ControllerBase
{
    private readonly IProductService _productService = productService;

    // GET: api/products?page=1&pageSize=20
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ProductDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult> GetPagedAsync(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        [FromQuery] string orderBy = "Name", [FromQuery] string orderDirection = "asc", 
        [FromQuery] string? filter = null, 
        [FromQuery] Guid? categoryId = null, CancellationToken ct = default)
    {
        var results = await _productService.GetPagedAsync(page, pageSize, orderBy, orderDirection, filter, categoryId, ct);

        return Ok(results);
    }

    // GET: api/products/{id:guid}
    // <snippet_GetByID>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ProductDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var product = await _productService.GetByIdAsync(id, ct);
        if (product is null)
            return NotFound();

        return product;
    }
    // </snippet_GetByID>

    // POST: api/products
    // <snippet_Create>
    [HttpPost]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> PostMultiparFormDatatAsync(
        [FromForm] ProductFormDto form,
        CancellationToken ct = default)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);
        
        var productDto = await _productService.AddAsync(form, ct);

        var url = $"{Request.Scheme}://{Request.Host}/api/Products/{productDto.Id}";
        return Created(url, productDto);
    }
    // </snippet_Create>

    // PUT: api/products/{id:guid}
    // <snippet_Update>
    [HttpPut("{id:guid}")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PutAsync([FromRoute] Guid id, [FromForm] ProductFormDto form,
        CancellationToken ct = default)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        if (!await _productService.ExistsAsync(id))
            return NotFound();

        await _productService.UpdateAsync(id, form, ct);

        return NoContent();
    }

    //</snippet_Update>

    // DELETE: api/products/{id:guid}
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var deleted = await _productService.DeleteAsync(id, ct);
        if (!deleted)
            return NotFound();

        return NoContent();
    }
}
