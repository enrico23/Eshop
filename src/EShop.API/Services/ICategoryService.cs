using EShop.API.Representations;
using EShop.API.Representations.Category;

namespace EShop.API.Services;

public interface ICategoryService
{
    Task<PaginatedResult<CategoryDto>> GetPagedAsync(int page, int pageSize,
        string orderBy = "Name", string orderDirection = "asc", CancellationToken ct = default);

    Task<bool> ExistsAsync(int id, CancellationToken ct = default);

    Task<CategoryDto?> GetByIdAsync(int id, CancellationToken ct = default);

    Task<bool> UpdateAsync(int id, UpdateCategoryDto updateCategoryDto, CancellationToken ct);

    Task<CategoryDto> AddAsync(CreateCategoryDto createCategoryDto, CancellationToken ct);

    Task<bool> DeleteAsync(int id, CancellationToken ct);
}
