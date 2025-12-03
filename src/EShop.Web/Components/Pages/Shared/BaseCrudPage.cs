using EShop.Web.Clients;
using EShop.Web.Exceptions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace EShop.Web.Components.Pages.Shared;

public abstract class BaseCrudPage : ComponentBase
{
    [Inject] protected ICatalogApiClient CategoryApiClient { get; set; } = null!;
    [Inject] protected IDialogService DialogService { get; set; } = null!;
    [Inject] protected ISnackbar Snackbar { get; set; } = null!;

    private DialogOptions _options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Medium, FullWidth = true };

    protected async Task<bool> ConfirmDeleteAsync<TEntity>(
       TEntity entity,
       string title,
       string message,
       Func<TEntity, Task> deleteAction)
    {
        var parameters = new DialogParameters
        {
            ["Message"] = message,
            ["ConfirmText"] = "Delete",
            ["CancelText"] = "Cancel"
        };

        var dialog = await DialogService.ShowAsync<DeleteConfirmationDialog>(
            title, parameters, _options);

        var result = await dialog.Result;

        if (result is { Canceled: false })
        {
            try
            {
                await deleteAction(entity);

                Snackbar.Add($"{title.Replace("Delete", "").Trim()} deleted successfully!", Severity.Success);

                return true;
            }
            catch (ApiException ex)
            {
                Snackbar.Add(ex.FriendlyMessage!, Severity.Error);
            }
            catch (Exception)
            {
                Snackbar.Add("Unexpected error.", Severity.Error);
            }
        }

        return false;
    }

    protected async Task<bool> ConfirmUpsertAsync<TDialog, TModel>(
        string title,
        DialogParameters parameters,
        Func<TModel, Task> saveAction,
        string successMessage)
        where TDialog : ComponentBase
        where TModel : class
    {
        var dialog = await DialogService.ShowAsync<TDialog>(title, parameters, _options);
        var result = await dialog.Result;

        if (result is { Canceled: false, Data: TModel model })
        {
            try
            {
                await saveAction(model);
                Snackbar.Add(successMessage, Severity.Success);
                return true;
            }
            catch (ApiException ex)
            {
                Snackbar.Add(ex.FriendlyMessage!, Severity.Error);
            }
            catch (Exception)
            {
                Snackbar.Add("Unexpected error.", Severity.Error);
            }
        }

        return false;
    }
}

