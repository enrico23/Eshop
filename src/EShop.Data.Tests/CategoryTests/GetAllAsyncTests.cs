namespace EShop.Data.Tests.CategoryTests;

public sealed class GetAllAsyncTests : CategoryRepositoryTestBase
{
    [Fact]
    public async Task GetAllCategoriesAsync_Success()
    {
        await TestAsync(25, async scope =>
        {
            var result = await scope.Sut.GetAllAsync();
            Assert.NotNull(result);
            Assert.Equal(25, result.Count);
        });
    }
}
