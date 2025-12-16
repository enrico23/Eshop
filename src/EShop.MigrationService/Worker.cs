using EShop.Data;
using EShop.Domain;
using EShop.Domain.Catalog;
using EShop.Domain.Products;
using Microsoft.EntityFrameworkCore;

namespace EShop.MigrationService;

public class Worker(
    IServiceProvider serviceProvider, 
    IHostApplicationLifetime hostApplicationLifetime) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
		try
		{
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
            await RunMigrationAsync(dbContext, stoppingToken);

            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            bool seedSampleData = configuration.GetValue<bool>("SeedSampleData");
            await RunSeedingAsync(dbContext, seedSampleData, stoppingToken);
        }
		catch (Exception)
		{
			throw;
		}

        hostApplicationLifetime.StopApplication();
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

        var categories = await dbContext.Categories.AsNoTracking().ToListAsync(ct);
        if (categories.Count == 0)
            return;

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

        await dbContext.Products.AddRangeAsync(products, ct);
        await dbContext.SaveChangesAsync(ct);
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
        var raw = rng.NextDouble() * 950 + 50; // $50–$1000
        return Math.Round((decimal)raw, 2, MidpointRounding.AwayFromZero);
    }
}
