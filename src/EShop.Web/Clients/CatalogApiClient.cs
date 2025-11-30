using EShop.Shared.Representations;
using EShop.Shared.Representations.Category;

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

    public Task CreateAsync(CreateCategoryDto model, CancellationToken ct = default) =>
        SendAsync(() => httpClient.PostAsJsonAsync(CategoriesEndpointsBasePath, model, ct), ct);

    public Task UpdateAsync(int id, UpdateCategoryDto model, CancellationToken ct = default) =>
        SendAsync(() => httpClient.PutAsJsonAsync($"{CategoriesEndpointsBasePath}/{id}", model, ct), ct);

    public Task DeleteAsync(int id, CancellationToken ct = default) =>
        SendAsync(() => httpClient.DeleteAsync($"{CategoriesEndpointsBasePath}/{id}", ct), ct);

    #endregion
}
