using EShop.Domain.Products;

namespace EShop.Data.Tests.Repositories.ProductRepositoryTests;

public sealed class SkuInUseTests : ProductRepositoryTestsBase
{
    [Fact]
    public Task SkuInUseAsync_WhenSkuExistsAndNoExclude_ReturnsTrue() =>
        TestAsync(seedCount: 1, async t =>
        {
            var sku = t.Products[0].Sku;

            var inUse = await t.Sut.SkuInUseAsync(sku, excludeId: null, CancellationToken.None);

            Assert.True(inUse);
        }).AsTask();

    [Fact]
    public Task SkuInUseAsync_WhenSkuDoesNotExist_ReturnsFalse() =>
        TestAsync(seedCount: 1, async t =>
        {
            var missingSku = t.Fixture.Create<Sku>();

            if (missingSku == t.Products[0].Sku)
                missingSku = t.Fixture.Create<Sku>();

            var inUse = await t.Sut.SkuInUseAsync(missingSku, excludeId: null, CancellationToken.None);

            Assert.False(inUse);
        }).AsTask();

    [Fact]
    public Task SkuInUseAsync_WhenSkuExistsButExcludedById_ReturnsFalse() =>
    TestAsync(seedCount: 1, async t =>
    {
        var product = t.Products[0];

        var inUse = await t.Sut.SkuInUseAsync(product.Sku, excludeId: product.Id, CancellationToken.None);

        Assert.False(inUse);
    }).AsTask();

    [Fact]
    public Task SkuInUseAsync_WhenSkuExistsOnAnotherProduct_NotExcluded_ReturnsTrue()
    {
        var sharedSku = new Sku($"SKU-{Guid.NewGuid():N}");

        var p1 = Product.Create(
           name: "P1",
           description: new ProductDescription("d1"),
           price: new Money(10),
           stock: new StockQuantity(1),
           sku: sharedSku,
           categoryId: Guid.NewGuid());

        var p2 = Product.Create(
            name: "P2",
            description: new ProductDescription("d2"),
            price: new Money(20),
            stock: new StockQuantity(2),
            sku: sharedSku,
            categoryId: Guid.NewGuid());

        return TestAsync(seed: [p1, p2], async t =>
        {
            var inUse = await t.Sut.SkuInUseAsync(sharedSku, excludeId: t.Products[0].Id, CancellationToken.None);

            Assert.True(inUse);
        }).AsTask();
    }

    [Fact]
    public Task SkuInUseAsync_WhenExcludeIdDoesNotMatchAnything_BehavesLikeNoExclude_ReturnsTrue() =>
        TestAsync(seedCount: 1, async t =>
        {
            var sku = t.Products[0].Sku;
            var excludeId = Guid.NewGuid();

            var inUse = await t.Sut.SkuInUseAsync(sku, excludeId, CancellationToken.None);

            Assert.True(inUse);
        }).AsTask();
}
