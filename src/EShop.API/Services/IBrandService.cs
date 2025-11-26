using EShop.Shared.Representations;
using EShop.Shared.Representations.Brand;

namespace EShop.API.Services;

public interface IBrandService
{
    Task<PagedResult<BrandDto>> GetPagedAsync(int page, int pageSize, 
        string orderBy = "Name", string orderDirection = "asc", CancellationToken ct = default);

    Task<ICollection<BrandDto>> GetAllAsync(CancellationToken ct);

    Task<bool> BrandExistsAsync(int brandId, CancellationToken ct = default);

    Task<BrandDto?> GetByIdAsync(int brandId, CancellationToken ct = default);

    Task<bool> UpdateAsync(int id, UpdateBrandDto updateBrandDto, CancellationToken ct);

    Task<BrandDto> AddAsync(CreateBrandDto createBrandDto, CancellationToken ct);

    Task<bool> DeleteAsync(int id, CancellationToken ct);
}
