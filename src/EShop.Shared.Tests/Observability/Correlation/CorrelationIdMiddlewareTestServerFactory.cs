using EShop.Shared.Observability.Correlation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EShop.Shared.Tests.Observability.Correlation;

internal static class CorrelationIdMiddlewareTestServerFactory
{
    public static async Task<IHost> CreateHostAsync(CorrelationIdPolicy policy)
    {
        return await new HostBuilder()
            .ConfigureWebHost(webBuilder =>
            {
                webBuilder
                    .UseTestServer()
                    .ConfigureServices(services =>
                    {
                        services.AddLogging(static logging => logging.AddDebug());
                        services.Configure<CorrelationIdOptions>(options =>
                        {
                            options.Policy = policy;
                            options.HeaderName = "X-Correlation-ID";
                        });
                    })
                    .Configure(app =>
                    {
                        app.UseMiddleware<CorrelationIdMiddleware>();
                        app.Run(context => context.Response.WriteAsync("ok"));
                    });
            })
            .StartAsync();
    }
}
