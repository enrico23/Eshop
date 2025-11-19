using EShop.API.Services;
using EShop.Data;
using Mapster;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

var connectionString = builder.Configuration.GetConnectionString("CatalogDb");
builder.Services.AddDbContext<CatalogDbContext>(options =>
    options.UseSqlServer(connectionString));

// // Mapster DI registration
builder.Services.AddMapster();
TypeAdapterConfig.GlobalSettings.Scan(typeof(Program).Assembly);

builder.Services.AddScoped<IBrandService, BrandService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IProductService, ProductService>();

builder.Services.AddOpenApi();
builder.Services.AddControllers();

var app = builder.Build();

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
