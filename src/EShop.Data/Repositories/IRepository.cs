namespace EShop.Data.Repositories;

public interface IRepository<TEntity, TKey, TDbContext> 
    where TEntity : class, IEntity<TKey>
    where TDbContext : DbContext
{
    Task<TEntity?> GetByIdAsync(TKey id, bool track = false, CancellationToken ct = default);

    Task<bool> ExistsAsync(Guid id, CancellationToken ct = default);

    Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default);

    public Task AddAsync(TEntity entity, CancellationToken ct = default);

    void Remove(TEntity entity);
}

public class EfRepository<TEntity, TKey, TDbContext> : IRepository<TEntity, TKey, TDbContext>
    where TEntity : class, IEntity<TKey>
    where TDbContext : DbContext
{
    private readonly DbSet<TEntity> _set;

    public EfRepository(TDbContext dbContext) => _set = dbContext.Set<TEntity>();

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