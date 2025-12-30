namespace EShop.Data.Tests.Repositories.ProductRepositoryTests;

public sealed class ExistsAsyncTests : ProductRepositoryTestsBase
{
    [Fact]
    public Task ExistsAsync_WhenProductExists_ReturnsTrue() =>
        TestAsync(seedCount: 1, async t =>
        {
            var exists = await t.Sut.ExistsAsync(t.Products[0].Id, CancellationToken.None);
            Assert.True(exists);
        }).AsTask();

    [Fact]
    public Task ExistsAsync_WhenProductDoesNotExist_ReturnsFalse() =>
        TestAsync(seedCount: 1, async t =>
        {
            var exists = await t.Sut.ExistsAsync(t.Products[0].Id, CancellationToken.None);
            Assert.True(exists);
        }).AsTask();
}
