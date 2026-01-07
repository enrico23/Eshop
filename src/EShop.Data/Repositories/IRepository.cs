using EShop.Domain;

namespace EShop.Data.Repositories;

/// <summary>
/// Generic repository for accessing entities by primary key.
/// </summary>
/// <typeparam name="TEntity">Entity type.</typeparam>
/// <typeparam name="TKey">Primary key type.</typeparam>
public interface IRepository<TEntity, TKey> 
    where TEntity : class, IEntity<TKey>
{
    /// <summary>
    /// Retrieves an entity by its primary key.
    /// </summary>
    /// <param name="id">Primary key value.</param>
    /// <param name="track">
    /// Indicates whether the entity should be tracked by the DbContext.
    /// </param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// The entity if found; otherwise <c>null</c>.
    /// </returns>
    Task<TEntity?> GetByIdAsync(TKey id, bool track = false, CancellationToken ct = default);
}

public class EfRepository<TEntity, TKey> : IRepository<TEntity, TKey>
    where TEntity : class, IEntity<TKey>
{
    private readonly DbSet<TEntity> _set;

    public EfRepository(DbContext dbContext)
    {
        _set = dbContext.Set<TEntity>();
    }

    /// <inheritdoc />
    public Task<TEntity?> GetByIdAsync(TKey id, bool track = false, CancellationToken ct = default)
    {
        if (track)
            return _set.FindAsync([id], ct).AsTask();

        return _set.AsNoTracking()
            .FirstOrDefaultAsync(e => EqualityComparer<TKey>.Default.Equals(e.Id, id), ct);
    }
}