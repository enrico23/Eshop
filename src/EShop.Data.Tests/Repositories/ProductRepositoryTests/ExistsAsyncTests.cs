namespace EShop.Data.Tests.Repositories.ProductRepositoryTests;

public sealed class ExistsAsyncTests : ProductRepositoryTestsBase
{
    [Fact]
    public Task ExistsAsync_WhenProductExists_ReturnsTrue() =>
        TestAsync(seedCount: 1, async scope =>
        {
            var exists = await scope.Sut.ExistsAsync(scope.Entities[0].Id, CancellationToken.None);
            Assert.True(exists);
        });

    [Fact]
    public Task ExistsAsync_WhenProductDoesNotExist_ReturnsFalse() =>
        TestAsync(seedCount: 1, async scope =>
        {
            var exists = await scope.Sut.ExistsAsync(scope.Entities[0].Id, CancellationToken.None);
            Assert.True(exists);
        });
}
