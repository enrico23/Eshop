using EShop.API.Representations;

namespace EShop.API.Services;

public interface IBrandService
{
    Task<PaginatedResult<BrandDto>> GetPagedAsync(int page, int pageSize, CancellationToken ct = default);

    Task<bool> BrandExistsAsync(int brandId, CancellationToken ct = default);

    Task<BrandDto?> GetByIdAsync(int brandId, CancellationToken ct = default);

    Task<bool> UpdateAsync(int id, UpdateBrandDto updateBrandDto, CancellationToken ct);

    Task<BrandDto> AddAsync(CreateBrandDto createBrandDto, CancellationToken ct);

    Task<bool> DeleteAsync(int id, CancellationToken ct);
}
