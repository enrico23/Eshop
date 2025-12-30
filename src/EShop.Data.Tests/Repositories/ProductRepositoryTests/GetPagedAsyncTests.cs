namespace EShop.Data.Tests.Repositories.ProductRepositoryTests;

public sealed class GetPagedAsyncTests : ProductRepositoryTestsBase
{
    [Fact]
    public async Task GetPagedAsync_WhenNoFilterAndNoOrder_ReturnsPageWithCorrectTotalAndItems()
    {
        // Arrange
        var fixture = CreateFixture();
        var products = fixture.CreateMany<Product>(25).ToList();

        var dbName = fixture.Create<string>();
        await using var context = CreateContext(dbName);
        await SeedAsync(context, products);

        var sut = new ProductRepository(context);

        // Act
        var result = await sut.GetPagedAsync(
            page: 2,
            pageSize: 10,
            orderBy: "",
            orderDirection: "",
            filter: null,
            ct: CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Page);
        Assert.Equal(10, result.PageSize);
        Assert.Equal(25, result.Total);
        Assert.Equal(10, result.Items.Count);
        Assert.All(result.Items, p => Assert.Contains(products, x => x.Id == p.Id));
    }

    [Fact]
    public async Task GetPagedAsync_WithFilter_IsCaseInsensitiveAndMatchesSubstring()
    {
        // Arrange
        var fixture = CreateFixture();
        var products = fixture.CreateMany<Product>(3).ToList();
        var dbName = fixture.Create<string>();
        await using var context = CreateContext(dbName);

        await SeedAsync(context, products);

        var sut = new ProductRepository(context);

        // Act
        var expected = products.First();
        var result = await sut.GetPagedAsync(
            page: 1,
            pageSize: 50,
            orderBy: "Name",
            orderDirection: "asc",
            filter: expected.Name.ToUpper(),
            ct: CancellationToken.None);

        // Assert
        Assert.Equal(1, result.Total);
        Assert.Collection(result.Items,
            item => Assert.Equal(expected.Name, item.Name));
    }

    [Fact]
    public async Task GetPagedAsync_WithOrderByNameDescending_SortsCorrectly()
    {
        // Arrange
        var fixture = CreateFixture();
        var products = fixture.CreateMany<Product>(3).ToList();
        var dbName = fixture.Create<string>();
        await using var context = CreateContext(dbName);
        await SeedAsync(context, products);
        var sut = new ProductRepository(context);

        // Act
        var result = await sut.GetPagedAsync(
            page: 1,
            pageSize: 10,
            orderBy: "Name",
            orderDirection: "desc",
            filter: null,
            ct: CancellationToken.None);

        // Assert
        Assert.Equal(3, result.Total);
        var names = result.Items.Select(x => x.Name).ToList();
        Assert.Equal(names.OrderByDescending(x => x).ToList(), names);
    }

    [Theory]
    [InlineData(1, 5, 12, 5)]
    [InlineData(2, 5, 12, 5)]
    [InlineData(3, 5, 12, 2)]
    public async Task GetPagedAsync_PaginatesCorrectly(int page, int pageSize, int totalSeed, int expectedCount)
    {
        // Arrange
        var fixture = CreateFixture();
        var products = fixture.CreateMany<Product>(totalSeed).ToList();
        var dbName = fixture.Create<string>();
        await using var context = CreateContext(dbName);
        await SeedAsync(context, products);

        var sut = new ProductRepository(context);

        // Act
        var result = await sut.GetPagedAsync(
            page: page,
            pageSize: pageSize,
            orderBy: "Name",
            orderDirection: "asc",
            filter: null,
            ct: CancellationToken.None);

        // Assert
        Assert.Equal(totalSeed, result.Total);
        Assert.Equal(expectedCount, result.Items.Count);
        Assert.All(result.Items, p => Assert.False(string.IsNullOrWhiteSpace(p.Name)));
    }
}
