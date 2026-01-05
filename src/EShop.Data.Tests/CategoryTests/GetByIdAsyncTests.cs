namespace EShop.Data.Tests.CategoryTests;

public sealed class GetByIdAsyncTests : CategoryTestBase
{
    [Fact]
    public async Task GetByIdAsync_WhenFound_ReturnsCategories()
    {
        var categories = Fixture.CreateMany<Category>().ToList();
        var expectedId = categories.First().Id;
        var categoryRepository = await GetSutAsync(categories);

        var result = await categoryRepository.GetByIdAsync(expectedId);

        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetByIdAsync_WhenCategoryNotFound_ReturnsNull()
    {
        var categoryRepository = await GetSutAsync(null);
        
        var result = await categoryRepository.GetByIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }
}
