using EShop.Shared.Representations;
using EShop.Shared.Representations.Category;
using EShop.Shared.Representations.Product;
using EShop.Web.Models;
using Microsoft.AspNetCore.Components.Forms;
using System.Globalization;

namespace EShop.Web.Clients;

public interface ICatalogApiClient
{
    #region categories

    Task CreateCategoryAsync(CreateCategoryDto createCategoryDto, CancellationToken ct = default);
    Task DeleteCategoryAsync(Guid id, CancellationToken ct = default);
    Task<List<CategoryDto>> GetAllCategoriesAsync(CancellationToken ct = default);
    Task<PagedResult<CategoryDto>> GetPagedCategoriesAsync(int page = 1, int pageSize = 20, string orderBy = "Name", string orderDirection = "asc", string? filter = null, CancellationToken ct = default);
    Task UpdateCategoryAsync(Guid id, UpdateCategoryDto updateCategoryDto, CancellationToken ct = default);

    #endregion

    Task CreateProductAsync(CreateProductViewModel model, IBrowserFile? file, CancellationToken ct = default);
    Task DeleteProductAsync(Guid id, CancellationToken ct = default);
    Task<List<ProductDto>> GetAllProductsAsync(CancellationToken ct = default);
    Task<PagedResult<ProductDto>> GetPagedProductsAsync(int page = 1, int pageSize = 20, string orderBy = "Name", string orderDirection = "asc", string? filter = null, CancellationToken ct = default);
    Task UpdateProductAsync(Guid id, CreateProductViewModel model, IBrowserFile? file, CancellationToken ct = default);

}


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

    public Task UpdateCategoryAsync(Guid id, UpdateCategoryDto model, CancellationToken ct = default) =>
        SendAsync(() => httpClient.PutAsJsonAsync($"{CategoriesEndpointsBasePath}/{id}", model, ct), ct);

    public Task DeleteCategoryAsync(Guid id, CancellationToken ct = default) =>
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

    public async Task CreateProductAsync(CreateProductViewModel model, IBrowserFile? file, CancellationToken ct = default)
    {
        using var form = BuildMultipart(model, file);
        await SendAsync(() => httpClient.PostAsync($"{ProductsEndpointsBasePath}/v2", form, ct), ct);
    }
        

    public async Task UpdateProductAsync(Guid id, CreateProductViewModel model, IBrowserFile? file, CancellationToken ct = default)
    {
        using var form = BuildMultipart(model, file);
        await SendAsync(() => httpClient.PutAsync($"{ProductsEndpointsBasePath}/v2/{id}", form, ct), ct);
    }
        

    public Task DeleteProductAsync(Guid id, CancellationToken ct = default) =>
        SendAsync(() => httpClient.DeleteAsync($"{ProductsEndpointsBasePath}/{id}", ct), ct);

    #endregion

    private static MultipartFormDataContent BuildMultipart(CreateProductViewModel model, IBrowserFile? file)
    {
        var form = new MultipartFormDataContent
        {
            { new StringContent(model.Name), "Name" },
            { new StringContent(model.Description), "Description" },
            { new StringContent(model.Sku), "Sku" },
            { new StringContent(((int)model.Status).ToString()), "Status" },
            { new StringContent(model.Price.ToString(CultureInfo.InvariantCulture)), "Price" },
            { new StringContent(model.Stock.ToString(CultureInfo.InvariantCulture)), "Stock" },
            { new StringContent(model.CategoryId!.Value.ToString()), "CategoryId" }
        };

        if (file is not null)
        {
            var stream = file.OpenReadStream(5_000_000);

            var fileContent = new StreamContent(stream);
            fileContent.Headers.ContentType =
                new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType ?? "application/octet-stream");

            form.Add(fileContent, "file", file.Name);
        }

        return form;
    }
}