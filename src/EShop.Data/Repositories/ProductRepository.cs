using EShop.Domain.Catalog;

namespace EShop.Data.Repositories;

public interface IProductRepository
{
    /// <summary>
    /// Get Products Paged
    /// </summary>
    /// <param name="page"></param>
    /// <param name="pageSize"></param>
    /// <param name="orderBy"></param>
    /// <param name="orderDirection"></param>
    /// <param name="filter"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task<Shared.Representations.PagedResult<Product>> GetPagedAsync(
          int page, int pageSize, string orderBy, string orderDirection, 
          string? filter, 
          CancellationToken ct);

    /// <summary>
    /// Product exists
    /// </summary>
    /// <param name="id"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task<bool> ExistsAsync(Guid id, CancellationToken ct);

    /// <summary>
    /// Get Product by Id
    /// </summary>
    /// <remarks>
    /// Track with EF Core
    /// </remarks>
    /// <param name="id"></param>
    /// <param name="track"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task<Product?> GetByIdAsync(Guid id, bool track = false, CancellationToken ct = default);

    Task<bool> NameExistsAsync(string name, CancellationToken ct);

    Task<bool> SkuInUseAsync(Sku sku, Guid? excludeId, CancellationToken ct);

    Task AddAsync(Product product, CancellationToken ct);

    void Remove(Product product);
}

public sealed class ProductRepository : 
    EfRepository<Product, Guid, CatalogDbContext>, IProductRepository
{
    private readonly CatalogDbContext _context;

    public ProductRepository(CatalogDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<Shared.Representations.PagedResult<Product>> GetPagedAsync(
          int page, int pageSize, string orderBy, string orderDirection, string? filter, CancellationToken ct)
    {
        IQueryable<Product> query = _context.Products.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(filter))
            query = query.Where("Name.ToLower().Contains(@0)", filter.ToLower());

        if (!string.IsNullOrWhiteSpace(orderBy))
            query = query.OrderBy($"{orderBy} {orderDirection}");

        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new Shared.Representations.PagedResult<Product>(items, page, pageSize, total);
    }

    public Task<bool> ExistsAsync(Guid id, CancellationToken ct) =>
          _context.Products.AsNoTracking().AnyAsync(p => p.Id == id, ct);

    public Task<bool> NameExistsAsync(string name, CancellationToken ct) =>
         _context.Products.AnyAsync(p => p.Name.ToLower() == name.ToLower(), ct);

    public Task<bool> SkuInUseAsync(Sku sku, Guid? excludeId, CancellationToken ct) =>
         _context.Products.AnyAsync(p =>
             p.Sku == sku && (!excludeId.HasValue || p.Id != excludeId.Value), ct);

    public Task AddAsync(Product product, CancellationToken ct) =>
         _context.Products.AddAsync(product, ct).AsTask();

    public void Remove(Product product) => 
        _context.Products.Remove(product);
}
