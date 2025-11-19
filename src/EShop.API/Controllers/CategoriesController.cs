using EShop.API.Representations;
using EShop.API.Representations.Brand;
using EShop.API.Representations.Category;
using EShop.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace EShop.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        // GET: api/categories?page=1&pageSize=20
        [HttpGet]
        [ProducesResponseType(typeof(PaginatedResult<CategoryDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAsync([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
        {
            var categories = await _categoryService.GetPagedAsync(page, pageSize,ct);

            return Ok(categories);
        }

        // GET: api/categories/5
        // <snippet_GetByID>
        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<CategoryDto>> GetProductAsync(int id, CancellationToken ct = default)
        {
            var category = await _categoryService.GetByIdAsync(id, ct);
            if (category is null)
                return NotFound();

            return category;
        }
        // </snippet_GetByID>

        // PUT: api/categories/5
        // <snippet_Update>
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PutAsync(int id, UpdateCategoryDto updateCategoryDto, CancellationToken ct = default)
        {
            if (updateCategoryDto is null)
                return BadRequest();

            if (!await _categoryService.ExistsAsync(id))
                return NotFound();

            await _categoryService.UpdateAsync(id, updateCategoryDto, ct);

            return NoContent();
        }
        // </snippet_Update>

        // POST: api/categories
        // <snippet_Create>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<CategoryDto>> PostProductAsync(CreateCategoryDto createCategoryDto, CancellationToken ct = default)
        {
            if (createCategoryDto is null)
                return BadRequest();

            var categoryDto = await _categoryService.AddAsync(createCategoryDto, ct);
            var url = $"{Request.Scheme}://{Request.Host}/api/Products/{categoryDto.Id}";
            return Created(url, categoryDto);
        }
        // </snippet_Create>

        // DELETE: api/categories/5
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> DeleteAsync(int id, CancellationToken ct = default)
        {
            var deleted = await _categoryService.DeleteAsync(id, ct);
            if (!deleted)
                return NotFound();

            return NoContent();
        }
    }
}
