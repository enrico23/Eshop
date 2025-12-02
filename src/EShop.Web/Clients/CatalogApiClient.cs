using EShop.Shared.Representations;
using EShop.Shared.Representations.Category;
using EShop.Shared.Representations.Product;

namespace EShop.Web.Clients;

public class CatalogApiClient(HttpClient httpClient, ILogger<CatalogApiClient> logger)
    : ApiClientBase(logger), ICatalogApiClient
{
    #region Categories

    private const string CategoriesEndpointsBasePath = "api/categories";

    public async Task<List<CategoryDto>> GetAllCategoriesAsync(CancellationToken ct = default) 
    {
        var result = await SendAsync<List<CategoryDto>>(() => httpClient.GetAsync($"{CategoriesEndpointsBasePath}/all", ct), ct);
        return result ?? [];
    }
        

    public async Task<PagedResult<CategoryDto>> GetPagedCategoriesAsync(
        int page = 1,
        int pageSize = 20,
        string orderBy = "Name",
        string orderDirection = "asc",
        string? filter = null,
        CancellationToken ct = default)
    {
        var orderByEscaped = Uri.EscapeDataString(orderBy ?? "Name");
        var orderDirEscaped = Uri.EscapeDataString(orderDirection ?? "asc");
        var escapedFilter = Uri.EscapeDataString(filter ?? string.Empty);

        var url = $"{CategoriesEndpointsBasePath}?page={page}&pageSize={pageSize}&orderBy={orderByEscaped}&orderDirection={orderDirEscaped}&filter={escapedFilter}";
        var result = await SendAsync<PagedResult<CategoryDto>>(() => httpClient.GetAsync(url, ct), ct);

        if (result is null)
        {
            return new PagedResult<CategoryDto>
            (
                Page : page,
                PageSize: pageSize,
                Total: 0,
                Items: []
            );
        }

        return result;
    }

    public Task CreateCategoryAsync(CreateCategoryDto model, CancellationToken ct = default) =>
        SendAsync(() => httpClient.PostAsJsonAsync(CategoriesEndpointsBasePath, model, ct), ct);

    public Task UpdateCategoryAsync(int id, UpdateCategoryDto model, CancellationToken ct = default) =>
        SendAsync(() => httpClient.PutAsJsonAsync($"{CategoriesEndpointsBasePath}/{id}", model, ct), ct);

    public Task DeleteCategoryAsync(int id, CancellationToken ct = default) =>
        SendAsync(() => httpClient.DeleteAsync($"{CategoriesEndpointsBasePath}/{id}", ct), ct);

    #endregion

    #region Products

    private const string ProductsEndpointsBasePath = "api/products";

    public async Task<List<ProductDto>> GetAllProductsAsync(CancellationToken ct = default)
    {
        var result = await SendAsync<List<ProductDto>>(() => httpClient.GetAsync($"{ProductsEndpointsBasePath}/all", ct), ct);
        return result ?? [];
    }


    public async Task<PagedResult<ProductDto>> GetPagedProductsAsync(
        int page = 1,
        int pageSize = 20,
        string orderBy = "Name",
        string orderDirection = "asc",
        string? filter = null,
        CancellationToken ct = default)
    {
        var orderByEscaped = Uri.EscapeDataString(orderBy ?? "Name");
        var orderDirEscaped = Uri.EscapeDataString(orderDirection ?? "asc");
        var escapedFilter = Uri.EscapeDataString(filter ?? string.Empty);

        var url = $"{ProductsEndpointsBasePath}?page={page}&pageSize={pageSize}&orderBy={orderByEscaped}&orderDirection={orderDirEscaped}&filter={escapedFilter}";
        var result = await SendAsync<PagedResult<ProductDto>>(() => httpClient.GetAsync(url, ct), ct);

        if (result is null)
        {
            return new PagedResult<ProductDto>
            (
                Page: page,
                PageSize: pageSize,
                Total: 0,
                Items: []
            );
        }

        return result;
    }

    public Task CreateProductAsync(CreateProductDto model, CancellationToken ct = default) =>
        SendAsync(() => httpClient.PostAsJsonAsync(ProductsEndpointsBasePath, model, ct), ct);

    public Task UpdateProductAsync(int id, UpdateProductDto model, CancellationToken ct = default) =>
        SendAsync(() => httpClient.PutAsJsonAsync($"{ProductsEndpointsBasePath}/{id}", model, ct), ct);

    public Task DeleteProductAsync(int id, CancellationToken ct = default) =>
        SendAsync(() => httpClient.DeleteAsync($"{ProductsEndpointsBasePath}/{id}", ct), ct);

    #endregion
}
