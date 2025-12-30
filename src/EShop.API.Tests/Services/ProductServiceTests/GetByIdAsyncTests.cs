using EShop.Shared.Representations.Product;

namespace EShop.API.Tests.Services.ProductServiceTests;

public sealed class GetByIdAsyncTests : ProductServiceTestBase
{
    [Fact]
    public async Task GetByIdAsync_WhenNotFound_ReturnsNull()
    {
        var id = Guid.NewGuid();
        var ct = CancellationToken.None;

        _repo.GetByIdAsync(id, track: false, ct).Returns((Product?)null);

        var sut = CreateSut();

        var result = await sut.GetByIdAsync(id, ct);

        Assert.Null(result);
        _mapper.DidNotReceiveWithAnyArgs().Map<ProductDto>(default!);
    }

    [Fact]
    public async Task GetByIdAsync_Success()
    {
        var id = Guid.NewGuid();
        var ct = CancellationToken.None;

        var product = CreateProductWithImages(id, sku: "SKU-1", imageUrls: Array.Empty<string>());
        var dto = new ProductDto();

        _repo.GetByIdAsync(id, track: false, ct).Returns(product);
        _mapper.Map<ProductDto>(product).Returns(dto);

        var sut = CreateSut();

        var result = await sut.GetByIdAsync(id, ct);

        Assert.Same(dto, result);
        _mapper.Received(1).Map<ProductDto>(product);
    }
}
