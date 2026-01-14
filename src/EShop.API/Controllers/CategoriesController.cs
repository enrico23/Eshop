using EShop.API.Services;
using EShop.Shared.Representations;
using EShop.Shared.Representations.Category;
using Microsoft.AspNetCore.Authorization;
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
        [AllowAnonymous]
        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<CategoryDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPagedAsync(
            [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
            [FromQuery] string orderBy = "Name", [FromQuery] string orderDirection = "asc", 
            [FromQuery] string? filter = null, CancellationToken ct = default)
        {
            var categories = await _categoryService.GetPagedAsync(page, pageSize, orderBy, orderDirection, filter, ct);

            return Ok(categories);
        }

        // GET: api/categories/all
        // <snippet_GetAll>
        [AllowAnonymous]
        [HttpGet("all")]
        [ProducesResponseType(typeof(ICollection<CategoryDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult> GetAllAsync(CancellationToken ct = default)
        {
            var results = await _categoryService.GetAllAsync(ct);

            return Ok(results);
        }
        // </snippet_GetAll>

        // GET: api/categories/{id:guid}
        // <snippet_GetByID>
        [AllowAnonymous]
        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<CategoryDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            var category = await _categoryService.GetByIdAsync(id, ct);
            if (category is null)
                return NotFound();

            return category;
        }
        // </snippet_GetByID>

        // PUT: api/categories/{id:guid}
        // <snippet_Update>
        [Authorize]
        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PutAsync(Guid id, UpdateCategoryDto updateCategoryDto, CancellationToken ct = default)
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
        [Authorize]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<CategoryDto>> PostAsync(CreateCategoryDto createCategoryDto, CancellationToken ct = default)
        {
            if (createCategoryDto is null)
                return BadRequest();

            var categoryDto = await _categoryService.AddAsync(createCategoryDto, ct);
            var url = $"{Request.Scheme}://{Request.Host}/api/Categories/{categoryDto.Id}";
            return Created(url, categoryDto);
        }
        // </snippet_Create>

        // DELETE: api/categories/{id:guid}
        [Authorize]
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var deleted = await _categoryService.DeleteAsync(id, ct);
            if (!deleted)
                return NotFound();

            return NoContent();
        }
    }
}
