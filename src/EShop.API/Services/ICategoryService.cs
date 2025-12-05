using EShop.Shared.Representations;
using EShop.Shared.Representations.Category;

namespace EShop.API.Services;

public interface ICategoryService
{
    Task<PagedResult<CategoryDto>> GetPagedAsync(int page, int pageSize,
        string orderBy = "Name", string orderDirection = "asc", string? filter = null, CancellationToken ct = default);

    Task<ICollection<CategoryDto>> GetAllAsync(CancellationToken ct);

    Task<bool> ExistsAsync(Guid id, CancellationToken ct = default);

    Task<CategoryDto?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<bool> UpdateAsync(Guid id, UpdateCategoryDto updateCategoryDto, CancellationToken ct);

    Task<CategoryDto> AddAsync(CreateCategoryDto createCategoryDto, CancellationToken ct);

    Task<bool> DeleteAsync(Guid id, CancellationToken ct);
}
