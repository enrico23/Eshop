
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
            IReadOnlyList<Product> products,
            ProductRepository sut,
            IUnitOfWork uow)
        {
            _context = context;
            Fixture = fixture;
            Products = products;
            Sut = sut;
            Uow = uow;
        }

        public ProductRepository Sut { get; }

        public IUnitOfWork Uow { get; }

        public IFixture Fixture { get; }

        public IReadOnlyList<Product> Products { get; }

        public Task SaveChangesAsync(CancellationToken none) => Uow.SaveChangesAsync();


        public static async Task CreateAsync(
            Func<IFixture, IEnumerable<Product>> seedFactory,
            Func<TestScope, Task> testBody)
        {
            var context = EfContextHelpers.CreateContext();
            var fixture = CreateFixture();

            try
            {
                var products = seedFactory(fixture)?.ToList() ?? new List<Product>();

                if (products.Count > 0)
                    await context.SeedAsync(products).ConfigureAwait(false);

                var uow = new EfUnitOfWork(context);
                var sut = new ProductRepository(context);

                await using var scope = new TestScope(context, fixture, products, sut, uow);
                await testBody(scope);
            }
            catch (Exception)
            {
                await context.DisposeAsync();
                throw;
            }
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
