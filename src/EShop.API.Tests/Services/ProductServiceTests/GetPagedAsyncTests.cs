namespace EShop.API.Tests.Services.ProductServiceTests;

public sealed class GetPagedAsyncTests : ProductServiceTestBase
{
    [Fact]
    public async Task GetPagedAsync_Success()
    {
        // Arrange
        var ct = CancellationToken.None;

        var paged = new PagedResult<Product>(
            Items: new List<Product> { /* could be empty */ },
            Page: 1,
            PageSize: 1,
            Total: 0);

        _repo.GetPagedAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), ct)
                .Returns(paged);

        var sut = CreateSut();

        // Act
        await sut.GetPagedAsync(page: 0, pageSize: 500, orderBy: "Name", orderDirection: "asc", filter: null, ct);

        // Assert: page should become 1, pageSize should clamp to 200
        await _repo.Received(1).GetPagedAsync(
            1,
            200,
            "Name",
            "asc",
            null,
            ct);
    }
}
