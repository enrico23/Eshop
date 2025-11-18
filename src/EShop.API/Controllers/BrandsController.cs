using EShop.API.Representations;
using EShop.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace EShop.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public partial class BrandsController : Controller
{
    private readonly IBrandService _brandService;

    public BrandsController(IBrandService brandService)
    {
        _brandService = brandService;
    }

    // GET: api/products?page=1&pageSize=20
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResult<BrandDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult> GetAsync([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var results = await _brandService.GetPagedAsync(page, pageSize, ct);

        return Ok(results);
    }

    // GET: api/brands/5
    // <snippet_GetByID>
    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(BrandDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<BrandDto>> GetProductAsync(int id, CancellationToken ct = default)
    {
        var brand = await _brandService.GetByIdAsync(id, ct);
        if (brand is null)
            return NotFound();

        return brand;
    }
    // </snippet_GetByID>

    // PUT: api/brands/5
    // <snippet_Update>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PutAsync(int id, UpdateBrandDto updateBrandDto, CancellationToken ct = default)
    {
        if (updateBrandDto is null)
            return BadRequest();

        if (!await _brandService.BrandExistsAsync(id))
            return NotFound();

        await _brandService.UpdateAsync(id, updateBrandDto, ct);
        
        return NoContent();
    }
    // </snippet_Update>

    // POST: api/brands
    // <snippet_Create>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<BrandDto>> PostProductAsync(CreateBrandDto createBrandDto, CancellationToken ct = default)
    {
        if (createBrandDto is null)
            return BadRequest();

        var productDto = await _brandService.AddAsync(createBrandDto, ct);
        var url = $"{Request.Scheme}://{Request.Host}/api/Products/{productDto.Id}";
        return Created(url, productDto);
    }
    // </snippet_Create>

    // DELETE: api/brands/5
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteTodoItem(int id, CancellationToken ct = default)
    {
        var deleted = await _brandService.DeleteAsync(id, ct);
        if(!deleted)
            return NotFound();

        return NoContent();
    }

}
