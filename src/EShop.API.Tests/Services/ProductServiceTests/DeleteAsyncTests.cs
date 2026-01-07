using EShop.Shared.Eventing;

namespace EShop.API.Tests.Services.ProductServiceTests;

public sealed class DeleteAsyncTests : ProductServiceTestBase
{
    [Fact]
    public async Task DeleteAsync_WhenNotFound_ReturnsFalse()
    {
        var id = Guid.NewGuid();
        var ct = CancellationToken.None;

        _repo2.GetByIdAsync(id, track: true, ct).Returns((Product?)null);

        var sut = CreateSut();

        var result = await sut.DeleteAsync(id, ct);

        Assert.False(result);
        _repo.DidNotReceiveWithAnyArgs().Remove(default!);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(ct);
    }

    [Fact]
    public async Task DeleteAsync_Success()
    {
        var id = Guid.NewGuid();
        var ct = CancellationToken.None;

        _corr.CorrelationId.Returns("corr-999");

        var entity = CreateProductWithImages(
            id,
            sku: "SKU-1",
            imageUrls: new[] { "https://cdn/a.png" });

        _repo2.GetByIdAsync(id, track: true, ct).Returns(entity);

        var sut = CreateSut();

        var ok = await sut.DeleteAsync(id, ct);

        Assert.True(ok);

        await _publisher.Received(1).PublishAsync(
            "images-queue",
            Arg.Any<EventEnvelope<DeleteImageRequest>>(),
            ct);

        _repo.Received(1).Remove(entity);
        await _unitOfWork.Received(1).SaveChangesAsync(ct);
    }
}
