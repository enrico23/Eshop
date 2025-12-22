using EShop.Web.Clients;
using Microsoft.Extensions.Http.Resilience;
using Polly;

namespace EShop.Web.Extensions;

public static class HttpClientExtension
{
    public static WebApplicationBuilder AddCatalogApiClient(this WebApplicationBuilder builder)
    {
        var gateway = builder.Configuration.GetValue("services:Gateway:https:0", "http://localhost:5261");
        builder.Services.AddSingleton(new ApiClientConfig { BaseUrl = gateway });
        builder.Services.AddHttpClient<ICatalogApiClient, CatalogApiClient>(client =>
        {
            client.BaseAddress = new Uri(gateway, UriKind.Absolute);
            client.Timeout = Timeout.InfiniteTimeSpan;
        })
            .AddResilienceHandler("catalog-api", static pipeline =>
            {
                pipeline.AddTimeout(new HttpTimeoutStrategyOptions
                {
                    Timeout = TimeSpan.FromSeconds(30)
                });

                pipeline.AddRetry(new HttpRetryStrategyOptions
                {
                    MaxRetryAttempts = 3,
                    Delay = TimeSpan.FromMilliseconds(200),
                    MaxDelay = TimeSpan.FromSeconds(2),
                    BackoffType = DelayBackoffType.Exponential,
                    UseJitter = true
                });

                pipeline.AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
                {
                    FailureRatio = 0.5,
                    MinimumThroughput = 10,
                    SamplingDuration = TimeSpan.FromSeconds(30),
                    BreakDuration = TimeSpan.FromSeconds(20)
                });
            });

        return builder;
    }
}
