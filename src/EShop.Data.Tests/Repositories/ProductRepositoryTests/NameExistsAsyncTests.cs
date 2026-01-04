using EShop.Domain.Products;

namespace EShop.Data.Tests.Repositories.ProductRepositoryTests;

public sealed class NameExistsAsyncTests : ProductRepositoryTestsBase
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
            testBody: async t =>
            {
                var id = t.Products.Single().Id;

                var exists = await t.Sut.NameExistsAsync(queryName, CancellationToken.None);

                Assert.Equal(expected, exists);
            });
    }
}
