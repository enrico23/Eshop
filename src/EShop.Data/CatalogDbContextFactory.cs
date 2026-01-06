namespace EShop.Data;

public sealed class CatalogDbContextFactory
    : IDesignTimeDbContextFactory<CatalogDbContext>
{
    public CatalogDbContext CreateDbContext(string[] args)
    {
        var config = new ConfigurationBuilder()
           .AddUserSecrets<CatalogDbContextFactory>(optional: true)
           .AddEnvironmentVariables()
           .Build();

        var cs = config.GetConnectionString("CatalogDb");

        if (string.IsNullOrWhiteSpace(cs))
            throw new InvalidOperationException(
                "Connection string 'CatalogDb' not found. Set User Secrets key 'ConnectionStrings:CatalogDb' or provide an environment variable.");

        var options = new DbContextOptionsBuilder<CatalogDbContext>()
            .UseSqlServer(cs)
            .Options;

        return new CatalogDbContext(options);
    }
}
