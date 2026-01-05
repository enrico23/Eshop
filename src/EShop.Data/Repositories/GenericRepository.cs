
using EShop.Domain.Catalog;

namespace EShop.Data.Repositories;

public interface IRepository<T>
{
    Task<T?> GetByIdAsync(Guid id, bool track = false, CancellationToken ct = default);
}

public sealed class Repository<T>(CatalogDbContext context)
    : IRepository<T> where T : class
{
    public async Task<T?> GetByIdAsync(Guid id, bool track = false, CancellationToken ct = default)
    {
        DbSet<T> result = context.Set<T>();
        return await result.FindAsync(id);
    }
}
