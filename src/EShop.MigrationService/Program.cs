using EShop.Data;
using EShop.MigrationService;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);
builder.AddServiceDefaults();


// Catalog Db Migration Service
builder.Services.AddHostedService<CatalogDbMigratorService>();
var connectionString = builder.Configuration.GetConnectionString("CatalogDb");
builder.Services.AddDbContext<CatalogDbContext>(options =>
{
    options.UseSqlServer(connectionString);
});

// Website Db Migration Service
builder.Services.AddTransient(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var connectionString = configuration.GetConnectionString("WebsiteDb");

    return new SqlConnection(connectionString);
});
builder.Services.AddHostedService<WebsiteDbMigratorService>();

var host = builder.Build();
host.Run();
