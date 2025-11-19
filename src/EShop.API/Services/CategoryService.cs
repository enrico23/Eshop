using EShop.API.Exceptions;
using EShop.API.Representations;
using EShop.API.Representations.Brand;
using EShop.API.Representations.Category;
using EShop.Data;
using EShop.Domain;
using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;

namespace EShop.API.Services;

public class CategoryService : ICategoryService
{
    private readonly CatalogDbContext _context;

    private readonly IMapper _mapper;

    public CategoryService(CatalogDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PaginatedResult<CategoryDto>> GetPagedAsync(int page, int pageSize, CancellationToken ct = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 200);

        var query = _context.Categories.AsNoTracking().OrderBy(b => b.Name);

        var total = await query.LongCountAsync(ct);

        var items = await query
            .ProjectToType<CategoryDto>()
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PaginatedResult<CategoryDto>(items, page, pageSize, total);
    }

    public async Task<bool> ExistsAsync(int id, CancellationToken ct = default)
    {
        return await _context.Categories
            .AsNoTracking()
            .AnyAsync(b => b.Id == id, ct);
    }


    public async Task<CategoryDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _context.Categories
             .AsNoTracking()
             .Where(b => b.Id == id)
             .ProjectToType<CategoryDto>()
             .FirstOrDefaultAsync(ct);
    }

    public async Task<bool> UpdateAsync(int id, UpdateCategoryDto updateCategoryDto, CancellationToken ct)
    {
        if (updateCategoryDto is null)
            throw new ArgumentNullException(nameof(updateCategoryDto));

        var entity = await _context.Categories.FirstOrDefaultAsync(b => b.Id == id, ct);
        if (entity is null)
            return false;

        updateCategoryDto.Adapt(entity);

        await _context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<CategoryDto> AddAsync(CreateCategoryDto createCategoryDto, CancellationToken ct)
    {
        if (createCategoryDto is null)
            throw new ArgumentNullException(nameof(createCategoryDto));

        var alreadyExists = await _context.Brands.AnyAsync(b => b.Name.ToLower() == createCategoryDto.Name.ToLower(), ct);
        if (alreadyExists)
            throw new DomainException($"A brand with name '{createCategoryDto.Name}' already exists.");

        var name = createCategoryDto.Name?.Trim();
        var category = _mapper.From(createCategoryDto).AdaptToType<Category>();

        await _context.Categories.AddAsync(category, ct);
        await _context.SaveChangesAsync(ct);

        return category.Adapt<CategoryDto>();
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct)
    {
        var entity = await _context.Categories.FirstOrDefaultAsync(b => b.Id == id, ct);

        if (entity is null)
            return false;

        _context.Categories.Remove(entity);
        await _context.SaveChangesAsync(ct);

        return true;
    }
}
