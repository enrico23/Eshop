using EShop.Shared.Testing;

namespace EShop.Data.Tests.Repositories.ProductRepositoryTests;

public abstract class ProductRepositoryTestsBase 
    : RepositoryTestBase<Product, ProductRepository, CatalogDbContext, EfUnitOfWork>
{

    protected override ProductRepository CreateSut(CatalogDbContext context) =>
       new(context);

    protected override void RegisterEntityFactory(IFixture fixture)
    {
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
    }

    protected override EfUnitOfWork CreateUnitOfWOrk(CatalogDbContext context)
    {
        return new EfUnitOfWork(context);
    }

    protected override CatalogDbContext CreateContext()
    {
        return EfContextHelpers.CreateContext();
    }
}
