using EShop.Shared.Representations;
using EShop.Shared.Representations.Category;
using EShop.Shared.Representations.Product;

namespace EShop.Web.Clients;

public interface ICatalogApiClient
{
    Task CreateCategoryAsync(CreateCategoryDto createCategoryDto, CancellationToken ct = default);
    Task DeleteCategoryAsync(int id, CancellationToken ct = default);
    Task<List<CategoryDto>> GetAllCategoriesAsync(CancellationToken ct = default);
    Task<PagedResult<CategoryDto>> GetPagedCategoriesAsync(int page = 1, int pageSize = 20, string orderBy = "Name", string orderDirection = "asc", string? filter = null, CancellationToken ct = default);
    Task UpdateCategoryAsync(int id, UpdateCategoryDto updateCategoryDto, CancellationToken ct = default);


    Task CreateProductAsync(CreateProductDto createCategoryDto, CancellationToken ct = default);
    Task DeleteProductAsync(int id, CancellationToken ct = default);
    Task<List<ProductDto>> GetAllProductsAsync(CancellationToken ct = default);
    Task<PagedResult<ProductDto>> GetPagedProductsAsync(int page = 1, int pageSize = 20, string orderBy = "Name", string orderDirection = "asc", string? filter = null, CancellationToken ct = default);
    Task UpdateProductAsync(int id, UpdateProductDto updateCategoryDto, CancellationToken ct = default);
}