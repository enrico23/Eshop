using EShop.Web.Exceptions;
using System.Net;

namespace EShop.Web.Clients;

public abstract class ApiClientBase(ILogger logger)
{
    protected async Task<T?> SendAsync<T>(Func<Task<HttpResponseMessage>> action, CancellationToken ct = default)
    {
        var response = await ExecuteAsync(action);

        if (response.IsSuccessStatusCode)
        {
            if (response.StatusCode == HttpStatusCode.NoContent)
                return default;

            return await response.Content.ReadFromJsonAsync<T>(cancellationToken: ct);
        }

        await ThrowApiException(response, ct);
        return default;
    }

    protected async Task SendAsync(Func<Task<HttpResponseMessage>> action, CancellationToken ct = default)
    {
        var response = await ExecuteAsync(action);

        if (!response.IsSuccessStatusCode)
            await ThrowApiException(response, ct);
    }

    private async Task<HttpResponseMessage> ExecuteAsync(Func<Task<HttpResponseMessage>> action)
    {
        try
        {
            return await action();
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Network error while calling API.");
            throw new ApiException(HttpStatusCode.ServiceUnavailable, ex.Message);
        }
        catch (TaskCanceledException ex) when (!ex.CancellationToken.IsCancellationRequested)
        {
            // Timeout (NOT user cancellation)
            logger.LogError(ex, "API call timed out.");
            throw new ApiException(HttpStatusCode.RequestTimeout, "The request to the server timed out.");
        }
    }

    private async Task ThrowApiException(HttpResponseMessage response, CancellationToken ct)
    {
        var body = await response.Content.ReadAsStringAsync(ct);

        logger.LogError("API call failed. Status={StatusCode}, Body={Body}", response.StatusCode, body);

        throw new ApiException(response.StatusCode, body);
    }
}
