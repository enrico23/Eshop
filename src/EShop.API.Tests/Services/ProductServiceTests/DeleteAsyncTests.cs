using EShop.Shared.Services.Eventing;

namespace EShop.API.Tests.Services.ProductServiceTests;

public sealed class DeleteAsyncTests : ProductServiceTestBase
{
    [Fact]
    public async Task DeleteAsync_ReturnsFalse_WhenNotFound()
    {
        var id = Guid.NewGuid();
        var ct = CancellationToken.None;

        _repo.GetByIdAsync(id, track: true, ct).Returns((Product?)null);

        var sut = CreateSut();

        var result = await sut.DeleteAsync(id, ct);

        Assert.False(result);
        _repo.DidNotReceiveWithAnyArgs().Remove(default!);
        await _repo.DidNotReceive().SaveChangesAsync(ct);
    }

    //[Fact]
    //public async Task DeleteAsync_PublishesDeleteEvents_RemovesAndSaves_WhenFoundWithImages()
    //{
    //    var id = Guid.NewGuid();
    //    var ct = CancellationToken.None;

    //    _corr.CorrelationId.Returns("corr-999");

    //    var entity = CreateProductWithImages(
    //        id,
    //        sku: "SKU-1",
    //        imageUrls: new[] { "https://cdn/a.png" });

    //    _repo.GetByIdAsync(id, track: true, ct).Returns(entity);

    //    var sut = CreateSut();

    //    var ok = await sut.DeleteAsync(id, ct);

    //    Assert.True(ok);

    //    await _publisher.Received(1).PublishAsync(
    //        "images-queue",
    //        Arg.Is<EventEnvelope<DeleteImageRequest>>(env =>
    //            env.Type == "image.deleted" &&
    //            env.Data.ImageUrl == "https://cdn/a.png" &&
    //            env.CorrelationId == "corr-999" &&
    //            env.Source == "eshop.api"),
    //        ct);

    //    _repo.Received(1).Remove(entity);
    //    await _repo.Received(1).SaveChangesAsync(ct);
    //}
}
