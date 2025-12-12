using EShop.Shared.Representations.Category;
using EShop.Shared.Representations.Product;
using EShop.Web.Clients;
using EShop.Web.Exceptions;
using EShop.Web.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using System.Net;

namespace EShop.Web.Components.Pages.Products;

public class ProductUpsertBase : ComponentBase
{
    [Parameter] public Guid? Id { get; set; }

    [Inject] protected ICatalogApiClient CatalogApiClient { get; set; } = null!;
    [Inject] protected NavigationManager NavigationManager { get; set; } = null!;
    [Inject] protected ISnackbar Snackbar { get; set; } = null!;
    [Inject] protected ApiClientConfig ApiClientConfig { get; set; } = null!;


    protected bool _isCreate => !Id.HasValue;
    protected bool _isLoading = true;
    protected bool _isSaving;
    protected string _title = "";
    protected IBrowserFile? _previewPrimaryImage;
    protected string? _previewPrimaryImageUrl;

    protected ProductDto? _product = null!;
    protected CreateProductViewModel _model = null!;
    protected List<CategoryDto> _categories = new();
    protected MudForm _form = null!;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            _categories = await CatalogApiClient.GetAllCategoriesAsync();
        }
        catch (ApiException)
        {
            _isLoading = false;
            NavigationManager.NavigateTo("/products?error=categories");
            return;
        }
        catch (Exception)
        {
            _isLoading = false;
            NavigationManager.NavigateTo("/products?error=unexpected");
            return;
        }

        _model = new CreateProductViewModel
        {
            Name = "",
            Description = "",
            Sku = "",
            Price = 0.01m,
            Stock = 1,
            Status = EShop.Shared.Domain.ProductStatus.Draft
        };

        if (_isCreate)
        {
            _title = "Create Product";
        }
        else
        {
            try
            {
                _product = await CatalogApiClient.GetByIdAsync(Id!.Value);

                if (_product is null)
                {
                    _isLoading = false;
                    NavigationManager.NavigateTo("/products?error=notfound");
                    return;
                }

                _title = $"Edit Product {_product.Name}";

                _model = new CreateProductViewModel()
                {
                    Name = _product.Name,
                    Description = _product.Description ?? string.Empty,
                    Sku = _product.Sku,
                    Price = _product.Price,
                    Stock = _product.Stock,
                    Status = _product.Status,
                    CategoryId = _product.CategoryId,
                    Images = _product.Images
                };
               
            }
            catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                NavigationManager.NavigateTo("/products?error=notfound");
            }
            catch (ApiException)
            {
                NavigationManager.NavigateTo("/products?error=failed");
            }
            catch (Exception)
            {
                NavigationManager.NavigateTo("/products?error=unexpected");
            }
        }

        _isLoading = false;
    }

    protected void GoBack() => NavigationManager.NavigateTo("/products");

    protected async Task SubmitAsync()
    {
        _isSaving = true;

        await _form.Validate();
        if (!_form.IsValid)
        {
            _isSaving = false;
            return;
        }

        try
        {
            if (_isCreate)
            {
                await CatalogApiClient.CreateProductAsync(_model, _model.File, CancellationToken.None);
                Snackbar.Add($"Product '{_model.Name}' created successfully.", Severity.Success);
            }
            else
            {
                await CatalogApiClient.UpdateProductAsync(Id!.Value, _model, _model.File, CancellationToken.None);
                Snackbar.Add($"Product '{_model.Name}' updated successfully.", Severity.Success);
            }

            NavigationManager.NavigateTo("/products");
        }
        catch (ApiException ex) when (ex.StatusCode is HttpStatusCode.Conflict)
        {
            Snackbar.Add(ex.Message!, Severity.Error);
        }
        catch (ApiException ex)
        {
            Snackbar.Add(ex.FriendlyMessage!, Severity.Error);
        }
        catch (Exception)
        {
            Snackbar.Add("Unexpected error.", Severity.Error);
        }
        finally
        {
            _isSaving = false;
        }
    }

    protected async Task UploadPrimaryImage(IBrowserFile file)
    {
        _previewPrimaryImage = file;
        _model.File = file;

        try
        {
            using var stream = file.OpenReadStream(5_000_000);
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms);
            var bytes = ms.ToArray();

            var base64 = Convert.ToBase64String(bytes);
            var contentType = file.ContentType ?? "image/png";

            _previewPrimaryImageUrl = $"data:{contentType};base64,{base64}";
        }
        catch (Exception)
        {
            _previewPrimaryImageUrl = null;
        }

        await InvokeAsync(StateHasChanged);
    }

    protected string GetImage(string url)
    {
        var baseUrl = ApiClientConfig.BaseUrl;
        return $"{baseUrl}/api/images/{url}";
    }
}
