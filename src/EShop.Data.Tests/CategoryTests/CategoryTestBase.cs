using EShop.Shared.Testing;

namespace EShop.Data.Tests.CategoryTests;

public abstract class CategoryTestBase
{
    public IFixture Fixture { get; }

    protected CategoryTestBase()
    {
        Fixture = CreateFixture();
    }

    protected static async Task<CategoryRepository> GetSutAsync(IEnumerable<Category>? entities)
    {
        var fixture = CreateFixture();
        var context = EfContextHelpers.CreateContext();
        var categories = entities ?? [.. fixture.CreateMany<Category>()];
        await context.SeedAsync(categories);
        return new CategoryRepository(context);
    }

    private static IFixture CreateFixture()
    {
        var fixture = new Fixture();

        fixture.Register(() =>
            new Category(fixture.Create<Guid>(), fixture.Create<string>())
        );

        return fixture;
    }
}