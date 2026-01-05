using EShop.Data;

namespace EShop.Shared.Testing;

internal static class EfContextHelpers
{
    internal static CatalogDbContext CreateContext()
    {
        var dbName = $"CatalogTests_{Guid.NewGuid():N}";

        var options = new DbContextOptionsBuilder<CatalogDbContext>()
            .UseInMemoryDatabase(dbName)
            .EnableSensitiveDataLogging()
            .Options;

        return new CatalogDbContext(options);
    }

    internal static async Task SeedAsync<T>(
           this CatalogDbContext context,
           IEnumerable<T> entities) where T : class
    {
        context.Set<T>().AddRange(entities);
        await context.SaveChangesAsync().ConfigureAwait(false);
        context.ChangeTracker.Clear();
    }
}
