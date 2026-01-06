using EShop.Shared.Testing;

namespace EShop.Data.Tests.CategoryTests;

public abstract class CategoryRepositoryTestsBase 
    : RepositoryTestBase<Category, CategoryRepository, CatalogDbContext, EfUnitOfWork>
{
    protected override CategoryRepository CreateSut(CatalogDbContext context) =>
        new(context);

    protected override void RegisterEntityFactory(IFixture fixture)
    {
        fixture.Register(() => new Category(fixture.Create<Guid>(), fixture.Create<string>()));
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