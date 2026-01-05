
using EShop.Shared.Testing;

namespace EShop.Data.Tests.Repositories.ProductRepositoryTests;

public abstract class ProductRepositoryTestsBase
{
    protected async Task TestAsync(
        int seedCount,
        Func<TestScope, Task> testBody) => await
        TestScope.CreateAsync(
            seedFactory: f => f.CreateMany<Product>(seedCount),
            testBody: testBody);

    protected async Task TestAsync(
        IEnumerable<Product> seed,
        Func<TestScope, Task> testBody) => await
        TestScope.CreateAsync(
            seedFactory: _ => seed,
            testBody: testBody);

    protected Task Test(
        int seedCount,
        Action<TestScope> testBody) =>
        TestAsync(seedCount, scope =>
        {
            testBody(scope);
            return Task.CompletedTask;
        });

    protected Task Test(
        IEnumerable<Product> seed,
        Action<TestScope> testBody) =>
        TestAsync(seed, scope =>
        {
            testBody(scope);
            return Task.CompletedTask;
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

        
        public static async Task CreateAsync(
            Func<IFixture, IEnumerable<Product>> seedFactory,
            Func<TestScope, Task> testBody)
        {
            var fixture = CreateFixture();

            await using var context = EfContextHelpers.CreateContext();

            var products = seedFactory(fixture)?.ToList() ?? new List<Product>();

            if (products.Count > 0)
                await context.SeedAsync(products).ConfigureAwait(false);

            await using var scope = new TestScope(context, fixture, products);
            await testBody(scope);
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
    }
}
