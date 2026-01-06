namespace EShop.Data.Tests.Repositories.ProductRepositoryTests;

public sealed class AddAsyncTests : ProductRepositoryTestBase
{
    [Fact]
    public Task AddAsync_WhenCommitted_PersistsAndCanBeReadBack() =>
       TestAsync(seedCount: 0, async t =>
       {
           // Arrange
           var product = t.Fixture.Create<Product>();
           await t.Sut.AddAsync(product, CancellationToken.None);
           await t.SaveChangesAsync(CancellationToken.None);
           
           // Act
           var loaded = await t.Sut.GetByIdAsync(product.Id, false, CancellationToken.None);

           // Assert
           Assert.NotNull(loaded);
           Assert.Equal(product.Id, loaded!.Id);
       });
}
