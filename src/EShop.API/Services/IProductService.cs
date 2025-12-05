using EShop.Shared.Representations;
using EShop.Shared.Representations.Product;

namespace EShop.API.Services;

public interface IProductService
{
    Task<PagedResult<ProductDto>> GetPagedAsync(int page, int pageSize, 
        string orderBy = "Name", string orderDirection = "asc", 
        string? filter = null, CancellationToken ct = default);

    Task<bool> ExistsAsync(Guid id, CancellationToken ct = default);

    Task<ProductDto?> GetByIdAsync(Guid id, CancellationToken ct);

    Task<ProductDto> AddAsync(CreateProductDto createProductDto, CancellationToken ct);

    Task<bool> UpdateAsync(Guid id, UpdateProductDto updateProductDto, CancellationToken ct);

    Task<bool> DeleteAsync(Guid id, CancellationToken ct);
}
