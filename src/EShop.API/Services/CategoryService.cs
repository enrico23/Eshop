using EShop.Data;
using EShop.Domain.Catalog;
using EShop.Domain.Exceptions;
using EShop.Shared.Representations.Category;
using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;

namespace EShop.API.Services;

public class CategoryService(CatalogDbContext context, IMapper mapper) : ICategoryService
{
    private readonly CatalogDbContext _context = context;

    private readonly IMapper _mapper = mapper;

    public async Task<Shared.Representations.PagedResult<CategoryDto>> GetPagedAsync(
        int page, int pageSize,
        string orderBy = "Name", string orderDirection = "asc", 
        string? filter = null, CancellationToken ct = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 200);

        IQueryable<Category> query = _context.Categories.AsNoTracking();

        if (!string.IsNullOrEmpty(filter))
        {
            query = query.Where("Name.ToLower().Contains(@0)", filter.ToLower());
        }

        if (!string.IsNullOrEmpty(orderBy))
        {
            var ordering = $"{orderBy} {orderDirection}";
            query = query.OrderBy(ordering);
        }

        var total = await query.CountAsync(ct);

        var items = await query
            .ProjectToType<CategoryDto>()
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new Shared.Representations.PagedResult<CategoryDto>(items, page, pageSize, total);
    }

    public async Task<ICollection<CategoryDto>> GetAllAsync(CancellationToken ct)
    {
        return await _context.Categories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ProjectToType<CategoryDto>()
            .ToListAsync(ct);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Categories
            .AsNoTracking()
            .AnyAsync(b => b.Id == id, ct);
    }


    public async Task<CategoryDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Categories
             .AsNoTracking()
             .Where(b => b.Id == id)
             .ProjectToType<CategoryDto>()
             .FirstOrDefaultAsync(ct);
    }

    public async Task<bool> UpdateAsync(Guid id, UpdateCategoryDto updateCategoryDto, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(updateCategoryDto, nameof(updateCategoryDto));

        var entity = await _context.Categories.FirstOrDefaultAsync(b => b.Id == id, ct);
        if (entity is null)
            return false;

        var categoryName = updateCategoryDto.Name?.Trim();
        if (string.IsNullOrWhiteSpace(categoryName))
            throw new DomainException("Category name is required.");

        entity.ChangeName(updateCategoryDto.Name!);

        await _context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<CategoryDto> AddAsync(CreateCategoryDto createCategoryDto, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(createCategoryDto, nameof(createCategoryDto));

        var categoryName = createCategoryDto.Name?.Trim();
        if (string.IsNullOrWhiteSpace(categoryName))
            throw new DomainException("Category name is required.");

        var alreadyExists = await _context.Categories.AnyAsync(c => c.Name.ToLower() == categoryName.ToLower(), ct);
        if (alreadyExists)
            throw new DomainException($"A category with name '{categoryName}' already exists.");

        var category = new Category(categoryName);

        await _context.Categories.AddAsync(category, ct);
        await _context.SaveChangesAsync(ct);

        return category.Adapt<CategoryDto>();
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct)
    {
        var entity = await _context.Categories.FirstOrDefaultAsync(b => b.Id == id, ct);

        if (entity is null) return false;

        var hasProducts = await _context.Products.AnyAsync(p => p.CategoryId == id, ct);
        if (hasProducts)
            throw new DomainException("Category cannot be deleted because it contains products.");

        _context.Categories.Remove(entity);
        await _context.SaveChangesAsync(ct);

        return true;
    }
}
