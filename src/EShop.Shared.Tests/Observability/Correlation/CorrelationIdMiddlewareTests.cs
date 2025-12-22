using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;

namespace EShop.Shared.Tests.Observability.Correlation;

public sealed class CorrelationIdMiddlewareGenerateAlwaysTests : IAsyncLifetime
{
    private IHost? _host;
    private HttpClient? _client;

    public async Task InitializeAsync()
    {
        _host = await CorrelationIdMiddlewareTestServerFactory.CreateHostAsync(
            CorrelationIdPolicy.GenerateAlways);
        _client = _host.GetTestClient();
    }

    public async Task DisposeAsync()
    {
        _client?.Dispose();
        if (_host is not null)
        {
            await _host.StopAsync();
            _host.Dispose();
        }
    }

    [Fact]
    public async Task GenerateAlways_IgnoresIncomingHeader()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/");
        request.Headers.TryAddWithoutValidation("X-Correlation-ID", "abc12345");

        using var response = await _client!.SendAsync(request);

        Assert.True(response.Headers.TryGetValues("X-Correlation-ID", out var values));
        var value = values!.Single();
        Assert.NotEqual("abc12345", value);
        Assert.Equal(32, value.Length);
    }

}
