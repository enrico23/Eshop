using EShop.Shared.Handlers;
using EShop.Shared.MIddlewares;
using EShop.Web.Clients;
using EShop.Web.Components;
using EShop.Web.Extensions;
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

// Add CatalogApi Http Client
builder.AddCatalogApiClient();

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
