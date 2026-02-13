using EShop.Shared.Representations.Category;
using Microsoft.EntityFrameworkCore;

namespace EShop.API.Tests.Services.CategoryServiceTests;

public sealed class AddAsyncTests : CategoryServiceTestBase
{
    [Fact]
    public async Task AddAsync_WhenFormIsNull_ThenThrowsArgumentNullException()
    {
        await TestAsync(async t =>
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                t.Sut.AddAsync(null!, CancellationToken.None));
        });
    }

    [Fact]
    public async Task AddAsync_WhenNameAlreadyExists_ThrowsDomainException()
    {
        await TestAsync(async t =>
        {
            var ct = CancellationToken.None;
            var form = new CategoryFormModel(
                Name: "Category A",
                File: null,
                ImageAltText: null);
             
            var dbEx = new DbUpdateException("db fail", innerException: new Exception("inner"));
            _sqlServerExceptionsHelper.IsUniqueViolation(dbEx).Returns(true);
            _repo2.AddAsync(Arg.Any<Category>(), ct).Returns<Task>(_ => throw dbEx);

            var ex = await Assert.ThrowsAsync<DomainException>(() => t.Sut.AddAsync(form, ct));
        });
    }

    [Fact]
    public async Task AddAsync_Success()
    {
        await TestAsync(async t =>
        {
            var ct = CancellationToken.None;
            var file = Substitute.For<Microsoft.AspNetCore.Http.IFormFile>();
            var form = new CategoryFormModel(
                Name: "Category A",
                File: file,
                ImageAltText: "Category A");

            _imageStorageService
                .UploadImageAsync(ImageStoragePurpose.Categories, form.File!, ct)
                .Returns("https://cdn/new-category.png");

            Category? added = null;
            await _repo2.AddAsync(Arg.Do<Category>(p => added = p), ct);

            _mapper
                .Map<CategoryDto>(Arg.Any<Category>())
                .Returns(new CategoryDto { /* set fields if you care */ });

            var result = await t.Sut.AddAsync(form, ct);

            Assert.NotNull(result);
            await _imageStorageService.Received(1).UploadImageAsync(ImageStoragePurpose.Categories, form.File!, ct);
             
            await _repo2.Received(1).AddAsync(Arg.Any<Category>(), ct);
            Assert.NotNull(added);
            Assert.Contains(added!.Images, i => i.Url == "https://cdn/new-category.png");
        });
    }
}
