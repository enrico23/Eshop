using EShop.Shared.Eventing;

namespace EShop.API.Tests.Services.ProductServiceTests;

public sealed class UpdateAsyncTests : ProductServiceTestBase
{
    [Fact]
    public async Task UpdateAsync_WhenProductNotFound_ThenReturnsFalse()
    {
        var id = Guid.NewGuid();
        var ct = CancellationToken.None;

        _repo2.GetByIdAsync(id, track: true, ct).Returns((Product?)null);

        var sut = CreateSut();

        var result = await sut.UpdateAsync(id, ValidForm(status: ProductStatus.Active, withFile: false), ct);

        Assert.False(result);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(ct);
    }

    [Fact]
    public async Task UpdateAsync_WhenSkuInUse_AndSkuChanged_ThenThrowsDomainException()
    {
        var id = Guid.NewGuid();
        var ct = CancellationToken.None;

        var product = CreateProductWithImages(id, sku: "SKU-OLD", imageUrls: Array.Empty<string>());

        _repo2.GetByIdAsync(id, track: true, ct).Returns(product);
        _repo.SkuInUseAsync(Arg.Any<Sku>(), product.Id, ct).Returns(true);

        var sut = CreateSut();

        var form = ValidForm(status: ProductStatus.Active, withFile: false);
        form.Sku = "SKU-NEW";

        await Assert.ThrowsAsync<DomainException>(() => sut.UpdateAsync(id, form, ct));
        await _unitOfWork.DidNotReceive().SaveChangesAsync(ct);
    }

    [Fact]
    public async Task UpdateAsync_WhenCategoryIdMissing_ThenThrowsDomainException()
    {
        var id = Guid.NewGuid();
        var ct = CancellationToken.None;

        var product = CreateProductWithImages(id, sku: "SKU-1", imageUrls: Array.Empty<string>());
        _repo2.GetByIdAsync(id, track: true, ct).Returns(product);

        var sut = CreateSut();

        var form = ValidForm(status: ProductStatus.Active, withFile: false);
        form.CategoryId = null;

        await Assert.ThrowsAsync<DomainException>(() => sut.UpdateAsync(id, form, ct));
        await _unitOfWork.DidNotReceive().SaveChangesAsync(ct);
    }

    [Fact]
    public async Task UpdateAsync_Success()
    {
        var id = Guid.NewGuid();
        var ct = CancellationToken.None;

        _corr.CorrelationId.Returns("corr-123");

        var product = CreateProductWithImages(
            id,
            sku: "SKU-1",
            imageUrls: new[] { "https://cdn/old1.png", "https://cdn/old2.png" });

        _repo2.GetByIdAsync(id, track: true, ct).Returns(product);

        var form = ValidForm(status: ProductStatus.Active, withFile: true);
        form.File = FormFileHelpers.CreateFakeFormFile();
        form.Sku = "SKU-1";

        _imageStorage.UploadImageAsync(form.File, ct).Returns("https://cdn/new.png");

        var sut = CreateSut();

        var ok = await sut.UpdateAsync(id, form, ct);

        Assert.True(ok);

        await _publisher.Received(2).PublishAsync(
            "images-queue",
           Arg.Any<EventEnvelope<DeleteImageRequest>>(),
           ct);

        
        await _imageStorage.Received(1).UploadImageAsync(form.File, ct);

        await _repo.DidNotReceiveWithAnyArgs().SkuInUseAsync(default!, default, default);

        await _unitOfWork.Received(1).SaveChangesAsync(ct);

    
        Assert.Contains(product.Images, i => i.Url == "https://cdn/new.png");
    }
}
