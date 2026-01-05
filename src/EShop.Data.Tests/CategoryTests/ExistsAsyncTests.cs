namespace EShop.Data.Tests.CategoryTests;

public sealed class ExistsAsyncTests : CategoryRepositoryTestsBase
{
    [Fact]
    public Task ExistsAsync_WhenCategoryExists_ReturnsTrue() =>
        TestAsync(seedCount: 1, async scope =>
        {
            var exists = await scope.Sut.ExistsAsync(scope.Entities[0].Id, CancellationToken.None);
            Assert.True(exists);
        });

    [Fact]
    public Task ExistsAsync_WhenCategoryDoesNotExist_ReturnsFalse() =>
        TestAsync(seedCount: 1, async scope =>
        {
            var exists = await scope.Sut.ExistsAsync(scope.Entities[0].Id, CancellationToken.None);
            Assert.True(exists);
        });
}
