using EShop.Shared.Observability.Correlation;
using EShop.Web.Clients;
using Microsoft.Extensions.Http.Resilience;
using Polly;

namespace EShop.Web.Extensions;

public static class HttpClientExtension
{
    public static WebApplicationBuilder AddCatalogApiClient(this WebApplicationBuilder builder)
    {
        builder.Services.Configure<CorrelationIdOptions>(options =>
        {
            options.Policy = CorrelationIdPolicy.GenerateAlways;
            options.HeaderName = "X-Request-Id";
        }); 

        builder.Services.AddHttpContextAccessor();
        builder.Services.AddTransient<CorrelationIdDelegatingHandler>();

        var catalogApiBase = builder.Configuration.GetValue("services:CatalogApi:https:0", "http://localhost:5002");
        builder.Services.AddSingleton(new ApiClientConfig { BaseUrl = catalogApiBase });
        builder.Services.AddHttpClient<ICatalogApiClient, CatalogApiClient>(client =>
        {
            client.BaseAddress = new Uri(catalogApiBase, UriKind.Absolute);
            client.Timeout = Timeout.InfiniteTimeSpan;
        })
            .AddHttpMessageHandler<CorrelationIdDelegatingHandler>()
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
