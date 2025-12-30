namespace EShop.Data.Tests.Repositories.ProductRepositoryTests;

public sealed class RemoveTests : ProductRepositoryTestsBase
{
    [Fact]
    public Task Remove_WhenSaved_RemovesEntityFromStore() =>
       TestAsync(seedCount: 1, async t =>
       {
           // Arrange
           var product = t.Products[0];

           // Sanity check it exists before delete
           var before = await t.Sut.GetByIdAsync(product.Id, false, CancellationToken.None);
           Assert.NotNull(before);

           // Act
           t.Sut.Remove(product);
           await t.Sut.SaveChangesAsync(CancellationToken.None);

           // Assert
           var after = await t.Sut.GetByIdAsync(product.Id, false, CancellationToken.None);
           Assert.Null(after);
       }).AsTask();
}
