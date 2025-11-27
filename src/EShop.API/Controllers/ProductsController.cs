using EShop.API.Services;
using EShop.Shared.Representations;
using EShop.Shared.Representations.Product;
using Microsoft.AspNetCore.Mvc;

namespace EShop.API.Controllers;


[Route("api/[controller]")]
[ApiController]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    // GET: api/products?page=1&pageSize=20
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ProductDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult> GetPagedAsync(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        [FromQuery] string orderBy = "Name", [FromQuery] string orderDirection = "asc", 
        [FromQuery] string? filter = null, CancellationToken ct = default)
    {
        var results = await _productService.GetPagedAsync(page, pageSize, orderBy, orderDirection, filter, ct);

        return Ok(results);
    }

    // GET: api/products/5
    // <snippet_GetByID>
    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ProductDto>> GetByIdAsync(int id, CancellationToken ct = default)
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
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProductDto>> PostAsync(CreateProductDto createProductDto, CancellationToken ct = default)
    {
        if (createProductDto is null)
            return BadRequest();

        var productDto = await _productService.AddAsync(createProductDto, ct);
        var url = $"{Request.Scheme}://{Request.Host}/api/Products/{productDto.Id}";
        return Created(url, productDto);
    }
    // </snippet_Create>

    // PUT: api/products/5
    // <snippet_Update>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PutAsync(int id, UpdateProductDto updateProductDto, CancellationToken ct = default)
    {
        if (updateProductDto is null)
            return BadRequest();

        if (!await _productService.ExistsAsync(id))
            return NotFound();

        await _productService.UpdateAsync(id, updateProductDto, ct);

        return NoContent();
    }
    // </snippet_Update>

    // DELETE: api/products/5
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteAsync(int id, CancellationToken ct = default)
    {
        var deleted = await _productService.DeleteAsync(id, ct);
        if (!deleted)
            return NotFound();

        return NoContent();
    }
}
