namespace EShop.Data.Tests.CategoryTests;

public abstract class CategoryRepositoryTestsBase 
    : RepositoryTestsBase<Category, CategoryRepository>
{
    protected override CategoryRepository CreateSut(CatalogDbContext context) =>
        new(context);

    protected override void RegisterEntityFactory(IFixture fixture)
    {
        fixture.Register(() => new Category(fixture.Create<Guid>(), fixture.Create<string>()));
    }
}