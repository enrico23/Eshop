using EShop.Shared.Representations;
using EShop.Shared.Representations.Category;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace EShop.Shared.Clients;

public class CategoryApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CategoryApiClient> _logger;

    public CategoryApiClient(HttpClient httpClient, ILogger<CategoryApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<List<CategoryDto>> GetAllCategoriesAsync(CancellationToken ct = default)
    {
        var response = await _httpClient.GetAsync("api/categories/all", ct);

        if (response.IsSuccessStatusCode)
        {
            var categories = await response.Content.ReadFromJsonAsync<List<CategoryDto>>(ct);
            return categories ?? new List<CategoryDto>();
        }
        else
        {
            _logger.LogError("Failed to fetch paged categories. Status code: {StatusCode}", response.StatusCode);
            throw new HttpRequestException($"Failed to fetch categories. Status code: {response.StatusCode}");
        }
    }

    public async Task<PagedResult<CategoryDto>?> GetPagedCategoriesAsync(
        int page = 1,
        int pageSize = 20,
        string orderBy = "Name",
        string orderDirection = "asc",
        CancellationToken ct = default)
    {
        var url = $"api/categories?page={page}&pageSize={pageSize}&orderBy={orderBy}&orderDirection={orderDirection}";

        var response = await _httpClient.GetAsync(url, ct);
        if (response.IsSuccessStatusCode)
        {
           return await response.Content.ReadFromJsonAsync<PagedResult<CategoryDto>>(ct);
        }
        else
        {
            _logger.LogError("Failed to fetch paged categories. Status code: {StatusCode}", response.StatusCode);
            throw new HttpRequestException($"Failed to fetch paged categories. Status code: {response.StatusCode}");
        }
    }
}
