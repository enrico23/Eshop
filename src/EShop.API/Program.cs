using Azure.Storage.Blobs;
using EShop.API.Exceptions;
using EShop.API.Services;
using EShop.Data;
using EShop.Data.Repositories;
using EShop.Domain;
using EShop.Shared.Eventing;
using EShop.Shared.Exceptions;
using EShop.Shared.Observability.Correlation;
using EShop.Shared.Services;
using Mapster;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

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

        // API Correlation Id options
        builder.Services.Configure<CorrelationIdOptions>(options =>
        {
            options.Policy = CorrelationIdPolicy.PreserveIncoming;
            options.HeaderName = "X-Correlation-ID";
        });
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<ICorrelationIdAccessor, CorrelationIdAccessor>();

        var app = builder.Build();

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

        app.Run();
    }
}