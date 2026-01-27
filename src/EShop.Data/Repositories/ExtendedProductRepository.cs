namespace EShop.Data.Repositories;

public interface IExtendedProductRepository
{
    Task<Shared.Representations.PagedResult<Product>> GetPagedAsync(
          int page, int pageSize, string orderBy, string orderDirection, 
          string? filter, Guid? categoryId,
          CancellationToken ct = default);

    Task<bool> SkuInUseAsync(Sku sku, Guid? excludeId, CancellationToken ct);
}

public sealed class ExtendedProductRepository : 
    EfRepository<Product, Guid, CatalogDbContext>, IExtendedProductRepository
{
    private readonly CatalogDbContext _context;

    public ExtendedProductRepository(CatalogDbContext context) : base(context) => _context = context;

    public async Task<Shared.Representations.PagedResult<Product>> GetPagedAsync(
          int page, int pageSize, string orderBy, string orderDirection, string? filter, Guid? categoryId, CancellationToken ct = default)
    {
        IQueryable<Product> query = _context.Products.AsNoTracking();

        if (categoryId != null)
            query = query.Where(p => p.CategoryId == categoryId);

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

    public Task<bool> SkuInUseAsync(Sku sku, Guid? excludeId, CancellationToken ct) =>
         _context.Products.AnyAsync(p =>
             p.Sku == sku && (!excludeId.HasValue || p.Id != excludeId.Value), ct);
}
