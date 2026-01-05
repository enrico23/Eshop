namespace EShop.Data.Tests.CategoryTests;

public sealed class AddAsyncTests : CategoryRepositoryTestsBase
{
    [Fact]
    public Task AddCategoryAsync_Success() =>
       TestAsync(seedCount: 0, async t =>
       {
           // Arrange
           var category = t.Fixture.Create<Category>();
           await t.Sut.AddAsync(category, CancellationToken.None);
           await t.SaveChangesAsync(CancellationToken.None);
           
           // Act
           var loaded = await t.Sut.GetByIdAsync(category.Id, false, CancellationToken.None);

           // Assert
           Assert.NotNull(loaded);
           Assert.Equal(category.Id, loaded!.Id);
       });
}
