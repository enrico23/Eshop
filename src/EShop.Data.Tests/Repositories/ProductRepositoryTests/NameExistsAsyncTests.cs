using EShop.Domain.Products;

namespace EShop.Data.Tests.Repositories.ProductRepositoryTests;

public sealed class NameExistsAsyncTests : ProductRepositoryTestBase
{
    [Theory]
    [InlineData("Widget", "widget", true)]
    [InlineData("WIDGET", "widget", true)]
    [InlineData("widget", "WIDGET", true)]
    [InlineData("Widget", "Gadget", false)]
    public Task NameExistsAsync_IsCaseInsensitive(string existingName, string queryName, bool expected)
    {
        return TestAsync(
            seed:
            [
                Product.Create(
                    name: existingName,
                    description: new ProductDescription("desc"),
                    price: new Money(10),
                    stock: new StockQuantity(5),
                    sku: new Sku("SKU-1"),
                    categoryId: Guid.NewGuid())
            ],
            testBody: async scope =>
            {
                var id = scope.Entities.Single().Id;

                var exists = await scope.Sut.NameExistsAsync(queryName, CancellationToken.None);

                Assert.Equal(expected, exists);
            });
    }
}
