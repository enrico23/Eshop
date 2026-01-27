using EShop.Shared.Helpers;
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

    Task<List<CategoryDto>> GetAllCategoriesAsync(CancellationToken ct = default);
    Task<PagedResult<CategoryDto>> GetPagedCategoriesAsync(int page = 1, int pageSize = 20, string orderBy = "Name", string orderDirection = "asc", string? filter = null, CancellationToken ct = default);
    Task<CategoryDto?> GetCategoryByIdAsync(Guid id, CancellationToken ct = default);
    Task CreateCategoryAsync(CategoryUpsertViewModel createCategoryDto, IBrowserFile? file, CancellationToken ct = default);
    Task UpdateCategoryAsync(Guid id, CategoryUpsertViewModel updateCategoryDto, IBrowserFile? file, CancellationToken ct = default);
    Task DeleteCategoryAsync(Guid id, CancellationToken ct = default);

    #endregion

    #region Products

    Task<List<ProductDto>> GetAllProductsAsync(CancellationToken ct = default);
    Task<PagedResult<ProductDto>> GetPagedProductsAsync(int page = 1, int pageSize = 20, string orderBy = "Name", string orderDirection = "asc",
       string? filter = null, Guid? categoryId = null, CancellationToken ct = default);
    Task<ProductDto?> GetProductByIdAsync(Guid id, CancellationToken ct = default);
    Task CreateProductAsync(ProductUpsertViewModel model, IBrowserFile? file, CancellationToken ct = default);
    Task UpdateProductAsync(Guid id, ProductUpsertViewModel model, IBrowserFile? file, CancellationToken ct = default);
    Task DeleteProductAsync(Guid id, CancellationToken ct = default);

    #endregion
}

public sealed class CatalogApiClient(HttpClient httpClient, ILogger<CatalogApiClient> logger)
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

    public async Task<CategoryDto?> GetCategoryByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await SendAsync<CategoryDto>(() => httpClient.GetAsync($"{CategoriesEndpointsBasePath}/{id}", ct), ct);
    }

    public async Task CreateCategoryAsync(CategoryUpsertViewModel model, IBrowserFile? file, CancellationToken ct = default)
    {
        using var form = BuildCategoryRequest(model, file);
        await SendAsync(() => httpClient.PostAsJsonAsync(CategoriesEndpointsBasePath, model, ct), ct);
    }

    public async Task UpdateCategoryAsync(Guid id, CategoryUpsertViewModel model, IBrowserFile? file, CancellationToken ct = default)
    {
        using var form = BuildCategoryRequest(model, file);
        await SendAsync(() => httpClient.PutAsJsonAsync($"{CategoriesEndpointsBasePath}/{id}", model, ct), ct);
    }

    public Task DeleteCategoryAsync(Guid id, CancellationToken ct = default) =>
        SendAsync(() => httpClient.DeleteAsync($"{CategoriesEndpointsBasePath}/{id}", ct), ct);

    private static MultipartFormDataContent BuildCategoryRequest(CategoryUpsertViewModel model, IBrowserFile? file)
    {
        FilePart? filePart = GetFilePart(file);
        var form = MultipartFormBuilder.Build(model, filePart, static (f, m) =>
        {
            f.Add(new StringContent(m.Name), "Name");
        });

        return form;
    }

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
        Guid? categoryId = null,
        CancellationToken ct = default)
    {
        var orderByEscaped = Uri.EscapeDataString(orderBy ?? "Name");
        var orderDirEscaped = Uri.EscapeDataString(orderDirection ?? "asc");
        var escapedFilter = Uri.EscapeDataString(filter ?? string.Empty);

        var url = $"{ProductsEndpointsBasePath}?page={page}&pageSize={pageSize}&orderBy={orderByEscaped}&orderDirection={orderDirEscaped}&filter={escapedFilter}" +
           $"&categoryId={categoryId}";
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

    public Task<ProductDto?> GetProductByIdAsync(Guid id, CancellationToken ct = default) =>
        SendAsync<ProductDto>(() => httpClient.GetAsync($"{ProductsEndpointsBasePath}/{id}", ct), ct);

    public async Task CreateProductAsync(ProductUpsertViewModel model, IBrowserFile? file, CancellationToken ct = default)
    {
        using var form = BuildProductRequest(model, file);
        await SendAsync(() => httpClient.PostAsync($"{ProductsEndpointsBasePath}", form, ct), ct);
    }

    public async Task UpdateProductAsync(Guid id, ProductUpsertViewModel model, IBrowserFile? file, CancellationToken ct = default)
    {
        using var form = BuildProductRequest(model, file);
        await SendAsync(() => httpClient.PutAsync($"{ProductsEndpointsBasePath}/{id}", form, ct), ct);
    }

    public Task DeleteProductAsync(Guid id, CancellationToken ct = default) =>
        SendAsync(() => httpClient.DeleteAsync($"{ProductsEndpointsBasePath}/{id}", ct), ct);

    private static MultipartFormDataContent BuildProductRequest(ProductUpsertViewModel model, IBrowserFile? file)
    {
        FilePart? filePart = GetFilePart(file);
        var form = MultipartFormBuilder.Build(model, filePart, static (f, m) =>
        {
            f.Add(new StringContent(m.Name), "Name");
            f.Add(new StringContent(m.Description), "Description");
            f.Add(new StringContent(m.Sku), "Sku");
            f.Add(new StringContent(((int)m.Status).ToString()), "Status");
            f.Add(new StringContent(m.Price.ToString(CultureInfo.InvariantCulture)), "Price");
            f.Add(new StringContent(m.Stock.ToString(CultureInfo.InvariantCulture)), "Stock");
            f.Add(new StringContent(m.CategoryId!.Value.ToString()), "CategoryId");
        });

        return form;
    }

    #endregion

    private static FilePart? GetFilePart(IBrowserFile? file)
    {
        FilePart? filePart = null;
        if (file is not null)
        {
            var stream = file.OpenReadStream(5_000_000);
            filePart = new(stream, file.Name, file.ContentType ?? "application/octet-stream");
        }

        return filePart;
    }
}