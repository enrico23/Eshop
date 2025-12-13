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

public abstract class ProductUpsertBase : ComponentBase
{
    [Parameter] public Guid? Id { get; set; }

    [Inject] protected ICatalogApiClient CatalogApiClient { get; set; } = null!;
    [Inject] protected NavigationManager NavigationManager { get; set; } = null!;
    [Inject] protected ISnackbar Snackbar { get; set; } = null!;
    [Inject] protected ApiClientConfig ApiClientConfig { get; set; } = null!;


    protected bool IsCreate => !Id.HasValue;
    protected bool IsLoading = true;
    protected bool IsSaving;
    protected string PageTitle = "";
    protected IBrowserFile? PreviewPrimaryImage;
    protected string? _previewPrimaryImageUrl;

    protected ProductDto? _product = null!;
    protected CreateProductViewModel UpsertModel = null!;
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
            IsLoading = false;
            NavigationManager.NavigateTo("/products?error=categories");
            return;
        }
        catch (Exception)
        {
            IsLoading = false;
            NavigationManager.NavigateTo("/products?error=unexpected");
            return;
        }

        UpsertModel = new CreateProductViewModel
        {
            Name = "",
            Description = "",
            Sku = "",
            Price = 0.01m,
            Stock = 1,
            Status = EShop.Shared.Domain.ProductStatus.Draft
        };

        if (IsCreate)
        {
            PageTitle = "Create Product";
        }
        else
        {
            try
            {
                _product = await CatalogApiClient.GetByIdAsync(Id!.Value);

                if (_product is null)
                {
                    IsLoading = false;
                    NavigationManager.NavigateTo("/products?error=notfound");
                    return;
                }

                PageTitle = $"Edit Product {_product.Name}";

                UpsertModel = new CreateProductViewModel()
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

        IsLoading = false;
    }

    protected async Task SubmitAsync()
    {
        IsSaving = true;

        await _form.Validate();
        if (!_form.IsValid)
        {
            IsSaving = false;
            return;
        }

        try
        {
            if (IsCreate)
            {
                await CatalogApiClient.CreateProductAsync(UpsertModel, UpsertModel.File, CancellationToken.None);
                Snackbar.Add($"Product '{UpsertModel.Name}' created successfully.", Severity.Success);
            }
            else
            {
                await CatalogApiClient.UpdateProductAsync(Id!.Value, UpsertModel, UpsertModel.File, CancellationToken.None);
                Snackbar.Add($"Product '{UpsertModel.Name}' updated successfully.", Severity.Success);
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
            IsSaving = false;
        }
    }

    protected async Task UploadPrimaryImage(IBrowserFile file)
    {
        PreviewPrimaryImage = file;
        UpsertModel.File = file;

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
