
using EShop.Domain.Catalog;
using EShop.Shared.Representations.Category;

namespace EShop.Data.Repositories;

public interface ICategoryRepository
{
    /// <summary>
    /// Get Categories Paged
    /// </summary>
    /// <param name="page"></param>
    /// <param name="pageSize"></param>
    /// <param name="orderBy"></param>
    /// <param name="orderDirection"></param>
    /// <param name="filter"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task<Shared.Representations.PagedResult<Category>> GetPagedAsync(
          int page, int pageSize, string orderBy, string orderDirection,
          string? filter,
          CancellationToken ct);

    Task<ICollection<Category>> GetAllAsync(CancellationToken ct);

    /// <summary>
    /// Category exists
    /// </summary>
    /// <param name="id"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task<bool> ExistsAsync(Guid id, CancellationToken ct);

    /// <summary>
    /// Get Category by Id
    /// </summary>
    /// <param name="id"></param>
    /// <param name="track"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task<Category?> GetByIdAsync(Guid id, bool track = false, CancellationToken ct = default);

    /// <summary>
    /// Category name aalready exists
    /// </summary>
    /// <param name="name"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task<bool> NameExistsAsync(string name, CancellationToken ct);

    /// <summary>
    /// Add Category to DbContext
    /// </summary>
    /// <param name="category"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task AddAsync(Category category, CancellationToken ct);

    /// <summary>
    /// Remove Category from DbContext
    /// </summary>
    /// <param name="category"></param>
    void Remove(Category category);

    /// <summary>
    /// Has any products
    /// </summary>
    /// <param name="id"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task<bool> HasAnyProductsAsync(Guid id, CancellationToken ct);
}

public sealed class CategoryRepository(CatalogDbContext context) : ICategoryRepository
{
    private readonly CatalogDbContext _context = context;

    public async Task<Shared.Representations.PagedResult<Category>> GetPagedAsync(int page, int pageSize, string orderBy, string orderDirection, string? filter, 
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

    public async Task<ICollection<Category>> GetAllAsync(CancellationToken ct)
    {
        return await _context.Categories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ToListAsync(ct);
    }

    public Task<bool> ExistsAsync(Guid id, CancellationToken ct) =>
         _context.Categories.AsNoTracking().AnyAsync(c => c.Id == id, ct);

    public Task<Category?> GetByIdAsync(Guid id, bool track = false, CancellationToken ct = default) =>
        track
            ? _context.Categories.FirstOrDefaultAsync(c => c.Id == id, ct)
            : _context.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, ct);

    public Task<bool> NameExistsAsync(string name, CancellationToken ct) =>
        _context.Categories.AnyAsync(c => c.Name.ToLower() == name.ToLower(), ct);

    public Task AddAsync(Category category, CancellationToken ct) =>
        _context.Categories.AddAsync(category, ct).AsTask();

    public void Remove(Category category) =>
        _context.Categories.Remove(category);

    public async Task<bool> HasAnyProductsAsync(Guid id, CancellationToken ct) =>
        await _context.Products.AnyAsync(p => p.CategoryId == id);
}