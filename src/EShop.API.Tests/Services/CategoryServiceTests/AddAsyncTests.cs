
using AutoFixture;
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
            var form = Fixture.Create<CreateCategoryDto>();
            
            var dbEx = new DbUpdateException("db fail", innerException: new Exception("inner"));
            _sqlServerExceptionsHelper.IsUniqueViolation(dbEx).Returns(true);
            _repo.AddAsync(Arg.Any<Category>(), ct).Returns<Task>(_ => throw dbEx);

            var ex = await Assert.ThrowsAsync<DomainException>(() => t.Sut.AddAsync(form, ct));
        });
    }

    [Fact]
    public async Task AddAsync_Success()
    {
        await TestAsync(async t =>
        {
            var ct = CancellationToken.None;
            var form = Fixture.Create<CreateCategoryDto>();

            Category? added = null;
            await _repo.AddAsync(Arg.Do<Category>(p => added = p), ct);

            _mapper
                .Map<CategoryDto>(Arg.Any<Category>())
                .Returns(new CategoryDto { /* set fields if you care */ });

            var result = await t.Sut.AddAsync(form, ct);

            Assert.NotNull(result);
            
            await _repo.Received(1).AddAsync(Arg.Any<Category>(), ct);
            Assert.NotNull(added);
        });
    }
}