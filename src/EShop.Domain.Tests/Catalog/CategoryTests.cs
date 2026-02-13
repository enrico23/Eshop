namespace EShop.Domain.Tests.Catalog;

public sealed class CategoryTests
{
    [Fact]
    public void Constructor_WithName_SetsIdAndTrimsName()
    {
        var category = Category.Create("  Shoes  ");

        Assert.NotEqual(Guid.Empty, category.Id);
        Assert.Equal("Shoes", category.Name);
        Assert.Empty(category.Products);
    }

    [Fact]
    public void Constructor_WithIdAndName_PreservesIdAndTrimsName()
    {
        var id = Guid.NewGuid();

        var category =Category.Create(id, "  Shirts  ");

        Assert.Equal(id, category.Id);
        Assert.Equal("Shirts", category.Name);
        Assert.Empty(category.Products);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidName_ThrowsArgumentException(string? name)
    {
        Assert.Throws<ArgumentException>(() => Category.Create(name!));
    }

    [Fact]
    public void ChangeName_TrimsAndUpdates()
    {
        var category = Category.Create("Old");

        category.ChangeName("  New  ");

        Assert.Equal("New", category.Name);
    }
}
