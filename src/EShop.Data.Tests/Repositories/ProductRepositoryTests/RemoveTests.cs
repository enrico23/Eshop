namespace EShop.Data.Tests.Repositories.ProductRepositoryTests;

public sealed class RemoveTests : ProductRepositoryTestsBase
{
    [Fact]
    public Task Remove_WhenSaved_RemovesEntityFromStore() =>
       TestAsync(seedCount: 1, async scope =>
       {
           // Arrange
           var product = scope.Entities[0];

           // Sanity check it exists before delete
           var before = await scope.Sut.GetByIdAsync(product.Id, false, CancellationToken.None);
           Assert.NotNull(before);

           // Act
           scope.Sut.Remove(product);
           await scope.SaveChangesAsync(CancellationToken.None);

           // Assert
           var after = await scope.Sut.GetByIdAsync(product.Id, false, CancellationToken.None);
           Assert.Null(after);
       });
}
