using EShop.Data;
using EShop.Domain;
using EShop.Domain.Catalog;
using EShop.Domain.Products;
using Microsoft.EntityFrameworkCore;

namespace EShop.MigrationService;

public class CatalogDbMigratorService(
    IServiceProvider serviceProvider, 
    IHostApplicationLifetime hostApplicationLifetime,
    IConfiguration configuration,
     ILogger<CatalogDbMigratorService> logger) 
        : BackgroundService
{
    private readonly bool seedSampleData = configuration.GetValue<bool>("SeedSampleData");

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
		try
		{
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
            await RunMigrationAsync(dbContext, stoppingToken);

            if (seedSampleData)
            {
                logger.LogInformation("Seeding sample data into CatalogDbContext...");
                await RunSeedingAsync(dbContext, seedSampleData, stoppingToken);
                logger.LogInformation("Seeding sample data completed.");
            }
            else
            {
                logger.LogInformation("Seeding sample data skipped (SeedSampleData=false).");
            }
        }
		catch (Exception)
		{
			throw;
		}
        finally
        {
            hostApplicationLifetime.StopApplication();
        }
    }

    private static async Task RunMigrationAsync(CatalogDbContext dbContext, CancellationToken cancellationToken)
    {
        var strategy = dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await dbContext.Database.MigrateAsync(cancellationToken);
        });
    }

    private async Task RunSeedingAsync(CatalogDbContext dbContext, bool seedSampleData, CancellationToken ct)
    {
        if (!seedSampleData)
            return;

        if (await dbContext.Products.AnyAsync(ct))
            return;

        if(await dbContext.Categories.AnyAsync(ct))
            return;

        var categories = new List<Category>()
        {
            Category.Create(new Guid("a3b4f1de-8241-4ff0-9c5e-af4c4c7a1e10"), "Laptops"),
            Category.Create(new Guid("c1e2d8ba-992f-4e8e-a0a9-5bcff3df8c77"), "Desktops"),
            Category.Create(new Guid("f5e1a4cc-2b9b-4db9-9b52-6d3f72440365"), "Smartphones"),
            Category.Create(new Guid("0b73b90f-9610-4f64-893e-37edc85c0a5f"), "Tablets"),
            Category.Create(new Guid("49d2e88d-241d-4fdb-9d41-cf4a6c9ab3c9"), "Wearables"),
            Category.Create(new Guid("7fb5c7d1-c2a4-4ff2-9f76-4e0b55a389b6"), "Audio"),
            Category.Create(new Guid("d0c52f04-4cd2-4e47-bc9f-06b251f8eae9"), "Gaming"),
            Category.Create(new Guid("3a4b77df-1a28-4f7e-b764-c4d62a6f6ed8"), "Cameras")
        };

        var rng = Random.Shared;
        var adjectives = new[] 
            { "Nova", "Quantum", "Pulse", "Apex", "Vertex", "Nimbus", "Halo", "Drift", "Edge", "Echo", "Shift", "Flux" };
        var nouns = new[] 
            { "Laptop", "Desktop", "Phone", "Tablet", "Watch", "Headset", "Speaker", "Camera", "Console", "Monitor", "Drive", "Router" };
        var usedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var products = new List<Product>();
     
        for (var i = 0; i < 21; i++)
        {
            var name = GenerateUniqueName(adjectives, nouns, usedNames, rng);
            var description = new ProductDescription($"Sample {name} ready to demo catalog features.");
            var price = new Money(GeneratePrice(rng));
            var stock = new StockQuantity(rng.Next(5, 250));
            var category = categories[rng.Next(categories.Count)];
            var sku = new Sku($"SKU-{rng.Next(100000, 999999)}-{i:D2}");

            var product = Product.Create(name, description, price, stock, category.Id, sku);
            product.Publish(); // start as Active
            products.Add(product);
        }

        var strategy = dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await dbContext.Categories.AddRangeAsync(categories, ct);
            await dbContext.Products.AddRangeAsync(products, ct);
            await dbContext.SaveChangesAsync(ct);
        });
    }

    private static string GenerateUniqueName(string[] adjectives, string[] nouns, HashSet<string> used, Random rng)
    {
        for (var attempts = 0; attempts < 50; attempts++)
        {
            var name = $"{adjectives[rng.Next(adjectives.Length)]} {nouns[rng.Next(nouns.Length)]}";
            if (used.Add(name))
                return name;
        }

        var fallback = $"Product {Guid.NewGuid():N}";
        used.Add(fallback);
        return fallback;
    }

    private static decimal GeneratePrice(Random rng)
    {
        var raw = rng.NextDouble() * 950 + 50; // 50–1000
        return Math.Round((decimal)raw, 2, MidpointRounding.AwayFromZero);
    }
}
