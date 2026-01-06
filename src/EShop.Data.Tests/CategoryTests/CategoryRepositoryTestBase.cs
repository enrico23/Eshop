using EShop.Shared.Testing;

namespace EShop.Data.Tests.CategoryTests;

public abstract class CategoryRepositoryTestBase 
    : RepositoryTestBase<Category, CategoryRepository, CatalogDbContext, EfUnitOfWork>
{
    protected override CategoryRepository CreateRepository(CatalogDbContext context) =>
        new(context);

    protected override void RegisterEntityFactory(IFixture fixture) =>
        fixture.Register(() => new Category(fixture.Create<Guid>(), fixture.Create<string>()));

    protected override EfUnitOfWork CreateUnitOfWOrk(CatalogDbContext context) =>
         new(context);
    
    protected override CatalogDbContext CreateContext() =>
        EfContextHelpers.CreateContext();
}