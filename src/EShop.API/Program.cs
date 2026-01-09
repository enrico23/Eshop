using Azure.Storage.Blobs;
using EShop.API.Exceptions;
using EShop.API.Services;
using EShop.Data;
using EShop.Data.Repositories;
using EShop.Shared.Eventing;
using EShop.Shared.Exceptions;
using EShop.Shared.Observability.Correlation;
using EShop.Shared.Services;
using Mapster;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.Diagnostics;
using System.Security.Claims;
using System.Text.Json;

public partial class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        if (builder.Environment.IsDevelopment())
            builder.Configuration.AddUserSecrets<Program>();

        builder.AddServiceDefaults();

        builder.Services.AddExceptionHandler<DomainExceptionHandler>();
        builder.Services.AddProblemDetails(options =>
        {
            options.CustomizeProblemDetails = context =>
            {
                context.ProblemDetails.Instance =
                    $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}";

                context.ProblemDetails.Extensions.TryAdd("requestId", context.HttpContext.TraceIdentifier);

                Activity? activity = context.HttpContext.Features.Get<IHttpActivityFeature>()?.Activity;
                context.ProblemDetails.Extensions.TryAdd("traceId", activity?.Id);
            };
        });

        // SQL SERVER 
        var connectionString = builder.Configuration.GetConnectionString("CatalogDb");
        builder.Services.AddDbContext<CatalogDbContext>(options =>
            options.UseSqlServer(connectionString));

        // BLOB FILE STORAGE
        var blobConnectionString = builder.Configuration.GetConnectionString("Blobs");
        var imageStorageServiceOptions = builder.Configuration.GetSection("AzureBlobStorage")
                .Get<ImageStorageServiceOptions>() 
                ?? throw new InvalidOperationException("ImageStorageServiceOptions configuration section is missing or invalid.");
        builder.Services.AddSingleton(_ => new BlobServiceClient(blobConnectionString));
        builder.Services.AddSingleton(imageStorageServiceOptions);
        builder.Services.AddSingleton(sp =>
        {
            var serviceClient = sp.GetRequiredService<BlobServiceClient>();
            return serviceClient.GetBlobContainerClient("product-images");
        });
        builder.Services.AddSingleton<IImageStorageService, ImageStorageService>();

        // Service Bus Client
        builder.AddAzureServiceBusClient("images-queue");
        builder.Services.Configure<ServiceBusPublisherOptions>(options =>
        {
            options.TimeToLive = TimeSpan.FromDays(1);
        });
        builder.Services.AddSingleton<IEventPublisher, ServiceBusEventPublisher>();

        // // Mapster DI registration
        builder.Services.AddMapster();
        TypeAdapterConfig.GlobalSettings.Scan(typeof(Program).Assembly);

        builder.Services.AddScoped<IUnitOfWork, EfUnitOfWork>();
        builder.Services.AddScoped(typeof(IRepository<,,>), typeof(EfRepository<,,>));
        builder.Services.AddScoped<IExtendedCategoryRepository, ExtendedCategoryRepository>();
        builder.Services.AddScoped<ICategoryService, CategoryService>();
        builder.Services.AddScoped<IExtendedProductRepository, ExtendedProductRepository>();
        builder.Services.AddScoped<IProductService, ProductService>();
        builder.Services.AddScoped<ISqlServerExceptionsHelper, SqlServerExceptionsHelper>();

        builder.Services.AddOpenApi();
        builder.Services.AddControllers();

        // OAUTH
        builder.Services.AddAuthentication()
                 .AddKeycloakJwtBearer(
                     serviceName: "keycloak",
                     realm: "eshop",
                     options =>
                     {
                         //if (builder.Environment.IsDevelopment())
                         //{
                         //    options.MetadataAddress = "http://keycloak:8080/realms/eshop/.well-known/openid-configuration";
                         //    options.ConfigurationManager =
                         //        new ConfigurationManager<OpenIdConnectConfiguration>(
                         //            options.MetadataAddress,
                         //            new OpenIdConnectConfigurationRetriever(),
                         //            new HttpDocumentRetriever
                         //            {
                         //                RequireHttps = false
                         //            })
                         //        {
                         //            AutomaticRefreshInterval = TimeSpan.FromMinutes(5),
                         //            RefreshInterval = TimeSpan.FromSeconds(10)
                         //        };
                         //}

                         options.Audience = "catalogapi";

                         if (builder.Environment.IsDevelopment())
                         {
                             options.RequireHttpsMetadata = false;
                         }

                         options.Events = new JwtBearerEvents
                         {
                             OnTokenValidated = ctx =>
                             {
                                 if (ctx.Principal?.Identity is ClaimsIdentity id)
                                 {
                                     AddKeycloakClientRolesAsRoleClaims(id, clientId: "catalogapi");
                                 }
                                 return Task.CompletedTask;
                             }
                         };
                     });

        builder.Services.AddAuthorizationBuilder()
            .AddPolicy("CatalogRead", p => p.RequireRole("catalog.read"))
            .AddPolicy("CatalogWrite", p => p.RequireRole("catalog.write"));

        // API Correlation Id options
        builder.Services.Configure<CorrelationIdOptions>(options =>
        {
            options.Policy = CorrelationIdPolicy.PreserveIncoming;
            options.HeaderName = "X-Correlation-ID";
        });
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<ICorrelationIdAccessor, CorrelationIdAccessor>();

        var app = builder.Build();
        app.UseAuthentication();
        app.UseAuthorization();

        app.UseMiddleware<CorrelationIdMiddleware>();

        // Converts unhandled exceptions into Problem Details responses
        app.UseExceptionHandler(
            new ExceptionHandlerOptions
            {
                StatusCodeSelector = ex => ex switch
                {
                    ArgumentException => StatusCodes.Status400BadRequest,
                    _ => StatusCodes.Status500InternalServerError
                }
            });
        // Returns the Problem Details response for (empty) non-successful responses
        app.UseStatusCodePages();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();

            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/openapi/v1.json", "v1");
            });
        }

        app.MapControllers();

        app.MapGet("/debug/token", (HttpContext ctx) =>
        {
            return Results.Json(new
            {
                HasAuthorizationHeader = ctx.Request.Headers.Authorization.Count > 0,
                AuthorizationHeader = ctx.Request.Headers.Authorization.ToString(), // remove once done
                IsAuthenticated = ctx.User.Identity?.IsAuthenticated ?? false,
                Name = ctx.User.Identity?.Name,
                Aud = ctx.User.Claims.Where(c => c.Type == "aud").Select(c => c.Value).ToArray(),
                ResourceAccess = ctx.User.Claims.FirstOrDefault(c => c.Type == "resource_access")?.Value,
                Roles = ctx.User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).Distinct().ToArray()
            });
        });

        app.Run();
    }

    private static void AddKeycloakClientRolesAsRoleClaims(ClaimsIdentity identity, string clientId)
    {
        var resourceAccess = identity.FindFirst("resource_access")?.Value;
        if (string.IsNullOrWhiteSpace(resourceAccess))
            return;

        using var doc = JsonDocument.Parse(resourceAccess);
        if (!doc.RootElement.TryGetProperty(clientId, out var clientObj)) return;
        if (!clientObj.TryGetProperty("roles", out var roles)) return;

        foreach (var r in roles.EnumerateArray())
        {
            var role = r.GetString();
            if (!string.IsNullOrWhiteSpace(role))
                identity.AddClaim(new Claim(ClaimTypes.Role, role));
        }
    }
}