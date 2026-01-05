namespace EShop.Data.Tests.Repositories.ProductRepositoryTests;

public sealed class GetPagedAsyncTests : ProductRepositoryTestsBase
{
    [Fact]
    public Task GetPagedAsync_WhenNoFilterAndNoOrder_ReturnsPageWithCorrectTotalAndItems()
    {
        return TestAsync(
            seedCount: 25,
            testBody: async scope =>
            {
                var result = await scope.Sut.GetPagedAsync(
                    page: 2,
                    pageSize: 10,
                    orderBy: "",
                    orderDirection: "",
                    filter: null,
                    ct: CancellationToken.None);

                Assert.Equal(2, result.Page);
                Assert.Equal(10, result.PageSize);
                Assert.Equal(25, result.Total);
                Assert.Equal(10, result.Items.Count);
                Assert.All(result.Items, p => Assert.Contains(scope.Entities, x => x.Id == p.Id));
            });
    }

    [Fact]
    public Task GetPagedAsync_WithFilter_IsCaseInsensitiveAndMatchesSubstring()
    {
        return TestAsync(
            seedCount: 25,
            testBody: async scope =>
            {
                var expected = scope.Entities.First();
                var result = await scope.Sut.GetPagedAsync(
                    page: 1,
                    pageSize: 50,
                    orderBy: "Name",
                    orderDirection: "asc",
                    filter: expected.Name.ToUpper(),
                    ct: CancellationToken.None);

                Assert.Equal(1, result.Total);
                Assert.Collection(result.Items,
                    item => Assert.Equal(expected.Name, item.Name));

            });
    }

    [Fact]
    public  Task GetPagedAsync_WithOrderByNameDescending_SortsCorrectly() =>
     TestAsync(
            seedCount: 3,
            testBody: async scope =>
            {
                var result = await scope.Sut.GetPagedAsync(
                    page: 1,
                    pageSize: 10,
                    orderBy: "Name",
                    orderDirection: "desc",
                    filter: null,
                    ct: CancellationToken.None);

                Assert.Equal(3, result.Total);
                var names = result.Items.Select(x => x.Name).ToList();
                Assert.Equal(names.OrderByDescending(x => x).ToList(), names);
            });

    [Theory]
    [InlineData(1, 5, 12, 5)]
    [InlineData(2, 5, 12, 5)]
    [InlineData(3, 5, 12, 2)]
    public Task GetPagedAsync_PaginatesCorrectly(int page, int pageSize, int totalSeed, int expectedCount)
    {
        return TestAsync(
            seedCount: totalSeed,
            testBody: async scope =>
            {
                // Act
                var result = await scope.Sut.GetPagedAsync(
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
            });
    }
}
