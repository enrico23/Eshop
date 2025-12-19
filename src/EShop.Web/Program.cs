using EShop.Shared.Handlers;
using EShop.Shared.MIddlewares;
using EShop.Web.Clients;
using EShop.Web.Components;
using Microsoft.Extensions.Http.Resilience;
using MudBlazor.Services;
using Polly;

var builder = WebApplication.CreateBuilder(args);

builder.ConfigureOpenTelemetry();
builder.AddDefaultHealthChecks();
builder.Services.AddServiceDiscovery();

// Add MudBlazor services
builder.Services.AddMudServices();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// CATALOG API HTTP CLIENT
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<CorrelationIdDelegatingHandler>();

var catalogApiBase = builder.Configuration.GetValue("services:CatalogApi:https:0", "http://localhost:5002");
builder.Services.AddSingleton(new ApiClientConfig { BaseUrl = catalogApiBase });
builder.Services.AddHttpClient<ICatalogApiClient, CatalogApiClient>(client =>
{
    client.BaseAddress = new Uri(catalogApiBase, UriKind.Absolute);
    client.Timeout = Timeout.InfiniteTimeSpan;
})
    // .AddHttpMessageHandler<CorrelationIdDelegatingHandler>()
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

var app = builder.Build();

// app.UseMiddleware<CorrelationIdMiddleware>();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
