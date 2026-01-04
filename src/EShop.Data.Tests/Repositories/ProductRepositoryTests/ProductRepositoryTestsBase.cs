
namespace EShop.Data.Tests.Repositories.ProductRepositoryTests;

public abstract class ProductRepositoryTestsBase
{
    protected async Task TestAsync(
        int seedCount,
        Func<TestScope, ValueTask> testBody) => await
        TestScope.CreateAsync(
            seedFactory: f => f.CreateMany<Product>(seedCount),
            testBody: testBody);

    protected async Task TestAsync(
        IEnumerable<Product> seed,
        Func<TestScope, ValueTask> testBody) => await
        TestScope.CreateAsync(
            seedFactory: _ => seed,
            testBody: testBody);

    protected Task Test(
        int seedCount,
        Action<TestScope> testBody) =>
        TestAsync(seedCount, scope =>
        {
            testBody(scope);
            return ValueTask.CompletedTask;
        });

    protected Task Test(
        IEnumerable<Product> seed,
        Action<TestScope> testBody) =>
        TestAsync(seed, scope =>
        {
            testBody(scope);
            return ValueTask.CompletedTask;
        });

    
    protected sealed class TestScope : IAsyncDisposable
    {
        private readonly CatalogDbContext _context;

        private TestScope(
            CatalogDbContext context,
            IFixture fixture,
            List<Product> products)
        {
            _context = context;
            Fixture = fixture;
            Products = products;
            Sut = new ProductRepository(context);
        }

      
        public ProductRepository Sut { get; }

        public IFixture Fixture { get; }

        public IReadOnlyList<Product> Products { get; }

        
        public static async ValueTask CreateAsync(
            Func<IFixture, IEnumerable<Product>> seedFactory,
            Func<TestScope, ValueTask> testBody)
        {
            var fixture = CreateFixture();

            await using var context = CreateContext();

            var products = seedFactory(fixture)?.ToList() ?? new List<Product>();

            if (products.Count > 0)
                await SeedAsync(context, products).ConfigureAwait(false);

            await using var scope = new TestScope(context, fixture, products);
            await testBody(scope).ConfigureAwait(false);
        }

        public ValueTask DisposeAsync() =>
            _context.DisposeAsync();

        private static IFixture CreateFixture()
        {
            var fixture = new Fixture();

            fixture.Register(() =>
                Product.Create(
                    name: $"Product {fixture.Create<int>()}",
                    description: new Domain.Products.ProductDescription(
                        fixture.Create<string>()),
                    price: new Domain.Money(
                        amount: (fixture.Create<decimal>() % 1000m) + 1m),
                    stock: new StockQuantity(
                        (fixture.Create<int>() % 100) + 1),
                    sku: new Sku(
                        $"SKU-{fixture.Create<Guid>()}"),
                    categoryId: fixture.Create<Guid>()
                )
            );

            return fixture;
        }

        private static CatalogDbContext CreateContext()
        {
            var dbName = $"CatalogTests_{Guid.NewGuid():N}";

            var options = new DbContextOptionsBuilder<CatalogDbContext>()
                .UseInMemoryDatabase(dbName)
                .EnableSensitiveDataLogging()
                .Options;

            return new CatalogDbContext(options);
        }

        private static async Task SeedAsync(
            CatalogDbContext context,
            IEnumerable<Product> products)
        {
            context.Products.AddRange(products);
            await context.SaveChangesAsync().ConfigureAwait(false);
            context.ChangeTracker.Clear();
        }
    }
}
