using Microsoft.EntityFrameworkCore;

namespace EShop.API.Tests.Services.ProductServiceTests;

public sealed class AddAsyncTests : ProductServiceTestBase
{
    [Fact]
    public async Task AddAsync_WhenFormIsNull_ThenThrowsArgumentNullException()
    {
        var sut = CreateSut();

        await Assert.ThrowsAsync<ArgumentNullException>(() => sut.AddAsync(null!, CancellationToken.None));
    }

    [Fact]
    public async Task AddAsync_WhenNameAlreadyExists_ThrowsDomainException()
    {
        var sut = CreateSut();
        var ct = CancellationToken.None;

        var form = new ProductFormDto
        {
            Name = "Test",
            Description = "Desc",
            Price = 10,
            Stock = 1,
            Sku = "SKU-1",
            CategoryId = Guid.NewGuid(),
            File = null
        };

        var dbEx = new DbUpdateException("db fail", innerException: new Exception("inner"));
        _sqlServerExceptionsHelper.IsUniqueViolation(dbEx).Returns(true);

        _repo2.AddAsync(Arg.Any<Product>(), ct).Returns<Task>(_ => throw dbEx);

        var ex = await Assert.ThrowsAsync<DomainException>(() => sut.AddAsync(form, ct));

        await _unitOfWork.DidNotReceive().SaveChangesAsync(ct);
    }

    [Fact]
    public async Task AddAsync_Success()
    {
        var ct = CancellationToken.None;

        var form = new ProductFormDto
        {
            Name = "Product A",
            Description = "Desc",
            Price = 10,
            Stock = 1,
            Sku = "SKU-1",
            CategoryId = Guid.NewGuid(),
            File = FormFileHelpers.CreateFakeFormFile()
        };

        _imageStorage.UploadImageAsync(form.File, ct).Returns("https://cdn/new.png");

        // Capture the entity passed into AddAsync to assert it contains the image URL
        Product? added = null;
        await _repo2.AddAsync(Arg.Do<Product>(p => added = p), ct);

        _mapper
            .Map<ProductDto>(Arg.Any<Product>())
            .Returns(new ProductDto { /* set fields if you care */ });

        var sut = CreateSut();

        var result = await sut.AddAsync(form, ct);

        Assert.NotNull(result);
        await _imageStorage.Received(1).UploadImageAsync(form.File, ct);
        await _repo2.Received(1).AddAsync(Arg.Any<Product>(), ct);
        await _unitOfWork.Received(1).SaveChangesAsync(ct);

        Assert.NotNull(added);
        Assert.Contains(added!.Images, i => i.Url == "https://cdn/new.png");
    }
}