using EShop.Shared.Testing;

namespace EShop.Data.Tests.CategoryTests;

public sealed class GetByIdTest : 
    CategoryTestBase, IAsyncDisposable
{
    [Fact]
    public async Task GetByIdAsync_WhenFound_ReturnsCategories()
    {
        var categories = Fixture.CreateMany<Category>().ToList();
        var expectedId = categories.First().Id;
        var categoryRepository = await GetSut(categories);

        var result = await categoryRepository.GetByIdAsync(expectedId);

        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetByIdAsync_WhenCategoryNotFound_ReturnsNull()
    {
        var categoryRepository = await GetSut(null);
        
        var result = await categoryRepository.GetByIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }


    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }
}

public abstract class CategoryTestBase
{
    public IFixture Fixture { get; }

    protected CategoryTestBase()
    {
        Fixture = CreateFixture();
    }

    protected async Task<Repository<Category>> GetSut(IEnumerable<Category>? entities)
    {
        var context = EfContextHelpers.CreateContext();
        var categories = entities ?? Fixture.CreateMany<Category>().ToList();
        await context.SeedAsync(categories);
        return new Repository<Category>(context);
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