using EShop.Api.Representations.Products;
using EShop.API.Representations;
using EShop.API.Representations.Products;

namespace EShop.API.Services;

public interface IProductService
{
    Task<PaginatedResult<ProductDto>> GetPagedAsync(int page, int pageSize, CancellationToken ct = default);

    Task<bool> ExistsAsync(int id, CancellationToken ct = default);

    Task<ProductDto?> GetByIdAsync(int id, CancellationToken ct);

    Task<ProductDto> AddAsync(CreateProductDto createProductDto, CancellationToken ct);

    Task<bool> UpdateAsync(int id, UpdateProductDto updateProductDto, CancellationToken ct);

    Task<bool> DeleteAsync(int id, CancellationToken ct);
}
