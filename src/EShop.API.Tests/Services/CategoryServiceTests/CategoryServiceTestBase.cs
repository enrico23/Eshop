using AutoFixture;
using EShop.Data;
using EShop.Shared.Exceptions;

namespace EShop.API.Tests.Services.CategoryServiceTests;

public abstract class CategoryServiceTestBase
{
    internal IExtendedCategoryRepository _repo = Substitute.For<IExtendedCategoryRepository>();
    internal IRepository<Category, Guid, CatalogDbContext> _repo2 = Substitute.For<IRepository<Category, Guid, CatalogDbContext>>();
    internal IMapper _mapper = Substitute.For<IMapper>();
    internal IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    internal ISqlServerExceptionsHelper _sqlServerExceptionsHelper = Substitute.For<ISqlServerExceptionsHelper>();
    internal IFixture Fixture = new Fixture();

    protected Task TestAsync(
       Func<TestScope, Task> testBody) =>
       CreateAsync(testBody: testBody);

    private async Task CreateAsync(Func<TestScope, Task> testBody)
    {
        var sut = new CategoryService(_repo, _repo2, _mapper, _unitOfWork, _sqlServerExceptionsHelper);
       
        try
        {
            await using var scope = new TestScope(sut);
            await testBody(scope).ConfigureAwait(false);
        }
        catch
        {
            throw;
        }
    }

    protected sealed class TestScope : IAsyncDisposable
    {
        internal TestScope(CategoryService service)
        {
            Sut = service;
        }

        public CategoryService Sut { get; }
      
        public ValueTask DisposeAsync() =>
            ValueTask.CompletedTask;
    }
}
