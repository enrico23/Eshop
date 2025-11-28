using EShop.Web.Clients;
using EShop.Web.Components;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

// Add MudBlazor services
builder.Services.AddMudServices();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// CATALOG API HTTP CLIENT
var catalogApiBaseFromConfig = builder.Configuration["services:api:http:0"];
var catalogApiBaseFromEnv = Environment.GetEnvironmentVariable("services__api__http__0");
var catalogApiBase = catalogApiBaseFromConfig ?? catalogApiBaseFromEnv ?? "https://localhost:5002/";
builder.Services.AddHttpClient<ICategoryApiClient, CategoryApiClient>(client =>
{
    client.BaseAddress = new Uri(catalogApiBase);
});

var app = builder.Build();

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
