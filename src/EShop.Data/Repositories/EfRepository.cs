namespace EShop.Data.Repositories;

public class EfRepository<TEntity, TKey, TDbContext> : IRepository<TEntity, TKey, TDbContext>
    where TEntity : class, IEntity<TKey>
    where TDbContext : DbContext
{
    private readonly DbSet<TEntity> _set;

    public EfRepository(TDbContext dbContext) => _set = dbContext.Set<TEntity>();

    public async Task<Shared.Representations.PagedResult<TEntity>> GetPagedAsync(
       int page, int pageSize, string orderBy, string orderDirection, string? filter,
       CancellationToken ct = default)
    {
        IQueryable<TEntity> query = _set.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(filter))
            query = query.Where("Name.ToLower().Contains(@0)", filter.ToLower());

        if (!string.IsNullOrWhiteSpace(orderBy))
            query = query.OrderBy($"{orderBy} {orderDirection}");

        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new Shared.Representations.PagedResult<TEntity>(items, page, pageSize, total);
    }

    public Task<TEntity?> GetByIdAsync(TKey id, bool track = false, CancellationToken ct = default)
    {
        if (track)
            return _set.FindAsync([id], ct).AsTask();

        return _set.AsNoTracking()
             .FirstOrDefaultAsync(e => e.Id != null && e.Id.Equals(id), ct);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken ct = default) =>
         await _set.AnyAsync(e=> e.Id != null && e.Id.Equals(id), ct);

    public Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default)
       => _set.AsNoTracking().AnyAsync(predicate, ct);

    public Task AddAsync(TEntity entity, CancellationToken ct = default)
        => _set.AddAsync(entity, ct).AsTask();

    public void Remove(TEntity entity)
        => _set.Remove(entity);
}