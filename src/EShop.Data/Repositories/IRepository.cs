namespace EShop.Data.Repositories;

public interface IRepository<TEntity, TKey, TDbContext> 
    where TEntity : class, IEntity<TKey>
    where TDbContext : DbContext
{
    Task<Shared.Representations.PagedResult<TEntity>> GetPagedAsync(
       int page, int pageSize, string orderBy, string orderDirection, string? filter,
       CancellationToken ct = default);

    Task<TEntity?> GetByIdAsync(TKey id, bool track = false, CancellationToken ct = default);

    Task<bool> ExistsAsync(Guid id, CancellationToken ct = default);

    Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default);

    public Task AddAsync(TEntity entity, CancellationToken ct = default);

    void Remove(TEntity entity);
}
