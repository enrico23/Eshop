namespace EShop.Data.Tests.CategoryTests;

public sealed class GetByIdAsyncTests : CategoryRepositoryTestsBase
{
    [Fact]
    public async Task GetByIdAsync_WhenFound_ReturnsCategories()
    {
        await TestAsync(seedCount: 5, async scope =>
        {
            var expected = scope.Entities[0];
            var expectedId = expected.Id;

            var result = await scope.Sut.GetByIdAsync(expectedId, false, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal(expected.Id, result!.Id);
        });
     }

    [Fact]
    public async Task GetByIdAsync_WhenCategoryNotFound_ReturnsNull()
    {
        await TestAsync(seedCount: 5, async scope =>
        {
            var expected = scope.Entities[0];
            var expectedId = expected.Id;

            var result = await scope.Sut.GetByIdAsync(Guid.NewGuid(), false, CancellationToken.None);

            Assert.Null(result);
        });
    }
}
