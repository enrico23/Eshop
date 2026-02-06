using EShop.Shared.Testing;

namespace EShop.Data.Tests.Repositories.CategoryTests;

public abstract class CategoryRepositoryTestBase 
    : RepositoryTestBase<Category, ExtendedCategoryRepository, CatalogDbContext, EfUnitOfWork>
{
    protected override ExtendedCategoryRepository CreateRepository(CatalogDbContext context) =>
        new(context);

    protected override void RegisterEntityFactory(IFixture fixture) =>
        fixture.Register(() => Category.Create(fixture.Create<Guid>(), fixture.Create<string>()));

    protected override EfUnitOfWork CreateUnitOfWOrk(CatalogDbContext context) =>
         new(context);
    
    protected override CatalogDbContext CreateContext() =>
        EfContextHelpers.CreateContext();
}