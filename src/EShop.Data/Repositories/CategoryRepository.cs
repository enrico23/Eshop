namespace EShop.Data.Repositories;

public interface ICategoryRepository
{
    Task<Shared.Representations.PagedResult<Category>> GetPagedAsync(
          int page, int pageSize, string orderBy, string orderDirection,
          string? filter,
          CancellationToken ct);

    Task<ICollection<Category>> GetAllAsync(CancellationToken ct = default);

    Task AddAsync(Category category, CancellationToken ct = default);

    void Remove(Category category);

    Task<bool> HasAnyProductsAsync(Guid id, CancellationToken ct);
}

public sealed class CategoryRepository :
    EfRepository<Category, Guid, CatalogDbContext>,
    ICategoryRepository
{
    private readonly CatalogDbContext _context;

    public CategoryRepository(CatalogDbContext context) : base(context) => _context = context;

    public async Task<Shared.Representations.PagedResult<Category>> GetPagedAsync(
        int page, int pageSize, string orderBy, string orderDirection, string? filter, 
        CancellationToken ct)
    {
        IQueryable<Category> query = _context.Categories.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(filter))
            query = query.Where("Name.ToLower().Contains(@0)", filter.ToLower());

        if (!string.IsNullOrWhiteSpace(orderBy))
            query = query.OrderBy($"{orderBy} {orderDirection}");

        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new Shared.Representations.PagedResult<Category>(items, page, pageSize, total);
    }

    public async Task<ICollection<Category>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.Categories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ToListAsync(ct);
    }

    public Task AddAsync(Category category, CancellationToken ct = default) =>
        _context.Categories.AddAsync(category, ct).AsTask();

    public void Remove(Category category) =>
        _context.Categories.Remove(category);

    public async Task<bool> HasAnyProductsAsync(Guid id, CancellationToken ct = default) =>
        await _context.Products.AnyAsync(p => p.CategoryId == id);
}