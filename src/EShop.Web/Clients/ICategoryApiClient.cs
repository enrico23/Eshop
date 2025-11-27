using EShop.Shared.Representations;
using EShop.Shared.Representations.Category;

namespace EShop.Web.Clients;

public interface ICategoryApiClient
{
    Task CreateAsync(CreateCategoryDto createCategoryDto, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
    Task<List<CategoryDto>?> GetAllCategoriesAsync(CancellationToken ct = default);
    Task<PagedResult<CategoryDto>?> GetPagedCategoriesAsync(int page = 1, int pageSize = 20, string orderBy = "Name", string orderDirection = "asc", string? filter = null, CancellationToken ct = default);
    Task UpdateAsync(int id, UpdateCategoryDto updateCategoryDto, CancellationToken ct = default);
}