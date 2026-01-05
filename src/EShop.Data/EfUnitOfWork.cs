namespace EShop.Data;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}

public class EfUnitOfWork : IUnitOfWork
{
    private readonly CatalogDbContext _db;
    public EfUnitOfWork(CatalogDbContext db) => _db = db;
    public Task<int> SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}