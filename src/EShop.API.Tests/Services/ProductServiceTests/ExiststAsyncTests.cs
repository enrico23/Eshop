namespace EShop.API.Tests.Services.ProductServiceTests;

public sealed class ExiststAsyncTests : ProductServiceTestBase
{
    [Fact]
    public async Task ExistsAsync_CallsRepository()
    {
        var id = Guid.NewGuid();
        var ct = CancellationToken.None;

        _repo.ExistsAsync(id, ct).Returns(true);

        var sut = CreateSut();

        var result = await sut.ExistsAsync(id, ct);

        Assert.True(result);
        await _repo.Received(1).ExistsAsync(id, ct);
    }
}
