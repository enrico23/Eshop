using EShop.Shared.Observability.Correlation;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;

namespace EShop.Shared.Tests.Observability.Correlation;

public sealed class CorrelationIdMiddlewarePreserveIncomingTests : IAsyncLifetime
{
    private IHost? _host;
    private HttpClient? _client;

    public async Task InitializeAsync()
    {
        _host = await CorrelationIdMiddlewareTestServerFactory.CreateHostAsync(
            CorrelationIdPolicy.PreserveIncoming);
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
    public async Task PreserveIncomingHeader_EchoesSameValue()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/");
        request.Headers.TryAddWithoutValidation("X-Correlation-ID", "abc12345");

        using var response = await _client!.SendAsync(request);

        Assert.True(response.Headers.TryGetValues("X-Correlation-ID", out var values));
        Assert.Equal("abc12345", values!.Single());
    }

    [Fact]
    public async Task MissingHeader_GeneratesNewValue()
    {
        using var response = await _client!.GetAsync("/");

        Assert.True(response.Headers.TryGetValues("X-Correlation-ID", out var values));
        var value = values!.Single();
        Assert.Equal(32, value.Length);
        Assert.All(value, c =>
            Assert.True((c >= '0' && c <= '9') || (c >= 'a' && c <= 'f')));
    }

    [Fact]
    public async Task InvalidHeader_GeneratesNewValue()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/");
        request.Headers.TryAddWithoutValidation("X-Correlation-ID", "bad!");

        using var response = await _client!.SendAsync(request);

        Assert.True(response.Headers.TryGetValues("X-Correlation-ID", out var values));
        var value = values!.Single();
        Assert.NotEqual("bad!", value);
        Assert.Equal(32, value.Length);
    }

}
