using EShop.Data;
using Microsoft.EntityFrameworkCore;

namespace EShop.MigrationService;

public class Worker(IServiceProvider serviceProvider, IHostApplicationLifetime hostApplicationLifetime) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
		try
		{
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
            await RunMigrationAsync(dbContext, stoppingToken);
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
            // Run migration in a transaction to avoid partial migration if it fails.
            await dbContext.Database.MigrateAsync(cancellationToken);
        });
    }
}
