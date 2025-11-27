using EShop.Shared.Representations.Category;
using EShop.Web.Clients;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace EShop.Web.Components.Pages.Categories;

public class CategoryIndexBase : ComponentBase
{
    protected MudDataGrid<CategoryDto> dataGrid = null!;

    [Inject] protected ICategoryApiClient CategoryApiClient { get; set; } = null!;
    [Inject] protected IDialogService DialogService { get; set; } = null!;
    [Inject] protected ISnackbar Snackbar { get; set; } = null!;

    protected string? _searchString;    

    protected async Task<GridData<CategoryDto>> ServerReloadAsync(GridState<CategoryDto> state, CancellationToken ct)
    {
        var sortBy = "Name";
        var sortDirection = "asc";
        var currentPage = state.Page;
        var pageSize = state.PageSize;
        var searchStringFilter = _searchString;


        var sortDefinition = state.SortDefinitions.FirstOrDefault();
        if (sortDefinition != null)
        {
            sortBy = sortDefinition.SortBy;
            sortDirection = sortDefinition.Descending ? "desc" : "asc";
        }

        var pageNumber = currentPage + 1;

        var pagedData = await CategoryApiClient.GetPagedCategoriesAsync(
            pageNumber,
            pageSize, sortBy, sortDirection, searchStringFilter, ct);

        return new GridData<CategoryDto>
        {
            TotalItems = pagedData!.Total,
            Items = pagedData.Items
        };
    }

    protected async Task OnSearch(string text)
    {
        _searchString = text;
        await dataGrid.ReloadServerData();
    }

    protected async Task OpenDialogAsync()
    {
        using var cts = new CancellationTokenSource();
        var parameters = new DialogParameters { ["CancellationToken"] = cts.Token };
        var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Small, FullWidth = true };
        var dialog = await DialogService.ShowAsync<CreateCategoryDialog>("Add Category", parameters, options);
        var result = await dialog.Result;

        if (result!.Canceled != true)
        {
            var created = (CreateCategoryDto)result.Data!;
            Snackbar.Configuration.PositionClass = Defaults.Classes.Position.TopCenter;
            Snackbar.Add($"Category '{created.Name}' succesfully created", Severity.Success);

            await dataGrid.ReloadServerData();
        }
    }

    protected async Task OpenEditDialogAsync(CategoryDto item)
    {
        using var cts = new CancellationTokenSource();
        var parameters = new DialogParameters { ["CategoryParam"] = item, ["CancellationToken"] = cts.Token };
        var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Small, FullWidth = true };

        var dialog = await DialogService.ShowAsync<EditCategoryDialog>("Edit Category", parameters, options);
        var result = await dialog.Result;

        if (result!.Canceled != true)
        {
            var updated = (UpdateCategoryDto)result.Data!;
            Snackbar.Add($"Category '{updated.Name}' succesfully updated", Severity.Info);
            await dataGrid.ReloadServerData();
        }
    }

    protected async Task OpenDeleteDialogAsync(CategoryDto item)
    {
        using var cts = new CancellationTokenSource();
        var parameters = new DialogParameters { ["CategoryParam"] = item, ["CancellationToken"] = cts.Token };
        var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Small, FullWidth = true };

        var dialog = await DialogService.ShowAsync<DeleteCategoryDialog>($"Delete Category",
            parameters, options);
        var result = await dialog.Result;

        if (result!.Canceled != true)
        {
            Snackbar.Add($"Category '{item.Name}' succesfully deleted!", Severity.Success);
            await dataGrid.ReloadServerData();
        }
    }
}
