using Microsoft.EntityFrameworkCore.Metadata;

namespace EShop.Data.Tests;

public sealed class CatalogDbContextConcurrencyConfigurationTests
{
    [Fact]
    public void Product_RowVersion_IsConfiguredAsConcurrencyToken()
    {
        using var context = CreateContext();

        var entityType = context.Model.FindEntityType(typeof(Product));
        var rowVersion = entityType?.FindProperty(nameof(Product.RowVersion));

        Assert.NotNull(rowVersion);
        Assert.True(rowVersion!.IsConcurrencyToken);
        Assert.Equal(ValueGenerated.OnAddOrUpdate, rowVersion.ValueGenerated);
    }

    [Fact]
    public void Category_RowVersion_IsConfiguredAsConcurrencyToken()
    {
        using var context = CreateContext();

        var entityType = context.Model.FindEntityType(typeof(Category));
        var rowVersion = entityType?.FindProperty(nameof(Category.RowVersion));

        Assert.NotNull(rowVersion);
        Assert.True(rowVersion!.IsConcurrencyToken);
        Assert.Equal(ValueGenerated.OnAddOrUpdate, rowVersion.ValueGenerated);
    }

    private static CatalogDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<CatalogDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new CatalogDbContext(options);
    }
}
