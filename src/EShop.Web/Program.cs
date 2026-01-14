using EShop.Web.Components;
using EShop.Web.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using MudBlazor.Services;

// Web //
var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

// Add MudBlazor services
builder.Services.AddMudServices();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add Authentication
builder.Services
    .AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
})
.AddCookie()
.AddKeycloakOpenIdConnect(
    serviceName: "keycloak",
    realm: "eshop",
    options =>
    {
        options.Authority = "http://localhost:8080/realms/eshop";
        options.ClientId = "blazor-ui";
        options.ResponseType = OpenIdConnectResponseType.Code;
        options.ClientSecret = builder.Configuration["Keycloak:ClientSecret"];
        options.SaveTokens = true;

        if (builder.Environment.IsDevelopment())
        {
            options.RequireHttpsMetadata = false;
        }

        options.Scope.Clear();
        options.Scope.Add("blazor_api_scope");
        options.Scope.Add("openid");
        options.Scope.Add("profile");
        options.Scope.Add("email");
    }); 

builder.Services.AddAuthorization();

builder.Services.AddHttpContextAccessor();

// Add CatalogApi Http Client
builder.AddCatalogApiClient();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

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

app.MapGet("/login", async (HttpContext ctx, string? returnUrl) =>
{
    returnUrl = string.IsNullOrWhiteSpace(returnUrl) ? "/" : returnUrl;
    await ctx.ChallengeAsync(OpenIdConnectDefaults.AuthenticationScheme,
        new AuthenticationProperties { RedirectUri = returnUrl });
});

app.MapGet("/whoami", (HttpContext ctx) =>
{
    return Results.Json(new
    {
        IsAuthenticated = ctx.User?.Identity?.IsAuthenticated ?? false,
        Name = ctx.User?.Identity?.Name,
        Claims = ctx.User?.Claims.Select(c => new { c.Type, c.Value })
    });
});

app.MapGet("/debug/token", async (HttpContext ctx) =>
{
    var accessToken = await ctx.GetTokenAsync("access_token");
    var idToken = await ctx.GetTokenAsync("id_token");

    return Results.Json(new
    {
        HasAccessToken = !string.IsNullOrWhiteSpace(accessToken),
        AccessToken = accessToken,
        IdToken = idToken
    });
})
.RequireAuthorization();

app.Run();
