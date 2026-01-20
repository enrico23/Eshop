using EShop.Shared.Representations.Category;
using EShop.Shared.Representations.Product;
using EShop.Web.Clients;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace EShop.Web.Components.Pages;

public abstract class HomeBase: ComponentBase
{
    [Inject] protected ICatalogApiClient CatalogApiClient { get; set; } = null!;
    [Inject] protected ApiClientConfig ApiClientConfig { get; set; } = null!;
    [Inject] protected IJSRuntime JS { get; set; } = default!;
    [Inject] protected NavigationManager Nav { get; set; } = null!;

    private const string _productDetailsBaseUrl = "/product";
    private const string DefaultSortBy = "Name";
    private const string DefaultSortDirection = "asc";
    private const string DefaultSearchTerm = "";

    protected int _page = 1;
    protected int _pageSize = 10;
    protected string _sortBy = DefaultSortBy;
    protected string _sortDirection = DefaultSortDirection;
    protected string _search = DefaultSearchTerm;
    protected Guid? _categoryId;

    protected string? _error;
    protected bool _isLoading;
    protected ElementReference _railRef;

    protected List<CategoryDto> _categories = [];
    protected List<ProductDto> _products = [];

    private int? _totalCount;
    protected bool _canPrev => _page > 1;
    protected bool _canNext
        => _totalCount is not null
            ? _page < (int)Math.Ceiling(_totalCount.Value / (double)_pageSize)
            : _products.Count == _pageSize;

    protected override async Task OnInitializedAsync()
    {
        _page = 1;
        await LoadCategoriesAsync();
        await LoadProductAsync();
    }

    protected async Task PrevPage()
    {
        if (!_canPrev) return;
        _page--;

        await LoadProductAsync();
    }

    protected async Task NextPage()
    {
        if (!_canNext) return;
        _page++;

        await LoadProductAsync();
    }

    protected async Task Reload() => await LoadProductAsync();

    private async Task LoadCategoriesAsync()
    {
        try
        {
            _categories = await CatalogApiClient.GetAllCategoriesAsync();
        }
        catch (Exception ex)
        {
            _error = ex.Message;
            throw;
        }
    }

    private async Task LoadProductAsync(bool append = false)
    {
        _isLoading = true;
        _error = null;

        try
        {
            var result = await CatalogApiClient.GetPagedProductsAsync(
                _page, _pageSize, _sortBy, _sortDirection, _search, _categoryId, CancellationToken.None);

            _totalCount = result.Total;

            if (append)
                _products.AddRange(result.Items);
            else
                _products = result.Items.ToList();
        }
        catch (Exception ex)
        {
            _error = ex.Message;
            if (!append) _products = new();
            _totalCount = null;
        }
        finally
        {
            _isLoading = false;
            StateHasChanged();
        }
    }

    protected static ProductImageDto? GetPrimaryOrFirstImage(ProductDto product)
    {
        if (product.Images is null || product.Images.Count == 0)
            return null;

        return product.Images.FirstOrDefault(i => i.IsPrimary)
            ?? product.Images.OrderBy(i => i.SortOrder).First();
    }

    protected string GetImageUrl(string url) => $"{ApiClientConfig.BaseUrl}/api/images/{url}";

    protected async Task ScrollByAsync(int dir)
    {
        await JS.InvokeVoidAsync("railScrollByPage", _railRef, dir);
    }

    protected void ViewProduct(Guid productId)
    {
        Nav.NavigateTo($"{_productDetailsBaseUrl}/{productId}");
    }

    protected Task SelectCategory(Guid id)
    {
        _error = id.ToString();
        return Task.CompletedTask;
    }
}
