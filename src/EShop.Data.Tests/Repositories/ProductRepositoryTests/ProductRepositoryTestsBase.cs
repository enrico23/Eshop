namespace EShop.Data.Tests.Repositories.ProductRepositoryTests;

public abstract class ProductRepositoryTestsBase
{
    protected static IFixture CreateFixture()
    {
        var fixture = new Fixture();

        fixture.Register(() =>
            Product.Create(
                name: $"Product {fixture.Create<int>()}",
                description: new Domain.Products.ProductDescription(
                    fixture.Create<string>()),
                price: new Domain.Money(
                    amount: fixture.Create<decimal>() % 1000 + 1),
                stock: new StockQuantity(
                    fixture.Create<int>() % 100 + 1),
                sku: new Sku(
                    $"SKU-{fixture.Create<Guid>()}"),
                categoryId: fixture.Create<Guid>()
            )
        );

        return fixture;
    }

    protected static CatalogDbContext CreateContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<CatalogDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .EnableSensitiveDataLogging()
            .Options;

        return new CatalogDbContext(options);
    }

    protected static async Task SeedAsync(CatalogDbContext context, IEnumerable<Product> products)
    {
        context.Products.AddRange(products);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();
    }
}
