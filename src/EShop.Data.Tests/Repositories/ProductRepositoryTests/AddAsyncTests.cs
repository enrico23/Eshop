namespace EShop.Data.Tests.Repositories.ProductRepositoryTests;

public sealed class AddAsyncTests : ProductRepositoryTestsBase
{
    [Fact]
    public Task AddAsync_WhenCommitted_PersistsAndCanBeReadBack() =>
       TestAsync(seedCount: 0, async t =>
       {
           var product = t.Fixture.Create<Product>();

           await t.Sut.AddAsync(product, CancellationToken.None);

           await t.Sut.SaveChangesAsync(CancellationToken.None);

           var loaded = await t.Sut.GetByIdAsync(product.Id, false, CancellationToken.None);
           Assert.NotNull(loaded);
           Assert.Equal(product.Id, loaded!.Id);
       });

}
