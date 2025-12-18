using EShop.Shared.Representations;
using EShop.Web.Exceptions;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net;

namespace EShop.Web.Components.Pages.Shared;

public class CrudPageBase : ComponentBase, IDisposable
{
    [Inject] protected IDialogService DialogService { get; set; } = null!;
    [Inject] protected ISnackbar Snackbar { get; set; } = null!;
    [Inject] protected NavigationManager NavigationManager { get; set; } = null!;

    private readonly DialogOptions _dialogOptions = new()
    {
        CloseButton = true,
        MaxWidth = MaxWidth.Medium,
        FullWidth = true
    };

    private readonly CancellationTokenSource _cts = new();
    protected CancellationToken PageToken => _cts.Token;

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
    }

    protected async Task<GridData<T>> LoadGridAsync<T>(
       GridState<T> state,
       string? searchText,
       string defaultSortBy,
       Func<int, int, string, string, string?, CancellationToken, Task<PagedResult<T>>> loadPage,
       CancellationToken cancellationToken = default)
    {
        var sortBy = defaultSortBy;
        var sortDirection = "asc";

        var sortDefinition = state.SortDefinitions.FirstOrDefault();
        if (sortDefinition is not null)
        {
            sortBy = sortDefinition.SortBy;
            sortDirection = sortDefinition.Descending ? "desc" : "asc";
        }

        var page = state.Page + 1;

        try
        {
            var pagedData = await loadPage(
                page,
                state.PageSize,
                sortBy,
                sortDirection,
                searchText,
                cancellationToken);

            return new GridData<T>
            {
                TotalItems = pagedData.Total,
                Items = pagedData.Items ?? new List<T>()
            };
        }
        catch (ApiException ex)
        {
            Snackbar.Add(ex.FriendlyMessage ?? "Request failed.", Severity.Error);
        }
        catch
        {
            Snackbar.Add("Unexpected error.", Severity.Error);
        }

        return new GridData<T> { TotalItems = 0, Items = new List<T>() };
    }

    protected async Task<bool> ConfirmDeleteAsync(
         string title,
         string message,
         Func<CancellationToken, Task> deleteAction,
         string successMessage)
    {
        var parameters = new DialogParameters
        {
            ["Message"] = message,
            ["ConfirmText"] = "Delete",
            ["CancelText"] = "Cancel"
        };

        var dialog = await DialogService.ShowAsync<DeleteConfirmationDialog>(title, parameters, _dialogOptions);
        var result = await dialog.Result;

        if (result!.Canceled) return false;

        try
        {
            await deleteAction(PageToken);
            Snackbar.Add(successMessage, Severity.Success);
            return true;
        }
        catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.Conflict)
        {
            Snackbar.Add(ex.Message is null ? "Conflict." : "Category cannot be deleted because it contains products.", Severity.Error);
        }
        catch (ApiException ex)
        {
            Snackbar.Add(ex.FriendlyMessage ?? "Request failed dddddd.", Severity.Error);
        }
        catch
        {
            Snackbar.Add("Unexpected error ddddd.", Severity.Error);
        }

        return false;
    }

    protected async Task<(bool Saved, TModel? Model)> ShowUpsertDialogAsync<TDialog, TModel>(
         string title,
         DialogParameters parameters)
         where TDialog : ComponentBase
         where TModel : class
    {
        var dialog = await DialogService.ShowAsync<TDialog>(title, parameters, _dialogOptions);
        var result = await dialog.Result;

        if (result is { Canceled: false, Data: TModel model })
            return (true, model);

        return (false, null);
    }

    protected async Task<bool> SaveWithFeedbackAsync<TModel>(
        TModel model,
        Func<TModel, CancellationToken, Task> saveAction,
        Func<TModel, string> successMessageFactory)
        where TModel : class
    {
        try
        {
            await saveAction(model, PageToken);
            Snackbar.Add(successMessageFactory(model), Severity.Success);
            return true;
        }
        catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.Conflict)
        {
            Snackbar.Add(ex.Message ?? "Conflict.", Severity.Error);
        }
        catch (ApiException ex)
        {
            Snackbar.Add(ex.FriendlyMessage ?? "Request failed.", Severity.Error);
        }
        catch
        {
            Snackbar.Add("Unexpected error.", Severity.Error);
        }

        return false;
    }
}
