using EShop.Domain;

namespace EShop.Data.Repositories;

public interface IRepository<TEntity, TKey, TDbContext> 
    where TEntity : class, IEntity<TKey>
    where TDbContext : DbContext
{
    Task<TEntity?> GetByIdAsync(TKey id, bool track = false, CancellationToken ct = default);
}

public class EfRepository<TEntity, TKey, TDbContext> : IRepository<TEntity, TKey, TDbContext>
    where TEntity : class, IEntity<TKey>
    where TDbContext : DbContext
{
    private readonly DbSet<TEntity> _set;

    public EfRepository(TDbContext dbContext)
    {
        _set = dbContext.Set<TEntity>();
    }

    public Task<TEntity?> GetByIdAsync(TKey id, bool track = false, CancellationToken ct = default)
    {
        if (track)
            return _set.FindAsync([id], ct).AsTask();

        return _set.AsNoTracking()
             .FirstOrDefaultAsync(e => e.Id != null && e.Id.Equals(id), ct);
    }
}