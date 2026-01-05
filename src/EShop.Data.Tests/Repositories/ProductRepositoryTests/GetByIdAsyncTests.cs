namespace EShop.Data.Tests.Repositories.ProductRepositoryTests;

public sealed class GetByIdAsyncTests : ProductRepositoryTestsBase
{
    [Fact]
    public Task GetByIdAsync_WhenFound_ReturnsProduct() =>
        TestAsync(seedCount: 1, async scope =>
        {
            var expected = scope.Entities[0];
            var product = await scope.Sut.GetByIdAsync(expected.Id, false, CancellationToken.None);
            Assert.NotNull(product);
            Assert.Equal(expected.Id, product!.Id);
            Assert.Equal(expected.Name, product.Name);
        });

    [Fact]
    public Task GetByIdAsync_WhenNotFound_ReturnsNull() =>
     TestAsync(seedCount: 1, async scope =>
        {
            var product = await scope.Sut.GetByIdAsync(Guid.NewGuid(), false, CancellationToken.None);
            Assert.Null(product);
        });
}
