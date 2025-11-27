using EShop.Shared.Representations;
using EShop.Shared.Representations.Category;

namespace EShop.Web.Clients;

public class CategoryApiClient(HttpClient httpClient, ILogger<CategoryApiClient> logger) 
    : ApiClientBase(httpClient, logger), ICategoryApiClient
{
    private const string BasePath = "api/categories";

    public Task<List<CategoryDto>?> GetAllCategoriesAsync(CancellationToken ct = default) =>
        SendAsync<List<CategoryDto>>(() => HttpClient.GetAsync($"{BasePath}/all", ct), ct);

    public async Task<PagedResult<CategoryDto>?> GetPagedCategoriesAsync(
        int page = 1,
        int pageSize = 20,
        string orderBy = "Name",
        string orderDirection = "asc",
        CancellationToken ct = default)
    {
        var orderByEscaped = Uri.EscapeDataString(orderBy ?? "Name");
        var orderDirEscaped = Uri.EscapeDataString(orderDirection ?? "asc");

        var url = $"{BasePath}?page={page}&pageSize={pageSize}&orderBy={orderByEscaped}&orderDirection={orderDirEscaped}";
        return await SendAsync<PagedResult<CategoryDto>>(() => HttpClient.GetAsync(url, ct), ct);
    }

    public Task CreateAsync(CreateCategoryDto model, CancellationToken ct = default) =>
        SendAsync(() => HttpClient.PostAsJsonAsync(BasePath, model, ct), ct);

    public Task UpdateAsync(int id, UpdateCategoryDto model, CancellationToken ct = default) =>
        SendAsync(() => HttpClient.PutAsJsonAsync($"{BasePath}/{id}", model, ct), ct);

    public Task DeleteAsync(int id, CancellationToken ct = default) =>
        SendAsync(() => HttpClient.DeleteAsync($"{BasePath}/{id}", ct), ct);
}
