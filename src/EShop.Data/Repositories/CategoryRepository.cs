namespace EShop.Data.Repositories;

public interface ICategoryRepository
{
    Task<ICollection<Category>> GetAllAsync(CancellationToken ct = default);

    Task<bool> HasAnyProductsAsync(Guid id, CancellationToken ct);
}

public sealed class CategoryRepository :
    EfRepository<Category, Guid, CatalogDbContext>,
    ICategoryRepository
{
    private readonly CatalogDbContext _context;

    public CategoryRepository(CatalogDbContext context) : base(context) => _context = context;

    public async Task<ICollection<Category>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.Categories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ToListAsync(ct);
    }

    public async Task<bool> HasAnyProductsAsync(Guid id, CancellationToken ct = default) =>
        await _context.Products.AnyAsync(p => p.CategoryId == id);
}