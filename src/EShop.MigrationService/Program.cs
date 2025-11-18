using EShop.Data;
using EShop.MigrationService;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();

var connectionString = builder.Configuration.GetConnectionString("CatalogDb");
builder.Services.AddDbContext<CatalogDbContext>(options =>
{
    options.UseSqlServer(connectionString);
});

var host = builder.Build();
host.Run();
