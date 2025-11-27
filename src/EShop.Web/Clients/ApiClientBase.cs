using EShop.Web.Exceptions;
using System.Net;

namespace EShop.Web.Clients;

public abstract class ApiClientBase
{
    protected readonly HttpClient HttpClient;

    protected readonly ILogger Logger;

    protected ApiClientBase(HttpClient httpClient, ILogger logger)
    {
        HttpClient = httpClient;
        Logger = logger;
    }

    protected async Task<T?> SendAsync<T>(Func<Task<HttpResponseMessage>> action, CancellationToken ct)
    {
        var response = await action();

        if (response.IsSuccessStatusCode)
        {
            if (response.StatusCode == HttpStatusCode.NoContent)
                return default;

            return await response.Content.ReadFromJsonAsync<T>(cancellationToken: ct);
        }

        await ThrowApiException(response, ct);
        return default; // unreachable
    }

    protected async Task SendAsync(Func<Task<HttpResponseMessage>> action, CancellationToken ct)
    {
        var response = await action();
        if (!response.IsSuccessStatusCode)
            await ThrowApiException(response, ct);
    }

    private async Task ThrowApiException(HttpResponseMessage response, CancellationToken ct)
    {
        var body = await response.Content.ReadAsStringAsync(ct);

        Logger.LogError(
            "API call failed. Status={StatusCode}, Body={Body}",
            response.StatusCode, body);

        throw new ApiException(response.StatusCode, body);
    }
}

